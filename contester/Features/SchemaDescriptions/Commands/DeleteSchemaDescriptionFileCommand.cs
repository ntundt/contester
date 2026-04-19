using contester.Common.MediatR;
using contester.Features.SchemaDescriptions.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Commands;

public class DeleteSchemaDescriptionFileCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid SchemaDescriptionId { get; set; }
    public string Dbms { get; set; } = null!;
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageSchemaDescriptions;
}

public class DeleteSchemaDescriptionFileCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService)
    : IRequestHandler<DeleteSchemaDescriptionFileCommand, Unit>
{
    private readonly IDirectoryService _directoryService = directoryService;

    public async Task<Unit> Handle(DeleteSchemaDescriptionFileCommand request, CancellationToken cancellationToken)
    {
        var schemaDescriptionFile = await context.SchemaDescriptionFiles.FirstOrDefaultAsync(s => s.SchemaDescriptionId == request.SchemaDescriptionId && s.Dbms == request.Dbms, cancellationToken);
        if (schemaDescriptionFile == null)
        {
            throw new SchemaDescriptionFileNotFoundException();
        }

        context.SchemaDescriptionFiles.Remove(schemaDescriptionFile);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}