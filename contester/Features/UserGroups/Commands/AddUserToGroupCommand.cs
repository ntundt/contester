using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class AddUserToGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid UserId { get; set; }
    public Guid GroupId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class AddUserToGroupCommandHandler(
    ApplicationDbContext context,
    IUserGroupService userGroupService) : IRequestHandler<AddUserToGroupCommand, Unit>
{
    public async Task<Unit> Handle(AddUserToGroupCommand request, CancellationToken ct)
    {
        await userGroupService.AddUserToGroup(request.GroupId, request.UserId, ct);
        await context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
