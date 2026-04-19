using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Commands;

public class CreateProblemCommand : IRequest<ProblemDto>, IAuthorizedRequest
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
    public Constants.Permission RequiredPermission { get; set; } =  Constants.Permission.ManageProblems;
}

public class CreateProblemCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IFileService fileService,
    ScoreboardUpdateNotifier notifier,
    ScoreboardService scoreboardService)
    : IRequestHandler<CreateProblemCommand, ProblemDto>
{
    public async Task<ProblemDto> Handle(CreateProblemCommand request, CancellationToken cancellationToken)
    {
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

        if (schemaDescriptionId == default)
            throw new NotifyUserException("There are no schemas. Please create at least one schema first");
        
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
        
        await scoreboardService.RefreshScoreboardEntriesAsync(problem.ContestId);

        await notifier.SendScoreboardUpdate(problem.ContestId);
        
        return mapper.Map<ProblemDto>(problem);
    }
}
