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

public class DeleteSchemaDescriptionCommandHandler : IRequestHandler<DeleteSchemaDescriptionCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly IClaimService _claimService;
    
    public DeleteSchemaDescriptionCommandHandler(ApplicationDbContext context, IClaimService claimService)
    {
        _context = context;
        _claimService = claimService;
    }
    
    public async Task<Unit> Handle(DeleteSchemaDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageSchemaDescriptions", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageSchemaDescriptions");
        }
        
        var schemaDescription = await _context.SchemaDescriptions
            .Include(s => s.Files)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (schemaDescription == null)
        {
            throw new SchemaDescriptionNotFoundException();
        }
        
        foreach (var file in schemaDescription.Files)
        {
            _context.SchemaDescriptionFiles.Remove(file);
        }

        _context.SchemaDescriptions.Remove(schemaDescription);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}