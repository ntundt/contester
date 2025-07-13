using contester.Application;
using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ApplicationSettings.Commands;

public class RemoveConnectionStringCommand : IRequest
{
    public Guid CallerId { get; set; }
    public int ConnectionStringId { get; set; }
}


public class RemoveConnectionStringCommandHandler(
    ApplicationDbContext context,
    IPermissionService permissionService
) : IRequestHandler<RemoveConnectionStringCommand>
{
    public async Task Handle(RemoveConnectionStringCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId,
                Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new NotifyUserException("You do not have a permission to manage connection strings.");
        }
        
        var connectionString = await context.ConnectionStrings.FindAsync(request.ConnectionStringId, cancellationToken);
        if (connectionString == null)
            throw new NotifyUserException("Connection string not found");
        
        context.ConnectionStrings.Remove(connectionString);
        await context.SaveChangesAsync(cancellationToken);
        
        var newConnectionStringsList = await context.ConnectionStrings.AsNoTracking()
            .ToListAsync(cancellationToken);
        ConnectionStringsCache.Instance.SetCachedValues(newConnectionStringsList);
    }
}
