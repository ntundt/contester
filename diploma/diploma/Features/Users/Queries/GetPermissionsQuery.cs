using diploma.Data;
using diploma.Features.Users.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Users.Queries;

public class GetPermissionsQuery : IRequest<GetPermissionsQueryResult>
{
    public Guid UserId { get; set; }
}

public class GetPermissionsQueryResult
{
    public List<string> Permissions { get; set; } = null!;
}

public class GetPermissionsQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetPermissionsQuery, GetPermissionsQueryResult>
{
    public async Task<GetPermissionsQueryResult> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Permissions)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        return new GetPermissionsQueryResult
        {
            Permissions = user.UserRole.Permissions.Select(c => c.Name).ToList()
        };
    }
}

