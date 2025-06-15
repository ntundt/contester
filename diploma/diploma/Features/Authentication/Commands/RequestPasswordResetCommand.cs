using diploma.Data;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using diploma.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Commands;

public class RequestPasswordResetCommand : IRequest
{
    public string Email { get; set; } = null!;
}

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
}

public class RequestPasswordResetCommandHandler(
    ApplicationDbContext context,
    IMediator mediator,
    IEmailService emailService,
    IAuthenticationService authenticationService)
    : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly IMediator _mediator = mediator;

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null) throw new UserNotFoundException();

        user.PasswordRecoveryToken = Guid.NewGuid();
        user.PasswordRecoveryTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        
        await context.SaveChangesAsync(cancellationToken);
        
        await emailService.SendEmailAsync(user.Email, "Password recovery", 
            $"Please follow this link to reset your password: {authenticationService.GetPasswordRecoveryUrl(user.PasswordRecoveryToken)}");
    }
}
