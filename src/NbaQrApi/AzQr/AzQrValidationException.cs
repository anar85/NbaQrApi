namespace NbaQrApi.AzQr;

public sealed class AzQrValidationException : Exception
{
    public AzQrValidationException(string message) : base(message)
    {
    }
}
