using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.UserGroups.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.UserGroups.Commands;

public class DeleteUserGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid GroupId { get; set; }
    [JsonIgnore] public Guid CallerId { get; set; }
    [JsonIgnore] public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class DeleteUserGroupCommandHandler(
    ApplicationDbContext context) : IRequestHandler<DeleteUserGroupCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUserGroupCommand request, CancellationToken ct)
    {
        var group = await context.UserGroups.FirstOrDefaultAsync(ug => ug.Id == request.GroupId, ct);
        if (group == null)
            throw new EntityNotFoundException(typeof(UserGroup), request.GroupId);

        if (group.IsSystemGroup)
            throw new CannotDropSystemGroupException();
        
        context.UserGroups.Remove(group);
        
        await context.SaveChangesAsync(ct);
        
        return Unit.Value;
    }
}
