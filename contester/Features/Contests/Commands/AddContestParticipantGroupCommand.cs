using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.UserGroups;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class AddContestParticipantGroupCommand : IRequest<ContestDto>, IAuthorizedRequest
{
    public Guid ContestId { get; set; }
    public Guid GroupId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore] public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class AddContestParticipantGroupCommandHandler(
    ApplicationDbContext context,
    IScoreboardService scoreboardService,
    ScoreboardUpdateNotifier notifier,
    IUserGroupService userGroupService,
    IMapper mapper
) : IRequestHandler<AddContestParticipantGroupCommand, ContestDto>
{
    public async Task<ContestDto> Handle(AddContestParticipantGroupCommand request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests
            .Include(c => c.ParticipantsGroup)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
            throw new EntityNotFoundException(typeof(Contest), request.ContestId);

        var userGroup = await context.UserGroups
            .FirstOrDefaultAsync(u => u.Id == request.GroupId, cancellationToken);
        if (userGroup == null)
            throw new EntityNotFoundException(typeof(UserGroup), request.GroupId);

        await userGroupService.AddGroupToGroup(contest.ParticipantsGroupId, request.GroupId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await scoreboardService.RefreshScoreboardEntriesAsync(contest.Id, cancellationToken);

        await notifier.SendScoreboardUpdate(contest.Id);

        var result = mapper.Map<ContestDto>(contest);
        return result;
    }
}
