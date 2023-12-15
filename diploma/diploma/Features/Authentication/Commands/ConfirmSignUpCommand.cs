using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Users.Exceptions;
using diploma.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Commands;

public class ConfirmSignUpCommand : IRequest<Unit>
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

public class ConfirmSignUpCommandHandler : IRequestHandler<ConfirmSignUpCommand, Unit>
{
    private ApplicationDbContext _context;
    private ILogger<ConfirmSignUpCommandHandler> _logger;
    private IEmailService _emailService;
    
    public ConfirmSignUpCommandHandler(ApplicationDbContext context, ILogger<ConfirmSignUpCommandHandler> logger,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(ConfirmSignUpCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
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
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
