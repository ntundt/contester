using diploma.Features.ApplicationSettings.Commands;
using diploma.Features.ApplicationSettings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.ApplicationSettings;

[Authorize]
[ApiController]
[Route("api/application-settings")]
public class ApplicationSettingsController(
    IMediator mediator,
    Features.Authentication.Services.IAuthorizationService authorizationService
)
{
    [HttpPost("connection-string")]
    public async Task<AddConnectionStringCommandResult> AddConnectionString(string connectionString, string dbms, CancellationToken cancellationToken)
    {
        var command = new AddConnectionStringCommand
        {
            CallerId = authorizationService.GetUserId(),
            Text = connectionString,
            Dbms = dbms
        };
        return await mediator.Send(command, cancellationToken);
    }

    [HttpGet("connection-string")]
    public async Task<List<ConnectionString>> GetAllConnectionStrings(CancellationToken cancellationToken)
    {
        var command = new GetAllConnectionStringsQuery()
        {
            CallerId = authorizationService.GetUserId()
        };
        return await mediator.Send(command, cancellationToken);
    }

    [HttpDelete("connection-string/{connectionStringId}")]
    public async Task DeleteConnectionString(int connectionStringId, CancellationToken cancellationToken)
    {
        var command = new RemoveConnectionStringCommand
        {
            CallerId = authorizationService.GetUserId(),
            ConnectionStringId = connectionStringId,
        };
        await mediator.Send(command, cancellationToken);
    }
    
    [HttpGet("connection-string/{connectionStringId}/health-check")]
    public async Task<ConnectionStringsHealthCheckQueryResult> HealthCheck(int connectionStringId, CancellationToken cancellationToken)
    {
        var command = new ConnectionStringsHealthCheckQuery
        {
            CallerId = authorizationService.GetUserId(),
            ConnectionStringId = connectionStringId,
        };
        return await mediator.Send(command, cancellationToken);
    }
}
