using contester.Features.Authentication.Commands;
using contester.Features.Authentication.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignInResult = contester.Features.Authentication.Commands.Common.SignInResult;

namespace contester.Features.Authentication;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(IMediator mediator)
{
    [HttpPost("begin-password-sign-up")]
    public async Task<BeginSignUpByPasswordCommandResult> BeginSignUp(BeginSignUpByPasswordCommand command)
    {
        return await mediator.Send(command);
    }
    
    [HttpPost("finish-password-sign-up")]
    public async Task<SignInResult> ConfirmSignUp(FinishSignUpByPasswordCommand byPasswordCommand)
    {
        var result = await mediator.Send(byPasswordCommand);
        return result;
    }
    
    [HttpGet("password-sign-in")]
    public async Task<SignInResult> PasswordSignIn([FromQuery] SignInByPasswordCommand query)
    {
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset(BeginPasswordResetCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPost("password-reset")]
    public async Task<IActionResult> ResetPassword(FinishPasswordResetCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }

    [HttpGet("begin-passwordless-sign-up")]
    public async Task<IActionResult> BeginPasswordlessSignUp([FromQuery] string email)
    {
        var command = new BeginSignUpByEmailCodeCommand { Email = email };
        await mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPost("finish-passwordless-sign-up")]
    public async Task<SignInResult> FinishPasswordlessSignUp([FromBody] FinishSignUpByEmailCodeCommand command)
    {
        return await mediator.Send(command);
    }

    [HttpPost("begin-passwordless-sign-in")]
    public async Task<IActionResult> BeginPasswordlessSignIn([FromBody] BeginSignInByEmailCodeCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }

    [HttpPost("finish-passwordless-sign-in")]
    public async Task<SignInResult> FinishPasswordlessSignIn([FromBody] FinishSignInByEmailCodeCommand command)
    {
        return await mediator.Send(command);
    }

    [HttpGet("renew-credentials")]
    public async Task<SignInResult> RenewCredentials([FromQuery] string refreshToken)
    {
        var command = new RenewCredentialsCommand { RefreshToken = refreshToken };
        return await mediator.Send(command);
    }

    [HttpGet("sign-in-or-sign-up-by-email")]
    public async Task<SignInOrSignUpByEmailCommandResult> SignInOrSignUpPrelude([FromQuery] string email)
    {
        var command = new SignInOrSignUpByEmailCommand { Email = email };
        return await mediator.Send(command);
    }

    [HttpGet("email-confirmation-link")]
    public async Task<GetEmailConfirmationLinkByCodeQueryResult> GetEmailConfirmationLinkByCode(
        [FromQuery] GetEmailConfirmationLinkByCodeQuery query)
    {
        return await mediator.Send(query);
    }
}
