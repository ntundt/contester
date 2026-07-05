using contester.Common;
using contester.Features.Authentication.Services;
using contester.Features.Common.Exceptions;
using contester.Features.Contests.Services;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Policies;

public class ProblemListAccessPolicy(
    IContestService contestService,
    IPermissionService permissionService,
    IUserGroupService userGroupService,
    ApplicationDbContext context
) : IResourceAccessPolicy<Guid, Contest>
{
    public async Task<bool> CanReadAsync(Guid userId, Guid resourceId, CancellationToken ct)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .Include(c => c.ParticipantsGroup)
            .FirstOrDefaultAsync(x => x.Id == resourceId, ct);
        if (contest is null)
            throw new EntityNotFoundException(typeof(Contest), resourceId);
        
        return await CanReadAsync(userId, contest, ct);
    }

    public async Task<bool> CanReadAsync(Guid userId, Contest resource, CancellationToken ct)
    {
        return (contestService.ContestGoingOn(resource) && await userGroupService.UserIsGroupMember(resource.ParticipantsGroupId, userId, ct))
               || (contestService.ContestGoingOn(resource) && resource.IsPublic)
               || resource.CommissionMembers.Any(cm => cm.Id == userId)
               || await permissionService.UserHasPermissionAsync(userId, Constants.Permission.ManageContests, ct);
    }

    public Task<bool> CanModifyAsync(Guid userId, Guid resourceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanModifyAsync(Guid userId, Contest resource, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
