using contester.Common.MediatR;
using contester.Features.Attempts.Services;
using MediatR;

namespace contester.Features.Attempts.Queries;

public class EvaluateResultSetsQuery : IRequest<EvaluateResultSetsQueryResult>, IAuthorizedRequest
{
    public Guid AttemptId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageProblems;
}

public class EvaluateResultSetsQueryResult
{
    public required string DeclineReason { get; set; }
    public required ResultSet ExpectedResult { get; set; }
    public required ResultSet ActualResult { get; set; }
}

public class EvaluateResultSetsQueryHandler(
    ISolutionRunnerService solutionCheckerService,
    IAttemptExecutionContextFactory attemptExecutionContextFactory)
    : IRequestHandler<EvaluateResultSetsQuery, EvaluateResultSetsQueryResult>
{
    public async Task<EvaluateResultSetsQueryResult> Handle(EvaluateResultSetsQuery request, CancellationToken cancellationToken)
    {
        var ctx = await attemptExecutionContextFactory.CreateAsync(request.AttemptId, cancellationToken);

        await using var etalonExecutionResult =
            await solutionCheckerService.GetResult(ctx.EtalonDbms, ctx.EtalonSchema, ctx.EtalonSolution, 1, cancellationToken);
        await using var solutionExecutionResult =
            await solutionCheckerService.GetResult(ctx.AttemptDbms, ctx.AttemptSchema, ctx.AttemptSolution, 2, cancellationToken);

        if (etalonExecutionResult.Reader is null)
            throw new ApplicationException("Etalon result is null");
        
        if (solutionExecutionResult.Error is not null)
        {
            return new EvaluateResultSetsQueryResult
            {
                ActualResult = new ResultSet(),
                ExpectedResult = ResultSetEvaluator.EvaluateResultSet(etalonExecutionResult.Reader),
                DeclineReason = solutionExecutionResult.Error,
            };
        }

        if (solutionExecutionResult.Reader is null)
            throw new ApplicationException("Actual result is null");

        var resultSets = new EvaluateResultSetsQueryResult
        {
            ActualResult = ResultSetEvaluator.EvaluateResultSet(solutionExecutionResult.Reader),
            ExpectedResult = ResultSetEvaluator.EvaluateResultSet(etalonExecutionResult.Reader),
            DeclineReason = string.Empty,
        };
        
        return resultSets;
    }
}
