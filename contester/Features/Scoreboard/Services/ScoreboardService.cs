using contester.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Scoreboard.Services;

public interface IScoreboardService
{
    Task RefreshScoreboardEntriesAsync(Guid contestId, CancellationToken ct);
    Task UpdateScoreboardAttemptIncrementallyAsync(Guid attemptId, bool reEvaluation, CancellationToken ct);
    Task UpdateScoreboardAdjustmentAsync(Guid attemptId, CancellationToken ct);
}

public class ScoreboardService(ApplicationDbContext context) : IScoreboardService
{
    public async Task RefreshScoreboardEntriesAsync(Guid contestId, CancellationToken ct)
    {
        await context.Database.ExecuteSqlAsync($"CALL refresh_scoreboard({contestId})", ct);
    }

    public async Task UpdateScoreboardAttemptIncrementallyAsync(Guid attemptId, bool reEvaluation, CancellationToken ct)
    {
        await context.Database.ExecuteSqlAsync($"CALL update_scoreboard_incrementally({attemptId}, {reEvaluation})", ct);
    }

    public async Task UpdateScoreboardAdjustmentAsync(Guid attemptId, CancellationToken ct)
    {
        await context.Database.ExecuteSqlAsync($"CALL update_scoreboard_adjustments({attemptId})", ct);
    }
}
