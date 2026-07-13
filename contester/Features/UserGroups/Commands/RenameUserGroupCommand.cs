using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.UserGroups.Commands;

public class RenameUserGroupCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid GroupId { get; set; }
    public required string NewName { get; set; }
    [JsonIgnore] public Guid CallerId { get; set; }
    [JsonIgnore] public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class RenameUserGroupCommandValidator : AbstractValidator<RenameUserGroupCommand>
{
    public RenameUserGroupCommandValidator()
    {
        RuleFor(x => x.CallerId).NotEmpty();
        RuleFor(x => x.NewName).NotEmpty();
    }
}

public class RenameUserGroupCommandHandler(
    ApplicationDbContext context    
) : IRequestHandler<RenameUserGroupCommand, Unit>
{
    public async Task<Unit> Handle(RenameUserGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await context.UserGroups
            .FirstOrDefaultAsync(ug => ug.Id == request.GroupId, cancellationToken);
        if (group is null)
            throw new EntityNotFoundException(typeof(UserGroup), request.GroupId);
        
        group.Name = request.NewName;
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
