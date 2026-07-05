using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.UserGroups.Exceptions;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using FluentValidation;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class AddGroupToGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid ChildGroupId { get; set; }
    public Guid ParentGroupId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class AddGroupToGroupCommandValidator : AbstractValidator<AddGroupToGroupCommand>
{
    public AddGroupToGroupCommandValidator()
    {
        RuleFor(x => x.ChildGroupId).NotEmpty();
        RuleFor(x => x.ParentGroupId).NotEmpty();
        RuleFor(x => x.CallerId).NotEmpty();
        RuleFor(x => x.ChildGroupId).NotEqual(r => r.ParentGroupId);
    }
}

public class AddGroupToGroupCommandHandler(
    ApplicationDbContext context,
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

        await context.SaveChangesAsync(ct);
        
        return Unit.Value;
    }
}
