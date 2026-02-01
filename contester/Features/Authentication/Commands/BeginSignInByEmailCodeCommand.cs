using contester.Features.Common.Exceptions;
using contester.Features.Authentication.Services;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class BeginSignInByEmailCodeCommand : IRequest
{
    public string Email { get; set; } = null!;
}

public class BeginSignInByEmailCodeCommandValidator : AbstractValidator<BeginSignInByEmailCodeCommand>
{
    public BeginSignInByEmailCodeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class BeginSignInByEmailCodeCommandHandler(
    ApplicationDbContext context,
    IValidator<BeginSignInByEmailCodeCommand> validator,
    IConfigurationReaderService configuration,
    IEmailService emailService,
    IAuthenticationService authenticationService) : IRequestHandler<BeginSignInByEmailCodeCommand>
{
    public async Task Handle(BeginSignInByEmailCodeCommand request, CancellationToken cancellationToken)
    {
        if (!configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Passwordless authentication is disabled");
        
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await context.Users
            .Where(u => u.Email == request.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new ApplicationException("User does not exist");

        user.EmailAuthenticationCode = Guid.NewGuid();
        user.EmailAuthenticationCodeExpiresAt = DateTime.UtcNow + TimeSpan.FromHours(1);
        
        await emailService.SendEmailAsync(request.Email, "SQL Contester Sign In",
            $"Please follow this link to sign in: {authenticationService.GetEmailSignInUrl(user.Email, user.EmailAuthenticationCode.Value)}");

        await context.SaveChangesAsync(cancellationToken);
    }
}
