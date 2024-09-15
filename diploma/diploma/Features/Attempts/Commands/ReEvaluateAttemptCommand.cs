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

public partial class ReEvaluateAttemptCommandHandler : IRequestHandler<ReEvaluateAttemptCommand, AttemptDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly ISolutionRunnerService _solutionRunnerService;
    private readonly IPermissionService _permissionService;
    private readonly ScoreboardUpdateNotifier _scoreboardUpdateNotifier;

    public ReEvaluateAttemptCommandHandler(ApplicationDbContext context, IDirectoryService directoryService,
        ISolutionRunnerService solutionRunnerService, IPermissionService permissionService, ScoreboardUpdateNotifier notifier)
    {
        _context = context;
        _directoryService = directoryService;
        _solutionRunnerService = solutionRunnerService;
        _permissionService = permissionService;
        _scoreboardUpdateNotifier = notifier;
    }

    public async Task<AttemptDto> Handle(ReEvaluateAttemptCommand request, CancellationToken cancellationToken)
    {
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageAttempts, cancellationToken))
        {
            throw new NotifyUserException("You do not have permission to re-evaluate attempts.");
        }

        var attempt = await _context.Attempts
            .Include(a => a.Problem)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);
        
        if (attempt == null)
        {
            throw new NotifyUserException("Attempt not found.");
        }

        var solutionPath = _directoryService.GetAttemptPath(attempt.Id);
        var solution = await File.ReadAllTextAsync(solutionPath, cancellationToken);

        var (status, error) = await _solutionRunnerService.RunAsync(attempt.Id, cancellationToken);

        attempt.Status = status;
        attempt.ErrorMessage = error;
        await _context.SaveChangesAsync(cancellationToken);

        await _scoreboardUpdateNotifier.SendScoreboardUpdate(attempt.Problem.ContestId);

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