using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using MediatR;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Commands;

public class AuthorizeCommand : IRequest<AuthorizeCommandResult>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthorizeCommandResult
{
    public string Token { get; set; } = null!;
}

public class AuthorizeCommandValidator : AbstractValidator<AuthorizeCommand>
{
    public AuthorizeCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class AuthorizeCommandHandler : IRequestHandler<AuthorizeCommand, AuthorizeCommandResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthenticationService _authenticationService;
    private readonly IJwtService _jwtService;
    
    public AuthorizeCommandHandler(ApplicationDbContext context, IAuthenticationService authenticationService, IJwtService jwtService)
    {
        _context = context;
        _authenticationService = authenticationService;
        _jwtService = jwtService;
    }
    
    public async Task<AuthorizeCommandResult> Handle(AuthorizeCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        if (!user.IsEmailConfirmed)
        {
            throw new EmailNotConfirmedException();
        }
        
        if (!_authenticationService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidPasswordException();
        }

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        
        var token = _jwtService.GenerateJwtToken(user.Id.ToString(), user.UserRole.Name);
        return new AuthorizeCommandResult
        {
            Token = token,
        };
    }
}