using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users;
using contester.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class BeginSignUpByEmailCodeCommand : IRequest
{
    public string Email { get; set; } = null!;
}

public class BeginSignUpByEmailCodeCommandValidator : AbstractValidator<BeginSignUpByEmailCodeCommand>
{
    public BeginSignUpByEmailCodeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class BeginSignUpByEmailCodeCommandHandler(
    IValidator<BeginSignUpByEmailCodeCommand> validator,
    IConfigurationReaderService configuration,
    ApplicationDbContext context,
    IEmailService emailService,
    IAuthenticationService authenticationService) : IRequestHandler<BeginSignUpByEmailCodeCommand>
{
    public async Task Handle(BeginSignUpByEmailCodeCommand request, CancellationToken cancellationToken)
    {
        if (!configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Passwordless authentication is disabled");
        
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingUser = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser is not null)
        {
            if (existingUser.IsEmailConfirmed)
                throw new UserAlreadyExistsException();
            
            context.Users.Remove(existingUser);
        }
        
        var userRole = await context.UserRoles.AsNoTracking().FirstOrDefaultAsync(ur => ur.Name == "User", cancellationToken);
        if (userRole == null) throw new ApplicationException("User role \"User\" not found");

        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "",
            LastName = "",
            Patronymic = "",
            AdditionalInfo = "",
            EmailAuthenticationCode = Guid.NewGuid(),
            EmailAuthenticationCodeExpiresAt = DateTime.UtcNow + TimeSpan.FromHours(1),
            IsEmailConfirmed = false,
            UserRoleId = userRole.Id,
        };
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, Guid.NewGuid().ToString());
        
        await emailService.SendEmailAsync(request.Email, "SQL Contester Sign Up", 
            $"You can proceed sign up by following this link: {authenticationService.GetEmailSignUpUrl(user.Email, user.EmailAuthenticationCode.Value)}");
        
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
