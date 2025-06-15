using System.Security.Claims;
using diploma.Features.Attempts.Commands;
using diploma.Features.Attempts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.Attempts;

[Authorize]
[ApiController]
[Route("api/attempts")]
public class AttemptController(IMediator mediator, Authentication.Services.IAuthorizationService authorizationService)
{
    [HttpGet]
    public async Task<GetAttemptsQueryResult> GetAttempts([FromQuery] GetAttemptsQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpGet("{attemptId:guid}")]
    public async Task<SingleAttemptDto> GetAttempt([FromRoute] Guid attemptId)
    {
        var query = new GetSingleAttemptQuery
        {
            CallerId = authorizationService.GetUserId(),
            AttemptId = attemptId,
        };
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPost]
    public async Task<AttemptDto> CreateAttempt([FromBody] CreateAttemptCommand command)
    {
        command.AuthorId = authorizationService.GetUserId();
        var result = await mediator.Send(command);
        return result;
    }

    [HttpPut("{attemptId:guid}")]
    public async Task<AttemptDto> ReEvaluateAttempt([FromRoute] Guid attemptId)
    {
        var command = new ReEvaluateAttemptCommand
        {
            CallerId = authorizationService.GetUserId(),
            AttemptId = attemptId,
        };
        var result = await mediator.Send(command);
        return result;
    }

    [HttpGet("{attemptId:guid}/result-set")]
    public async Task<EvaluateResultSetsQueryResult> GetAttemptResultSets([FromRoute] Guid attemptId)
    {
        var command = new EvaluateResultSetsQuery()
        {
            CallerId = authorizationService.GetUserId(),
            AttemptId = attemptId,
        };
        var result = await mediator.Send(command);
        return result;
    }
}