using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Users;
using diploma.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Commands;

public class BeginInvoluntarySignUpCommand : IRequest
{
    public string Email { get; set; } = null!;
}

public class BeginInvoluntarySignUpCommandValidator : AbstractValidator<BeginInvoluntarySignUpCommand>
{
    public BeginInvoluntarySignUpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class BeginInvoluntarySignUpCommandHandler(
    ApplicationDbContext context,
    IAuthenticationService authenticationService,
    ILogger<RegisterCommandHandler> logger,
    IEmailService emailService)
    : IRequestHandler<BeginInvoluntarySignUpCommand>
{
    private ILogger<RegisterCommandHandler> _logger = logger;

    private async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
    
    private string GeneratePassword()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public async Task Handle(BeginInvoluntarySignUpCommand request, CancellationToken cancellationToken)
    {
        if (await UserExistsAsync(request.Email, cancellationToken))
        {
            throw new UserAlreadyExistsException();
        }
        
        var hasher = new PasswordHasher<User>();
        var generatedPassword = GeneratePassword();
        var userRole = await context.UserRoles.FirstOrDefaultAsync(ur => ur.Name == "User", cancellationToken);
        if (userRole == null) throw new ApplicationException("User role \"User\" not found");

        var user = new User
        {
            Email = request.Email,
            FirstName = "",
            LastName = "",
            Patronymic = "",
            AdditionalInfo = "",
            EmailConfirmationToken = Guid.NewGuid(),
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            IsEmailConfirmed = false,
            UserRoleId = userRole.Id,
        };
        user.PasswordHash = hasher.HashPassword(user, generatedPassword);

        await emailService.SendEmailAsync(user.Email, "Sign Up Confirmation",
            $"You were signed up to the system by the administrator. Your password is {generatedPassword}.\n" +
            $"To confirm the sign up, follow the link: {authenticationService.GetEmailConfirmationUrl(user.EmailConfirmationToken)}");
        
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
