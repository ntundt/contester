using diploma.Features.ContestApplications.Commands;
using diploma.Features.ContestApplications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.ContestApplications;

[ApiController]
[Route("api/contestApplications")]
[Authorize]
public class ContestApplicationsController
{
    private readonly IMediator _mediator;
    private readonly Authentication.Services.IAuthorizationService _authorizationService;
    
    public ContestApplicationsController(IMediator mediator, Authentication.Services.IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(ApplyForContestCommand command)
    {
        command.CallerId = _authorizationService.GetUserId();
        await _mediator.Send(command);
        return new OkResult();
    }
    
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var command = new ApproveContestApplicationCommand {
            ContestApplicationId = id,
            CallerId = _authorizationService.GetUserId()
        };
        await _mediator.Send(command);
        return new OkResult();
    }

    [HttpGet]
    public async Task<CheckContestApplicationQueryResult> GetContestApplications([FromQuery] CheckContestApplicationQuery query)
    {
        query.CallerId = _authorizationService.GetUserId();
        var result = await _mediator.Send(query);
        return result;
    }
}
