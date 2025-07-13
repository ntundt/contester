using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Users;
using contester.Features.Users.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

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

public class ResetPasswordCommandHandler(ApplicationDbContext context, IAuthenticationService authenticationService)
    : IRequestHandler<ResetPasswordCommand>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PasswordRecoveryToken == request.PasswordResetToken, cancellationToken);
        if (user is null) throw new UserNotFoundException();

        if (user.PasswordRecoveryTokenExpiresAt < DateTime.UtcNow)
        {
            throw new PasswordResetTokenExpiredException();
        }
        
        var hasher = new PasswordHasher<User>();
        
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.PasswordRecoveryTokenExpiresAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
