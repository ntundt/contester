using AutoMapper;

namespace contester.Features.UserGroups.Services;

public interface IUserGroupMapperService
{
    Task<List<PrincipalDto>> GetUserGroupMembers(Guid groupId, CancellationToken ct = default);
}

public class UserGroupMapperService(
    IUserGroupService userGroupService,
    IMapper mapper
) : IUserGroupMapperService
{
    public async Task<List<PrincipalDto>> GetUserGroupMembers(Guid groupId, CancellationToken ct = default)
    {
        var users = await userGroupService.GetGroupMemberUsers(groupId, false, ct);
        var groups = await userGroupService.GetGroupMemberGroups(groupId, ct);
    
        var result = new List<PrincipalDto>(mapper.Map<List<PrincipalDto>>(groups));
        result.AddRange(mapper.Map<List<PrincipalDto>>(users));
    
        return result;
    }
}