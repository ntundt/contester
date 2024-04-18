using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Problems.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Problems.Queries;

public class GetExpectedSolutionQuery : IRequest<ExpectedSolutionDto>
{
    public Guid ProblemId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetExpectedSolutionQueryHandler : IRequestHandler<GetExpectedSolutionQuery, ExpectedSolutionDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;

    public GetExpectedSolutionQueryHandler(ApplicationDbContext dbContext, IMapper mapper, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _permissionService = permissionService;
    }

    public async Task<ExpectedSolutionDto> Handle(GetExpectedSolutionQuery request, CancellationToken cancellationToken)
    {
        var problem = await _dbContext.Problems
            .Include(p => p.Contest)
            .Include(p => p.SchemaDescription)
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageAttempts, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageAttempts);
        }

        return new ExpectedSolutionDto
        {
            ProblemId = problem.Id,
            Dbms = problem.SolutionDbms,
            Solution = await File.ReadAllTextAsync(problem.SolutionPath, cancellationToken),
        };
    }
}