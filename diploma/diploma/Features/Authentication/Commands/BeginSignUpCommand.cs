using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Users;
using MediatR;
using diploma.Features.Authentication.Services;
using diploma.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;

namespace diploma.Features.Authentication.Commands;

public class BeginSignUpCommand : IRequest<BeginSignUpCommandResult>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class BeginSignUpCommandResult
{
    public Guid UserId { get; set; }
}

public class RegisterCommandValidator : AbstractValidator<BeginSignUpCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<BeginSignUpCommand, BeginSignUpCommandResult>
{
    private ApplicationDbContext _context;
    private IAuthenticationService _authenticationService;
    private ILogger<RegisterCommandHandler> _logger;
    private IEmailService _emailService;
    private IValidator<BeginSignUpCommand> _validator;
    
    public RegisterCommandHandler(ApplicationDbContext context, IAuthenticationService authenticationService,
        ILogger<RegisterCommandHandler> logger, IEmailService emailService, IValidator<BeginSignUpCommand> validator)
    {
        _context = context;
        _authenticationService = authenticationService;
        _logger = logger;
        _emailService = emailService;
        _validator = validator;
    }
    
    private async Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    private bool CheckPasswordRequirements(string password)
    {
        return password.Length >= 8 && password.Any(char.IsDigit) && password.Any(char.IsUpper) && password.Any(char.IsLower);
    }

    private static Random _random = new Random();

    private string GetRandomEmailConfirmationCode()
    {
        const string digits = "0123456789";
        const int length = 6;
        return new string(Enumerable.Repeat(digits, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
    
    public async Task<BeginSignUpCommandResult> Handle(BeginSignUpCommand request, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

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
            _context.Users.Remove(existingUser);
        }
        
        var userRole = await _context.UserRoles.AsNoTracking().FirstAsync(ur => ur.Name == "User", cancellationToken);
        if (userRole == null) throw new ApplicationException("User role \"User\" not found");

        var hasher = new PasswordHasher<User>();
        
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
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        
        await _emailService.SendEmailAsync(request.Email, "Email Confirmation", 
            $"Your code: {user.EmailConfirmationCode}. This code will expire in 15 minutes. \nYou can confirm the email by following the link as well: {_authenticationService.GetEmailConfirmationUrl(user.EmailConfirmationToken)}");
        
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new BeginSignUpCommandResult
        {
            UserId = user.Id
        };
    }
}