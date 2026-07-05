using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.UserGroups.Commands;

public class DeleteUserGroupCommand : IRequest<Unit>
{
    public Guid GroupId { get; set; }
}

public class DeleteUserGroupCommandHandler(
    ApplicationDbContext context) : IRequestHandler<DeleteUserGroupCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUserGroupCommand request, CancellationToken ct)
    {
        var group = await context.UserGroups.FirstOrDefaultAsync(ug => ug.Id == request.GroupId, ct);
        if (group == null)
            throw new NotifyUserException("Group not found");
        
        context.UserGroups.Remove(group);
        
        await context.SaveChangesAsync(ct);
        
        return Unit.Value;
    }
}
