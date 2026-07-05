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
    IMediator mediator,
    Authentication.Services.IAuthorizationService authorizationService)
{
    [HttpPost]
    public async Task<IActionResult> Create(ApplyForContestCommand command)
    {
        command.CallerId = authorizationService.GetUserId();
        await mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPut("approve")]
    public async Task<IActionResult> Approve([FromQuery] Guid contestId, [FromQuery] Guid userId)
    {
        var command = new ApproveContestApplicationCommand {
            ContestId = contestId,
            UserId = userId,
            CallerId = authorizationService.GetUserId()
        };
        await mediator.Send(command);
        return new OkResult();
    }

    [HttpGet]
    public async Task<CheckContestApplicationQueryResult> GetContestApplications([FromQuery] CheckContestApplicationQuery query)
    {
        query.CallerId = authorizationService.GetUserId();
        var result = await mediator.Send(query);
        return result;
    }
}
