using NbaQrApi.AzQr;
using NbaQrApi.Data;
using NbaQrApi.Models;

namespace NbaQrApi.Services;

public interface IQrPaymentService
{
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken);
    Task<PaymentResponse?> GetByUniqueIdAsync(string uniqueId, bool includeQr, CancellationToken cancellationToken);
    Task<PaymentResponse?> CancelAsync(string uniqueId, CancellationToken cancellationToken);
}

public sealed class QrPaymentService : IQrPaymentService
{
    private readonly ITerminalService _terminals;
    private readonly IQrPaymentRepository _payments;
    private readonly IUniqueIdGenerator _uniqueIds;

    public QrPaymentService(
        ITerminalService terminals,
        IQrPaymentRepository payments,
        IUniqueIdGenerator uniqueIds)
    {
        _terminals = terminals;
        _payments = payments;
        _uniqueIds = uniqueIds;
    }

    public async Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var terminal = await _terminals.GetBySerialNumberAsync(request.SerialNumber, cancellationToken)
            ?? throw new InvalidOperationException($"Terminal '{request.SerialNumber}' was not found.");

        return request.PaymentType switch
        {
            PaymentTypes.Sale => await CreateSaleAsync(terminal, request, cancellationToken),
            PaymentTypes.Refund => await CreateRefundAsync(terminal, request, cancellationToken),
            _ => throw new AzQrValidationException($"Unsupported paymentType '{request.PaymentType}'. Use 1 (sale) or 4 (refund).")
        };
    }

    public async Task<PaymentResponse?> GetByUniqueIdAsync(string uniqueId, bool includeQr, CancellationToken cancellationToken)
    {
        var payment = await _payments.GetByUniqueIdAsync(uniqueId, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        var terminal = await _terminals.GetBySerialNumberAsync(payment.SerialNumber, cancellationToken);
        return Map(payment, terminal, includeQr);
    }

    public async Task<PaymentResponse?> CancelAsync(string uniqueId, CancellationToken cancellationToken)
    {
        var payment = await _payments.GetByUniqueIdAsync(uniqueId, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        await _payments.UpdateStatusAsync(
            uniqueId,
            PaymentStatuses.CanceledByMerchant,
            "Cancelled by Merchant",
            isCanceled: true,
            ipsStatus: "Cancelled",
            description: "cancelledbyterminal",
            cancellationToken);

        payment.StatusCode = PaymentStatuses.CanceledByMerchant;
        payment.StatusDesc = "Cancelled by Merchant";
        payment.IsCanceled = true;
        payment.IpsStatus = "Cancelled";
        payment.Description = "cancelledbyterminal";

        var terminal = await _terminals.GetBySerialNumberAsync(payment.SerialNumber, cancellationToken);
        return Map(payment, terminal, includeQr: false);
    }

    private async Task<PaymentResponse> CreateSaleAsync(
        Terminal terminal,
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var uniqueId = await _uniqueIds.NextAsync(terminal.RrnPrefix, cancellationToken);
        var qr = AzQrPayloadBuilder.Build(ToPayload(terminal, uniqueId, request.TotalAmount, isRefund: false));

        var payment = NewPayment(terminal, uniqueId, request.PaymentType, request.TotalAmount, qr);
        payment.Id = await _payments.InsertAsync(payment, cancellationToken);
        return Map(payment, terminal, includeQr: true);
    }

    private async Task<PaymentResponse> CreateRefundAsync(
        Terminal terminal,
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UniqueId))
        {
            throw new AzQrValidationException("UniqueId of the original payment is required for refund.");
        }

        var original = await _payments.GetByUniqueIdAsync(request.UniqueId, cancellationToken)
            ?? throw new AzQrValidationException($"Original payment '{request.UniqueId}' was not found.");

        var uniqueId = await _uniqueIds.NextAsync(terminal.RrnPrefix, cancellationToken);
        var amount = request.TotalAmount > 0 ? request.TotalAmount : original.TotalAmount;
        var qr = AzQrPayloadBuilder.Build(ToPayload(terminal, uniqueId, amount, isRefund: true));

        var payment = NewPayment(terminal, uniqueId, PaymentTypes.Refund, amount, qr);
        payment.RefundedPaymentId = original.Id;
        payment.RefundedUniqueId = original.UniqueId;
        payment.RefundedEndToEndId = original.EndToEndId;
        payment.StatusCode = PaymentStatuses.RefundWaitProcess;
        payment.StatusDesc = PaymentStatuses.Describe(PaymentStatuses.RefundWaitProcess);
        payment.Id = await _payments.InsertAsync(payment, cancellationToken);
        return Map(payment, terminal, includeQr: true);
    }

    private static QrPayment NewPayment(Terminal terminal, string uniqueId, int paymentType, decimal amount, string qr)
        => new()
        {
            UniqueId = uniqueId,
            EndToEndId = uniqueId,
            SerialNumber = terminal.SerialNumber,
            PaymentType = paymentType,
            TotalAmount = amount,
            QrCodeStr = qr,
            StatusCode = PaymentStatuses.WaitInquiry,
            StatusDesc = PaymentStatuses.Describe(PaymentStatuses.WaitInquiry),
            MerchantNo = terminal.MerchantNo,
            TerminalNo = terminal.TerminalNo,
            CurrencyCode = terminal.CurrencyCode
        };

    internal static AzQrPayload ToPayload(Terminal terminal, string uniqueId, decimal amount, bool isRefund)
    {
        AzQrAltLanguage? alt = null;
        if (!string.IsNullOrWhiteSpace(terminal.AltLanguageCode) && !string.IsNullOrWhiteSpace(terminal.AltMerchantName))
        {
            alt = new AzQrAltLanguage
            {
                LanguageCode = terminal.AltLanguageCode,
                MerchantName = terminal.AltMerchantName,
                City = terminal.AltCity
            };
        }

        return new AzQrPayload
        {
            IpsSpecVersion = terminal.IpsSpecVersion,
            IpsUuid = terminal.IpsUuid.Replace("-", "", StringComparison.Ordinal),
            EmvVersion = "01",
            QrCodeType = AzQrPayloadBuilder.DynamicQr,
            Main = new AzQrMainData
            {
                Version = "01",
                UniqueIdentifier = uniqueId,
                TerminalType = terminal.TerminalType
            },
            Presenter = new AzQrPresenter
            {
                AliasType = terminal.AliasType,
                AliasValue = terminal.AliasValue,
                BankBic = terminal.BankBic,
                ObjectIdentifier = terminal.MerchantNo
            },
            Coordinates = string.IsNullOrWhiteSpace(terminal.Coordinates) ? null : terminal.Coordinates.Trim(),
            Ips = new AzQrIpsSpecific
            {
                ProviderBic = terminal.ProviderBic,
                OperationCode = terminal.OperationCode,
                TransactionType = terminal.TransactionType
            },
            Additional = new AzQrAdditionalInfo
            {
                TerminalNumber = terminal.SerialNumber,
                ReturnAttribute = isRefund ? AzQrPayloadBuilder.ReturnAttribute : null
            },
            MerchantCategoryCode = terminal.CategoryCode,
            CurrencyNumericCode = terminal.CurrencyNumericCode,
            Amount = amount > 0 ? AzQrPayloadBuilder.FormatAmount(amount) : null,
            TipFeeType = terminal.TipFeeType,
            FixedConvenienceFee = terminal.FixedConvenienceFee,
            ConvenienceFeePercent = terminal.ConvenienceFeePercent,
            CountryCode = terminal.CountryCode,
            MerchantName = terminal.MerchantName,
            City = terminal.City,
            PostalCode = terminal.PostalCode,
            AdditionalData = new AzQrAdditionalData
            {
                BranchName = terminal.BranchName,
                ConsumerInfoQuery = terminal.ConsumerInfoQuery,
                TaxNumber = terminal.TaxNumber,
                DeliveryChannel = terminal.DeliveryChannel
            },
            AltLanguage = alt
        };
    }

    private static PaymentResponse Map(QrPayment payment, Terminal? terminal, bool includeQr)
        => new()
        {
            PaymentType = payment.PaymentType,
            EndToEndId = payment.EndToEndId,
            UniqueId = payment.UniqueId,
            QrCodeStr = includeQr ? payment.QrCodeStr : "",
            TotalAmount = payment.TotalAmount,
            SerialNumber = payment.SerialNumber,
            MerchantNo = payment.MerchantNo,
            TerminalNo = payment.TerminalNo,
            CurrencyCode = payment.CurrencyCode,
            StatusCode = payment.StatusCode,
            StatusDesc = payment.StatusDesc,
            RefundedPaymentId = payment.RefundedPaymentId ?? 0,
            RefundedUniqueId = payment.RefundedUniqueId,
            RefundedEndToEndId = payment.RefundedEndToEndId,
            IpsStatus = payment.IpsStatus,
            IsCanceled = payment.IsCanceled,
            Description = payment.Description,
            Merchant = new MerchantInfoDto
            {
                MerchantName = terminal?.MerchantName,
                MerchantAddress1 = terminal?.MerchantAddress1,
                PhoneNumber = terminal?.PhoneNumber,
                CategoryCode = terminal?.CategoryCode,
                MerchantNo = terminal?.MerchantNo ?? payment.MerchantNo,
                Email = terminal?.Email,
                TaxNumber = terminal?.TaxNumber
            }
        };
}
