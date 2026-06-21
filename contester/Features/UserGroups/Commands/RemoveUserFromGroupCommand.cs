using contester.Common.MediatR;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class RemoveUserFromGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageContestParticipants;
}

public class RemoveUserFromGroupCommandHandler(
    ApplicationDbContext context,
    IUserGroupService userGroupService) : IRequestHandler<RemoveUserFromGroupCommand, Unit>
{
    public async Task<Unit> Handle(RemoveUserFromGroupCommand request, CancellationToken ct)
    {
        await userGroupService.RemoveUserFromGroup(request.GroupId, request.UserId, ct);
        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
