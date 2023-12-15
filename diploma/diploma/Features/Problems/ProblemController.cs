using System.Security.Claims;
using diploma.Features.Problems.Commands;
using diploma.Features.Problems.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.Problems;

[ApiController]
[Authorize]
[Route("api/problems")]
public class ProblemController
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProblemController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }
    
    [HttpGet]
    public async Task<GetProblemsQueryResult> GetProblems([FromQuery] GetProblemsQuery query)
    {
        query.CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpGet("{problemId:guid}/expected-solution")]
    public async Task<ExpectedSolutionDto> GetExpectedSolution([FromRoute] Guid problemId)
    {
        var query = new GetExpectedSolutionQuery
        {
            CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            ProblemId = problemId,
        };
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpPost]
    public async Task<ProblemDto> CreateProblem([FromBody] CreateProblemCommand command)
    {
        command.CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(command);
        return result;
    }

    [HttpPut]
    [Route("{problemId:guid}")]
    public async Task<ProblemDto> UpdateProblem([FromRoute] Guid problemId, [FromBody] UpdateProblemCommand command)
    {
        command.CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        command.Id = problemId;
        var result = await _mediator.Send(command);
        return result;
    }
    
    [HttpDelete]
    [Route("{problemId:guid}")]
    public async Task DeleteProblem([FromRoute] Guid problemId)
    {
        var command = new DeleteProblemCommand
        {
            CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Id = problemId,
        };
        await _mediator.Send(command);
    }
}