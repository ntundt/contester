using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.UserGroups.Exceptions;
using contester.Features.UserGroups.Services;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class AddGroupToGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid ChildGroupId { get; set; }
    public Guid ParentGroupId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageContestParticipants;
}

public class AddGroupToGroupCommandHandler(
    IUserGroupService userGroupService) : IRequestHandler<AddGroupToGroupCommand, Unit>
{
    public async Task<Unit> Handle(AddGroupToGroupCommand request, CancellationToken ct)
    {
        try
        {
            await userGroupService.AddGroupToGroup(request.ParentGroupId, request.ChildGroupId, ct);
        }
        catch (RecursiveGroupMembershipException)
        {
            throw new NotifyUserException("Recursive group membership is not allowed");
        }

        return Unit.Value;
    }
}
