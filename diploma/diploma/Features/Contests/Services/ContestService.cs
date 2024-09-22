using diploma.Data;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests.Services;

public interface IContestService
{
    bool ContestGoingOn(Guid contestId);
    bool ContestGoingOn(Contest contest);
}

public class ContestService : IContestService
{
    private readonly ApplicationDbContext _context;
    public ContestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public bool ContestGoingOn(Guid contestId)
    {
        var contest = _context.Contests.AsNoTracking()
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
