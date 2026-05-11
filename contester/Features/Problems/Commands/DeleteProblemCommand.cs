using contester.Common.MediatR;
using contester.Features.Authentication.Services;
using contester.Features.Problems.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Commands;

public class DeleteProblemCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public Constants.Permission RequiredPermission { get; set; } =  Constants.Permission.ManageProblems;
}

public class DeleteProblemCommandHandler(
    ApplicationDbContext context,
    IScoreboardService scoreboardService,
    ScoreboardUpdateNotifier notifier)
    : IRequestHandler<DeleteProblemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProblemCommand request, CancellationToken cancellationToken)
    {
        var problem = await context.Problems.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        
        var contestId = problem.ContestId;
        
        context.Problems.Remove(problem);
        
        await context.SaveChangesAsync(cancellationToken);
        
        await scoreboardService.RefreshScoreboardEntriesAsync(contestId, cancellationToken);
        await notifier.SendScoreboardUpdate(problem.ContestId);
        
        return Unit.Value;
    }
}
