using contester.Common.Sieve;
using contester.Features.UserGroups.Commands;
using contester.Features.UserGroups.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace contester.Features.UserGroups;

[Authorize]
[ApiController]
[Route("api/user-group")]
public class UserGroupController(
    IMediator mediator,
    Authentication.Services.IAuthorizationService authorizationService)
{
    [HttpPost("{groupId:guid}/member-users/{userId:guid}")]
    public async Task<Unit> AddUserToGroup([FromRoute] Guid userId, [FromRoute] Guid groupId)
    {
        var command = new AddUserToGroupCommand
        {
            UserId = userId,
            GroupId = groupId,
            CallerId = authorizationService.GetUserId(),
        };
        return await mediator.Send(command);
    }

    [HttpPost("{parentGroupId:guid}/member-groups/{childGroupId:guid}")]
    public async Task<Unit> AddGroupToGroup([FromRoute] Guid parentGroupId, [FromRoute] Guid childGroupId)
    {
        var command = new AddGroupToGroupCommand
        {
            ParentGroupId = parentGroupId,
            ChildGroupId = childGroupId,
            CallerId = authorizationService.GetUserId(),
        };
        return await mediator.Send(command);
    }

    [HttpDelete("{groupId:guid}/member-users/{userId:guid}")]
    public async Task<Unit> RemoveUserFromGroup([FromRoute] Guid userId, [FromRoute] Guid groupId)
    {
        var command = new RemoveUserFromGroupCommand
        {
            UserId = userId,
            GroupId = groupId,
            CallerId = authorizationService.GetUserId(),
        };
        return await mediator.Send(command);
    }

    [HttpDelete("{parentGroupId:guid}/member-groups/{childGroupId:guid}")]
    public async Task<Unit> RemoveGroupFromGroup([FromRoute] Guid parentGroupId, [FromRoute] Guid childGroupId)
    {
        var command = new RemoveGroupFromGroupCommand
        {
            ParentGroupId = parentGroupId,
            ChildGroupId = childGroupId,
            CallerId = authorizationService.GetUserId(),
        };
        return await mediator.Send(command);
    }

    [HttpGet("{parentGroupId:guid}/members")]
    public async Task<List<PrincipalDto>> GetGroupMembers([FromRoute] Guid groupId)
    {
        var query = new GetGroupMembersQuery
        {
            GroupId = groupId,
        };
        return await mediator.Send(query);
    }

    [HttpGet("search")]
    public async Task<PaginatedResult<PrincipalDto>> SearchUserGroup([FromQuery] SieveModel? sieveModel)
    {
        var query = new GetUserGroupsQuery
        {
            SieveModel = sieveModel,
        };
        return await mediator.Send(query);
    }
}
