using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.Users.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Users.Commands;

public class SetUserRoleCommand : IRequest<Unit>, IAuthorizedRequest
{
    public string Role { get; set; } = null!;
    public Guid UserId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class SetUserRoleCommandHandler(ApplicationDbContext context)
    : IRequestHandler<SetUserRoleCommand, Unit>
{
    public async Task<Unit> Handle(SetUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        var role = await context.UserRoles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);
        if (role is null)
        {
            throw new NotifyUserException("Role not found");
        }

        user.UserRoleId = role.Id;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
