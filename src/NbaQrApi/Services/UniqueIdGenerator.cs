using System.Security.Cryptography;
using NbaQrApi.Data;

namespace NbaQrApi.Services;

public interface IUniqueIdGenerator
{
    Task<string> NextAsync(string prefix, CancellationToken cancellationToken);
}

public sealed class UniqueIdGenerator : IUniqueIdGenerator
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly IQrPaymentRepository _payments;

    public UniqueIdGenerator(IQrPaymentRepository payments)
    {
        _payments = payments;
    }

    public async Task<string> NextAsync(string prefix, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var candidate = prefix + RandomSuffix(9);
            if (!await _payments.UniqueIdExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not generate a unique RRN.");
    }

    private static string RandomSuffix(int length)
    {
        var chars = new char[length];
        var bytes = RandomNumberGenerator.GetBytes(length);
        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }
}
