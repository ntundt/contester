using contester.Features.Scoreboard;
using Microsoft.AspNetCore.SignalR;

namespace contester.Hubs;

public class ScoreboardUpdatesHub : Hub
{
    public async Task JoinContestGroup(string contestId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, contestId);
    }
}
