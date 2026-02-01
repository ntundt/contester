using contester.Features.Common.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class BeginPasswordResetCommand : IRequest
{
    public string Email { get; set; } = null!;
}

public class BeginPasswordResetCommandValidator : AbstractValidator<BeginPasswordResetCommand>
{
    public BeginPasswordResetCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
}

public class BeginPasswordResetCommandHandler(
    ApplicationDbContext context,
    IEmailService emailService,
    IAuthenticationService authenticationService,
    IConfigurationReaderService configuration)
    : IRequestHandler<BeginPasswordResetCommand>
{
    public async Task Handle(BeginPasswordResetCommand begin, CancellationToken cancellationToken)
    {
        if (configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Password authentication is disabled");
        
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == begin.Email, cancellationToken);
        if (user is null) throw new UserNotFoundException();
        
        if (user.Email == configuration.GetAdminUserEmail())
            throw new NotifyUserException("Password recovery is not available for this user. Please change the application configuration.");

        user.PasswordRecoveryToken = Guid.NewGuid();
        user.PasswordRecoveryTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        
        await context.SaveChangesAsync(cancellationToken);
        
        await emailService.SendEmailAsync(user.Email, "Password recovery", 
            $"Please follow this link to reset your password: {authenticationService.GetPasswordRecoveryUrl(user.PasswordRecoveryToken.Value)}");
    }
}
