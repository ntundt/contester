using diploma.Data;
using diploma.Features.Users.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Services;

public interface IClaimService
{
    Task<bool> UserHasClaimAsync(Guid userId, string claimName, CancellationToken cancellationToken = default);
    Task<List<Claim>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class ClaimService : IClaimService
{
    private readonly ApplicationDbContext _context;
    
    public ClaimService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> UserHasClaimAsync(Guid userId, string claimName, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Claims)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        return user.UserRole.Claims.Any(c => c.Name == claimName);
    }
    
    public async Task UserAddClaimsAsync(Guid userId, IEnumerable<string> claimNames, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Claims)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        var claims = await _context.Claims
            .Where(c => claimNames.Contains(c.Name))
            .ToListAsync(cancellationToken);
        
        user.UserRole.Claims.AddRange(claims);
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Claim>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Claims)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        return user.UserRole.Claims;
    }
}