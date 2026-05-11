using contester.Common;
using contester.Features.Common.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Attempts.Services;

public interface IAttemptExecutionContextFactory
{
    Task<AttemptExecutionContext> CreateAsync(Guid attemptId, CancellationToken ct);
}

public class AttemptExecutionContextFactory(
    ApplicationDbContext context,
    IFileService fileService) : IAttemptExecutionContextFactory
{
    public async Task<AttemptExecutionContext> CreateAsync(Guid attemptId, CancellationToken ct)
    {
        var attempt = await context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .ThenInclude(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct)
            ?? throw new ApplicationException("Attempt not found");

        var attemptSchemaFile = attempt.Problem.SchemaDescription.Files
            .FirstOrDefault(f => f.Dbms == attempt.Dbms)
            ?? throw new NotifyUserException("Unsupported DBMS");

        var etalonSchemaFile = attempt.Problem.SchemaDescription.Files
            .FirstOrDefault(f => f.Dbms == attempt.Problem.SolutionDbms)
            ?? throw new NotifyUserException("Problem is corrupted");

        var (attemptSchema, attemptSolution, etalonSchema, etalonSolution) = await TaskEx.WhenAll(
            fileService.ReadApplicationDirectoryFileAllTextAsync(attemptSchemaFile.FilePath, ct),
            fileService.ReadApplicationDirectoryFileAllTextAsync(attempt.SolutionPath, ct),
            fileService.ReadApplicationDirectoryFileAllTextAsync(etalonSchemaFile.FilePath, ct),
            fileService.ReadApplicationDirectoryFileAllTextAsync(attempt.Problem.SolutionPath, ct)
        );

        return new AttemptExecutionContext
        {
            AttemptDbms = attempt.Dbms,
            AttemptSchema = attemptSchema,
            AttemptSolution = attemptSolution,
            EtalonDbms = attempt.Problem.SolutionDbms,
            EtalonSchema = etalonSchema,
            EtalonSolution = etalonSolution,
            Settings = new ProblemExecutionSettings
            {
                FloatMaxDelta = attempt.Problem.FloatMaxDelta,
                CaseSensitive = attempt.Problem.CaseSensitive,
                OrderMatters = attempt.Problem.OrderMatters
            }
        };
    }
}
