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
    private readonly IHttpContextAccessor _httpContextAccessor;
    public GradeAdjustmentsController(MediatR.IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] AdjustGradeCommand command)
    {
        command.UserId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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