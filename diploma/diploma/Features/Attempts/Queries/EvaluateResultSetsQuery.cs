using diploma.Application.Extensions;
using diploma.Data;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Attempts.Queries;

public class EvaluateResultSetsQuery : IRequest<EvaluateResultSetsQueryResult>
{
    public Guid AttemptId { get; set; }
    public Guid CallerId { get; set; }
}

public class EvaluateResultSetsQueryResult
{
    public required string DeclineReason { get; set; }
    public required ResultSet ExpectedResult { get; set; }
    public required ResultSet ActualResult { get; set; }
}

public class EvaluateResultSetsQueryHandler : IRequestHandler<EvaluateResultSetsQuery, EvaluateResultSetsQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ISolutionCheckerService _solutionCheckerService;
    
    public EvaluateResultSetsQueryHandler(ApplicationDbContext context, ISolutionCheckerService solutionCheckerService)
    {
        _context = context;
        _solutionCheckerService = solutionCheckerService;
    }
    
    public async Task<EvaluateResultSetsQueryResult> Handle(EvaluateResultSetsQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts.AsNoTracking()
            .FirstOrDefaultAsync(attempt => attempt.Id == request.AttemptId, cancellationToken);

        if (attempt == default)
        {
            throw new ApplicationException($"Attempt {request.AttemptId} not found");
        }
        
        var problemId = attempt.ProblemId;
        
        var (expectedResult, (actualResult, error)) = await TaskEx.WhenAll(
            _solutionCheckerService.GetExpectedSolutionResult(problemId, cancellationToken),
            _solutionCheckerService.GetSolutionResult(attempt.Id, cancellationToken)
        );
        
        if (error != null)
        {
            return new EvaluateResultSetsQueryResult()
            {
                ActualResult = new ResultSet(),
                ExpectedResult = ResultSetEvaluator.EvaluateResultSet(expectedResult),
                DeclineReason = error,
            };
        }

        return new EvaluateResultSetsQueryResult()
        {
            ActualResult = ResultSetEvaluator.EvaluateResultSet(actualResult),
            ExpectedResult = ResultSetEvaluator.EvaluateResultSet(expectedResult),
            DeclineReason = string.Empty,
        };
    }
}
