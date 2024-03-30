using diploma.Data;
using diploma.Features.Authentication;
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

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, GetPermissionsQueryResult>
{
    private readonly ApplicationDbContext _context;

    public GetPermissionsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetPermissionsQueryResult> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking()
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

