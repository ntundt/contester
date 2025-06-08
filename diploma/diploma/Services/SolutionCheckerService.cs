using System.Data.Common;
using diploma.Application;
using diploma.Application.Extensions;
using diploma.Data;
using diploma.Features.Attempts;
using diploma.Features.Attempts.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace diploma.Services;

public interface ISolutionCheckerService
{
    Task<(DbConnection, DbCommand, DbDataReader)> GetExpectedSolutionResult(Guid problemId, CancellationToken cancellationToken);
    Task<(DbConnection? connection, DbCommand? command, DbDataReader? reader, string? error)> GetSolutionResult(
        Guid attemptId,
        CancellationToken cancellationToken);
    Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken);   
}

public class SolutionCheckerService(
    ApplicationDbContext context,
    IFileService fileService,
    IConfiguration configuration,
    IConfigurationReaderService configurationReaderService)
    : ISolutionCheckerService
{
    /*
     * Note: The caller is responsible for freeing the resources.
     */
    public async Task<(DbConnection, DbCommand, DbDataReader)> GetExpectedSolutionResult(Guid problemId, CancellationToken cancellationToken)
    {
        var problem = await context.Problems.AsNoTracking()
            .Include(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(p => p.Id == problemId, cancellationToken);
        if (problem is null) throw new ApplicationException("Problem with the GUID specified not found");

        var dbmsAdapter = new DbmsAdapterFactory(configuration).Create(problem.SolutionDbms);

        var schemaDescriptionFile = problem.SchemaDescription.Files.FirstOrDefault(f => f.Dbms == problem.SolutionDbms);
        if (schemaDescriptionFile is null) throw new ApplicationException("Schema description file for expected solution does not exist");
        
        var (schemaDescription, solution) = await TaskEx.WhenAll(
            fileService.ReadApplicationDirectoryFileAllTextAsync(schemaDescriptionFile.FilePath, cancellationToken),
            fileService.ReadApplicationDirectoryFileAllTextAsync(problem.SolutionPath, cancellationToken)
        );

        DbConnection connection;
        DbCommand command;
        DbDataReader reader;
        await dbmsAdapter.GetLockAsync(1);
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaTimeoutAsync(schemaDescription,
                configurationReaderService.GetSchemaCreationExecutionTimeout(), cancellationToken);
            (connection, command, reader) = await dbmsAdapter.ExecuteQueryTimeoutAsync(solution,
                configurationReaderService.GetSolutionExecutionTimeout(), cancellationToken);
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }

        return (connection, command, reader);
    }
    
    /*
     * Note: The caller is responsible for freeing the resources.
     */
    public async Task<(DbConnection? connection, DbCommand? command, DbDataReader? reader, string? error)>
        GetSolutionResult(Guid attemptId,
            CancellationToken cancellationToken)
    {
        var attempt = await context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .ThenInclude(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);
        if (attempt is null) throw new ApplicationException("Attempt with the GUID specified not found");

        var dbmsAdapter = new DbmsAdapterFactory(configuration).Create(attempt.Dbms);

        var schemaDescriptionFile = attempt.Problem.SchemaDescription.Files.FirstOrDefault(f => f.Dbms == attempt.Dbms);
        if (schemaDescriptionFile is null) throw new ApplicationException("Schema description file for solution's DBMS does not exist");
        
        var (schemaDescription, solution) = await TaskEx.WhenAll(
            fileService.ReadApplicationDirectoryFileAllTextAsync(schemaDescriptionFile.FilePath, cancellationToken),
            fileService.ReadApplicationDirectoryFileAllTextAsync(attempt.SolutionPath, cancellationToken)
        );

        DbConnection? connection = null;
        DbCommand? command = null;
        DbDataReader? reader = null;
        string? error = null;
        await dbmsAdapter.GetLockAsync(2);
        try
        {
            /*
             * Note: Error during schema drop or creation is treated as a `Syntax error` status.
             * TODO: fix
             */
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaTimeoutAsync(schemaDescription,
                configurationReaderService.GetSchemaCreationExecutionTimeout(), cancellationToken);
            (connection, command, reader) = await dbmsAdapter.ExecuteQueryTimeoutAsync(solution,
                configurationReaderService.GetSolutionExecutionTimeout(), cancellationToken);
        }
        catch (DbException e)
        {
            error = e.Message;
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }

        /*
         * The caller is responsible for freeing the resources. The DbConnection, DbCommand and DbDataReader are passed
         * to the outside even in the case of an error for the caller to be able to Close() and Dispose() them.
         */
        return (connection, command, reader, error);
    }

    private static readonly SemaphoreSlim ConcurrentEvaluationsSemaphore = new(40, 40);

    public async Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);
        if (attempt == null)
        {
            throw new ApplicationException("Attempt requested to be run does not exist");
        }

        /*
         * The solution is evaluated in the following steps:
         *  1. The ethalon solution is run, DbDataReader is retrieved. The DbConnection is left open to be able to read
         *     from the DbDataReader.
         *  2. The provided solution is run, DbDataReader is retrieved. The DbConnection is also left open.
         *  3. The DbDataReaders are read from and the results af the queries are compared.
         *  4. Freeing the resources produced by previous steps.
         *
         * The semaphore here is needed to prevent the situation where a hundred of solutions is left on the first step
         * of evaluation, occupying all the available database connections, and not a single one of them is able to
         * proceed to the second step.
         */
        await ConcurrentEvaluationsSemaphore.WaitAsync(cancellationToken);
        
        DbConnection? solutionConnection = null;
        DbCommand? solutionCommand = null;
        DbDataReader? solutionResult = null;
        DbConnection? ethalonConnection = null;
        DbCommand? ethalonCommand = null;
        DbDataReader? ethalonResult = null;
        try
        {
            (ethalonConnection, ethalonCommand, ethalonResult) =
                await GetExpectedSolutionResult(attempt.ProblemId, cancellationToken);
            string? error;
            try
            {
                (solutionConnection, solutionCommand, solutionResult, error) =
                    await GetSolutionResult(attemptId, cancellationToken);
            }
            catch (QueryExecutionTimeoutException)
            {
                return (AttemptStatus.TimeLimitExceeded, null);
            }

            if (error != null)
            {
                return (AttemptStatus.Error, error);
            }

            if (solutionResult is null)
                throw new ApplicationException("The solution result is null");

            (AttemptStatus, string?) result = ResultSetComparator.CompareRows(solutionResult, ethalonResult,
                    attempt.Problem.FloatMaxDelta,
                    attempt.Problem.CaseSensitive, attempt.Problem.OrderMatters) switch
                {
                    ResultSetComparisonResult.Equal => (AttemptStatus.Accepted, null),
                    ResultSetComparisonResult.MismatchedRow => (AttemptStatus.WrongAnswer, null),
                    ResultSetComparisonResult.MismatchedSchema => (AttemptStatus.WrongOutputFormat, null),
                    _ => throw new ArgumentOutOfRangeException()
                };

            return result;
        }
        finally
        {
            /*
             * Note: in PostgreSQL (at least), if there are multiple statements in a command, and the second or later
             * one of them contains a syntax error, it will throw upon attempt to close the connection. To mitigate
             * this, there are "ignore any DbException" try-catch blocks around every statement. This makes sure all
             * the resources are freed properly.
             */
            if (solutionConnection is not null)
                try
                {
                    await solutionConnection.CloseAsync();
                    await solutionConnection.DisposeAsync();
                } catch (DbException) { }

            if (solutionCommand is not null)
                try
                {
                    await solutionCommand.DisposeAsync();
                } catch (DbException) { }

            if (solutionResult is not null)
                try {
                    await solutionResult.CloseAsync();
                    await solutionResult.DisposeAsync();
                } catch (DbException) { }

            if (ethalonConnection is not null)
                try {
                    await ethalonConnection.CloseAsync();
                    await ethalonConnection.DisposeAsync();
                } catch (DbException) { }

            if (ethalonCommand is not null)
                try
                {
                    await ethalonCommand.DisposeAsync();
                } catch (DbException) { }

            if (ethalonResult is not null)
                try {
                    await ethalonResult.CloseAsync();
                    await ethalonResult.DisposeAsync();
                } catch (DbException) { }
            
            ConcurrentEvaluationsSemaphore.Release();
        }
    }
}
