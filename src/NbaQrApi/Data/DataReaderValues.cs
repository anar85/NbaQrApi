using System.Data.Common;
using System.Globalization;
using Oracle.ManagedDataAccess.Types;

namespace NbaQrApi.Data;

internal static class DataReaderValues
{
    public static string GetString(DbDataReader reader, string column)
        => GetStringOrNull(reader, column) ?? "";

    public static string? GetStringOrNull(DbDataReader reader, string column)
    {
        var value = GetValueOrNull(reader, column);
        return value switch
        {
            null => null,
            string text => text,
            OracleClob clob => clob.Value,
            char[] chars => new string(chars),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    public static int GetInt32(DbDataReader reader, string column)
        => ToInt32(GetRequiredValue(reader, column));

    public static int? GetInt32OrNull(DbDataReader reader, string column)
    {
        var value = GetValueOrNull(reader, column);
        return value is null ? null : ToInt32(value);
    }

    public static long GetInt64(DbDataReader reader, string column)
        => ToInt64(GetRequiredValue(reader, column));

    public static long? GetInt64OrNull(DbDataReader reader, string column)
    {
        var value = GetValueOrNull(reader, column);
        return value is null ? null : ToInt64(value);
    }

    public static decimal GetDecimal(DbDataReader reader, string column)
        => ToDecimal(GetRequiredValue(reader, column));

    public static bool GetBoolean(DbDataReader reader, string column)
    {
        var value = GetRequiredValue(reader, column);
        return value switch
        {
            bool boolValue => boolValue,
            string text => text.Trim() switch
            {
                "1" => true,
                "0" => false,
                var boolText => bool.Parse(boolText)
            },
            _ => ToInt32(value) != 0
        };
    }

    public static Guid? GetGuidOrNull(DbDataReader reader, string column)
    {
        var value = GetValueOrNull(reader, column);
        if (value is null)
        {
            return null;
        }

        if (value is byte[] bytes && bytes.Length == 16)
        {
            return new Guid(bytes);
        }

        var text = GetStringOrNull(reader, column);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return Guid.ParseExact(text.Replace("-", "", StringComparison.Ordinal), "N");
    }

    public static int GetInt32Output(OracleParameter parameter)
        => ToInt32(GetRequiredParameterValue(parameter));

    public static long GetInt64Output(OracleParameter parameter)
        => ToInt64(GetRequiredParameterValue(parameter));

    private static object GetRequiredValue(DbDataReader reader, string column)
        => GetValueOrNull(reader, column) ?? throw new InvalidOperationException($"Column '{column}' is null.");

    private static object? GetValueOrNull(DbDataReader reader, string column)
    {
        var index = reader.GetOrdinal(column);
        return reader.IsDBNull(index) ? null : reader.GetValue(index);
    }

    private static object GetRequiredParameterValue(OracleParameter parameter)
        => parameter.Value is null or DBNull
            ? throw new InvalidOperationException($"Output parameter '{parameter.ParameterName}' is null.")
            : parameter.Value;

    private static int ToInt32(object value)
        => decimal.ToInt32(ToDecimal(value));

    private static long ToInt64(object value)
        => decimal.ToInt64(ToDecimal(value));

    private static decimal ToDecimal(object value)
        => value switch
        {
            OracleDecimal oracleDecimal => oracleDecimal.Value,
            decimal decimalValue => decimalValue,
            int intValue => intValue,
            long longValue => longValue,
            short shortValue => shortValue,
            byte byteValue => byteValue,
            _ => Convert.ToDecimal(value, CultureInfo.InvariantCulture)
        };
}
