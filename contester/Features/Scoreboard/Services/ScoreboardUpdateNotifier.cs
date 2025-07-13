using contester.Features.Scoreboard.Queries;
using contester.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace contester.Features.Scoreboard.Services;

public class ScoreboardUpdateNotifier(IMediator mediator, IHubContext<ScoreboardUpdatesHub> hub)
{
    public async Task SendScoreboardUpdate(Guid contestId)
    {
        var scoreboard = await mediator.Send(new GetScoreboardQuery
        {
            ContestId = contestId,
            CallerId = Guid.Empty
        });

        await hub.Clients.Group(contestId.ToString()).SendAsync("ReceiveScoreboardUpdate", scoreboard.Rows, contestId.ToString());
    }
}
