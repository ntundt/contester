using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using contester.Infrastructure.Databases;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ApplicationSettings.Commands;

public class RemoveConnectionStringCommand : IRequest, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public int ConnectionStringId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageSchemaDescriptions;
}

public class RemoveConnectionStringCommandHandler(
    ApplicationDbContext context
) : IRequestHandler<RemoveConnectionStringCommand>
{
    public async Task Handle(RemoveConnectionStringCommand request, CancellationToken cancellationToken)
    {
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
