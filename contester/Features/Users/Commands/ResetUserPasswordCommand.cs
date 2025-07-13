using contester.Data;
using contester.Features.Authentication.Services;
using contester.Features.Users.Exceptions;
using MediatR;

namespace contester.Features.Users.Commands;

public class ResetUserPasswordCommand : IRequest<Unit>
{
    public string Password { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid CallerId { get; set; }
}

public class ResetUserPasswordCommandHandler(
    ApplicationDbContext context,
    IAuthenticationService authenticationService)
    : IRequestHandler<ResetUserPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(request.UserId);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        user.PasswordHash = authenticationService.HashPassword(request.Password);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
