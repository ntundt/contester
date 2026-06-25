using System.Diagnostics;
using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using contester.Infrastructure.Databases;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ApplicationSettings.Queries;

public class ConnectionStringsHealthCheckQuery : IRequest<ConnectionStringsHealthCheckQueryResult>, IAuthorizedRequest
{
    public int ConnectionStringId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageSchemaDescriptions;
}

public class ConnectionStringsHealthCheckQueryResult
{
    public ConnectionString ConnectionString { get; set; } = null!;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
}

public class ConnectionStringsHealthCheckQueryHandler(
    ApplicationDbContext context,
    IConfiguration configuration
) : IRequestHandler<ConnectionStringsHealthCheckQuery,
    ConnectionStringsHealthCheckQueryResult>
{
    private async Task<(bool, string?, long)> HealthCheck(string connectionString, string dbms, CancellationToken cancellationToken)
    {
        var dbmsAdapter = new DbmsAdapterFactory(configuration)
            .CreateWithConnectionString(dbms, connectionString);

        try
        {
            await dbmsAdapter.GetLockAsync(3);
            var stopwatch = Stopwatch.StartNew();
            var (healthy, message) = await dbmsAdapter.VerifyDbmsAvailableAsync(cancellationToken);
            stopwatch.Stop();
            return (healthy, message, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }
    }
    
    public async Task<ConnectionStringsHealthCheckQueryResult> Handle(ConnectionStringsHealthCheckQuery request, CancellationToken cancellationToken)
    {
        var connectionString = await context.ConnectionStrings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ConnectionStringId, cancellationToken);

        if (connectionString is null)
            throw new NotifyUserException("Connection string not found");
        
        var (healthy, message, elapsed) = await HealthCheck(connectionString.Text, connectionString.Dbms, cancellationToken); 
            
        return new ConnectionStringsHealthCheckQueryResult
        {
            ConnectionString = connectionString,
            Success = healthy,
            Message = message ?? string.Empty,
            ElapsedMilliseconds = elapsed,
        };
    }
}
