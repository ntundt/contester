using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.Grade.Services;
using contester.Features.Scoreboard.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Attempts.Commands;

public class ReEvaluateAttemptCommand : IRequest<AttemptDto>, IAuthorizedRequest
{
    public Guid AttemptId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } =  Constants.Permission.ManageAttempts;
}

public class ReEvaluateAttemptCommandHandler(
    ApplicationDbContext context,
    ISolutionCheckerService solutionCheckerService,
    ScoreboardUpdateNotifier notifier,
    ScoreboardService scoreboardService)
    : IRequestHandler<ReEvaluateAttemptCommand, AttemptDto>
{
    public async Task<AttemptDto> Handle(ReEvaluateAttemptCommand request, CancellationToken cancellationToken)
    {
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

        await scoreboardService.RefreshScoreboardEntriesAsync(attempt.Problem.ContestId);

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
