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
public class AttemptController
{
    private readonly IMediator _mediator;
    private readonly Authentication.Services.IAuthorizationService _authorizationService;
    
    public AttemptController(IMediator mediator, Authentication.Services.IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
    }
    
    [HttpGet]
    public async Task<GetAttemptsQueryResult> GetAttempts([FromQuery] GetAttemptsQuery query)
    {
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpGet("{attemptId:guid}")]
    public async Task<SingleAttemptDto> GetAttempt([FromRoute] Guid attemptId)
    {
        var query = new GetSingleAttemptQuery
        {
            CallerId = _authorizationService.GetUserId(),
            AttemptId = attemptId,
        };
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpPost]
    public async Task<AttemptDto> CreateAttempt([FromBody] CreateAttemptCommand command)
    {
        command.AuthorId = _authorizationService.GetUserId();
        var result = await _mediator.Send(command);
        return result;
    }

    [HttpPut("{attemptId:guid}")]
    public async Task<AttemptDto> ReEvaluateAttempt([FromRoute] Guid attemptId)
    {
        var command = new ReEvaluateAttemptCommand
        {
            CallerId = _authorizationService.GetUserId(),
            AttemptId = attemptId,
        };
        var result = await _mediator.Send(command);
        return result;
    }
}