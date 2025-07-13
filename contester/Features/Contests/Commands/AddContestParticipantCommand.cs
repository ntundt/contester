using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Contests.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.Users.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class AddContestParticipantCommand : IRequest<ContestDto>
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public Guid ParticipantId { get; set; }
}

public class AddContestParticipantCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IPermissionService permissionService,
    ScoreboardUpdateNotifier notifier)
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

        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContestParticipants, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageContestParticipants);
        }

        contest.Participants.Add(participant);
        await context.SaveChangesAsync(cancellationToken);

        await context.RefreshScoreboardEntriesAsync();

        await notifier.SendScoreboardUpdate(contest.Id);

        var result = mapper.Map<ContestDto>(contest);
        return result;
    }
}
