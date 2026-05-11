using System.Data.Common;
using contester.Features.Common.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Databases;

namespace contester.Features.Attempts.Services;

public interface ISolutionRunnerService
{
    Task<SolutionExecutionResult>
        GetResult(string dbms, string schema, string solution, int priority, CancellationToken ct);
}

public class SolutionRunnerService(
    IConfiguration configuration,
    IConfigurationReaderService configurationReaderService) : ISolutionRunnerService
{
    public async Task<SolutionExecutionResult>
        GetResult(string dbms, string schema, string solution, int priority, CancellationToken ct)
    {
        var dbmsAdapter =  new DbmsAdapterFactory(configuration).CreateRandom(dbms);
        
        DbConnection? connection = null;
        DbCommand? command = null;
        DbDataReader? reader = null;
        string? error = null;
        await dbmsAdapter.GetLockAsync(priority);
        bool timeoutHit = false;
        try
        {

            try
            {
                await dbmsAdapter.DropCurrentSchemaAsync(ct);
                await dbmsAdapter.CreateSchemaTimeoutAsync(schema,
                    configurationReaderService.GetSchemaCreationExecutionTimeout(), ct);
            }
            catch (Exception e)
            {
                throw new NotifyUserException($"Could not prepare the execution environment: {e.Message}");
            }

            try
            {
                (connection, command, reader) = await dbmsAdapter.ExecuteQueryTimeoutAsync(solution,
                    configurationReaderService.GetSolutionExecutionTimeout(), ct);
            }
            catch (TimeoutException)
            {
                timeoutHit = true;
            }
            catch (DbException e)
            {
                error = e.Message;
            }

        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }

        /*
         * The caller is responsible for freeing the resources. The DbConnection, DbCommand and DbDataReader are passed
         * to the outside even in the case of an error for the caller to be able to Close() and Dispose() them.
         */
        return new SolutionExecutionResult(connection, command, reader, timeoutHit, error);
    }
}