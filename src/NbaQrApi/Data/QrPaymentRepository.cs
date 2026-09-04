using System.Data;
using System.Data.Common;
using NbaQrApi.Domain;
using Oracle.ManagedDataAccess.Client;
using static NbaQrApi.Data.DataReaderValues;

namespace NbaQrApi.Data;

public interface IQrPaymentRepository
{
    Task<bool> UniqueIdExistsAsync(string uniqueId, CancellationToken cancellationToken);
    Task<long> InsertAsync(QrPayment payment, CancellationToken cancellationToken);
    Task<QrPayment?> GetByUniqueIdAsync(string uniqueId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(string uniqueId, int statusCode, string statusDesc, bool isCanceled, string? ipsStatus, string? description, CancellationToken cancellationToken);
}

public sealed class QrPaymentRepository : IQrPaymentRepository
{
    private readonly IOracleConnectionFactory _connections;

    public QrPaymentRepository(IOracleConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<bool> UniqueIdExistsAsync(string uniqueId, CancellationToken cancellationToken)
    {
        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = _connections.CreateCommand("UNIQUE_ID_EXISTS", connection);

        AddInput(command, "P_UNIQUE_ID", OracleDbType.Varchar2, uniqueId);
        var exists = AddOutput(command, "P_EXISTS", OracleDbType.Decimal);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return GetInt32Output(exists) == 1;
    }

    public async Task<long> InsertAsync(QrPayment payment, CancellationToken cancellationToken)
    {
        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = _connections.CreateCommand("INSERT_QR_PAYMENT", connection);

        AddInput(command, "P_UNIQUE_ID", OracleDbType.Varchar2, payment.UniqueId);
        AddInput(command, "P_END_TO_END_ID", OracleDbType.Varchar2, payment.EndToEndId);
        AddInput(command, "P_SERIAL_NUMBER", OracleDbType.Varchar2, payment.SerialNumber);
        AddInput(command, "P_PAYMENT_TYPE", OracleDbType.Int32, payment.PaymentType);
        AddInput(command, "P_TOTAL_AMOUNT", OracleDbType.Decimal, payment.TotalAmount);
        AddInput(command, "P_QR_CODE_STR", OracleDbType.Clob, payment.QrCodeStr);
        AddInput(command, "P_STATUS_CODE", OracleDbType.Int32, payment.StatusCode);
        AddInput(command, "P_STATUS_DESC", OracleDbType.Varchar2, payment.StatusDesc);
        AddInput(command, "P_MERCHANT_NO", OracleDbType.Varchar2, payment.MerchantNo);
        AddInput(command, "P_TERMINAL_NO", OracleDbType.Varchar2, payment.TerminalNo);
        AddInput(command, "P_CURRENCY_CODE", OracleDbType.Char, payment.CurrencyCode);
        AddInput(command, "P_REFUNDED_PAYMENT_ID", OracleDbType.Int64, payment.RefundedPaymentId);
        AddInput(command, "P_REFUNDED_UNIQUE_ID", OracleDbType.Varchar2, payment.RefundedUniqueId);
        AddInput(command, "P_REFUNDED_END_TO_END_ID", OracleDbType.Varchar2, payment.RefundedEndToEndId);
        AddInput(command, "P_IPS_STATUS", OracleDbType.Varchar2, payment.IpsStatus);
        AddInput(command, "P_IS_CANCELED", OracleDbType.Int16, payment.IsCanceled ? 1 : 0);
        AddInput(command, "P_DESCRIPTION", OracleDbType.Varchar2, payment.Description);
        var id = AddOutput(command, "P_ID", OracleDbType.Decimal);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return GetInt64Output(id);
    }

    public async Task<QrPayment?> GetByUniqueIdAsync(string uniqueId, CancellationToken cancellationToken)
    {
        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = _connections.CreateCommand("GET_QR_PAYMENT_BY_UNIQUE_ID", connection);

        AddInput(command, "P_UNIQUE_ID", OracleDbType.Varchar2, uniqueId);
        AddOutput(command, "P_RESULT", OracleDbType.RefCursor);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return Map(reader);
    }

    public async Task UpdateStatusAsync(
        string uniqueId,
        int statusCode,
        string statusDesc,
        bool isCanceled,
        string? ipsStatus,
        string? description,
        CancellationToken cancellationToken)
    {
        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = _connections.CreateCommand("UPDATE_QR_PAYMENT_STATUS", connection);

        AddInput(command, "P_UNIQUE_ID", OracleDbType.Varchar2, uniqueId);
        AddInput(command, "P_STATUS_CODE", OracleDbType.Int32, statusCode);
        AddInput(command, "P_STATUS_DESC", OracleDbType.Varchar2, statusDesc);
        AddInput(command, "P_IS_CANCELED", OracleDbType.Int16, isCanceled ? 1 : 0);
        AddInput(command, "P_IPS_STATUS", OracleDbType.Varchar2, ipsStatus);
        AddInput(command, "P_DESCRIPTION", OracleDbType.Varchar2, description);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static QrPayment Map(DbDataReader reader)
        => new()
        {
            Id = GetInt64(reader, "Id"),
            UniqueId = GetString(reader, "UniqueId"),
            EndToEndId = GetString(reader, "EndToEndId"),
            SerialNumber = GetString(reader, "SerialNumber"),
            PaymentType = GetInt32(reader, "PaymentType"),
            TotalAmount = GetDecimal(reader, "TotalAmount"),
            QrCodeStr = GetString(reader, "QrCodeStr"),
            StatusCode = GetInt32(reader, "StatusCode"),
            StatusDesc = GetString(reader, "StatusDesc"),
            MerchantNo = GetString(reader, "MerchantNo"),
            TerminalNo = GetString(reader, "TerminalNo"),
            CurrencyCode = GetString(reader, "CurrencyCode").Trim(),
            RefundedPaymentId = GetInt64OrNull(reader, "RefundedPaymentId"),
            RefundedUniqueId = GetStringOrNull(reader, "RefundedUniqueId"),
            RefundedEndToEndId = GetStringOrNull(reader, "RefundedEndToEndId"),
            IpsStatus = GetStringOrNull(reader, "IpsStatus"),
            IsCanceled = GetBoolean(reader, "IsCanceled"),
            Description = GetStringOrNull(reader, "Description")
        };

    private static void AddInput(OracleCommand command, string name, OracleDbType type, object? value)
    {
        var parameter = command.Parameters.Add(name, type);
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = value ?? DBNull.Value;
    }

    private static OracleParameter AddOutput(OracleCommand command, string name, OracleDbType type)
    {
        var parameter = command.Parameters.Add(name, type);
        parameter.Direction = ParameterDirection.Output;
        return parameter;
    }
}
