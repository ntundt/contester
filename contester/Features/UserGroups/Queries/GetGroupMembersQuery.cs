using contester.Features.UserGroups.Services;
using MediatR;

namespace contester.Features.UserGroups.Queries;

public class GetGroupMembersQuery : IRequest<List<PrincipalDto>>
{
    public Guid GroupId { get; set; }
}

public class GetGroupMembersQueryHandler(
    IUserGroupMapperService userGroupMapperService
) : IRequestHandler<GetGroupMembersQuery, List<PrincipalDto>>
{
    public async Task<List<PrincipalDto>> Handle(GetGroupMembersQuery request, CancellationToken ct)
    {
        return await userGroupMapperService.GetUserGroupMembers(request.GroupId, ct);
    }
}
