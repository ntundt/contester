using AutoMapper;
using diploma.Data;
using diploma.Features.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Scoreboard.Queries;

public class GetScoreboardApprovalStatusQuery : IRequest<GetScoreboardApprovalStatusQueryResult>
{
    public Guid ContestId { get; set; }
}

public class GetScoreboardApprovalStatusQueryResult
{
    public List<UserDto> ApprovedUsers { get; set; } = null!;
    public List<UserDto> NotApprovedUsers { get; set; } = null!;
}

public class GetScoreboardApprovalStatusQueryHandler(ApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetScoreboardApprovalStatusQuery, GetScoreboardApprovalStatusQueryResult>
{
    public async Task<GetScoreboardApprovalStatusQueryResult> Handle(GetScoreboardApprovalStatusQuery request, CancellationToken cancellationToken)
    {
        var commissionMembers = await context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .Where(c => c.Id == request.ContestId)
            .SelectMany(c => c.CommissionMembers)
            .ToListAsync(cancellationToken);

        var approvedUsers = await context.ScoreboardApprovals.AsNoTracking()
            .Where(a => a.ContestId == request.ContestId)
            .Select(a => a.ApprovingUser)
            .ToListAsync(cancellationToken);

        var notApprovedUsers = commissionMembers.Where(cm => approvedUsers.All(u => u.Id != cm.Id)).ToList();

        return new GetScoreboardApprovalStatusQueryResult
        {
            ApprovedUsers = mapper.Map<List<UserDto>>(approvedUsers),
            NotApprovedUsers = mapper.Map<List<UserDto>>(notApprovedUsers),
        };
    }
}
