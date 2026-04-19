using AutoMapper;
using contester.Common.MediatR;
using contester.Features.SchemaDescriptions.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Commands;

public class UpdateSchemaDescriptionCommand : IRequest<SchemaDescriptionDto>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageSchemaDescriptions;
}

public class UpdateSchemaDescriptionCommandHandler(
    ApplicationDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateSchemaDescriptionCommand, SchemaDescriptionDto>
{
    public async Task<SchemaDescriptionDto> Handle(UpdateSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
        var schemaDescription = await context.SchemaDescriptions.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (schemaDescription == null)
        {
            throw new SchemaDescriptionNotFoundException();
        }

        schemaDescription.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<SchemaDescriptionDto>(schemaDescription);
    }
}
