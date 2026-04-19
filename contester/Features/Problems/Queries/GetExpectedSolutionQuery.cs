using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Problems.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Queries;

public class GetExpectedSolutionQuery : IRequest<ExpectedSolutionDto>, IAuthorizedRequest
{
    public Guid ProblemId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageAttempts;
}

public class GetExpectedSolutionQueryHandler(
    ApplicationDbContext dbContext,
    IMapper mapper,
    IFileService fileService)
    : IRequestHandler<GetExpectedSolutionQuery, ExpectedSolutionDto>
{
    private readonly IMapper _mapper = mapper;

    public async Task<ExpectedSolutionDto> Handle(GetExpectedSolutionQuery request, CancellationToken cancellationToken)
    {
        var problem = await dbContext.Problems
            .Include(p => p.Contest)
            .Include(p => p.SchemaDescription)
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        
        return new ExpectedSolutionDto
        {
            ProblemId = problem.Id,
            Dbms = problem.SolutionDbms,
            Solution = await fileService.ReadApplicationDirectoryFileAllTextAsync(problem.SolutionPath, cancellationToken),
        };
    }
}
