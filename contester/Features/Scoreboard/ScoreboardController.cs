using contester.Features.Scoreboard.Commands;
using contester.Features.Scoreboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.Scoreboard;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController(
    IMediator mediator,
    Authentication.Services.IAuthorizationService authorizationService)
{
    [HttpGet]
    public async Task<GetScoreboardQueryResult> GetScoreboard([FromQuery] GetScoreboardQuery query)
    {
        try 
        {
            query.CallerId = authorizationService.GetUserId();
        }
        catch (Exception)
        {
            query.CallerId = Guid.Empty;
        }
        var result = await mediator.Send(query);
        return result;
    }

    [HttpPost("approve")]
    [Authorize]
    public async Task ApproveScoreboard([FromQuery] Guid contestId)
    {
        var command = new ApproveScoreboardCommand
        {
            ContestId = contestId,
            CallerId = authorizationService.GetUserId(),
        };
        await mediator.Send(command);
    }

    [HttpGet("approval-status")]
    public async Task<GetScoreboardApprovalStatusQueryResult> GetScoreboardApprovalStatus([FromQuery] Guid contestId)
    {
        var query = new GetScoreboardApprovalStatusQuery
        {
            ContestId = contestId,
        };
        var result = await mediator.Send(query);
        return result;
    }
}
