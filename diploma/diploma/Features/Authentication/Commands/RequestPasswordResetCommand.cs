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

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly IAuthenticationService _authenticationService;
    
    public RequestPasswordResetCommandHandler(ApplicationDbContext context, IMediator mediator, IEmailService emailService, IAuthenticationService authenticationService)
    {
        _context = context;
        _mediator = mediator;
        _emailService = emailService;
        _authenticationService = authenticationService;
    }

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null) throw new UserNotFoundException();

        user.PasswordRecoveryToken = Guid.NewGuid();
        user.PasswordRecoveryTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        await _emailService.SendEmailAsync(user.Email, "Password recovery", 
            $"Please follow this link to reset your password: {_authenticationService.GetPasswordRecoveryUrl(user.PasswordRecoveryToken)}");
    }
}
