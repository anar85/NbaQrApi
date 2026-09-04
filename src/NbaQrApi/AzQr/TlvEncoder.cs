namespace NbaQrApi.AzQr;

/// <summary>
/// EMV / IPS AZQR TLV: ID (2 digits) + length (2 digits, character count 01-99) + value.
/// </summary>
public static class TlvEncoder
{
    public static string Primitive(string id, string value)
    {
        if (id.Length != 2 || !char.IsDigit(id[0]) || !char.IsDigit(id[1]))
        {
            throw new AzQrValidationException($"Field ID must be two digits: '{id}'.");
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new AzQrValidationException($"Field {id} value is required.");
        }

        if (value.Length > 99)
        {
            throw new AzQrValidationException($"Field {id} exceeds 99 characters (length={value.Length}).");
        }

        return id + value.Length.ToString("00") + value;
    }

    public static string? PrimitiveOrNull(string id, string? value)
        => string.IsNullOrEmpty(value) ? null : Primitive(id, value);

    public static string Template(string id, params string?[] children)
    {
        var inner = string.Concat(children.Where(c => !string.IsNullOrEmpty(c)));
        if (inner.Length == 0)
        {
            throw new AzQrValidationException($"Template {id} has no sub-fields.");
        }

        return Primitive(id, inner);
    }
}
