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
    public string Name { get; set; } = null!;
}

public class CreateSchemaDescriptionCommandHandler : IRequestHandler<CreateSchemaDescriptionCommand, SchemaDescriptionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;
    
    public CreateSchemaDescriptionCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, IMapper mapper, IClaimService claimService)
    {
        _context = context;
        _directoryService = directoryService;
        _mapper = mapper;
        _claimService = claimService;
    }
    
    public async Task<SchemaDescriptionDto> Handle(CreateSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageSchemaDescriptions", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageSchemaDescriptions");
        }
        
        var schemaDescription = new SchemaDescription
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };
        
        await _context.SchemaDescriptions.AddAsync(schemaDescription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        schemaDescription = await _context.SchemaDescriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schemaDescription.Id, cancellationToken);
        
        return _mapper.Map<SchemaDescriptionDto>(schemaDescription);
    }
}