using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Users;
using MediatR;
using diploma.Features.Authentication.Services;
using diploma.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Commands;

public class BeginSignUpCommand : IRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterCommandValidator : AbstractValidator<BeginSignUpCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<BeginSignUpCommand>
{
    private ApplicationDbContext _context;
    private IAuthenticationService _authenticationService;
    private ILogger<RegisterCommandHandler> _logger;
    private IEmailService _emailService;
    
    public RegisterCommandHandler(ApplicationDbContext context, IAuthenticationService authenticationService, ILogger<RegisterCommandHandler> logger,
        IEmailService emailService)
    {
        _context = context;
        _authenticationService = authenticationService;
        _logger = logger;
        _emailService = emailService;
    }
    
    private async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    private bool CheckPasswordRequirements(string password)
    {
        return password.Length >= 8 && password.Any(char.IsDigit) && password.Any(char.IsUpper) && password.Any(char.IsLower);
    }
    
    public async Task Handle(BeginSignUpCommand request, CancellationToken cancellationToken)
    {
        if (!CheckPasswordRequirements(request.Password))
        {
            throw new PasswordDoesNotMeetRequirementsException();
        }
        
        if (await UserExistsAsync(request.Email, cancellationToken))
        {
            var existingUser = await _context.Users.AsNoTracking().FirstAsync(u => u.Email == request.Email, cancellationToken);
            if (existingUser.IsEmailConfirmed)
            {
                throw new UserAlreadyExistsException();
            }
        }
        
        var userRole = await _context.UserRoles.AsNoTracking().FirstAsync(ur => ur.Name == "User", cancellationToken);
        if (userRole == null) throw new ApplicationException("User role \"User\" not found");

        var hasher = new PasswordHasher<User>();
        
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
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        
        await _emailService.SendEmailAsync(request.Email, "Sign Up Confirmation", $"To confirm the sign up, follow the link: {_authenticationService.GetEmailConfirmationUrl(user.EmailConfirmationToken)}");
        
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}