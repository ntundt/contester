using contester.Features.ContestApplications.Commands;
using contester.Features.ContestApplications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.ContestApplications;

[ApiController]
[Route("api/contestApplications")]
[Authorize]
public class ContestApplicationsController(
    IMediator mediator)
{
    [HttpPost]
    public async Task<IActionResult> Create(ApplyForContestCommand command)
    {
        await mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPut("approve")]
    public async Task<IActionResult> Approve([FromQuery] Guid contestId, [FromQuery] Guid userId)
    {
        var command = new ApproveContestApplicationCommand {
            ContestId = contestId,
            UserId = userId,
        };
        await mediator.Send(command);
        return new OkResult();
    }

    [HttpGet]
    public async Task<CheckContestApplicationQueryResult> GetContestApplications([FromQuery] CheckContestApplicationQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }
}
