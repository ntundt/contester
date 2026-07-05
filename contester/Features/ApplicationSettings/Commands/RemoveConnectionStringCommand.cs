using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using contester.Infrastructure.Databases;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ApplicationSettings.Commands;

public class RemoveConnectionStringCommand : IRequest, IAuthorizedRequest
{
    public int ConnectionStringId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageSchemaDescriptions;
}

public class RemoveConnectionStringCommandHandler(
    ApplicationDbContext context
) : IRequestHandler<RemoveConnectionStringCommand>
{
    public async Task Handle(RemoveConnectionStringCommand request, CancellationToken cancellationToken)
    {
        var connectionString = await context.ConnectionStrings.FindAsync(request.ConnectionStringId, cancellationToken);
        if (connectionString == null)
            throw new EntityNotFoundException(typeof(ConnectionString), request.ConnectionStringId);
        
        context.ConnectionStrings.Remove(connectionString);
        await context.SaveChangesAsync(cancellationToken);
        
        var newConnectionStringsList = await context.ConnectionStrings.AsNoTracking()
            .ToListAsync(cancellationToken);
        ConnectionStringsCache.Instance.SetCachedValues(newConnectionStringsList);
    }
}
