using System.Globalization;
using System.Text.RegularExpressions;

namespace NbaQrApi.AzQr;

/// <summary>
/// Builds merchant/customer presented AZQR payloads per IPS-FS-00-07 (EMV QRCPS TLV + CRC-16).
/// </summary>
public static class AzQrPayloadBuilder
{
    public const string StaticQr = "11";
    public const string DynamicQr = "12";
    public const string MerchantPresented = "MPV002";
    public const string CustomerPresented = "CPV002";
    public const string ReturnAttribute = "RT";

    private static readonly HashSet<string> TerminalTypesRequiringTerminalNo = ["02", "03", "04", "05", "06"];
    private static readonly HashSet<string> AliasTypes = ["01", "02", "03", "04", "05", "06", "07"];
    private static readonly Regex Hex32 = new("^[0-9a-fA-F]{32}$", RegexOptions.Compiled);
    private static readonly Regex Digits = new("^[0-9]+$", RegexOptions.Compiled);
    private static readonly Regex Date14 = new("^[0-9]{14}$", RegexOptions.Compiled);

    public static string Build(AzQrPayload payload)
    {
        Validate(payload);

        var parts = new List<string>
        {
            TlvEncoder.Primitive("39", payload.IpsSpecVersion),
            TlvEncoder.Primitive("40", payload.IpsUuid),
            TlvEncoder.Primitive("00", payload.EmvVersion),
            TlvEncoder.Primitive("01", payload.QrCodeType),
            BuildMain(payload.Main),
            BuildPresenter(payload.Presenter)
        };

        if (!string.IsNullOrEmpty(payload.Coordinates))
        {
            parts.Add(TlvEncoder.Primitive("28", payload.Coordinates));
        }

        parts.Add(BuildIps(payload.Ips));

        if (payload.Additional is not null)
        {
            parts.Add(BuildAdditionalInfo(payload.Additional));
        }

        if (payload.Billing is not null)
        {
            parts.Add(BuildBilling(payload.Billing));
        }

        parts.Add(TlvEncoder.Primitive("52", payload.MerchantCategoryCode));
        parts.Add(TlvEncoder.Primitive("53", payload.CurrencyNumericCode));

        if (!string.IsNullOrEmpty(payload.Amount))
        {
            parts.Add(TlvEncoder.Primitive("54", payload.Amount));
        }

        if (!string.IsNullOrEmpty(payload.TipFeeType))
        {
            parts.Add(TlvEncoder.Primitive("55", payload.TipFeeType));
        }

        if (!string.IsNullOrEmpty(payload.FixedConvenienceFee))
        {
            parts.Add(TlvEncoder.Primitive("56", payload.FixedConvenienceFee));
        }

        if (!string.IsNullOrEmpty(payload.ConvenienceFeePercent))
        {
            parts.Add(TlvEncoder.Primitive("57", payload.ConvenienceFeePercent));
        }

        parts.Add(TlvEncoder.Primitive("58", payload.CountryCode));
        parts.Add(TlvEncoder.Primitive("59", payload.MerchantName));
        parts.Add(TlvEncoder.Primitive("60", payload.City));

        if (!string.IsNullOrEmpty(payload.PostalCode))
        {
            parts.Add(TlvEncoder.Primitive("61", payload.PostalCode));
        }

        if (payload.AdditionalData is not null)
        {
            parts.Add(BuildAdditionalData(payload.AdditionalData));
        }

        if (payload.AltLanguage is not null)
        {
            parts.Add(BuildAltLanguage(payload.AltLanguage));
        }

        return Crc16Ccitt.AppendChecksum(string.Concat(parts));
    }

    public static string FormatAmount(decimal amount)
        => amount.ToString("0.00", CultureInfo.InvariantCulture);

    public static string FormatCoordinates(decimal latitude, decimal longitude)
    {
        static string EightDigits(decimal value)
        {
            var scaled = Math.Abs(value).ToString("000.000000", CultureInfo.InvariantCulture).Replace(".", "");
            if (scaled.Length > 8)
            {
                scaled = scaled[..8];
            }

            return scaled.PadLeft(8, '0');
        }

        return EightDigits(latitude) + EightDigits(longitude);
    }

    private static string BuildMain(AzQrMainData main)
        => TlvEncoder.Template(
            "26",
            TlvEncoder.Primitive("00", main.Version),
            TlvEncoder.PrimitiveOrNull("03", main.UniqueIdentifier),
            TlvEncoder.Primitive("04", main.TerminalType),
            TlvEncoder.PrimitiveOrNull("06", main.GeneratedAt),
            TlvEncoder.PrimitiveOrNull("07", main.ExpiresAt));

    private static string BuildPresenter(AzQrPresenter presenter)
        => TlvEncoder.Template(
            "27",
            TlvEncoder.Primitive("00", presenter.AliasType),
            TlvEncoder.Primitive("01", presenter.AliasValue),
            TlvEncoder.PrimitiveOrNull("02", presenter.BankBic),
            TlvEncoder.PrimitiveOrNull("03", presenter.ObjectIdentifier));

