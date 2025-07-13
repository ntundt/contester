using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using diploma.Services;
using Oracle.ManagedDataAccess.Client;

namespace diploma.Application;

public class OracleAdapter : DbmsAdapter
{
    private readonly string _sqlplus;
    private readonly string _connectionString;
    public OracleAdapter(Func<DbConnection> connectionFactory, string sqlplus, string connectionString) : base(connectionFactory)
    {
        _sqlplus = sqlplus;
        _connectionString = connectionString;
    }

    protected override string GetVerifyDbmsAvailableCommandText()
    {
        return "select 1 from dual";
    }

    public override async Task CreateSchemaAsync(string description, CancellationToken cancellationToken)
    {
        var sqlPlusService = new SqlPlusService(_sqlplus, _connectionString);
        await sqlPlusService.ExecuteScript(description, cancellationToken);
    }

    private static async Task<string> GetDropCurrentSchemaSqlAsync()
    {
        return await File.ReadAllTextAsync("Assets/Scripts/Oracle/DropSchema.sql");
    }

    public override async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
        var sqlPlusService = new SqlPlusService(_sqlplus, _connectionString);
        await sqlPlusService.ExecuteScript(await GetDropCurrentSchemaSqlAsync(), cancellationToken);
    }
    
    private string PrepareQueryText(string sql)
    {
        return Regex.Replace(sql, ";\\s*$", "");
    }

    protected override async Task<(DbConnection, DbCommand, DbDataReader)> ExecuteQueryAsync(string query, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        
        var command = connection.CreateCommand();
        command.CommandText = PrepareQueryText(query);
        command.CommandTimeout = (int)timeout.TotalSeconds;
        try
        {
            var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);

            if (reader is not OracleDataReader oracleDataReader)
                throw new ApplicationException("Oracle data reader expected");
            oracleDataReader.SuppressGetDecimalInvalidCastException = true;
            return (connection, command, reader);
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException();
        }
    }
}
