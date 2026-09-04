using System.Text;

namespace NbaQrApi.AzQr;

/// <summary>
/// ISO/IEC 13239 CRC-16 (poly 0x1021, init 0xFFFF) used by EMV QRCPS / AZQR field 63.
/// </summary>
public static class Crc16Ccitt
{
    public const string FieldId = "63";
    public const string LengthPrefix = "6304";

    public static string ComputeHex(string payloadIncludingCrcHeader)
    {
        var data = Encoding.UTF8.GetBytes(payloadIncludingCrcHeader);
        ushort crc = 0xFFFF;

        foreach (var b in data)
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ 0x1021);
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        return crc.ToString("X4");
    }

    public static string AppendChecksum(string payloadWithoutCrc)
    {
        var withHeader = payloadWithoutCrc + LengthPrefix;
        return withHeader + ComputeHex(withHeader);
    }
}
