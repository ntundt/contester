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
    public async Task<AddConnectionStringCommandResult> AddConnectionString(string connectionString, string dbms, CancellationToken ct)
    {
        var command = new AddConnectionStringCommand
        {
            Text = connectionString,
            Dbms = dbms
        };
        return await mediator.Send(command, ct);
    }

    [HttpGet("connection-string")]
    public async Task<List<ConnectionString>> GetAllConnectionStrings(CancellationToken ct)
    {
        var command = new GetAllConnectionStringsQuery();
        return await mediator.Send(command, ct);
    }

    [HttpDelete("connection-string/{connectionStringId}")]
    public async Task DeleteConnectionString(int connectionStringId, CancellationToken ct)
    {
        var command = new RemoveConnectionStringCommand
        {
            ConnectionStringId = connectionStringId,
        };
        await mediator.Send(command, ct);
    }
    
    [HttpGet("connection-string/{connectionStringId}/health-check")]
    public async Task<ConnectionStringsHealthCheckQueryResult> HealthCheck(int connectionStringId, CancellationToken ct)
    {
        var command = new ConnectionStringsHealthCheckQuery
        {
            ConnectionStringId = connectionStringId,
        };
        return await mediator.Send(command, ct);
    }

    [HttpGet("server-configuration")]
    public async Task<GetPublicServerConfigurationQueryResult> GetPublicServerConfiguration(CancellationToken ct)
    {
        var query = new GetPublicServerConfigurationQuery();
        return await mediator.Send(query, ct);
    }

    [HttpGet("runtime-settings")]
    public async Task<AllRuntimeSettings> GetAllRuntimeSettings(CancellationToken ct)
    {
        return await mediator.Send(new GetAllRuntimeSettingsQuery(), ct);
    }

    [HttpGet("privacy-policy")]
    [AllowAnonymous]
    public async Task<GetPrivacyPolicyQueryResult> GetPrivacyPolicy(CancellationToken ct)
    {
        return await mediator.Send(new GetPrivacyPolicyQuery(), ct);
    }
    
    [HttpGet("setting/{settingName}")]
    public async Task<Setting> GetSetting(string settingName, CancellationToken ct)
    {
        var query = new GetSettingQuery
        {
            Key = settingName,
        };
        return await mediator.Send(query, ct);
    }

    [HttpPatch("setting/{settingName}")]
    public async Task<Setting> SetSetting([FromRoute] string settingName, [FromBody] string settingValue, CancellationToken ct)
    {
        return await mediator.Send(new SetSettingCommand
        {
            Key = settingName,
            Value = settingValue,
        }, ct);
    }
    
}
