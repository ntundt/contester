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
    
    public ScoreboardController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<GetScoreboardQueryResult> GetScoreboard([FromQuery] GetScoreboardQuery query)
    {
        var result = await _mediator.Send(query);
        return result;
    }
}