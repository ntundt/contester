using System.Text.Json.Serialization;
using AutoMapper;
using contester.Features.Common.Exceptions;
using contester.Features.Contests;
using contester.Features.Contests.Policies;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Queries;

public class GetProblemsQuery : IRequest<GetProblemsQueryResult>
{
    public Guid ContestId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
}

public class GetProblemsQueryResult
{
    public List<ProblemDto> Problems { get; set; } = null!;
}

public class GetProblemsQueryHandler(
    ApplicationDbContext context,
    IMapper mapper,
    ProblemListAccessPolicy problemListAccessPolicy)
    : IRequestHandler<GetProblemsQuery, GetProblemsQueryResult>
{
    public async Task<GetProblemsQueryResult> Handle(GetProblemsQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.ParticipantsGroup)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        
        if (contest is null)
            throw new EntityNotFoundException(typeof(Contest), request.ContestId);

        if (!await problemListAccessPolicy.CanReadAsync(request.CallerId, contest.Id, cancellationToken))
            throw new NotifyUserException("You cannot view this contest's problems");
        
        var problems = await context.Problems.AsNoTracking()
            .Include(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files.Where(f => !f.HasProblems))
            .Where(p => p.ContestId == request.ContestId)
            .OrderBy(p => p.Ordinal)
            .ToListAsync(cancellationToken);
        return new GetProblemsQueryResult
        {
            Problems = mapper.Map<List<ProblemDto>>(problems)
        };
    }
}
