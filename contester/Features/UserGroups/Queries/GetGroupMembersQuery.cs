using AutoMapper;
using contester.Features.UserGroups.Services;
using MediatR;

namespace contester.Features.UserGroups.Queries;

public class GetGroupMembersQuery : IRequest<List<PrincipalDto>>
{
    public Guid GroupId { get; set; }
}

public class GetGroupMembersQueryHandler(
    IUserGroupService userGroupService,
    IMapper mapper) : IRequestHandler<GetGroupMembersQuery, List<PrincipalDto>>
{
    public async Task<List<PrincipalDto>> Handle(GetGroupMembersQuery request, CancellationToken ct)
    {
        var users = await userGroupService.GetGroupMemberUsers(request.GroupId, false, ct);
        var groups = await userGroupService.GetGroupMemberGroups(request.GroupId, ct);
        
        var result = new List<PrincipalDto>(mapper.Map<List<PrincipalDto>>(groups));
        result.AddRange(mapper.Map<List<PrincipalDto>>(users));
        
        return result;
    }
}
