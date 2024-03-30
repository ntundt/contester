using diploma.Data;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CanManageGradeAdjustmentsQuery : IRequest<bool>
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
}

public class CanManageGradeAdjustmentsQueryHandler : IRequestHandler<CanManageGradeAdjustmentsQuery, bool>
{
    private readonly ApplicationDbContext _context;
    private readonly IPermissionService _permissionService;

    public CanManageGradeAdjustmentsQueryHandler(ApplicationDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<bool> Handle(CanManageGradeAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests.AsNoTracking()
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