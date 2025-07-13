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
    
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var command = new ApproveContestApplicationCommand {
            ContestApplicationId = id,
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
