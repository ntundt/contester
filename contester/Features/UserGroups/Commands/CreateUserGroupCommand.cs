using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Infrastructure.Persistence;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class CreateUserGroupCommand : IRequest<PrincipalDto>, IAuthorizedRequest
{
    public required string Name { get; set; }
    [JsonIgnore] public Guid CallerId { get; set; }
    [JsonIgnore] public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class CreateUserGroupCommandHandler(
    IMapper mapper,
    ApplicationDbContext context) : IRequestHandler<CreateUserGroupCommand, PrincipalDto>
{
    public async Task<PrincipalDto> Handle(CreateUserGroupCommand request, CancellationToken ct)
    {
        var group = new UserGroup
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            IsSystemGroup = false,
        };

        await context.AddAsync(group, ct);
        await context.SaveChangesAsync(ct);

        return mapper.Map<PrincipalDto>(group);
    }
}
