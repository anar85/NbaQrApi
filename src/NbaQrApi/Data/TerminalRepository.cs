using Microsoft.Data.SqlClient;
using NbaQrApi.Domain;

namespace NbaQrApi.Data;

public interface ITerminalRepository
{
    Task<Terminal?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken);
}

public sealed class TerminalRepository : ITerminalRepository
{
    private readonly ISqlConnectionFactory _connections;

    public TerminalRepository(ISqlConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<Terminal?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1)
                Id, SerialNumber, TerminalNo, TerminalModel, TerminalType,
                CountryCode, HeaderCountryCode, CurrencyCode, CurrencyNumericCode,
                TerminalLanguageCode, ReceiptLanguageCode, TimeZone, RrnPrefix,
                CompanyId, CompanyCode, CompanyName, MerchantId, RegisterId, RegisterTsmId,
                MerchantName, MerchantAddress1, PhoneNumber, CategoryCode, MerchantNo, Email,
                TaxNumber, City, PostalCode, BranchName,
                AliasType, AliasValue, BankBic, ProviderBic, OperationCode, TransactionType,
                IpsSpecVersion, IpsUuid, DeliveryChannel, Coordinates, ConsumerInfoQuery,
                TipFeeType, FixedConvenienceFee, ConvenienceFeePercent,
                AltLanguageCode, AltMerchantName, AltCity, IsActive
            FROM dbo.Terminals
            WHERE SerialNumber = @SerialNumber AND IsActive = 1
            """;

        await using var connection = _connections.Create();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SerialNumber", serialNumber);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return Map(reader);
    }

    private static Terminal Map(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        SerialNumber = reader.GetString(reader.GetOrdinal("SerialNumber")),
        TerminalNo = reader.GetString(reader.GetOrdinal("TerminalNo")),
        TerminalModel = GetStringOrNull(reader, "TerminalModel"),
        TerminalType = reader.GetString(reader.GetOrdinal("TerminalType")).Trim(),
        CountryCode = reader.GetString(reader.GetOrdinal("CountryCode")).Trim(),
        HeaderCountryCode = reader.GetString(reader.GetOrdinal("HeaderCountryCode")),
        CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")).Trim(),
        CurrencyNumericCode = reader.GetString(reader.GetOrdinal("CurrencyNumericCode")).Trim(),
        TerminalLanguageCode = reader.GetString(reader.GetOrdinal("TerminalLanguageCode")),
        ReceiptLanguageCode = reader.GetString(reader.GetOrdinal("ReceiptLanguageCode")),
        TimeZone = reader.GetString(reader.GetOrdinal("TimeZone")),
        RrnPrefix = reader.GetString(reader.GetOrdinal("RrnPrefix")),
        CompanyId = GetIntOrNull(reader, "CompanyId"),
        CompanyCode = reader.GetString(reader.GetOrdinal("CompanyCode")),
        CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
        MerchantId = GetIntOrNull(reader, "MerchantId"),
        RegisterId = GetIntOrNull(reader, "RegisterId"),
        RegisterTsmId = GetGuidOrNull(reader, "RegisterTsmId"),
        MerchantName = reader.GetString(reader.GetOrdinal("MerchantName")),
        MerchantAddress1 = GetStringOrNull(reader, "MerchantAddress1"),
        PhoneNumber = GetStringOrNull(reader, "PhoneNumber"),
        CategoryCode = reader.GetString(reader.GetOrdinal("CategoryCode")).Trim(),
        MerchantNo = reader.GetString(reader.GetOrdinal("MerchantNo")),
        Email = GetStringOrNull(reader, "Email"),
        TaxNumber = GetStringOrNull(reader, "TaxNumber"),
        City = reader.GetString(reader.GetOrdinal("City")),
        PostalCode = GetStringOrNull(reader, "PostalCode"),
        BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
        AliasType = reader.GetString(reader.GetOrdinal("AliasType")).Trim(),
        AliasValue = reader.GetString(reader.GetOrdinal("AliasValue")),
        BankBic = GetStringOrNull(reader, "BankBic"),
        ProviderBic = reader.GetString(reader.GetOrdinal("ProviderBic")),
        OperationCode = reader.GetString(reader.GetOrdinal("OperationCode")),
        TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")).Trim(),
        IpsSpecVersion = reader.GetString(reader.GetOrdinal("IpsSpecVersion")).Trim(),
        IpsUuid = reader.GetString(reader.GetOrdinal("IpsUuid")).Trim(),
        DeliveryChannel = reader.GetString(reader.GetOrdinal("DeliveryChannel")).Trim(),
        Coordinates = GetStringOrNull(reader, "Coordinates"),
        ConsumerInfoQuery = GetStringOrNull(reader, "ConsumerInfoQuery"),
        TipFeeType = GetStringOrNull(reader, "TipFeeType"),
        FixedConvenienceFee = GetStringOrNull(reader, "FixedConvenienceFee"),
        ConvenienceFeePercent = GetStringOrNull(reader, "ConvenienceFeePercent"),
        AltLanguageCode = GetStringOrNull(reader, "AltLanguageCode"),
        AltMerchantName = GetStringOrNull(reader, "AltMerchantName"),
        AltCity = GetStringOrNull(reader, "AltCity"),
        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
    };

    private static string? GetStringOrNull(SqlDataReader reader, string column)
    {
        var i = reader.GetOrdinal(column);
        return reader.IsDBNull(i) ? null : reader.GetString(i);
    }

    private static int? GetIntOrNull(SqlDataReader reader, string column)
    {
        var i = reader.GetOrdinal(column);
        return reader.IsDBNull(i) ? null : reader.GetInt32(i);
    }

    private static Guid? GetGuidOrNull(SqlDataReader reader, string column)
    {
        var i = reader.GetOrdinal(column);
        return reader.IsDBNull(i) ? null : reader.GetGuid(i);
    }
}
