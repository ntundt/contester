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
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public AttemptController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
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
            CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            AttemptId = attemptId,
        };
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpPost]
    public async Task<AttemptDto> CreateAttempt([FromBody] CreateAttemptCommand command)
    {
        command.AuthorId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(command);
        return result;
    }
}