using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.SchemaDescriptions.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.SchemaDescriptions.Commands;

public class UpdateSchemaDescriptionCommand : IRequest<SchemaDescriptionDto>
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class UpdateSchemaDescriptionCommandHandler : IRequestHandler<UpdateSchemaDescriptionCommand, SchemaDescriptionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;
    
    public UpdateSchemaDescriptionCommandHandler(ApplicationDbContext context, IMapper mapper, IClaimService claimService)
    {
        _context = context;
        _mapper = mapper;
        _claimService = claimService;
    }
    
    public async Task<SchemaDescriptionDto> Handle(UpdateSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageSchemaDescriptions", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageSchemaDescriptions");
        }
        
        var schemaDescription = await _context.SchemaDescriptions.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (schemaDescription == null)
        {
            throw new SchemaDescriptionNotFoundException();
        }

        schemaDescription.Name = request.Name;
        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<SchemaDescriptionDto>(schemaDescription);
    }
}
