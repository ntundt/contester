using contester.Features.Authentication.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Users.Queries;

public class CanManageGradeAdjustmentsQuery : IRequest<bool>
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
}

public class CanManageGradeAdjustmentsQueryHandler(ApplicationDbContext context)
    : IRequestHandler<CanManageGradeAdjustmentsQuery, bool>
{
    public async Task<bool> Handle(CanManageGradeAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            return false;
        }

        if (contest.CommissionMembers.Any(cm => cm.Id == request.CallerId))
        {
            return true;
        }

        return false;
    }
}