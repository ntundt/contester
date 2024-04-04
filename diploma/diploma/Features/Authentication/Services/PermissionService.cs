using diploma.Data;
using diploma.Features.Users.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Authentication.Services;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string claimName, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    
    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> UserHasPermissionAsync(Guid userId, string claimName, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        return user.UserRole.Permissions.Any(c => c.Name == claimName);
    }
    
    public async Task UserAddClaimsAsync(Guid userId, IEnumerable<string> claimNames, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        var permissions = await _context.Permissions
            .Where(c => claimNames.Contains(c.Name))
            .ToListAsync(cancellationToken);
        
        user.UserRole.Permissions.AddRange(permissions);
        
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        return user.UserRole.Permissions;
    }
}