using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Queries;

public class GetExpectedSolutionQuery : IRequest<ExpectedSolutionDto>, IAuthorizedRequest
{
    public Guid ProblemId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageAttempts;
}

public class GetExpectedSolutionQueryHandler(
    ApplicationDbContext dbContext,
    IFileService fileService)
    : IRequestHandler<GetExpectedSolutionQuery, ExpectedSolutionDto>
{
    public async Task<ExpectedSolutionDto> Handle(GetExpectedSolutionQuery request, CancellationToken cancellationToken)
    {
        var problem = await dbContext.Problems
            .Include(p => p.Contest)
            .Include(p => p.SchemaDescription)
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);
        if (problem is null)
            throw new EntityNotFoundException(typeof(Problem), request.ProblemId);
        
        return new ExpectedSolutionDto
        {
            ProblemId = problem.Id,
            Dbms = problem.SolutionDbms,
            Solution = await fileService.ReadApplicationDirectoryFileAllTextAsync(problem.SolutionPath, cancellationToken),
        };
    }
}
