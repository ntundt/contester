﻿using contester.Data;
using contester.Features.Users.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Authentication.Services;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, Constants.Permission permission, CancellationToken cancellationToken = default);
    Task<bool> UserHasPermissionAsync(Guid userId, string claimName, CancellationToken cancellationToken = default);
    Task<List<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class PermissionService(ApplicationDbContext context) : IPermissionService
{
    public async Task<bool> UserHasPermissionAsync(Guid userId, Constants.Permission permission, CancellationToken cancellationToken = default)
    {
        return await UserHasPermissionAsync(userId, permission.ToString(), cancellationToken);
    }
    
    public async Task<bool> UserHasPermissionAsync(Guid userId, string claimName, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.AsNoTracking()
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
        var user = await context.Users
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        var permissions = await context.Permissions
            .Where(c => claimNames.Contains(c.Name))
            .ToListAsync(cancellationToken);
        
        user.UserRole.Permissions.AddRange(permissions);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.AsNoTracking()
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
