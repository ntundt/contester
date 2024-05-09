using System.Security.Claims;
using diploma.Features.Users.Commands;
using diploma.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.Users;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Authentication.Services.IAuthorizationService _authorizationService;
    
    public UserController(IMediator mediator, IHttpContextAccessor httpContextAccessor,
        Authentication.Services.IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }
    
    [HttpGet("my-permissions")]
    [ResponseCache(Duration = 60 * 60 * 24)]
    public async Task<GetPermissionsQueryResult> GetMyPermissions([FromQuery] GetPermissionsQuery query)
    {
        query.UserId = _authorizationService.GetUserId();
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpGet]
    public async Task<UserDto> GetMyInfo([FromQuery] GetUserInfoQuery query)
    {
        query.Id = query.CallerId = _authorizationService.GetUserId();
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpGet("{userId:guid}")]
    public async Task<UserDto> GetUserInfo([FromRoute] Guid userId)
    {
        var query = new GetUserInfoQuery
        {
            Id = userId,
            CallerId = _authorizationService.GetUserId()
        };
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpPut("my-info")]
    public async Task<IActionResult> UpdateMyInfo([FromBody] UpdateUserInfoCommand command)
    {
        command.CallerId = _authorizationService.GetUserId();
        await _mediator.Send(command);
        return new OkResult();
    }

    [HttpGet("search")]
    public async Task<IEnumerable<UserDto>> SearchUsers([FromQuery] SearchUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return result;
    }

    [HttpGet("can-manage-grade-adjustments")]
    //[ResponseCache(Duration = 60 * 60 * 24)]
    public async Task<bool> CanManageGradeAdjustments([FromQuery] CanManageGradeAdjustmentsQuery query)
    {
        query.CallerId = _authorizationService.GetUserId();
        var result = await _mediator.Send(query);
        return result;
    }

    [HttpGet("all")]
    public async Task<List<AdminPanelUserDto>> GetAllUsers([FromQuery] GetAllUsersQuery query)
    {
        query.CallerId = _authorizationService.GetUserId();
        var result = await _mediator.Send(query);
        return result;
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<IActionResult> SetUserRole([FromRoute] Guid userId, [FromBody] SetUserRoleCommand command)
    {
        command.UserId = userId;
        command.CallerId = _authorizationService.GetUserId();
        await _mediator.Send(command);
        return new OkResult();
    }

    [HttpPut("{userId:guid}/password")]
    public async Task<IActionResult> ResetUserPassword([FromRoute] Guid userId, [FromBody] ResetUserPasswordCommand command)
    {
        command.UserId = userId;
        command.CallerId = _authorizationService.GetUserId();
        await _mediator.Send(command);
        return new OkResult();
    }
}