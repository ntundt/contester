using diploma.Data;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using MediatR;

namespace diploma.Features.Users;

public class ResetUserPasswordCommand : IRequest<Unit>
{
    public string Password { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid CallerId { get; set; }
}

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthenticationService _authenticationService;

    public ResetUserPasswordCommandHandler(ApplicationDbContext context,
        IAuthenticationService authenticationService)
    {
        _context = context;
        _authenticationService = authenticationService;
    }

    public async Task<Unit> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        user.PasswordHash = _authenticationService.HashPassword(request.Password);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}