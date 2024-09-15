using diploma.Features.Scoreboard.Queries;
using diploma.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace diploma.Features.Scoreboard.Services;

public class ScoreboardUpdateNotifier
{
    private readonly IMediator _mediator;
    private readonly IHubContext<ScoreboardUpdatesHub> _hub;

    public ScoreboardUpdateNotifier(IMediator mediator, IHubContext<ScoreboardUpdatesHub> hub)
    {
        _mediator = mediator;
        _hub = hub;
    }

    public async Task SendScoreboardUpdate(Guid contestId)
    {
        var scoreboard = await _mediator.Send(new GetScoreboardQuery
        {
            ContestId = contestId,
            CallerId = Guid.Empty
        });

        await _hub.Clients.Group(contestId.ToString()).SendAsync("ReceiveScoreboardUpdate", scoreboard.Rows, contestId.ToString());
    }
}