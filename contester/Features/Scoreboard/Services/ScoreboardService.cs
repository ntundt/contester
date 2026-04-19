using contester.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Scoreboard.Services;

public class ScoreboardService(ApplicationDbContext context, ILogger<ScoreboardService> logger)
{
    public async Task RefreshScoreboardEntriesAsync(Guid contestId)
    {
        var sql = $"CALL refresh_scoreboard(uuid '{contestId}')";
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
