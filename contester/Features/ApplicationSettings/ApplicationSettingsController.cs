using contester.Features.ApplicationSettings.Commands;
using contester.Features.ApplicationSettings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.ApplicationSettings;

[Authorize]
[ApiController]
[Route("api/application-settings")]
public class ApplicationSettingsController(
    IMediator mediator
)
{
    [HttpPost("connection-string")]
    public async Task<AddConnectionStringCommandResult> AddConnectionString(string connectionString, string dbms, CancellationToken cancellationToken)
    {
        var command = new AddConnectionStringCommand
        {
            Text = connectionString,
            Dbms = dbms
        };
        return await mediator.Send(command, cancellationToken);
    }

    [HttpGet("connection-string")]
    public async Task<List<ConnectionString>> GetAllConnectionStrings(CancellationToken cancellationToken)
    {
        var command = new GetAllConnectionStringsQuery();
        return await mediator.Send(command, cancellationToken);
    }

    [HttpDelete("connection-string/{connectionStringId}")]
    public async Task DeleteConnectionString(int connectionStringId, CancellationToken cancellationToken)
    {
        var command = new RemoveConnectionStringCommand
        {
            ConnectionStringId = connectionStringId,
        };
        await mediator.Send(command, cancellationToken);
    }
    
    [HttpGet("connection-string/{connectionStringId}/health-check")]
    public async Task<ConnectionStringsHealthCheckQueryResult> HealthCheck(int connectionStringId, CancellationToken cancellationToken)
    {
        var command = new ConnectionStringsHealthCheckQuery
        {
            ConnectionStringId = connectionStringId,
        };
        return await mediator.Send(command, cancellationToken);
    }

    [HttpGet("server-configuration")]
    public async Task<GetPublicServerConfigurationQueryResult> GetPublicServerConfiguration(CancellationToken cancellationToken)
    {
        var query = new GetPublicServerConfigurationQuery();
        return await mediator.Send(query, cancellationToken);
    }
}
