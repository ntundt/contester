using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using diploma.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Commands;

public class ConfirmSignUpCommand : IRequest<AuthorizeCommandResult>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string AdditionalInfo { get; set; } = null!;
    public Guid Token { get; set; }
}

public class ConfirmSignUpCommandValidator : AbstractValidator<ConfirmSignUpCommand>
{
    public ConfirmSignUpCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.AdditionalInfo).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class ConfirmSignUpCommandHandler(
    ApplicationDbContext context,
    ILogger<ConfirmSignUpCommandHandler> logger,
    IEmailService emailService,
    IJwtService jwtService)
    : IRequestHandler<ConfirmSignUpCommand, AuthorizeCommandResult>
{
    private ILogger<ConfirmSignUpCommandHandler> _logger = logger;
    private IEmailService _emailService = emailService;

    public async Task<AuthorizeCommandResult> Handle(ConfirmSignUpCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == request.Token, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        if (user.IsEmailConfirmed)
        {
            throw new EmailAlreadyConfirmedException();
        }
        user.IsEmailConfirmed = true;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Patronymic = request.Patronymic;
        user.AdditionalInfo = request.AdditionalInfo;
        user.LastLogin = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new AuthorizeCommandResult
        {
            Token = jwtService.GenerateJwtToken(user.Id.ToString(), user.UserRole.Name),
        };
    }
}
