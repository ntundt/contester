using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using MediatR;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Queries;

public class AuthorizeQuery : IRequest<AuthorizeQueryResult>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthorizeQueryResult
{
    public string Token { get; set; } = null!;
}

public class AuthorizeQueryValidator : AbstractValidator<AuthorizeQuery>
{
    public AuthorizeQueryValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class AuthorizeQueryHandler : IRequestHandler<AuthorizeQuery, AuthorizeQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthenticationService _authenticationService;
    private readonly IJwtService _jwtService;
    
    public AuthorizeQueryHandler(ApplicationDbContext context, IAuthenticationService authenticationService, IJwtService jwtService)
    {
        _context = context;
        _authenticationService = authenticationService;
        _jwtService = jwtService;
    }
    
    public async Task<AuthorizeQueryResult> Handle(AuthorizeQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking()
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
        
        var token = _jwtService.GenerateJwtToken(user.Id.ToString(), user.UserRole.Name);
        return new AuthorizeQueryResult
        {
            Token = token,
        };
    }
}