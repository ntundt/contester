using System.Data.Common;
using contester.Data;
using contester.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Attempts.Queries;

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

public class EvaluateResultSetsQueryHandler(
    ApplicationDbContext context,
    ISolutionCheckerService solutionCheckerService)
    : IRequestHandler<EvaluateResultSetsQuery, EvaluateResultSetsQueryResult>
{
    public async Task<EvaluateResultSetsQueryResult> Handle(EvaluateResultSetsQuery request, CancellationToken cancellationToken)
    {
        var attempt = await context.Attempts.AsNoTracking()
            .FirstOrDefaultAsync(attempt => attempt.Id == request.AttemptId, cancellationToken);

        if (attempt == default)
        {
            throw new ApplicationException($"Attempt {request.AttemptId} not found");
        }
        
        var problemId = attempt.ProblemId;

        DbConnection? solutionConnection = null;
        DbCommand? solutionCommand = null;
        DbDataReader? actualResult = null;
        DbConnection? ethalonConnection = null;
        DbCommand? ethalonCommand = null;
        DbDataReader? ethalonResult = null;
        try
        {
            (ethalonConnection, ethalonCommand, ethalonResult) = await solutionCheckerService.GetExpectedSolutionResult(problemId, cancellationToken);
            (solutionConnection, solutionCommand, actualResult, var error) = await solutionCheckerService.GetSolutionResult(attempt.Id, cancellationToken);

            if (error != null)
            {
                return new EvaluateResultSetsQueryResult()
                {
                    ActualResult = new ResultSet(),
                    ExpectedResult = ResultSetEvaluator.EvaluateResultSet(ethalonResult),
                    DeclineReason = error,
                };
            }

            if (actualResult is null)
                throw new ApplicationException("actualResult is null");

            var resultSets = new EvaluateResultSetsQueryResult()
            {
                ActualResult = ResultSetEvaluator.EvaluateResultSet(actualResult),
                ExpectedResult = ResultSetEvaluator.EvaluateResultSet(ethalonResult),
                DeclineReason = string.Empty,
            };
            
            return resultSets;
        }
        finally
        {
            /*
             * Refer to SolutionCheckerService.cs for explanation.
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
            if (actualResult is not null)
                try
                {
                    await actualResult.CloseAsync();
                    await actualResult.DisposeAsync();
                } catch (DbException) { }

            if (ethalonConnection is not null)
                try
                {
                    await ethalonConnection.CloseAsync();
                    await ethalonConnection.DisposeAsync();
                } catch (DbException) { }
            if (ethalonCommand is not null)
                try
                {
                    await ethalonCommand.DisposeAsync();
                } catch (DbException) { }
            if (ethalonResult is not null)
                try
                {
                    await ethalonResult.CloseAsync();
                    await ethalonResult.DisposeAsync();
                } catch (DbException) { }
        }
    }
}
