using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using diploma.Services;
using Oracle.ManagedDataAccess.Client;
using Npgsql;

namespace diploma.Application;

public class DbmsAdapterFactory
{
    private readonly IConfiguration _configuration;
    private readonly ConfigurationReaderService _configurationReaderService;

    public DbmsAdapterFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _configurationReaderService = new ConfigurationReaderService(configuration);
    }
    
    public IDbmsAdapter Create(string dbmsType)
    {
        var connectionString = _configuration.GetConnectionString(dbmsType);
        if (connectionString is null) throw new ApplicationException($"Dbms type {dbmsType} does not have a connection string");

        var dbmsAdapterTypes = Assembly.GetAssembly(typeof(DbmsAdapter))!.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(DbmsAdapter)));
        
        var dbmsAdapterType = dbmsAdapterTypes.FirstOrDefault(type => type.Name == $"{dbmsType}Adapter");
        
        DbConnection connection = dbmsType switch
        {
            "SqlServer" => new SqlConnection(connectionString),
            "Oracle" => new OracleConnection(connectionString),
            "Postgres" => new NpgsqlConnection(connectionString),
            _ => throw new Exception($"Unknown dbms {dbmsType}")
        };
        
        if (dbmsAdapterType == default)
        {
            throw new Exception($"Dbms adapter for {dbmsType} is not implemented");
        }

        return dbmsType switch
        {
            // ConnectionString stored inside a DbConnection does not contain the password, which we need to establish
            // a connection with sqlplus
            "Oracle" => (IDbmsAdapter)Activator.CreateInstance(dbmsAdapterType, connection,
                _configurationReaderService.GetSqlPlus(), connectionString)!,
            _ => (IDbmsAdapter)Activator.CreateInstance(dbmsAdapterType, connection)!
        };
    }
}
