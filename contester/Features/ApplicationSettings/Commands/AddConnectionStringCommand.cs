using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.ApplicationSettings.Services;
using contester.Infrastructure.Persistence;
using contester.Infrastructure.Databases;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ApplicationSettings.Commands;

public class AddConnectionStringCommand : IRequest<AddConnectionStringCommandResult>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public string Text { get; set; } = null!;
    public string Dbms { get; set; } = null!;
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageSchemaDescriptions;
}

public class AddConnectionStringCommandResult
{
    public bool Success { get; set; }
    public TimeSpan ResponseTime { get; set; }
}

public class AddConnectionStringCommandHandler(
    ApplicationDbContext context,
    HealthCheckerService healthCheckerService
) : IRequestHandler<AddConnectionStringCommand, AddConnectionStringCommandResult>
{
    public async Task<AddConnectionStringCommandResult> Handle(AddConnectionStringCommand request, CancellationToken cancellationToken)
    {
        var (healthy, message, elapsed) = await healthCheckerService.HealthCheck(request.Text, request.Dbms, cancellationToken);

        if (!healthy)
        {
            throw new NotifyUserException($"Could not validate the connection string: {message}");
        }
        
        var newKey = await context.ConnectionStrings.AsNoTracking()
            .Select(cs => cs.Id)
            .MaxAsync(cancellationToken) + 1;

        var connectionString = new ConnectionString
        {
            Id = newKey,
            Text = request.Text,
            Dbms = request.Dbms,
        };
        await context.ConnectionStrings.AddAsync(connectionString, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        var newConnectionStringsList = await context.ConnectionStrings.AsNoTracking()
            .ToListAsync(cancellationToken);
        ConnectionStringsCache.Instance.SetCachedValues(newConnectionStringsList);

        return new AddConnectionStringCommandResult
        {
            Success = true,
            ResponseTime = elapsed,
        };
    }
}
