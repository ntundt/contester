using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Exceptions;
using contester.Features.Users;
using contester.Features.Users.Exceptions;
using contester.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class FinishPasswordResetCommand : IRequest
{
    public Guid PasswordResetToken { get; set; }
    public string NewPassword { get; set; } = null!;
}

public class FinishPasswordResetCommandValidator : AbstractValidator<FinishPasswordResetCommand>
{
    public FinishPasswordResetCommandValidator()
    {
        RuleFor(c => c.PasswordResetToken).NotEmpty();
    }
}

public class FinishPasswordResetCommandHandler(ApplicationDbContext context, IAuthenticationService authenticationService, IConfigurationReaderService configuration)
    : IRequestHandler<FinishPasswordResetCommand>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task Handle(FinishPasswordResetCommand request, CancellationToken cancellationToken)
    {
        if (configuration.IsPasswordlessAuthenticationEnabled())
            throw new NotifyUserException("Password authentication is disabled");
        
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PasswordRecoveryToken == request.PasswordResetToken, cancellationToken);
        if (user is null) throw new UserNotFoundException();

        if (user.PasswordRecoveryTokenExpiresAt < DateTime.UtcNow)
            throw new PasswordResetTokenExpiredException();
        
        var hasher = new PasswordHasher<User>();
        
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        user.PasswordRecoveryTokenExpiresAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
