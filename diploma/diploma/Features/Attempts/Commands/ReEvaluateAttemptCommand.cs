using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Scoreboard.Services;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Attempts.Commands;

public class ReEvaluateAttemptCommand : IRequest<AttemptDto>
{
    public Guid AttemptId { get; set; }
    public Guid CallerId { get; set; }
}

public class ReEvaluateAttemptCommandHandler(
    ApplicationDbContext context,
    ISolutionCheckerService solutionCheckerService,
    IPermissionService permissionService,
    ScoreboardUpdateNotifier notifier)
    : IRequestHandler<ReEvaluateAttemptCommand, AttemptDto>
{
    public async Task<AttemptDto> Handle(ReEvaluateAttemptCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageAttempts, cancellationToken))
        {
            throw new NotifyUserException("You do not have a permission to re-evaluate attempts.");
        }

        var attempt = await context.Attempts
            .Include(a => a.Problem)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);
        
        if (attempt == null)
        {
            throw new NotifyUserException("Attempt not found.");
        }
        
        var (status, error) = await solutionCheckerService.RunAsync(attempt.Id, cancellationToken);

        attempt.Status = status;
        attempt.ErrorMessage = error;
        await context.SaveChangesAsync(cancellationToken);

        await notifier.SendScoreboardUpdate(attempt.Problem.ContestId);

        return new AttemptDto
        {
            Id = attempt.Id,
            ProblemId = attempt.ProblemId,
            AuthorId = attempt.AuthorId,
            Status = status,
            CreatedAt = attempt.CreatedAt,
            ErrorMessage = error,
        };
    }
}
