using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;

namespace diploma.Application;

public class DbmsAdapterFactory
{
    private readonly IConfiguration _configuration;

    public DbmsAdapterFactory(IConfiguration configuration)
    {
        _configuration = configuration;
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
            _ => throw new Exception($"Unknown dbms {dbmsType}")
        };
        
        if (dbmsAdapterType != default)
        {
            return (IDbmsAdapter) Activator.CreateInstance(dbmsAdapterType, connection)!;
        }

        throw new Exception($"Dbms adapter for {dbmsType} is not implemented");
    }
}