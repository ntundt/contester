using System.Security.Claims;
using diploma.Features.Contests.Commands;
using diploma.Features.Contests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Features.Contests;

[ApiController]
[Route("api/contests")]
public class ContestController
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContestController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }
    
    [HttpGet]
    public async Task<GetContestsQueryResult> GetContests([FromQuery] GetContestsQuery query)
    {
        try
        {
            query.UserId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        } catch (Exception)
        {
            query.UserId = null;
        }

        var result = await _mediator.Send(query);
        return result;
    }

    [HttpPut]
    [Authorize]
    [Route("{contestId:guid}")]
    public async Task<ContestDto> UpdateContest([FromRoute] Guid contestId, UpdateContestCommand command)
    {
        command.ContestId = contestId;
        command.CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(command);
        return result;
    }
    
    [HttpPost]
    [Authorize]
    public async Task<ContestDto> CreateContest(CreateContestCommand command)
    {
        command.CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(command);
        return result;
    }
    
    [HttpGet]
    [Authorize]
    [Route("{contestId:guid}/participants")]
    public async Task<GetContestParticipantsQueryResult> GetContestParticipants([FromRoute] Guid contestId)
    {
        var query = new GetContestParticipantsQuery
        {
            ContestId = contestId,
        };
        var result = await _mediator.Send(query);
        return result;
    }
    
    [HttpPost]
    [Authorize]
    [Route("{contestId:guid}/participants")]
    public async Task AddContestParticipant([FromRoute] Guid contestId, AddContestParticipantCommand command)
    {
        command.CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        command.ContestId = contestId;
        await _mediator.Send(command);
    }
    
    [HttpDelete]
    [Authorize]
    [Route("{contestId:guid}/participants/{userId:guid}")]
    public async Task RemoveContestParticipant([FromRoute] Guid contestId, [FromRoute] Guid userId)
    {
        var command = new RemoveContestParticipantCommand
        {
            CallerId = Guid.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            ContestId = contestId,
            ParticipantId = userId,
        };
        await _mediator.Send(command);
    }
}