using System.Data.Common;
using System.Data.SqlClient;
using AutoMapper;
using diploma.Application;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Problems.Exceptions;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Problems.Commands;

public class UpdateProblemCommand : IRequest<ProblemDto>
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Statement { get; set; } = null!;
    public bool OrderMatters { get; set; }
    public decimal FloatMaxDelta { get; set; }
    public bool CaseSensitive { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public Guid SchemaDescriptionId { get; set; }
    public string Solution { get; set; } = null!;
    public string SolutionDbms { get; set; } = null!;
}

public class UpdateProblemCommandHandler : IRequestHandler<UpdateProblemCommand, ProblemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;
    private readonly IConfiguration _configuration;
    
    public UpdateProblemCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, IMapper mapper,
        IClaimService claimService, IConfiguration configuration)
    {
        _context = context;
        _directoryService = directoryService;
        _mapper = mapper;
        _claimService = claimService;
        _configuration = configuration;
    }
    
    public async Task<ProblemDto> Handle(UpdateProblemCommand request, CancellationToken cancellationToken)
    {
        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageProblems", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageProblems");
        }
        
        var problem = await _context.Problems
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
        problem.SchemaDescriptionId = request.SchemaDescriptionId;
        problem.SolutionDbms = request.SolutionDbms;
        problem.StatementPath = _directoryService.GetProblemStatementPath(problem.Id);
        problem.SolutionPath = _directoryService.GetProblemSolutionPath(problem.Id, request.SolutionDbms);

        var schemaFilePath = problem.SchemaDescription.Files.FirstOrDefault(f => f.Dbms == request.SolutionDbms)!
            .FilePath;
        var schema = await File.ReadAllTextAsync(schemaFilePath, cancellationToken);
        
        var dbmsAdapter = new DbmsAdapterFactory(_configuration).Create(request.SolutionDbms);

        try
        {
            await dbmsAdapter.GetLockAsync(cancellationToken);
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaAsync(schema, cancellationToken);
            await dbmsAdapter.ExecuteQueryAsync(request.Solution, cancellationToken);
        }
        catch (DbException e)
        {
            throw new ProblemSolutionInvalidException(e.Message);
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }
        
        await _directoryService.SaveProblemStatementToFileAsync(problem.Id, request.Statement, cancellationToken);
        await _directoryService.SaveProblemSolutionToFileAsync(problem.Id, request.SolutionDbms, request.Solution, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ProblemDto>(problem);
    }
}