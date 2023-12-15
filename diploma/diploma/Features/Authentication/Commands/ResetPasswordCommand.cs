using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Users;
using diploma.Features.Users.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Commands;

public class ResetPasswordCommand : IRequest
{
    public Guid PasswordResetToken { get; set; }
    public string NewPassword { get; set; } = null!;
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(c => c.PasswordResetToken).NotEmpty();
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthenticationService _authenticationService;

    public ResetPasswordCommandHandler(ApplicationDbContext context, IAuthenticationService authenticationService)
    {
        _context = context;
        _authenticationService = authenticationService;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordRecoveryToken == request.PasswordResetToken, cancellationToken);
        if (user is null) throw new UserNotFoundException();

        if (user.PasswordRecoveryTokenExpiresAt < DateTime.Now)
        {
            throw new PasswordResetTokenExpiredException();
        }
        
        var hasher = new PasswordHasher<User>();
        
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.PasswordRecoveryTokenExpiresAt = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }
}