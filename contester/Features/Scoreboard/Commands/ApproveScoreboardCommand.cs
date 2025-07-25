using contester.Data;
using contester.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Scoreboard.Commands;

public class ApproveScoreboardCommand : IRequest<Unit>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class ApproveScoreboardCommandHandler(ApplicationDbContext context)
    : IRequestHandler<ApproveScoreboardCommand, Unit>
{
    public async Task<Unit> Handle(ApproveScoreboardCommand request, CancellationToken cancellationToken)
    {
        var existingApproval = await context.ScoreboardApprovals.AsNoTracking()
            .Include(a => a.Contest)
            .FirstOrDefaultAsync(a => a.ContestId == request.ContestId, cancellationToken);
        if (existingApproval != null)
        {
            throw new NotifyUserException("You already approved the scoreboard");
        }

        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new NotifyUserException("Contest not found");
        }

        if (contest.CommissionMembers.All(cm => cm.Id != request.CallerId))
        {
            throw new NotifyUserException("You are not a commission member");
        }

        if (contest.FinishDate > DateTime.UtcNow)
        {
            throw new NotifyUserException("Contest is not finished yet");
        }

        var approval = new ScoreboardApproval
        {
            ApprovingUserId = request.CallerId,
            ContestId = request.ContestId,
        };
        context.ScoreboardApprovals.Add(approval);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}