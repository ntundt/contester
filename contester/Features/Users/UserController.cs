using contester.Features.Users.Commands;
using contester.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.Users;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController(
    IMediator mediator,
    Authentication.Services.IAuthorizationService authorizationService)
{
    [HttpGet("my-permissions")]
    [ResponseCache(Duration = 60 * 60 * 24)]
    public async Task<GetPermissionsQueryResult> GetMyPermissions([FromQuery] GetPermissionsQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpGet]
    public async Task<UserDto> GetMyInfo()
    {
        var query = new GetUserInfoQuery
        {
            Id = authorizationService.GetUserId(),
        };
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpGet("{userId:guid}")]
    public async Task<UserDto> GetUserInfo([FromRoute] Guid userId)
    {
        var query = new GetUserInfoQuery
        {
            Id = userId,
        };
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPut("my-info")]
    public async Task<IActionResult> UpdateMyInfo([FromBody] UpdateUserInfoCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }

    [HttpGet("search")]
    public async Task<IEnumerable<UserDto>> SearchUsers([FromQuery] SearchUsersQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }

    [HttpGet("can-manage-grade-adjustments")]
    //[ResponseCache(Duration = 60 * 60 * 24)]
    public async Task<bool> CanManageGradeAdjustments([FromQuery] CanManageGradeAdjustmentsQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }

    [HttpGet("all")]
    public async Task<List<AdminPanelUserDto>> GetAllUsers([FromQuery] GetAllUsersQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<IActionResult> SetUserRole([FromRoute] Guid userId, [FromBody] SetUserRoleCommand command)
    {
        command.UserId = userId;
        await mediator.Send(command);
        return new OkResult();
    }

    [HttpPut("{userId:guid}/password")]
    public async Task<IActionResult> ResetUserPassword([FromRoute] Guid userId, [FromBody] ResetUserPasswordCommand command)
    {
        command.UserId = userId;
        await mediator.Send(command);
        return new OkResult();
    }
}
