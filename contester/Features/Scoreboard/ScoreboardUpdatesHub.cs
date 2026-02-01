using Microsoft.AspNetCore.SignalR;

namespace contester.Features.Scoreboard;

public class ScoreboardUpdatesHub : Hub
{
    public async Task JoinContestGroup(string contestId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, contestId);
    }
}
