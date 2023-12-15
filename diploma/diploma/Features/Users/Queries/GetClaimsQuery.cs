using diploma.Data;
using diploma.Features.Authentication;
using diploma.Features.Users.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Users.Queries;

public class GetClaimsQuery : IRequest<GetClaimsQueryResult>
{
    public Guid UserId { get; set; }
}

public class GetClaimsQueryResult
{
    public List<string> Claims { get; set; } = null!;
}

public class GetClaimsQueryHandler : IRequestHandler<GetClaimsQuery, GetClaimsQueryResult>
{
    private readonly ApplicationDbContext _context;

    public GetClaimsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetClaimsQueryResult> Handle(GetClaimsQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Claims)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        return new GetClaimsQueryResult
        {
            Claims = user.UserRole.Claims.Select(c => c.Name).ToList()
        };
    }
}

