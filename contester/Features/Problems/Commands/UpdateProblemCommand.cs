using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Attempts.Services;
using contester.Features.Common.Exceptions;
using contester.Features.Problems.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Commands;

public class UpdateProblemCommand : IRequest<ProblemDto>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Statement { get; set; } = null!;
    public bool OrderMatters { get; set; }
    public decimal FloatMaxDelta { get; set; }
    public bool CaseSensitive { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public int MaxGrade { get; set; }
    public int Ordinal { get; set; }
    public Guid SchemaDescriptionId { get; set; }
    public string Solution { get; set; } = null!;
    public string SolutionDbms { get; set; } = null!;
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageProblems;
}

public class UpdateProblemCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IFileService fileService,
    IScoreboardService scoreboardService,
    ScoreboardUpdateNotifier notifier,
    ISolutionRunnerService solutionRunnerService)
    : IRequestHandler<UpdateProblemCommand, ProblemDto>
{
    public async Task<ProblemDto> Handle(UpdateProblemCommand request, CancellationToken cancellationToken)
    {
        var problem = await context.Problems
            .Include(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        problem.Name = request.Name;
        problem.OrderMatters = request.OrderMatters;
        problem.FloatMaxDelta = request.FloatMaxDelta;
        problem.CaseSensitive = request.CaseSensitive;
        problem.TimeLimit = request.TimeLimit;
        problem.MaxGrade = request.MaxGrade;
        problem.SchemaDescriptionId = request.SchemaDescriptionId;
        problem.SolutionDbms = request.SolutionDbms;
        problem.StatementPath = directoryService.GetProblemStatementRelativePath(problem.Id);
        problem.SolutionPath = directoryService.GetProblemSolutionRelativePath(problem.Id, request.SolutionDbms);

        var targetSchemaDescription = await context.SchemaDescriptions.AsNoTracking()
            .Include(sd => sd.Files)
            .FirstOrDefaultAsync(sd => sd.Id == request.SchemaDescriptionId, cancellationToken);
        if (targetSchemaDescription is null)
        {
            throw new NotifyUserException("Schema description specified not found");
        }

        var schemaFile = 
            targetSchemaDescription.Files.FirstOrDefault(f => f.Dbms == request.SolutionDbms) 
                ?? throw new NotifyUserException("Schema description for DBMS specified not found");
        var schema = await fileService.ReadApplicationDirectoryFileAllTextAsync(schemaFile.FilePath, cancellationToken);
        
        await using var runResult = await solutionRunnerService.GetResult(request.SolutionDbms, schema, request.Solution, 3, cancellationToken);
        
        if (runResult.TimeoutHit)
            throw new ProblemSolutionInvalidException("Solution execution timed out");
        if (!string.IsNullOrEmpty(runResult.Error))
            throw new ProblemSolutionInvalidException(runResult.Error);
        if (runResult.Reader is null)
            throw new ProblemSolutionInvalidException("Reader is null");
        
        await fileService.SaveProblemStatementToFileAsync(problem.Id, request.Statement, cancellationToken);
        await fileService.SaveProblemSolutionToFileAsync(problem.Id, request.SolutionDbms, request.Solution, cancellationToken);

        var oldOrdinal = problem.Ordinal;
        var newOrdinal = request.Ordinal;
        var problems = await context.Problems
            .Where(p => p.ContestId == problem.ContestId && p.Id != problem.Id)
            .OrderBy(p => p.Ordinal)
            .ToListAsync(cancellationToken);
        foreach (var p in problems)
        {
            if (oldOrdinal < newOrdinal)
            {
                if (p.Ordinal > oldOrdinal && p.Ordinal <= newOrdinal)
                {
                    p.Ordinal--;
                }
            }
            else
            {
                if (p.Ordinal < oldOrdinal && p.Ordinal >= newOrdinal)
                {
                    p.Ordinal++;
                }
            }
        }
        problem.Ordinal = request.Ordinal;

        await context.SaveChangesAsync(cancellationToken);

        await scoreboardService.RefreshScoreboardEntriesAsync(problem.ContestId, cancellationToken);
        await notifier.SendScoreboardUpdate(problem.ContestId);
        
        return mapper.Map<ProblemDto>(problem);
    }
}
