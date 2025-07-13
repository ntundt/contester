using contester.Data;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Services;

public interface IContestService
{
    bool ContestGoingOn(Guid contestId);
    bool ContestGoingOn(Contest contest);
}

public class ContestService(ApplicationDbContext context) : IContestService
{
    public bool ContestGoingOn(Guid contestId)
    {
        var contest = context.Contests.AsNoTracking()
            .FirstOrDefault(c => c.Id == contestId);

        if (contest == null)
        {
            throw new ArgumentException("Contest not found");
        }

        return ContestGoingOn(contest);
    }

    public bool ContestGoingOn(Contest contest)
    {
        return contest.StartDate <= DateTime.UtcNow && contest.FinishDate >= DateTime.UtcNow;
    }
}
