using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.UserGroups.Services;
using contester.Features.Users;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class AddContestParticipantUserCommand : IRequest<ContestDto>, IAuthorizedRequest
{
    public Guid ContestId { get; set; }
    public Guid ParticipantId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class AddContestParticipantUserCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    ScoreboardUpdateNotifier notifier,
    IUserGroupService userGroupService,
    IScoreboardService scoreboardService)
    : IRequestHandler<AddContestParticipantUserCommand, ContestDto>
{
    public async Task<ContestDto> Handle(AddContestParticipantUserCommand request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests
            .Include(c => c.ParticipantsGroup)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
            throw new EntityNotFoundException(typeof(Contest), request.ContestId);

        var participant = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ParticipantId, cancellationToken);
        if (participant == null)
            throw new EntityNotFoundException(typeof(User), request.ParticipantId);

        await userGroupService.AddUserToGroup(contest.ParticipantsGroupId, participant.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await scoreboardService.RefreshScoreboardEntriesAsync(contest.Id, cancellationToken);

        await notifier.SendScoreboardUpdate(contest.Id);

        var result = mapper.Map<ContestDto>(contest);
        return result;
    }
}