    private static string BuildIps(AzQrIpsSpecific ips)
        => TlvEncoder.Template(
            "36",
            TlvEncoder.PrimitiveOrNull("00", ips.ProviderBic),
            TlvEncoder.Primitive("01", ips.OperationCode),
            TlvEncoder.Primitive("02", ips.TransactionType),
            TlvEncoder.PrimitiveOrNull("03", ips.OrderNumber),
            TlvEncoder.PrimitiveOrNull("04", ips.AuthenticationValue),
            TlvEncoder.PrimitiveOrNull("05", ips.Email));

    private static string BuildAdditionalInfo(AzQrAdditionalInfo info)
        => TlvEncoder.Template(
            "37",
            TlvEncoder.PrimitiveOrNull("04", info.CustomerLoyaltyNumber),
            TlvEncoder.PrimitiveOrNull("06", info.CustomerNumber),
            TlvEncoder.PrimitiveOrNull("07", info.TerminalNumber),
            TlvEncoder.PrimitiveOrNull("10", info.Pin),
            TlvEncoder.PrimitiveOrNull("11", info.ReturnAttribute));

    private static string BuildBilling(AzQrBilling billing)
        => TlvEncoder.Template(
            "38",
            TlvEncoder.Primitive("00", billing.OrganizationCode),
            TlvEncoder.PrimitiveOrNull("01", billing.InvoiceType),
            TlvEncoder.PrimitiveOrNull("02", billing.InvoiceNumber),
            TlvEncoder.PrimitiveOrNull("03", billing.PayerType),
            TlvEncoder.PrimitiveOrNull("04", billing.PayerCode));

    private static string BuildAdditionalData(AzQrAdditionalData data)
        => TlvEncoder.Template(
            "62",
            TlvEncoder.Primitive("03", data.BranchName),
            TlvEncoder.PrimitiveOrNull("08", data.PaymentInformation),
            TlvEncoder.PrimitiveOrNull("09", data.ConsumerInfoQuery),
            TlvEncoder.PrimitiveOrNull("10", data.TaxNumber),
            TlvEncoder.PrimitiveOrNull("11", data.DeliveryChannel));

    private static string BuildAltLanguage(AzQrAltLanguage alt)
        => TlvEncoder.Template(
            "64",
            TlvEncoder.Primitive("00", alt.LanguageCode),
            TlvEncoder.Primitive("01", alt.MerchantName),
            TlvEncoder.PrimitiveOrNull("02", alt.City));

