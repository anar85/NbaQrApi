using NbaQrApi.Data;
using NbaQrApi.Models;

namespace NbaQrApi.Services;

public interface ITerminalService
{
    Task<Terminal?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken);
}

public sealed class TerminalService : ITerminalService
{
    private readonly ITerminalRepository _terminals;

    public TerminalService(ITerminalRepository terminals)
    {
        _terminals = terminals;
    }

    public Task<Terminal?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken)
        => _terminals.GetBySerialNumberAsync(serialNumber, cancellationToken);
}
