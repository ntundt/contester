using contester.Data;
using contester.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Commands;

public class SignInOrSignUpByEmailCommand : IRequest<SignInOrSignUpByEmailCommandResult>
{
    public string Email { get; set; } = null!;
}

public class SignInOrSignUpByEmailCommandResult
{
    public bool UserFound { get; set; }
    public string AuthenticationMethod { get; set; } = null!;
}

public class SignInOrSignUpByEmailCommandValidator : AbstractValidator<SignInOrSignUpByEmailCommand>
{
    public SignInOrSignUpByEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class
    SignInOrSignUpByEmailCommandHandler(ApplicationDbContext context, IConfigurationReaderService configuration) : IRequestHandler<SignInOrSignUpByEmailCommand,
    SignInOrSignUpByEmailCommandResult>
{
    public async Task<SignInOrSignUpByEmailCommandResult> Handle(SignInOrSignUpByEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.AsNoTracking()
            .Where(u => u.Email == request.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return new SignInOrSignUpByEmailCommandResult
        {
            UserFound = user != default,
            AuthenticationMethod = configuration.IsPasswordlessAuthenticationEnabled() ? "EmailCode" : "Password"
        };
    }
}
