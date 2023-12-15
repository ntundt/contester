using diploma.Data;
using diploma.Features.Attempts;
using diploma.Features.Contests.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Scoreboard.Queries;

public class GetScoreboardQuery : IRequest<GetScoreboardQueryResult>
{
    public Guid ContestId { get; set; }
}

public class GetScoreboardQueryResult
{
    public List<ScoreboardEntryDto> Rows { get; set; } = null!;
}

public class GetScoreboardQueryHandler : IRequestHandler<GetScoreboardQuery, GetScoreboardQueryResult>
{
    private readonly ApplicationDbContext _context;

    public GetScoreboardQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetScoreboardQueryResult> Handle(GetScoreboardQuery request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests.AsNoTracking()
            .Include(c => c.Problems)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }

        var attempts = await _context.Attempts.AsNoTracking()
            .Where(a => a.Problem.ContestId == request.ContestId)
            .ToListAsync(cancellationToken);

        var rows = new List<ScoreboardEntryDto>();
        foreach (var user in contest.Participants)
        {
            var row = new ScoreboardEntryDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic ?? "",
                AdditionalInfo = user.AdditionalInfo,
                Fee = 0,
                Problems = new List<ScoreboardProblemEntryDto>(),
            };
            foreach (var problem in contest.Problems)
            {
                var problemAttempts = attempts.Where(a => a.ProblemId == problem.Id && a.AuthorId == user.Id).ToList();
                var solvedAttempt = problemAttempts.FirstOrDefault(a => a.Status == AttemptStatus.Accepted);
                var entry = new ScoreboardProblemEntryDto
                {
                    ProblemId = problem.Id,
                    Name = problem.Name,
                    AttemptsCount = problemAttempts.Count(),
                    IsSolved = solvedAttempt != null,
                    SolvedAt = solvedAttempt?.CreatedAt ?? DateTime.MinValue,
                };
                row.Fee += problemAttempts.Count(a => a.Status != AttemptStatus.Accepted) * 10;
                row.Problems.Add(entry);
            }
            rows.Add(row);
        }

        return new GetScoreboardQueryResult
        {
            Rows = rows.OrderByDescending(r => r.Problems.Sum(p => p.IsSolved ? 1 : 0))
                .ThenBy(r => r.Fee)
                .ToList(),
        };
    }
}