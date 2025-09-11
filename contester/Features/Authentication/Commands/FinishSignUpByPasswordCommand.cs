using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Commands.Common;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users.Exceptions;
using contester.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class FinishSignUpByPasswordCommand : IRequest<SignInResult>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string AdditionalInfo { get; set; } = null!;
    public Guid Token { get; set; }
}

public class FinishSignUpByPasswordCommandValidator : AbstractValidator<FinishSignUpByPasswordCommand>
{
    public FinishSignUpByPasswordCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.AdditionalInfo).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class ConfirmSignUpCommandHandler(
    ApplicationDbContext context,
    IAuthenticationService authenticationService,
    IValidator<FinishSignUpByPasswordCommand> validator,
    IConfigurationReaderService configuration)
    : IRequestHandler<FinishSignUpByPasswordCommand, SignInResult>
{
    public async Task<SignInResult> Handle(FinishSignUpByPasswordCommand request, CancellationToken cancellationToken)
    {
        if (configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Password authentication is disabled");
        
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
        
        var user = await context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == request.Token, cancellationToken);
        if (user == null)
            throw new UserNotFoundException();
        
        if (user.IsEmailConfirmed)
            throw new EmailAlreadyConfirmedException();
        
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
