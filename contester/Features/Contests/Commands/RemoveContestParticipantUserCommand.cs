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

public class RemoveContestParticipantUserCommand : IRequest<ContestDto>, IAuthorizedRequest
{
    [JsonIgnore]
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public Guid ParticipantId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class RemoveContestParticipantUserCommandHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IUserGroupService userGroupService,
    ScoreboardUpdateNotifier notifier,
    IScoreboardService scoreboardService)
    : IRequestHandler<RemoveContestParticipantUserCommand, ContestDto>
{
    public async Task<ContestDto> Handle(RemoveContestParticipantUserCommand request, CancellationToken cancellationToken)
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
        
        var contestApplication = context.ContestApplications
            .FirstOrDefault(ca => ca.ContestId == contest.Id && ca.UserId == participant.Id);
        if (contestApplication != null)
        {
            contestApplication.IsApproved = false;
        }

        await userGroupService.RemoveUserFromGroup(contest.ParticipantsGroupId, participant.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await scoreboardService.RefreshScoreboardEntriesAsync(contest.Id, cancellationToken);

        await notifier.SendScoreboardUpdate(contest.Id);

        var result = mapper.Map<ContestDto>(contest);
        return result;
    }
}
