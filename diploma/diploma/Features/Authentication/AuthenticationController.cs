using diploma.Features.Authentication.Commands;
using diploma.Features.Authentication.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.Authentication;

[ApiController]
[Route("api/auth")]
public class AuthenticationController
{
    private readonly IMediator _mediator;
    
    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("begin-sign-up")]
    public async Task<IActionResult> BeginSignUp(BeginSignUpCommand command)
    {
        await _mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPost("confirm-sign-up")]
    public async Task<IActionResult> ConfirmSignUp(ConfirmSignUpCommand command)
    {
        await _mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPost("begin-involuntary-sign-up")]
    public async Task<IActionResult> BeginInvoluntarySignUp(BeginInvoluntarySignUpCommand command)
    {
        await _mediator.Send(command);
        return new OkResult();
    }
    
    [HttpGet("sign-in")]
    public async Task<AuthorizeQueryResult> Authorize([FromQuery] AuthorizeQuery query)
    {
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetCommand command)
    {
        await _mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
    {
        await _mediator.Send(command);
        return new OkResult();
    }
}