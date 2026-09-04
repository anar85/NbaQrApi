using Microsoft.Data.SqlClient;
using NbaQrApi.Domain;

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
    private readonly ISqlConnectionFactory _connections;

    public QrPaymentRepository(ISqlConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<bool> UniqueIdExistsAsync(string uniqueId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.QrPayments WHERE UniqueId = @UniqueId) THEN 1 ELSE 0 END";
        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UniqueId", uniqueId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }

    public async Task<long> InsertAsync(QrPayment payment, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.QrPayments
            (
                UniqueId, EndToEndId, SerialNumber, PaymentType, TotalAmount, QrCodeStr,
                StatusCode, StatusDesc, MerchantNo, TerminalNo, CurrencyCode,
                RefundedPaymentId, RefundedUniqueId, RefundedEndToEndId, IpsStatus, IsCanceled, Description
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @UniqueId, @EndToEndId, @SerialNumber, @PaymentType, @TotalAmount, @QrCodeStr,
                @StatusCode, @StatusDesc, @MerchantNo, @TerminalNo, @CurrencyCode,
                @RefundedPaymentId, @RefundedUniqueId, @RefundedEndToEndId, @IpsStatus, @IsCanceled, @Description
            )
            """;

        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UniqueId", payment.UniqueId);
        command.Parameters.AddWithValue("@EndToEndId", payment.EndToEndId);
        command.Parameters.AddWithValue("@SerialNumber", payment.SerialNumber);
        command.Parameters.AddWithValue("@PaymentType", payment.PaymentType);
        command.Parameters.AddWithValue("@TotalAmount", payment.TotalAmount);
        command.Parameters.AddWithValue("@QrCodeStr", payment.QrCodeStr);
        command.Parameters.AddWithValue("@StatusCode", payment.StatusCode);
        command.Parameters.AddWithValue("@StatusDesc", payment.StatusDesc);
        command.Parameters.AddWithValue("@MerchantNo", payment.MerchantNo);
        command.Parameters.AddWithValue("@TerminalNo", payment.TerminalNo);
        command.Parameters.AddWithValue("@CurrencyCode", payment.CurrencyCode);
        command.Parameters.AddWithValue("@RefundedPaymentId", (object?)payment.RefundedPaymentId ?? DBNull.Value);
        command.Parameters.AddWithValue("@RefundedUniqueId", (object?)payment.RefundedUniqueId ?? DBNull.Value);
        command.Parameters.AddWithValue("@RefundedEndToEndId", (object?)payment.RefundedEndToEndId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IpsStatus", (object?)payment.IpsStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCanceled", payment.IsCanceled);
        command.Parameters.AddWithValue("@Description", (object?)payment.Description ?? DBNull.Value);

        var id = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(id);
    }

    public async Task<QrPayment?> GetByUniqueIdAsync(string uniqueId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1)
                Id, UniqueId, EndToEndId, SerialNumber, PaymentType, TotalAmount, QrCodeStr,
                StatusCode, StatusDesc, MerchantNo, TerminalNo, CurrencyCode,
                RefundedPaymentId, RefundedUniqueId, RefundedEndToEndId, IpsStatus, IsCanceled, Description
            FROM dbo.QrPayments
            WHERE UniqueId = @UniqueId
            """;

        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UniqueId", uniqueId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new QrPayment
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            UniqueId = reader.GetString(reader.GetOrdinal("UniqueId")),
            EndToEndId = reader.GetString(reader.GetOrdinal("EndToEndId")),
            SerialNumber = reader.GetString(reader.GetOrdinal("SerialNumber")),
            PaymentType = reader.GetInt32(reader.GetOrdinal("PaymentType")),
            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
            QrCodeStr = reader.GetString(reader.GetOrdinal("QrCodeStr")),
            StatusCode = reader.GetInt32(reader.GetOrdinal("StatusCode")),
            StatusDesc = reader.GetString(reader.GetOrdinal("StatusDesc")),
            MerchantNo = reader.GetString(reader.GetOrdinal("MerchantNo")),
            TerminalNo = reader.GetString(reader.GetOrdinal("TerminalNo")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")).Trim(),
            RefundedPaymentId = reader.IsDBNull(reader.GetOrdinal("RefundedPaymentId")) ? null : reader.GetInt64(reader.GetOrdinal("RefundedPaymentId")),
            RefundedUniqueId = reader.IsDBNull(reader.GetOrdinal("RefundedUniqueId")) ? null : reader.GetString(reader.GetOrdinal("RefundedUniqueId")),
            RefundedEndToEndId = reader.IsDBNull(reader.GetOrdinal("RefundedEndToEndId")) ? null : reader.GetString(reader.GetOrdinal("RefundedEndToEndId")),
            IpsStatus = reader.IsDBNull(reader.GetOrdinal("IpsStatus")) ? null : reader.GetString(reader.GetOrdinal("IpsStatus")),
            IsCanceled = reader.GetBoolean(reader.GetOrdinal("IsCanceled")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
        };
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
        const string sql = """
            UPDATE dbo.QrPayments
            SET StatusCode = @StatusCode,
                StatusDesc = @StatusDesc,
                IsCanceled = @IsCanceled,
                IpsStatus = @IpsStatus,
                Description = @Description,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE UniqueId = @UniqueId
            """;

        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UniqueId", uniqueId);
        command.Parameters.AddWithValue("@StatusCode", statusCode);
        command.Parameters.AddWithValue("@StatusDesc", statusDesc);
        command.Parameters.AddWithValue("@IsCanceled", isCanceled);
        command.Parameters.AddWithValue("@IpsStatus", (object?)ipsStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
