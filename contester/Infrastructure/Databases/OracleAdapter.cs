using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace contester.Infrastructure.Databases;

public class OracleAdapter(Func<DbConnection> connectionFactory, string sqlplus, string connectionString)
    : DbmsAdapter(connectionFactory)
{
    protected override string GetVerifyDbmsAvailableCommandText()
    {
        return "select 1 from dual";
    }

    public override async Task CreateSchemaAsync(string description, CancellationToken cancellationToken)
    {
        var sqlPlusService = new SqlPlusService(sqlplus, connectionString);
        await sqlPlusService.ExecuteScript(description, cancellationToken);
    }

    private static async Task<string> GetDropCurrentSchemaSqlAsync()
    {
        return await File.ReadAllTextAsync("Assets/Scripts/Oracle/DropSchema.sql");
    }

    public override async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
        var sqlPlusService = new SqlPlusService(sqlplus, connectionString);
        await sqlPlusService.ExecuteScript(await GetDropCurrentSchemaSqlAsync(), cancellationToken);
    }
    
    private string PrepareQueryText(string sql)
    {
        return Regex.Replace(sql, ";\\s*$", "");
    }

    protected override async Task<(DbConnection, DbCommand, DbDataReader)> ExecuteQueryAsync(string query, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var connection = _connectionFactory();
        
        try
        {
            await connection.OpenAsync(cancellationToken);
            
            var command = connection.CreateCommand();
            command.CommandText = PrepareQueryText(query);
            command.CommandTimeout = (int)timeout.TotalSeconds;
        
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            var readerTask = command.ExecuteReaderAsync(CommandBehavior.CloseConnection, timeoutCts.Token);
            timeoutCts.Token.Register(command.Cancel);

            var timeoutTask = Task.Delay(timeout + TimeSpan.FromSeconds(1), cancellationToken);

            if (await Task.WhenAny(timeoutTask, readerTask) == timeoutTask)
            {
                await timeoutCts.CancelAsync();
                
                _ = readerTask.ContinueWith(
                    static t => _ = t.Exception,
                    TaskContinuationOptions.OnlyOnFaulted);

                await command.DisposeAsync();
                await connection.DisposeAsync();
                
                throw new TimeoutException();
            }
            
            var reader = await readerTask;
            
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
