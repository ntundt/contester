using AutoMapper;
using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Contests.Exceptions;
using contester.Features.Contests.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Queries;

public class GetProblemsQuery : IRequest<GetProblemsQueryResult>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetProblemsQueryResult
{
    public List<ProblemDto> Problems { get; set; } = null!;
}

public class GetProblemsQueryHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IPermissionService permissionService,
    IContestService contestService)
    : IRequestHandler<GetProblemsQuery, GetProblemsQueryResult>
{
    public async Task<GetProblemsQueryResult> Handle(GetProblemsQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.Participants)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);

        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }

        if (!((contestService.ContestGoingOn(contest) && contest.Participants.Any(p => p.Id == request.CallerId))
              || (contestService.ContestGoingOn(contest) && contest.IsPublic)
              || contest.CommissionMembers.Any(cm => cm.Id == request.CallerId)
              || await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContests,
                  cancellationToken)))
        {
            throw new NotifyUserException("You cannot view this contest's problems");
        }

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