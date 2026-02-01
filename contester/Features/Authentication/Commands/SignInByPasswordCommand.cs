using contester.Features.Common.Exceptions;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users;
using contester.Features.Users.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SignInResult = contester.Features.Authentication.Commands.Common.SignInResult;

namespace contester.Features.Authentication.Commands;

public class SignInByPasswordCommand : IRequest<SignInResult>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class SignInByPasswordCommandValidator : AbstractValidator<SignInByPasswordCommand>
{
    public SignInByPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class SignInByPasswordCommandHandler(
    ApplicationDbContext context,
    IAuthenticationService authenticationService,
    IConfigurationReaderService configuration,
    IValidator<SignInByPasswordCommand> validator)
    : IRequestHandler<SignInByPasswordCommand, SignInResult>
{
    public async Task<SignInResult> Handle(SignInByPasswordCommand request, CancellationToken cancellationToken)
    {
        if (configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Password authentication is disabled");
        
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var user = await context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
            throw new UserNotFoundException();
        
        if (user.PasswordHash is null)
            throw new InvalidPasswordException();
        
        if (!user.IsEmailConfirmed)
            throw new EmailNotConfirmedException();
        
        var hasher = new PasswordHasher<User>();
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) != PasswordVerificationResult.Success)
            throw new InvalidPasswordException();

        user.LastLogin = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        
        return authenticationService.IssueCredentials(user.Id, user.UserRole.Name);
    }
}
