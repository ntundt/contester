using contester.Features.Users;
using contester.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.UserGroups.Services;

public interface IUserGroupService
{
    Task<bool> UserIsGroupMember(Guid groupId, Guid userId, CancellationToken ct = default);
    Task<List<User>> GetGroupMemberUsers(Guid groupId, bool recursive, CancellationToken ct = default);
    Task<List<UserGroup>> GetGroupMemberGroups(Guid groupId, CancellationToken ct = default);
    Task AddUserToGroup(Guid groupId, Guid userId, CancellationToken ct = default);
    Task RemoveUserFromGroup(Guid groupId, Guid userId, CancellationToken ct = default);
    Task AddGroupToGroup(Guid parentGroupId, Guid childGroupId, CancellationToken ct = default);
    Task RemoveGroupFromGroup(Guid parentGroupId, Guid childGroupId, CancellationToken ct = default);
}

public class UserGroupService(ApplicationDbContext context) : IUserGroupService
{
    public async Task<bool> UserIsGroupMember(Guid groupId, Guid userId, CancellationToken ct = default)
    {
        var group = await context.UserGroups.AsNoTracking()
            .Include(g => g.MemberUsers)
            .Include(g => g.MemberGroups)
            .FirstAsync(g => g.Id == groupId, ct);

        if (group.MemberUsers.Any(u => u.Id == userId))
            return true;

        foreach (var memberGroup in group.MemberGroups)
        {
            if (await UserIsGroupMember(memberGroup.Id, userId, ct))
                return true;
        }

        return false;
    }

    public async Task<List<User>> GetGroupMemberUsers(Guid groupId, bool recursive, CancellationToken ct = default)
    {
        var group = await context.UserGroups.AsNoTracking()
            .Include(g => g.MemberUsers)
            .Include(g => g.MemberGroups)
            .FirstAsync(g => g.Id == groupId, ct);
        
        var members = new List<User>(group.MemberUsers);

        if (!recursive)
        {
            return members;
        }

        foreach (var memberGroup in group.MemberGroups)
        {
            members.AddRange(await GetGroupMemberUsers(memberGroup.Id, true, ct));
        }
        
        return members.DistinctBy(g => g.Id).ToList();
    }

    public async Task<List<UserGroup>> GetGroupMemberGroups(Guid groupId, CancellationToken ct = default)
    {
        var group = await context.UserGroups.AsNoTracking()
            .Include(g => g.MemberGroups)
            .FirstAsync(g => g.Id == groupId, ct);

        return group.MemberGroups;
    }

    public async Task AddUserToGroup(Guid groupId, Guid userId, CancellationToken ct = default)
    {
        var group = await context.UserGroups
            .Include(ug => ug.MemberUsers)
            .FirstAsync(ug => ug.Id == groupId, ct);
        
        if (group.MemberUsers.Any(u => u.Id == userId))
            return;

        var user = await context.Users
            .FirstAsync(u => u.Id == userId, ct);
        
        group.MemberUsers.Add(user);
    }

    public async Task RemoveUserFromGroup(Guid groupId, Guid userId, CancellationToken ct = default)
    {
        var group = await context.UserGroups
            .Include(ug => ug.MemberUsers)
            .FirstAsync(ug => ug.Id == groupId, ct);
        
        if (group.MemberUsers.All(u => u.Id != userId))
            return;
        
        group.MemberUsers.Remove(group.MemberUsers.First(u => u.Id == userId));
    }

    private async Task<List<UserGroup>> GetGroupMemberGroupsRecursively(Guid groupId, CancellationToken ct = default)
    {
        var group = await context.UserGroups.AsNoTracking()
            .Include(g => g.MemberGroups)
            .FirstAsync(g => g.Id == groupId, ct);
        
        var groups = new List<UserGroup>(group.MemberGroups);

        foreach (var memberGroup in group.MemberGroups)
        {
            groups.AddRange(await GetGroupMemberGroupsRecursively(memberGroup.Id, ct));
        }
        
        return groups.DistinctBy(g => g.Id).ToList();
    }

    public async Task AddGroupToGroup(Guid parentGroupId, Guid childGroupId, CancellationToken ct = default)
    {
        var childGroup = await context.UserGroups
            .FirstAsync(ug => ug.Id == childGroupId, ct);
        
        var parentGroup = await context.UserGroups
            .Include(ug => ug.MemberGroups)
            .FirstAsync(ug => ug.Id == parentGroupId, ct);

        if (parentGroup.MemberGroups.Any(g => g.Id == childGroup.Id))
            return;
        
        if ((await GetGroupMemberGroupsRecursively(childGroupId, ct)).Any(g => g.Id == parentGroupId))
            throw new InvalidOperationException("Recursive group membership is not allowed");
        
        parentGroup.MemberGroups.Add(childGroup);
    }

    public async Task RemoveGroupFromGroup(Guid parentGroupId, Guid childGroupId, CancellationToken ct = default)
    {
        var childGroup = await context.UserGroups
            .FirstAsync(ug => ug.Id == childGroupId, ct);
        
        var parentGroup = await context.UserGroups
            .Include(ug => ug.MemberGroups)
            .FirstAsync(ug => ug.Id == parentGroupId, ct);

        if (parentGroup.MemberGroups.All(g => g.Id != childGroup.Id))
            return;
        
        parentGroup.MemberGroups.Remove(childGroup);
    }
}
