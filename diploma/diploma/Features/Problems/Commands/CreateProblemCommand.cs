using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Problems.Commands;

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

public class CreateProblemCommandHandler : IRequestHandler<CreateProblemCommand, ProblemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    
    public CreateProblemCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, IMapper mapper, IPermissionService permissionService)
    {
        _context = context;
        _directoryService = directoryService;
        _mapper = mapper;
        _permissionService = permissionService;
    }
    
    public async Task<ProblemDto> Handle(CreateProblemCommand request, CancellationToken cancellationToken)
    {
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageProblems, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageProblems);
        }

        int ordinal;
        try
        {
            ordinal = await _context.Problems.AsNoTracking()
                .Where(p => p.ContestId == request.ContestId)
                .Select(p => p.Ordinal)
                .MaxAsync(cancellationToken) + 1;
        }
        catch
        {
            ordinal = 1;
        }

        var schemaDescriptionId = request.SchemaDescriptionId ?? await _context.SchemaDescriptions.AsNoTracking()
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
        problem.StatementPath = _directoryService.GetProblemStatementPath(problem.Id);
        problem.SolutionPath = _directoryService.GetProblemSolutionPath(problem.Id, request.SolutionDbms);
        await _directoryService.SaveProblemStatementToFileAsync(problem.Id, request.Statement, cancellationToken);
        await _directoryService.SaveProblemSolutionToFileAsync(problem.Id, request.SolutionDbms, request.Solution, cancellationToken);
        await _context.Problems.AddAsync(problem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ProblemDto>(problem);
    }
}