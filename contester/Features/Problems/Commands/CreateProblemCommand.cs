using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Scoreboard.Services;
using contester.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Commands;

public class CreateProblemCommand : IRequest<ProblemDto>
{
    public Guid CallerId { get; set; }
    public string Name { get; set; } = null!;
    public string Statement { get; set; } = null!;
    public bool OrderMatters { get; set; }
    public decimal FloatMaxDelta { get; set; }
    public bool CaseSensitive { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public int MaxGrade { get; set; }
    public Guid ContestId { get; set; }
    public Guid? SchemaDescriptionId { get; set; } = null!;
    public string Solution { get; set; } = null!;
    public string SolutionDbms { get; set; } = null!;
}

public class CreateProblemCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IPermissionService permissionService,
    IFileService fileService,
    ScoreboardUpdateNotifier notifier)
    : IRequestHandler<CreateProblemCommand, ProblemDto>
{
    public async Task<ProblemDto> Handle(CreateProblemCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageProblems, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageProblems);
        }

        int ordinal;
        try
        {
            ordinal = await context.Problems.AsNoTracking()
                .Where(p => p.ContestId == request.ContestId)
                .Select(p => p.Ordinal)
                .MaxAsync(cancellationToken) + 1;
        }
        catch
        {
            ordinal = 1;
        }

        var schemaDescriptionId = request.SchemaDescriptionId ?? await context.SchemaDescriptions.AsNoTracking()
            .Where(sd => sd.ContestId == request.ContestId)
            .Select(sd => sd.Id)
            .FirstOrDefaultAsync(cancellationToken);
        
        var problem = new Problem
        {
            Name = request.Name,
            OrderMatters = request.OrderMatters,
            FloatMaxDelta = request.FloatMaxDelta,
            CaseSensitive = request.CaseSensitive,
            TimeLimit = request.TimeLimit,
            MaxGrade = request.MaxGrade,
            Ordinal = ordinal,
            ContestId = request.ContestId,
            SchemaDescriptionId = schemaDescriptionId,
            SolutionDbms = request.SolutionDbms,
        };
        problem.Id = Guid.NewGuid();
        problem.StatementPath = directoryService.GetProblemStatementRelativePath(problem.Id);
        problem.SolutionPath = directoryService.GetProblemSolutionRelativePath(problem.Id, request.SolutionDbms);
        await fileService.SaveProblemStatementToFileAsync(problem.Id, request.Statement, cancellationToken);
        await fileService.SaveProblemSolutionToFileAsync(problem.Id, request.SolutionDbms, request.Solution, cancellationToken);
        await context.Problems.AddAsync(problem, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        await context.RefreshScoreboardEntriesAsync();

        await notifier.SendScoreboardUpdate(problem.ContestId);
        
        return mapper.Map<ProblemDto>(problem);
    }
}
