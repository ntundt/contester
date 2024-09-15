using diploma.Features.Scoreboard;
using Microsoft.AspNetCore.SignalR;

namespace diploma.Hubs;

public class ScoreboardUpdatesHub : Hub
{
    public async Task JoinContestGroup(string contestId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, contestId);
    }
}
