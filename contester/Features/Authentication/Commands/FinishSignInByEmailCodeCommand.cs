using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Commands.Common;
using contester.Features.Authentication.Services;
using contester.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class FinishSignInByEmailCodeCommand : IRequest<SignInResult>
{
    public string Email { get; set; } = null!;
    public Guid EmailCode { get; set; }
}

public class FinishSignInByEmailCodeCommandValidator : AbstractValidator<FinishSignInByEmailCodeCommand>
{
    public FinishSignInByEmailCodeCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.EmailCode).NotEmpty();
    }
}

public class FinishSignInByEmailCodeCommandHandler(
    IConfigurationReaderService configuration,
    IValidator<FinishSignInByEmailCodeCommand> validator,
    ApplicationDbContext context,
    IAuthenticationService authenticationService) : IRequestHandler<FinishSignInByEmailCodeCommand, SignInResult>
{
    public async Task<SignInResult> Handle(FinishSignInByEmailCodeCommand request, CancellationToken cancellationToken)
    {
        if (!configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Passwordless authentication is disabled");
        
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null)
            throw new ApplicationException("User not found");

        if (user.EmailAuthenticationCode != request.EmailCode)
            throw new NotifyUserException("Email authentication code does not match");

        if (user.EmailAuthenticationCodeExpiresAt < DateTime.UtcNow)
            throw new NotifyUserException("Email authentication code has expired. Please request a new one.");

        user.EmailAuthenticationCodeExpiresAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return authenticationService.IssueCredentials(user.Id, user.UserRole.Name);
    }
}
