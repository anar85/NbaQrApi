using System.Data;
using System.Data.Common;
using NbaQrApi.Domain;
using Oracle.ManagedDataAccess.Client;
using static NbaQrApi.Data.DataReaderValues;

namespace NbaQrApi.Data;

public interface ITerminalRepository
{
    Task<Terminal?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken);
}

public sealed class TerminalRepository : ITerminalRepository
{
    private readonly IOracleConnectionFactory _connections;

    public TerminalRepository(IOracleConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<Terminal?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken)
    {
        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = _connections.CreateCommand("GET_TERMINAL_BY_SERIAL", connection);

        AddInput(command, "P_SERIAL_NUMBER", OracleDbType.Varchar2, serialNumber);
        AddOutput(command, "P_RESULT", OracleDbType.RefCursor);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return Map(reader);
    }

    private static Terminal Map(DbDataReader reader) => new()
    {
        Id = GetInt32(reader, "Id"),
        SerialNumber = GetString(reader, "SerialNumber"),
        TerminalNo = GetString(reader, "TerminalNo"),
        TerminalModel = GetStringOrNull(reader, "TerminalModel"),
        TerminalType = GetString(reader, "TerminalType").Trim(),
        CountryCode = GetString(reader, "CountryCode").Trim(),
        HeaderCountryCode = GetString(reader, "HeaderCountryCode"),
        CurrencyCode = GetString(reader, "CurrencyCode").Trim(),
        CurrencyNumericCode = GetString(reader, "CurrencyNumericCode").Trim(),
        TerminalLanguageCode = GetString(reader, "TerminalLanguageCode"),
        ReceiptLanguageCode = GetString(reader, "ReceiptLanguageCode"),
        TimeZone = GetString(reader, "TimeZone"),
        RrnPrefix = GetString(reader, "RrnPrefix"),
        CompanyId = GetInt32OrNull(reader, "CompanyId"),
        CompanyCode = GetString(reader, "CompanyCode"),
        CompanyName = GetString(reader, "CompanyName"),
        MerchantId = GetInt32OrNull(reader, "MerchantId"),
        RegisterId = GetInt32OrNull(reader, "RegisterId"),
        RegisterTsmId = GetGuidOrNull(reader, "RegisterTsmId"),
        MerchantName = GetString(reader, "MerchantName"),
        MerchantAddress1 = GetStringOrNull(reader, "MerchantAddress1"),
        PhoneNumber = GetStringOrNull(reader, "PhoneNumber"),
        CategoryCode = GetString(reader, "CategoryCode").Trim(),
        MerchantNo = GetString(reader, "MerchantNo"),
        Email = GetStringOrNull(reader, "Email"),
        TaxNumber = GetStringOrNull(reader, "TaxNumber"),
        City = GetString(reader, "City"),
        PostalCode = GetStringOrNull(reader, "PostalCode"),
        BranchName = GetString(reader, "BranchName"),
        AliasType = GetString(reader, "AliasType").Trim(),
        AliasValue = GetString(reader, "AliasValue"),
        BankBic = GetStringOrNull(reader, "BankBic"),
        ProviderBic = GetString(reader, "ProviderBic"),
        OperationCode = GetString(reader, "OperationCode"),
        TransactionType = GetString(reader, "TransactionType").Trim(),
        IpsSpecVersion = GetString(reader, "IpsSpecVersion").Trim(),
        IpsUuid = GetString(reader, "IpsUuid").Trim(),
        DeliveryChannel = GetString(reader, "DeliveryChannel").Trim(),
        Coordinates = GetStringOrNull(reader, "Coordinates"),
        ConsumerInfoQuery = GetStringOrNull(reader, "ConsumerInfoQuery"),
        TipFeeType = GetStringOrNull(reader, "TipFeeType"),
        FixedConvenienceFee = GetStringOrNull(reader, "FixedConvenienceFee"),
        ConvenienceFeePercent = GetStringOrNull(reader, "ConvenienceFeePercent"),
        AltLanguageCode = GetStringOrNull(reader, "AltLanguageCode"),
        AltMerchantName = GetStringOrNull(reader, "AltMerchantName"),
        AltCity = GetStringOrNull(reader, "AltCity"),
        IsActive = GetBoolean(reader, "IsActive")
    };

    private static void AddInput(OracleCommand command, string name, OracleDbType type, object value)
    {
        var parameter = command.Parameters.Add(name, type);
        parameter.Direction = ParameterDirection.Input;
        parameter.Value = value;
    }

    private static void AddOutput(OracleCommand command, string name, OracleDbType type)
    {
        var parameter = command.Parameters.Add(name, type);
        parameter.Direction = ParameterDirection.Output;
    }
}
