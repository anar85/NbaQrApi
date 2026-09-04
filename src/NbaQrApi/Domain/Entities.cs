namespace NbaQrApi.Domain;

public sealed class Terminal
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = "";
    public string TerminalNo { get; set; } = "";
    public string? TerminalModel { get; set; }
    public string TerminalType { get; set; } = "02";
    public string CountryCode { get; set; } = "AZ";
    public string HeaderCountryCode { get; set; } = "AZE";
    public string CurrencyCode { get; set; } = "AZN";
    public string CurrencyNumericCode { get; set; } = "944";
    public string TerminalLanguageCode { get; set; } = "az";
    public string ReceiptLanguageCode { get; set; } = "az";
    public string TimeZone { get; set; } = "Asia/Baku";
    public string RrnPrefix { get; set; } = "";
    public int? CompanyId { get; set; }
    public string CompanyCode { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public int? MerchantId { get; set; }
    public int? RegisterId { get; set; }
    public Guid? RegisterTsmId { get; set; }
    public string MerchantName { get; set; } = "";
    public string? MerchantAddress1 { get; set; }
    public string? PhoneNumber { get; set; }
    public string CategoryCode { get; set; } = "";
    public string MerchantNo { get; set; } = "";
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string City { get; set; } = "";
    public string? PostalCode { get; set; }
    public string BranchName { get; set; } = "";
    public string AliasType { get; set; } = "04";
    public string AliasValue { get; set; } = "";
    public string? BankBic { get; set; }
    public string ProviderBic { get; set; } = "";
    public string OperationCode { get; set; } = "MPRQ-ATP";
    public string TransactionType { get; set; } = "613";
    public string IpsSpecVersion { get; set; } = "MPV002";
    public string IpsUuid { get; set; } = "";
    public string DeliveryChannel { get; set; } = "400";
    public string? Coordinates { get; set; }
    public string? ConsumerInfoQuery { get; set; }
    public string? TipFeeType { get; set; }
    public string? FixedConvenienceFee { get; set; }
    public string? ConvenienceFeePercent { get; set; }
    public string? AltLanguageCode { get; set; }
    public string? AltMerchantName { get; set; }
    public string? AltCity { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class QrPayment
{
    public long Id { get; set; }
    public string UniqueId { get; set; } = "";
    public string EndToEndId { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public int PaymentType { get; set; }
    public decimal TotalAmount { get; set; }
    public string QrCodeStr { get; set; } = "";
    public int StatusCode { get; set; }
    public string StatusDesc { get; set; } = "";
    public string MerchantNo { get; set; } = "";
    public string TerminalNo { get; set; } = "";
    public string CurrencyCode { get; set; } = "";
    public long? RefundedPaymentId { get; set; }
    public string? RefundedUniqueId { get; set; }
    public string? RefundedEndToEndId { get; set; }
    public string? IpsStatus { get; set; }
    public bool IsCanceled { get; set; }
    public string? Description { get; set; }
}

public static class PaymentTypes
{
    public const int Sale = 1;
    public const int Refund = 4;
}

public static class PaymentStatuses
{
    public const int Successful = 0;
    public const int WaitInquiry = 1;
    public const int WaitProcess = 2;
    public const int RefundWaitProcess = 3;
    public const int CanceledByMerchant = 8;
    public const int Fail = 99;

    public static string Describe(int statusCode) => statusCode switch
    {
        Successful => "Successfull",
        WaitInquiry => "Wait Inquiry",
        WaitProcess => "Wait Process",
        RefundWaitProcess => "Refund Wait Process",
        CanceledByMerchant => "Canceled by Merchant",
        Fail => "FAil",
        _ => "Unknown"
    };
}
