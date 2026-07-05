using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Commands;

public class UpdateSchemaDescriptionCommand : IRequest<SchemaDescriptionDto>, IAuthorizedRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageSchemaDescriptions;
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
            throw new EntityNotFoundException(typeof(SchemaDescription), request.Id);

        schemaDescription.Name = request.Name;
        await context.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<SchemaDescriptionDto>(schemaDescription);
    }
}
