using contester.Features.Grade.Commands;
using contester.Features.Grade.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.Grade;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GradeAdjustmentsController(
    MediatR.IMediator mediator,
    Authentication.Services.IAuthorizationService authorizationService)
{
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AdjustGradeCommand command)
    {
        command.UserId = authorizationService.GetUserId();
        await mediator.Send(command);
        return new OkResult();
    }
 
    [HttpGet]
    public async Task<List<GradeAdjustmentDto>> GetList([FromQuery] GetGradeAdjustmentsQuery query)
    {
        var result = await mediator.Send(query);
        return result;
    }
}
