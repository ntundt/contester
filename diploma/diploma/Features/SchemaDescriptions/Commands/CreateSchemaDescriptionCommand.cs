using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.SchemaDescriptions.Commands;

public class CreateSchemaDescriptionCommand : IRequest<SchemaDescriptionDto>
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public string Name { get; set; } = null!;
}

public class CreateSchemaDescriptionCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IPermissionService permissionService)
    : IRequestHandler<CreateSchemaDescriptionCommand, SchemaDescriptionDto>
{
    private readonly IDirectoryService _directoryService = directoryService;

    public async Task<SchemaDescriptionDto> Handle(CreateSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageSchemaDescriptions);
        }

        if (!await context.Contests.AnyAsync(c => c.Id == request.ContestId, cancellationToken))
        {
            throw new Exception("Contest not found");
        }
        
        var schemaDescription = new SchemaDescription
        {
            Id = Guid.NewGuid(),
            ContestId = request.ContestId,
            Name = request.Name,
        };
        
        await context.SchemaDescriptions.AddAsync(schemaDescription, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        schemaDescription = await context.SchemaDescriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schemaDescription.Id, cancellationToken);
        
        return mapper.Map<SchemaDescriptionDto>(schemaDescription);
    }
}