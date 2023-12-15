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
    private readonly IClaimService _claimService;

    public GetExpectedSolutionQueryHandler(ApplicationDbContext dbContext, IMapper mapper, IClaimService claimService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _claimService = claimService;
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
        
        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageAttempts", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageAttempts");
        }

        return new ExpectedSolutionDto
        {
            ProblemId = problem.Id,
            Dbms = problem.SolutionDbms,
            Solution = await File.ReadAllTextAsync(problem.SolutionPath, cancellationToken),
        };
    }
}