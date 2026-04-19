using AutoMapper;
using contester.Common.MediatR;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Commands;

public class CreateSchemaDescriptionCommand : IRequest<SchemaDescriptionDto>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public string Name { get; set; } = null!;
    public Constants.Permission RequiredPermission { get; set; } =  Constants.Permission.ManageSchemaDescriptions;
}

public class CreateSchemaDescriptionCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper)
    : IRequestHandler<CreateSchemaDescriptionCommand, SchemaDescriptionDto>
{
    private readonly IDirectoryService _directoryService = directoryService;

    public async Task<SchemaDescriptionDto> Handle(CreateSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
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