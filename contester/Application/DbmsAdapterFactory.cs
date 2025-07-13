using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using contester.Services;
using Oracle.ManagedDataAccess.Client;
using Npgsql;

namespace contester.Application;

public class DbmsAdapterFactory(IConfiguration configuration)
{
    private readonly ConfigurationReaderService _configurationReaderService = new(configuration);
    
    public IDbmsAdapter CreateRandom(string dbmsType)
    {
        var pickedConnectionString = ConnectionStringsCache.Instance.GetRandomConnectionString(dbmsType);

        return CreateWithConnectionString(dbmsType, pickedConnectionString);
    }

    public IDbmsAdapter CreateWithConnectionString(string dbmsType, string connectionString)
    {
        Func<DbConnection> connectionFactory = dbmsType switch
        {
            "SqlServer" => () => new SqlConnection(connectionString),
            "Oracle"    => () => new OracleConnection(connectionString),
            "Postgres"  => () => new NpgsqlConnection(connectionString),
            _ => throw new Exception($"Unknown dbms {dbmsType}")
        };
        
        var dbmsAdapterTypes = Assembly.GetAssembly(typeof(DbmsAdapter))!.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(DbmsAdapter)));
        var dbmsAdapterType = dbmsAdapterTypes.FirstOrDefault(type => type.Name == $"{dbmsType}Adapter");
        
        if (dbmsAdapterType == default)
        {
            throw new Exception($"Dbms adapter for {dbmsType} is not implemented");
        }
        
        return dbmsType switch
        {
            // ConnectionString stored inside a DbConnection does not contain the password, which we need to establish
            // a connection with sqlplus
            "Oracle" => (IDbmsAdapter)Activator.CreateInstance(dbmsAdapterType, connectionFactory,
                _configurationReaderService.GetSqlPlus(), connectionString)!,
            _ => (IDbmsAdapter)Activator.CreateInstance(dbmsAdapterType, connectionFactory)!
        };
    }
}
