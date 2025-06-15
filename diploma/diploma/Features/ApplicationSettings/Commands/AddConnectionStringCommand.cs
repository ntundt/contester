using diploma.Application;
using diploma.Data;
using diploma.Exceptions;
using diploma.Features.ApplicationSettings.Services;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.ApplicationSettings.Commands;

public class AddConnectionStringCommand : IRequest<AddConnectionStringCommandResult>
{
    public Guid CallerId { get; set; }
    public string Text { get; set; } = null!;
    public string Dbms { get; set; } = null!;
}

public class AddConnectionStringCommandResult
{
    public bool Success { get; set; }
    public TimeSpan ResponseTime { get; set; }
}

public class AddConnectionStringCommandHandler(
    ApplicationDbContext context,
    IPermissionService permissionService,
    HealthCheckerService healthCheckerService
) : IRequestHandler<AddConnectionStringCommand, AddConnectionStringCommandResult>
{
    public async Task<AddConnectionStringCommandResult> Handle(AddConnectionStringCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId,
                Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new NotifyUserException("You do not have a permission to manage connection strings.");
        }
        
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
