using contester.Common.MediatR;
using contester.Features.SchemaDescriptions.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Commands;

public class DeleteSchemaDescriptionCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageSchemaDescriptions;
}

public class DeleteSchemaDescriptionCommandHandler(ApplicationDbContext context)
    : IRequestHandler<DeleteSchemaDescriptionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
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