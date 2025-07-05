using AutoMapper;
using diploma.Data;
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

public class GetScoreboardQueryHandler(ApplicationDbContext context, IGradeCalculationService gradeCalculationService,
    IMapper mapper)
    : IRequestHandler<GetScoreboardQuery, GetScoreboardQueryResult>
{
    public async Task<GetScoreboardQueryResult> Handle(GetScoreboardQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }
        
        var entries = await context.ScoreboardEntries
            .Where(se => se.ContestId == contest.Id)
            .ToListAsync(cancellationToken);

        return new GetScoreboardQueryResult()
        {
            Rows = mapper.Map<List<ScoreboardEntryDto>>(entries),
            UserCanManageGrades = contest.CommissionMembers.Any(cm => cm.Id == request.CallerId),
        };
    }
}
