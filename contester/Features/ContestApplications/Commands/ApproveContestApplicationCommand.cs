using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ContestApplications.Commands;

public class ApproveContestApplicationCommand : IRequest<Unit>, IAuthorizedRequest
{
    public Guid ContestId { get; set; }
    public Guid UserId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class ApproveContestApplicationCommandHandler(ApplicationDbContext context,
    ScoreboardUpdateNotifier notifier,
    IUserGroupService userGroupService,
    IScoreboardService scoreboardService)
    : IRequestHandler<ApproveContestApplicationCommand, Unit>
{
    public async Task<Unit> Handle(ApproveContestApplicationCommand request, CancellationToken ct)
    {
        var contestApplication = await context.ContestApplications
            .Include(ca => ca.User)
            .Include(ca => ca.Contest)
            .FirstOrDefaultAsync(ca => ca.ContestId == request.ContestId && ca.UserId == request.UserId, ct);
        if (contestApplication is null)
            throw new EntityNotFoundException(typeof(ContestApplication), request.ContestId, request.UserId);
        
        await userGroupService.AddUserToGroup(contestApplication.Contest.ParticipantsGroupId, contestApplication.User.Id, ct);
        
        contestApplication.IsApproved = true;
        await context.SaveChangesAsync(ct);
        
        await scoreboardService.RefreshScoreboardEntriesAsync(contestApplication.ContestId, ct);

        await notifier.SendScoreboardUpdate(contestApplication.ContestId);
        
        return Unit.Value;
    }
}
