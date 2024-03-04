using diploma.Data;
using diploma.Features.Attempts;
using diploma.Features.Contests.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Scoreboard.Queries;

public class GetScoreboardQuery : IRequest<GetScoreboardQueryResult>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetScoreboardQueryResult
{
    public List<ScoreboardEntryDto> Rows { get; set; } = null!;
    public bool UserCanManageGrades { get; set; }
}

public class GetScoreboardQueryHandler : IRequestHandler<GetScoreboardQuery, GetScoreboardQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IGradeCalculationService _gradeCalculationService;

    public GetScoreboardQueryHandler(ApplicationDbContext context, IGradeCalculationService gradeCalculationService)
    {
        _context = context;
        _gradeCalculationService = gradeCalculationService;
    }

    public async Task<GetScoreboardQueryResult> Handle(GetScoreboardQuery request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests.AsNoTracking()
            .Include(c => c.Problems.OrderBy(p => p.Ordinal))
            .Include(c => c.Participants)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }

        var attempts = await _context.Attempts.AsNoTracking()
            .Include(a => a.Author)
            .Where(a => a.Problem.ContestId == request.ContestId)
            .ToListAsync(cancellationToken);

        var nonParticipatingAttempteesIds = attempts
            .Where(a => contest.Participants.All(p => p.Id != a.AuthorId))
            .Select(a => a.AuthorId)
            .Distinct()
            .ToList();
        var nonParticipatingAttemptees = await _context.Users.AsNoTracking()
            .Where(u => nonParticipatingAttempteesIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var rows = new List<ScoreboardEntryDto>();
        foreach (var user in contest.Participants.Concat(nonParticipatingAttemptees))
        {
            var row = new ScoreboardEntryDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic ?? "",
                AdditionalInfo = user.AdditionalInfo,
                Fee = 0,
                FinalGrade = 0,
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
                    MaxGrade = problem.MaxGrade,
                    Grade = 0,
                    SolvingAttemptId = solvedAttempt?.Id ?? Guid.Empty,
                };
                if (solvedAttempt != null)
                {
                    entry.Grade = await _gradeCalculationService.CalculateAttemptGrade(solvedAttempt.Id, cancellationToken);
                }
                row.Fee += problemAttempts.Count(a => a.Status != AttemptStatus.Accepted) * 10;
                row.Problems.Add(entry);
            }
            row.FinalGrade = row.Problems.Sum(p => p.Grade);
            rows.Add(row);
        }

        return new GetScoreboardQueryResult
        {
            Rows = rows.OrderByDescending(r => r.FinalGrade).ToList(),
            UserCanManageGrades = contest.CommissionMembers.Any(cm => cm.Id == request.CallerId),
        };
    }
}