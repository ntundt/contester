using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class RemoveGroupFromGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid ChildGroupId { get; set; }
    public Guid ParentGroupId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
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
