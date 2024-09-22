using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Problems.Exceptions;
using diploma.Services;
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
    private readonly IFileService _fileService;

    public GetExpectedSolutionQueryHandler(ApplicationDbContext dbContext, IMapper mapper, IPermissionService permissionService,
        IFileService fileService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _permissionService = permissionService;
        _fileService = fileService;
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
            Solution = await _fileService.ReadApplicationDirectoryFileAllTextAsync(problem.SolutionPath, cancellationToken),
        };
    }
}
