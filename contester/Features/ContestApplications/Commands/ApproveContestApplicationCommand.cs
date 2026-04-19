using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ContestApplications.Commands;

public class ApproveContestApplicationCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid ContestApplicationId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageContestParticipants;
}

public class ApproveContestApplicationCommandHandler(ApplicationDbContext context,
    ScoreboardUpdateNotifier notifier,
    ScoreboardService scoreboardService)
    : IRequestHandler<ApproveContestApplicationCommand, Unit>
{
    public async Task<Unit> Handle(ApproveContestApplicationCommand request, CancellationToken cancellationToken)
    {
        var contestApplication = await context.ContestApplications
            .Include(ca => ca.User)
            .Include(ca => ca.Contest)
            .ThenInclude(c => c.Participants)
            .FirstOrDefaultAsync(ca => ca.Id == request.ContestApplicationId, cancellationToken);
        if (contestApplication is null)
        {
            throw new NotifyUserException("Contest application not found");
        }
        
        contestApplication.Contest.Participants.Add(contestApplication.User);
        
        contestApplication.IsApproved = true;
        await context.SaveChangesAsync(cancellationToken);
        
        await scoreboardService.RefreshScoreboardEntriesAsync(contestApplication.ContestId);

        await notifier.SendScoreboardUpdate(contestApplication.ContestId);
        
        return Unit.Value;
    }
}
