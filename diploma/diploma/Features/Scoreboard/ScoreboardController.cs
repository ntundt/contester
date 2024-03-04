using System.Security.Claims;
using diploma.Features.Scoreboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.Scoreboard;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public ScoreboardController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }
    
    [HttpGet]
    public async Task<GetScoreboardQueryResult> GetScoreboard([FromQuery] GetScoreboardQuery query)
    {
        try 
        {
            query.CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
        catch (ArgumentNullException)
        {
            query.CallerId = Guid.Empty;
        }
        var result = await _mediator.Send(query);
        return result;
    }
}