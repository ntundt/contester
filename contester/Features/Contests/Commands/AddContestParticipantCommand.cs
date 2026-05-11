using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Contests.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.Users.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class AddContestParticipantCommand : IRequest<ContestDto>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public Guid ParticipantId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageContestParticipants;
}

public class AddContestParticipantCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    ScoreboardUpdateNotifier notifier,
    IScoreboardService scoreboardService)
    : IRequestHandler<AddContestParticipantCommand, ContestDto>
{
    public async Task<ContestDto> Handle(AddContestParticipantCommand request, CancellationToken cancellationToken)
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

        contest.Participants.Add(participant);
        await context.SaveChangesAsync(cancellationToken);

        await scoreboardService.RefreshScoreboardEntriesAsync(contest.Id, cancellationToken);

        await notifier.SendScoreboardUpdate(contest.Id);

        var result = mapper.Map<ContestDto>(contest);
        return result;
    }
}
