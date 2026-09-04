namespace NbaQrApi.AzQr;

public sealed class AzQrPayload
{
    /// <summary>IPS 39. MPV002 merchant-presented, CPV002 customer-presented.</summary>
    public required string IpsSpecVersion { get; init; }

    /// <summary>IPS 40. RFC 4122 UUID without hyphens, 32 hex chars.</summary>
    public required string IpsUuid { get; init; }

    /// <summary>EMV 00. Initial version is 01.</summary>
    public string EmvVersion { get; init; } = "01";

    /// <summary>EMV 01. 11 static, 12 dynamic.</summary>
    public required string QrCodeType { get; init; }

    public required AzQrMainData Main { get; init; }
    public required AzQrPresenter Presenter { get; init; }

    /// <summary>IPS 28. 16 digits: latitude 8 + longitude 8, no decimal point.</summary>
    public string? Coordinates { get; init; }

    public required AzQrIpsSpecific Ips { get; init; }
    public AzQrAdditionalInfo? Additional { get; init; }
    public AzQrBilling? Billing { get; init; }

    public required string MerchantCategoryCode { get; init; }
    public required string CurrencyNumericCode { get; init; }
    public string? Amount { get; init; }
    public string? TipFeeType { get; init; }
    public string? FixedConvenienceFee { get; init; }
    public string? ConvenienceFeePercent { get; init; }
    public required string CountryCode { get; init; }
    public required string MerchantName { get; init; }
    public required string City { get; init; }
    public string? PostalCode { get; init; }
    public AzQrAdditionalData? AdditionalData { get; init; }
    public AzQrAltLanguage? AltLanguage { get; init; }
}

public sealed class AzQrMainData
{
    public string Version { get; init; } = "01";
    public string? UniqueIdentifier { get; init; }
    public required string TerminalType { get; init; }
    public string? GeneratedAt { get; init; }
    public string? ExpiresAt { get; init; }
}

public sealed class AzQrPresenter
{
    public required string AliasType { get; init; }
    public required string AliasValue { get; init; }
    public string? BankBic { get; init; }
    public string? ObjectIdentifier { get; init; }
}

public sealed class AzQrIpsSpecific
{
    public string? ProviderBic { get; init; }
    public required string OperationCode { get; init; }
    public required string TransactionType { get; init; }
    public string? OrderNumber { get; init; }
    public string? AuthenticationValue { get; init; }
    public string? Email { get; init; }
}

public sealed class AzQrAdditionalInfo
{
    public string? CustomerLoyaltyNumber { get; init; }
    public string? CustomerNumber { get; init; }
    public string? TerminalNumber { get; init; }
    public string? Pin { get; init; }
    public string? ReturnAttribute { get; init; }
}

public sealed class AzQrBilling
{
    public required string OrganizationCode { get; init; }
    public string? InvoiceType { get; init; }
    public string? InvoiceNumber { get; init; }
    public string? PayerType { get; init; }
    public string? PayerCode { get; init; }
}

public sealed class AzQrAdditionalData
{
    public required string BranchName { get; init; }
    public string? PaymentInformation { get; init; }
    public string? ConsumerInfoQuery { get; init; }
    public string? TaxNumber { get; init; }
    public string? DeliveryChannel { get; init; }
}

public sealed class AzQrAltLanguage
{
    public required string LanguageCode { get; init; }
    public required string MerchantName { get; init; }
    public string? City { get; init; }
}
