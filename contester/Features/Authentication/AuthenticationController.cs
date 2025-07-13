using contester.Features.Authentication.Commands;
using contester.Features.Authentication.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.Authentication;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(IMediator mediator)
{
    [HttpPost("begin-sign-up")]
    public async Task<BeginSignUpCommandResult> BeginSignUp(BeginSignUpCommand command)
    {
        return await mediator.Send(command);
    }
    
    [HttpPost("confirm-sign-up")]
    public async Task<AuthorizeCommandResult> ConfirmSignUp(ConfirmSignUpCommand command)
    {
        var result = await mediator.Send(command);
        return result;
    }
    
    [HttpPost("begin-involuntary-sign-up")]
    public async Task<IActionResult> BeginInvoluntarySignUp(BeginInvoluntarySignUpCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }
    
    [HttpGet("sign-in")]
    public async Task<AuthorizeCommandResult> Authorize([FromQuery] AuthorizeCommand query)
    {
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }

    [HttpGet("email-confirmation-link")]
    public async Task<GetEmailConfirmationLinkByCodeQueryResult> GetEmailConfirmationLinkByCode(
        [FromQuery] GetEmailConfirmationLinkByCodeQuery query)
    {
        return await mediator.Send(query);
    }
}
