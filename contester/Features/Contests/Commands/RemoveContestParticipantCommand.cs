using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Contests.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.Users.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class RemoveContestParticipantCommand : IRequest<ContestDto>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public Guid ParticipantId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageContestParticipants;
}

public class RemoveContestParticipantCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    ScoreboardUpdateNotifier notifier,
    IScoreboardService scoreboardService)
    : IRequestHandler<RemoveContestParticipantCommand, ContestDto>
{
    public async Task<ContestDto> Handle(RemoveContestParticipantCommand request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests
            .Include(c => c.Participants)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }

        var participant = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ParticipantId, cancellationToken);
        if (participant == null)
        {
            throw new UserNotFoundException();
        }
        
        var contestApplication = context.ContestApplications
            .FirstOrDefault(ca => ca.ContestId == contest.Id && ca.UserId == participant.Id);
        if (contestApplication != null)
        {
            contestApplication.IsApproved = false;
        }

        contest.Participants.Remove(participant);
        await context.SaveChangesAsync(cancellationToken);

        await scoreboardService.RefreshScoreboardEntriesAsync(contest.Id, cancellationToken);

        await notifier.SendScoreboardUpdate(contest.Id);

        var result = mapper.Map<ContestDto>(contest);
        return result;
    }
}
