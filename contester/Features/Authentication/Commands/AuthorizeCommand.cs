using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users.Exceptions;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class AuthorizeCommand : IRequest<AuthorizeCommandResult>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthorizeCommandResult
{
    public string Token { get; set; } = null!;
}

public class AuthorizeCommandValidator : AbstractValidator<AuthorizeCommand>
{
    public AuthorizeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class AuthorizeCommandHandler(
    ApplicationDbContext context,
    IAuthenticationService authenticationService,
    IJwtService jwtService)
    : IRequestHandler<AuthorizeCommand, AuthorizeCommandResult>
{
    public async Task<AuthorizeCommandResult> Handle(AuthorizeCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        if (!user.IsEmailConfirmed)
        {
            throw new EmailNotConfirmedException();
        }
        
        if (!authenticationService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidPasswordException();
        }

        user.LastLogin = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        
        var token = jwtService.GenerateJwtToken(user.Id.ToString(), user.UserRole.Name);
        return new AuthorizeCommandResult
        {
            Token = token,
        };
    }
}