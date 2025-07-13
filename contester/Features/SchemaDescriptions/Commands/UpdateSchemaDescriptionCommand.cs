using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.SchemaDescriptions.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Commands;

public class UpdateSchemaDescriptionCommand : IRequest<SchemaDescriptionDto>
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class UpdateSchemaDescriptionCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IPermissionService permissionService)
    : IRequestHandler<UpdateSchemaDescriptionCommand, SchemaDescriptionDto>
{
    public async Task<SchemaDescriptionDto> Handle(UpdateSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageSchemaDescriptions);
        }
        
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
