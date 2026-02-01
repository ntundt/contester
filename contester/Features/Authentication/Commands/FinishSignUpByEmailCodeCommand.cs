using contester.Features.Common.Exceptions;
using contester.Features.Authentication.Commands.Common;
using contester.Features.Authentication.Services;
using contester.Features.Users.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class FinishSignUpByEmailCodeCommand : IRequest<SignInResult>
{
    public string Email { get; set; } = null!;
    public Guid EmailCode { get; set; }
    
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string AdditionalInfo { get; set; } = null!;
}

public class FinishSignUpByEmailCodeCommandValidator : AbstractValidator<FinishSignUpByEmailCodeCommand>
{
    public FinishSignUpByEmailCodeCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.EmailCode).NotEmpty();
    }
}

public class FinishSignUpByEmailCodeCommandHandler(
    IConfigurationReaderService configuration,
    IValidator<FinishSignUpByEmailCodeCommand> validator,
    ApplicationDbContext context,
    IAuthenticationService authenticationService
) : IRequestHandler<FinishSignUpByEmailCodeCommand, SignInResult>
{
    public async Task<SignInResult> Handle(FinishSignUpByEmailCodeCommand request, CancellationToken cancellationToken)
    {
        if (!configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Passwordless authentication is disabled");
        
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var user = await context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
            throw new UserNotFoundException();

        if (user.EmailAuthenticationCode != request.EmailCode)
            throw new ApplicationException("Email code does not match");

        if (user.EmailAuthenticationCodeExpiresAt < DateTime.UtcNow)
            throw new NotifyUserException("Email authentication code has expired");
        
        user.IsEmailConfirmed = true;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Patronymic = request.Patronymic;
        user.AdditionalInfo = request.AdditionalInfo;
        user.LastLogin = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return authenticationService.IssueCredentials(user.Id, user.UserRole.Name);
    }
}
