using contester.Features.Common.Exceptions;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;

namespace contester.Features.Authentication.Commands;

public class BeginSignUpByPasswordCommand : IRequest<BeginSignUpByPasswordCommandResult>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class BeginSignUpByPasswordCommandResult
{
    public Guid UserId { get; set; }
}

public class BeginSignUpByPasswordCommandValidator : AbstractValidator<BeginSignUpByPasswordCommand>
{
    public BeginSignUpByPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class BeginSignUpByPasswordCommandHandler(
    ApplicationDbContext context,
    IAuthenticationService authenticationService,
    IEmailService emailService,
    IValidator<BeginSignUpByPasswordCommand> validator,
    IConfigurationReaderService configuration)
    : IRequestHandler<BeginSignUpByPasswordCommand, BeginSignUpByPasswordCommandResult>
{
    private async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    private bool CheckPasswordRequirements(string password)
    {
        return password.Length >= 8 && password.Any(char.IsDigit) && password.Any(char.IsUpper) && password.Any(char.IsLower);
    }

    private static readonly Random Random = new();

    private string GetRandomEmailConfirmationCode()
    {
        const string digits = "0123456789";
        const int length = 6;
        return new string(Enumerable.Repeat(digits, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
    
    public async Task<BeginSignUpByPasswordCommandResult> Handle(BeginSignUpByPasswordCommand request, CancellationToken cancellationToken)
    {
        if (configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Password authentication is disabled");
        
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!CheckPasswordRequirements(request.Password))
            throw new PasswordDoesNotMeetRequirementsException();
        
        if (await UserExistsAsync(request.Email, cancellationToken))
        {
            var existingUser = await context.Users.AsNoTracking().FirstAsync(u => u.Email == request.Email, cancellationToken);
            if (existingUser.IsEmailConfirmed)
                throw new UserAlreadyExistsException();
            context.Users.Remove(existingUser);
        }
        
        var userRole = await context.UserRoles.AsNoTracking().FirstAsync(ur => ur.Name == "User", cancellationToken);
        if (userRole == null) throw new ApplicationException("User role \"User\" not found");
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = "",
            LastName = "",
            Patronymic = "",
            AdditionalInfo = "",
            EmailConfirmationToken = Guid.NewGuid(),
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            EmailConfirmationCode = GetRandomEmailConfirmationCode(),
            EmailConfirmationCodeExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsEmailConfirmed = false,
            UserRoleId = userRole.Id,
        };
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        
        await emailService.SendEmailAsync(request.Email, "SQL Contester Sign Up", 
            $"Your code: {user.EmailConfirmationCode}. This code will expire in 15 minutes. \nYou can confirm the email by following the link as well: {authenticationService.GetEmailConfirmationUrl(user.EmailConfirmationToken.Value)}");
        
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new BeginSignUpByPasswordCommandResult
        {
            UserId = user.Id
        };
    }
}