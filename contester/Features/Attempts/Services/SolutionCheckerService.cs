namespace contester.Features.Attempts.Services;

public interface ISolutionCheckerService
{
    Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken);   
}

public class SolutionCheckerService(
    ISolutionRunnerService solutionRunner,
    IAttemptExecutionContextFactory attemptExecutionContextFactory)
    : ISolutionCheckerService
{
    private static readonly SemaphoreSlim ConcurrentEvaluationsSemaphore = new(9, 9);

    public async Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var ctx = await attemptExecutionContextFactory.CreateAsync(attemptId, cancellationToken);
        
        /*
         * The solution is evaluated in the following steps:
         *  1. The etalon solution is run, DbDataReader is retrieved. The DbConnection is left open to be able to read
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
        
        try
        {
            await using var attemptExecutionResult =
                await solutionRunner.GetResult(ctx.AttemptDbms, ctx.AttemptSchema, ctx.AttemptSolution, 1, cancellationToken);

            if (attemptExecutionResult.Error != null)
                return (AttemptStatus.Error, attemptExecutionResult.Error);
            else if (attemptExecutionResult.TimeoutHit)
                return (AttemptStatus.TimeLimitExceeded, null);
            else if (attemptExecutionResult.Reader is null)
                throw new ApplicationException("The solution result is null");
            
            await using var etalonExecutionResult =
                await solutionRunner.GetResult(ctx.EtalonDbms, ctx.EtalonSchema, ctx.EtalonSolution, 2, cancellationToken);

            if (etalonExecutionResult.Reader is null)
                throw new ApplicationException("The etalon result is null");
            
            AttemptStatus result = ResultSetComparator.CompareRows(attemptExecutionResult.Reader, etalonExecutionResult.Reader,
                    ctx.Settings.FloatMaxDelta, ctx.Settings.CaseSensitive, ctx.Settings.OrderMatters) switch
                {
                    ResultSetComparisonResult.Equal => AttemptStatus.Accepted,
                    ResultSetComparisonResult.MismatchedRow => AttemptStatus.WrongAnswer,
                    ResultSetComparisonResult.MismatchedSchema => AttemptStatus.WrongOutputFormat,
                    _ => throw new ArgumentOutOfRangeException()
                };

            return (result, null);
        }
        finally
        {
            ConcurrentEvaluationsSemaphore.Release();
        }
    }
}
