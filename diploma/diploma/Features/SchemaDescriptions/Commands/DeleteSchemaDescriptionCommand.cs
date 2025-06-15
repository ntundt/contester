using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.SchemaDescriptions.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.SchemaDescriptions.Commands;

public class DeleteSchemaDescriptionCommand : IRequest<Unit>
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
}

public class DeleteSchemaDescriptionCommandHandler(ApplicationDbContext context, IPermissionService permissionService)
    : IRequestHandler<DeleteSchemaDescriptionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageSchemaDescriptions);
        }
        
        var schemaDescription = await context.SchemaDescriptions
            .Include(s => s.Files)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (schemaDescription == null)
        {
            throw new SchemaDescriptionNotFoundException();
        }
        
        foreach (var file in schemaDescription.Files)
        {
            context.SchemaDescriptionFiles.Remove(file);
        }

        context.SchemaDescriptions.Remove(schemaDescription);
        await context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}