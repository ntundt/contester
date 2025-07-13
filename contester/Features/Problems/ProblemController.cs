using System.Security.Claims;
using contester.Features.Problems.Commands;
using contester.Features.Problems.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.Problems;

[ApiController]
[Authorize]
[Route("api/problems")]
public class ProblemController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
{
    [HttpGet]
    public async Task<GetProblemsQueryResult> GetProblems([FromQuery] GetProblemsQuery query)
    {
        query.CallerId = Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpGet("{problemId:guid}/expected-solution")]
    public async Task<ExpectedSolutionDto> GetExpectedSolution([FromRoute] Guid problemId)
    {
        var query = new GetExpectedSolutionQuery
        {
            CallerId = Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            ProblemId = problemId,
        };
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPost]
    public async Task<ProblemDto> CreateProblem([FromBody] CreateProblemCommand command)
    {
        command.CallerId = Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await mediator.Send(command);
        return result;
    }

    [HttpPut]
    [Route("{problemId:guid}")]
    public async Task<ProblemDto> UpdateProblem([FromRoute] Guid problemId, [FromBody] UpdateProblemCommand command)
    {
        command.CallerId = Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        command.Id = problemId;
        var result = await mediator.Send(command);
        return result;
    }
    
    [HttpDelete]
    [Route("{problemId:guid}")]
    public async Task DeleteProblem([FromRoute] Guid problemId)
    {
        var command = new DeleteProblemCommand
        {
            CallerId = Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Id = problemId,
        };
        await mediator.Send(command);
    }
}
