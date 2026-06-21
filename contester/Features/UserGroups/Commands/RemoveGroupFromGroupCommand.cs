using contester.Common.MediatR;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class RemoveGroupFromGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid ChildGroupId { get; set; }
    public Guid ParentGroupId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageContestParticipants;
}

public class RemoveGroupFromGroupCommandHandler(
    ApplicationDbContext context,
    IUserGroupService userGroupService) : IRequestHandler<RemoveGroupFromGroupCommand, Unit>
{
    public async Task<Unit> Handle(RemoveGroupFromGroupCommand request, CancellationToken ct)
    {
        await userGroupService.RemoveGroupFromGroup(request.ParentGroupId, request.ChildGroupId, ct);
        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
