using System.Security.Claims;
using diploma.Features.GradeAdjustments.Commands;
using diploma.Features.GradeAdjustments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.GradeAdjustments;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GradeAdjustmentsController
{
    private readonly MediatR.IMediator _mediator;
    private readonly Authentication.Services.IAuthorizationService _authorizationService;
    public GradeAdjustmentsController(MediatR.IMediator mediator, Authentication.Services.IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AdjustGradeCommand command)
    {
        command.UserId = _authorizationService.GetUserId();
        await _mediator.Send(command);
        return new OkResult();
    }
 
    [HttpGet]
    public async Task<List<GradeAdjustmentDto>> GetList([FromQuery] GetGradeAdjustmentsQuery query)
    {
        var result = await _mediator.Send(query);
        return result;
    }
}