using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace NbaQrApi.Data;

public interface IOracleConnectionFactory
{
    OracleConnection Create();
    OracleCommand CreateCommand(string procedureName, OracleConnection connection);
}

public sealed class OracleConnectionFactory : IOracleConnectionFactory
{
    private readonly string _connectionString;
    private readonly string _packageName;

    public OracleConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");
        _packageName = configuration["Oracle:PackageName"] ?? "NBA_QR_API_PKG";
    }

    public OracleConnection Create() => new(_connectionString);

    public OracleCommand CreateCommand(string procedureName, OracleConnection connection)
    {
        var command = connection.CreateCommand();
        command.BindByName = true;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandText = $"{_packageName}.{procedureName}";
        return command;
    }
}