    private static void Validate(AzQrPayload payload)
    {
        if (payload.IpsSpecVersion is not MerchantPresented and not CustomerPresented)
        {
            throw new AzQrValidationException("IPS 39 must be MPV002 or CPV002.");
        }

        if (!Hex32.IsMatch(payload.IpsUuid))
        {
            throw new AzQrValidationException("IPS 40 must be a 32-character UUID without hyphens.");
        }

        if (payload.EmvVersion.Length != 2 || !Digits.IsMatch(payload.EmvVersion))
        {
            throw new AzQrValidationException("EMV 00 must be 2 digits.");
        }

        if (payload.QrCodeType is not StaticQr and not DynamicQr)
        {
            throw new AzQrValidationException("EMV 01 must be 11 (static) or 12 (dynamic).");
        }

        if (payload.QrCodeType == DynamicQr && string.IsNullOrEmpty(payload.Main.UniqueIdentifier))
        {
            throw new AzQrValidationException("IPS 26-03 is mandatory for dynamic QR.");
        }

        if (payload.Main.UniqueIdentifier is { Length: > 32 })
        {
            throw new AzQrValidationException("IPS 26-03 max length is 32.");
        }

        if (!Digits.IsMatch(payload.Main.TerminalType) || payload.Main.TerminalType.Length != 2)
        {
            throw new AzQrValidationException("IPS 26-04 terminal type must be 2 digits.");
        }

        RequireDate(payload.Main.GeneratedAt, "26-06");
        RequireDate(payload.Main.ExpiresAt, "26-07");

        if (!AliasTypes.Contains(payload.Presenter.AliasType))
        {
            throw new AzQrValidationException("IPS 27-00 alias type must be 01-07.");
        }

        if (payload.Presenter.AliasValue.Length > 35)
        {
            throw new AzQrValidationException("IPS 27-01 max length is 35.");
        }

        if (payload.Presenter.AliasType == "04" && string.IsNullOrEmpty(payload.Presenter.BankBic))
        {
            throw new AzQrValidationException("IPS 27-02 BIC is required when alias type is 04 (IBAN).");
        }

        if (payload.Ips.OperationCode.Length is 0 or > 10)
        {
            throw new AzQrValidationException("IPS 36-01 operation code is mandatory, max 10.");
        }

        if (payload.Ips.TransactionType.Length != 3 || !Digits.IsMatch(payload.Ips.TransactionType))
        {
            throw new AzQrValidationException("IPS 36-02 transaction type must be 3 digits.");
        }

        if (payload.Coordinates is not null && (payload.Coordinates.Length != 16 || !Digits.IsMatch(payload.Coordinates)))
        {
            throw new AzQrValidationException("IPS 28 must be 16 digits (lat+lon without decimal point).");
        }

        if (payload.Additional is not null)
        {
            var info = payload.Additional;
            if (!string.IsNullOrEmpty(info.Pin) && !string.IsNullOrEmpty(info.ReturnAttribute))
            {
                throw new AzQrValidationException("IPS 37-10 PIN and 37-11 return attribute cannot be set together.");
            }

            if (!string.IsNullOrEmpty(info.ReturnAttribute) && info.ReturnAttribute != ReturnAttribute)
            {
                throw new AzQrValidationException("IPS 37-11 must be RT.");
            }

            if (info.Pin is { Length: not 7 })
            {
                throw new AzQrValidationException("IPS 37-10 PIN must be 7 characters.");
            }

            if (TerminalTypesRequiringTerminalNo.Contains(payload.Main.TerminalType)
                && string.IsNullOrEmpty(info.TerminalNumber))
            {
                throw new AzQrValidationException("IPS 37-07 terminal number is required for terminal types 02-06.");
            }
        }
        else if (TerminalTypesRequiringTerminalNo.Contains(payload.Main.TerminalType))
        {
            throw new AzQrValidationException("IPS 37 is required when terminal type is 02-06 (terminal number).");
        }

        if (payload.MerchantCategoryCode.Length != 4 || !Digits.IsMatch(payload.MerchantCategoryCode))
        {
            throw new AzQrValidationException("IPS 52 MCC must be 4 digits.");
        }

        if (payload.CurrencyNumericCode.Length != 3 || !Digits.IsMatch(payload.CurrencyNumericCode))
        {
            throw new AzQrValidationException("IPS 53 currency must be 3 digits.");
        }

        if (payload.Amount is { Length: > 13 })
        {
            throw new AzQrValidationException("IPS 54 amount max length is 13.");
        }

        if (payload.TipFeeType == "02" && string.IsNullOrEmpty(payload.FixedConvenienceFee))
        {
            throw new AzQrValidationException("IPS 56 is required when tip type is 02.");
        }

        if (payload.TipFeeType == "03" && string.IsNullOrEmpty(payload.ConvenienceFeePercent))
        {
            throw new AzQrValidationException("IPS 57 is required when tip type is 03.");
        }

        if (payload.CountryCode.Length != 2 || !payload.CountryCode.All(char.IsAsciiLetterUpper))
        {
            throw new AzQrValidationException("IPS 58 must be ISO 3166-1 alpha-2.");
        }

        RequireMax(payload.MerchantName, 25, "59");
        RequireMax(payload.City, 15, "60");
        RequireMax(payload.PostalCode, 10, "61");

        if (payload.AdditionalData is not null)
        {
            RequireMax(payload.AdditionalData.BranchName, 25, "62-03");
            RequireMax(payload.AdditionalData.PaymentInformation, 35, "62-08");
            RequireMax(payload.AdditionalData.ConsumerInfoQuery, 3, "62-09");
            if (payload.AdditionalData.TaxNumber is { Length: > 0 } and not { Length: 10 })
            {
                throw new AzQrValidationException("IPS 62-10 TIN must be 10 characters.");
            }

            if (payload.AdditionalData.DeliveryChannel is { Length: > 0 } channel
                && (channel.Length != 3 || !Digits.IsMatch(channel)))
            {
                throw new AzQrValidationException("IPS 62-11 delivery channel must be 3 digits.");
            }
        }

        if (payload.AltLanguage is not null)
        {
            if (payload.AltLanguage.LanguageCode.Length != 2)
            {
                throw new AzQrValidationException("IPS 64-00 language code must be 2 letters.");
            }

            RequireMax(payload.AltLanguage.MerchantName, 25, "64-01");
            RequireMax(payload.AltLanguage.City, 15, "64-02");
        }

        if (payload.Billing is not null && payload.Billing.OrganizationCode.Length != 6)
        {
            throw new AzQrValidationException("IPS 38-00 organization code must be 6 characters.");
        }
    }

    private static void RequireDate(string? value, string field)
    {
        if (value is not null && !Date14.IsMatch(value))
        {
            throw new AzQrValidationException($"IPS {field} must be YYYYMMDDhhmmss.");
        }
    }

    private static void RequireMax(string? value, int max, string field)
    {
        if (value is { Length: var n } && n > max)
        {
            throw new AzQrValidationException($"IPS {field} max length is {max}.");
        }
    }
}
