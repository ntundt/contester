using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Problems.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Commands;

public class DeleteProblemCommand : IRequest<Unit>
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
}

public class DeleteProblemCommandHandler(ApplicationDbContext context, IPermissionService permissionService)
    : IRequestHandler<DeleteProblemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProblemCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageProblems, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageProblems);
        }
        
        var problem = await context.Problems.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        
        context.Problems.Remove(problem);
        await context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
