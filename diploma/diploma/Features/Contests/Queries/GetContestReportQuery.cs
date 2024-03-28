using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Contests.Exceptions;
using diploma.Features.Scoreboard.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests.Queries;

public class GetContestReportQuery : IRequest<ContestReportDto>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetContestReportQueryHandler : IRequestHandler<GetContestReportQuery, ContestReportDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IClaimService _claimsService;

    public GetContestReportQueryHandler(ApplicationDbContext context, IMediator mediator, IClaimService claimsService)
    {
        _context = context;
        _mediator = mediator;
        _claimsService = claimsService;
    }

    public async Task<ContestReportDto> Handle(GetContestReportQuery request, CancellationToken cancellationToken)
    {
        if (!await _claimsService.UserHasClaimAsync(request.CallerId, "ManageContests", cancellationToken))
        {
            throw new NotifyUserException("You do not have permission to view this contest report.");
        }

        var contest = await _context.Contests.AsNoTracking()
            .Include(c => c.Participants)
            .Include(c => c.ContestApplications)
            .ThenInclude(ca => ca.User)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        
        if (contest is null) throw new ContestNotFoundException(request.ContestId);

        var scoreboard = await _mediator.Send(new GetScoreboardQuery
        {
            ContestId = request.ContestId,
            CallerId = request.CallerId,
        }, cancellationToken);

        var users = _context.Users.AsNoTracking()
            .AsEnumerable()
            .Where(u => scoreboard.Rows.Any(r => r.UserId == u.Id));

        var attemptsCount = await _context.Attempts.AsNoTracking()
            .Where(a => a.Problem.Contest.Id == request.ContestId)
            .GroupBy(a => a.AuthorId)
            .Select(g => new { UserId = g.Key, AttemptsCount = g.Count() })
            .ToListAsync(cancellationToken);

        var report = new ContestReportDto
        {
            ContestId = contest.Id,
            ContestName = contest.Name,
            StartDate = contest.StartDate,
            FinishDate = contest.FinishDate,
            IsPublic = contest.IsPublic,
            Participants = scoreboard.Rows.Select(r => {
                var user = users.First(u => u.Id == r.UserId);
                return new ContestReportUserDto {
                    UserId = r.UserId,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Patronymic = r.Patronymic,
                    AdditionalInfo = user.AdditionalInfo,
                    Email = user.Email,
                    LastLoginUtc = user.LastLogin,
                    SignedUpUtc = user.CreatedAt,
                    Score = r.FinalGrade,
                    AttemptsCount = attemptsCount.FirstOrDefault(a => a.UserId == r.UserId)?.AttemptsCount ?? 0,
                };
            }).OrderByDescending(u => u.Score).ToList()
        };
        
        return report;
    }
}
