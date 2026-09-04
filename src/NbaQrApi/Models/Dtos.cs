namespace NbaQrApi.Models;

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public string Title { get; init; } = "";
    public T? Data { get; init; }
    public string Message { get; init; } = "";

    public static ApiResponse<T> Ok(T data, string title = "Success!") => new()
    {
        IsSuccess = true,
        Title = title,
        Data = data,
        Message = ""
    };

    public static ApiResponse<T> Fail(string message, string title = "Error") => new()
    {
        IsSuccess = false,
        Title = title,
        Data = default,
        Message = message
    };
}

public sealed class AuthenticateRequest
{
    public string SerialNumber { get; set; } = "";
}

public sealed class TokenData
{
    public string AccessToken { get; init; } = "";
}

public sealed class CreatePaymentRequest
{
    public string SerialNumber { get; set; } = "";
    public int PaymentType { get; set; }
    public decimal TotalAmount { get; set; }
    public string? UniqueId { get; set; }
}

public sealed class MerchantInfoDto
{
    public string? MerchantName { get; init; }
    public string? MerchantAddress1 { get; init; }
    public string? PhoneNumber { get; init; }
    public string? CategoryCode { get; init; }
    public string? MerchantNo { get; init; }
    public string? Email { get; init; }
    public string? TaxNumber { get; init; }
}

public sealed class PaymentResponse
{
    public int PaymentType { get; init; }
    public string EndToEndId { get; init; } = "";
    public string UniqueId { get; init; } = "";
    public string QrCodeStr { get; init; } = "";
    public decimal TotalAmount { get; init; }
    public string SerialNumber { get; init; } = "";
    public string MerchantNo { get; init; } = "";
    public string TerminalNo { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public int StatusCode { get; init; }
    public string StatusDesc { get; init; } = "";
    public long RefundedPaymentId { get; init; }
    public string? RefundedUniqueId { get; init; }
    public string? RefundedEndToEndId { get; init; }
    public string? IpsStatus { get; init; }
    public bool IsCanceled { get; init; }
    public string? Description { get; init; }
    public MerchantInfoDto Merchant { get; init; } = new();
}

public sealed class HeaderInfoResponse
{
    public string SerialNumber { get; init; } = "";
    public IReadOnlyList<string> RrnPrefix { get; init; } = [];
    public RegisterInfoDto Register { get; init; } = new();
    public CompanyInfoDto Company { get; init; } = new();
    public MerchantInfoDto Merchant { get; init; } = new();
}

public sealed class RegisterInfoDto
{
    public string SerialNumber { get; init; } = "";
    public string? TerminalModel { get; init; }
    public string CountryCode { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string TerminalLanguageCode { get; init; } = "";
    public string ReceiptLanguageCode { get; init; } = "";
    public string TimeZone { get; init; } = "";
}

public sealed class CompanyInfoDto
{
    public string CompanyCode { get; init; } = "";
    public string CompanyName { get; init; } = "";
}
