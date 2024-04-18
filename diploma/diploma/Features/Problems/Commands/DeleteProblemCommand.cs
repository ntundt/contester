using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Problems.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Problems.Commands;

public class DeleteProblemCommand : IRequest<Unit>
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
}

public class DeleteProblemCommandHandler : IRequestHandler<DeleteProblemCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly IPermissionService _permissionService;
    
    public DeleteProblemCommandHandler(ApplicationDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }
    
    public async Task<Unit> Handle(DeleteProblemCommand request, CancellationToken cancellationToken)
    {
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageProblems, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageProblems);
        }
        
        var problem = await _context.Problems.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        
        _context.Problems.Remove(problem);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}