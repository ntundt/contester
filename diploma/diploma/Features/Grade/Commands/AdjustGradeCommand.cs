using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Attempts;
using diploma.Features.Scoreboard.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Grade.Commands;

public class AdjustGradeCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid AttemptId { get; set; }
    public int Grade { get; set; }
    public string Comment { get; set; } = null!;
}

public class AdjustGradeCommandHandler(ApplicationDbContext context, ScoreboardUpdateNotifier notifier) 
    : IRequestHandler<AdjustGradeCommand, Unit>
{
    public async Task<Unit> Handle(AdjustGradeCommand request, CancellationToken cancellationToken)
    {
        var attempt = await context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .ThenInclude(p => p.Contest)
            .ThenInclude(c => c.CommissionMembers)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);

        if (attempt is null) throw new NotifyUserException("Attempt not found");

        var scoreboardApprovals = await context.ScoreboardApprovals.AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.ApprovingUserId == request.UserId && sa.ContestId == attempt.Problem.ContestId, cancellationToken);

        if (scoreboardApprovals is not null) throw new NotifyUserException("You can not adjust grade after you approved the results");

        if (attempt.Status != AttemptStatus.Accepted)
            throw new NotifyUserException("Attempt is not accepted. Grade is forced to be 0");

        if (attempt.Problem.Contest.CommissionMembers.All(cm => cm.Id != request.UserId))
            throw new NotifyUserException("You're not a commission member");
        
        if (request.Grade < 0 || request.Grade > attempt.Problem.MaxGrade * 2)
            throw new NotifyUserException($"Grade must be between 0 and {attempt.Problem.MaxGrade * 2}");
        
        var existingGradeAdjustment = await context.GradeAdjustments
            .FirstOrDefaultAsync(ga => ga.AttemptId == request.AttemptId
                && ga.UserId == request.UserId, cancellationToken);
        
        if (existingGradeAdjustment != null)
        {
            existingGradeAdjustment.Grade = request.Grade;
            existingGradeAdjustment.Comment = request.Comment;
            await context.SaveChangesAsync(cancellationToken);
            
            await context.RefreshScoreboardEntriesAsync();

            await notifier.SendScoreboardUpdate(attempt.Problem.ContestId);
            
            return Unit.Value;
        }
        
        var gradeAdjustment = new GradeAdjustment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            AttemptId = request.AttemptId,
            Grade = request.Grade,
            Comment = request.Comment
        };

        context.GradeAdjustments.Add(gradeAdjustment);
        await context.SaveChangesAsync(cancellationToken);

        await context.RefreshScoreboardEntriesAsync();

        await notifier.SendScoreboardUpdate(attempt.Problem.ContestId);

        return Unit.Value;
    }
}