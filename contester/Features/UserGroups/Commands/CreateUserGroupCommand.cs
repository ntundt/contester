using AutoMapper;
using contester.Infrastructure.Persistence;
using MediatR;

namespace contester.Features.UserGroups.Commands;

public class CreateUserGroupCommand : IRequest<PrincipalDto>
{
    public required string Name { get; set; }
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
