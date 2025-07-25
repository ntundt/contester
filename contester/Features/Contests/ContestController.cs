﻿using contester.Features.Contests.Commands;
using contester.Features.Contests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace contester.Features.Contests;

[ApiController]
[Route("api/contests")]
public class ContestController(IMediator mediator, Authentication.Services.IAuthorizationService authorizationService)
{
    [HttpGet]
    public async Task<GetContestsQueryResult> GetContests([FromQuery] GetContestsQuery query)
    {
        try
        {
            query.UserId = authorizationService.GetUserId();
        } catch (Exception)
        {
            query.UserId = null;
        }

        var result = await mediator.Send(query);
        return result;
    }

    [HttpPut]
    [Authorize]
    [Route("{contestId:guid}")]
    public async Task<ContestDto> UpdateContest([FromRoute] Guid contestId, UpdateContestCommand command)
    {
        command.ContestId = contestId;
        command.CallerId = authorizationService.GetUserId();
        var result = await mediator.Send(command);
        return result;
    }
    
    [HttpPost]
    [Authorize]
    public async Task<ContestDto> CreateContest(CreateContestCommand command)
    {
        command.CallerId = authorizationService.GetUserId();
        var result = await mediator.Send(command);
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
        var result = await mediator.Send(query);
        return result;
    }
    
    [HttpPost]
    [Authorize]
    [Route("{contestId:guid}/participants")]
    public async Task AddContestParticipant([FromRoute] Guid contestId, AddContestParticipantCommand command)
    {
        command.ContestId = contestId;
        command.CallerId = authorizationService.GetUserId();
        await mediator.Send(command);
    }
    
    [HttpDelete]
    [Authorize]
    [Route("{contestId:guid}/participants/{userId:guid}")]
    public async Task RemoveContestParticipant([FromRoute] Guid contestId, [FromRoute] Guid userId)
    {
        var command = new RemoveContestParticipantCommand
        {
            CallerId = authorizationService.GetUserId(),
            ContestId = contestId,
            ParticipantId = userId,
        };
        await mediator.Send(command);
    }

    [HttpGet("{contestId:guid}/report")]
    [Authorize]
    public async Task<ContestReportDto> GetContestReport([FromRoute] Guid contestId)
    {
        var query = new GetContestReportQuery
        {
            ContestId = contestId,
            CallerId = authorizationService.GetUserId(),
        };
        var result = await mediator.Send(query);
        return result;
    }

    [HttpGet("{contestId:guid}/settings")]
    [Authorize]
    public async Task<ContestSettingsDto> GetContestSettings([FromRoute] Guid contestId)
    {
        var query = new GetContestSettingsQuery
        {
            ContestId = contestId,
            CallerId = authorizationService.GetUserId(),
        };
        var result = await mediator.Send(query);
        return result;
    }
}
