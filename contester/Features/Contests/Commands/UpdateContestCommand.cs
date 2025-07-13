using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Contests.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.Users.Exceptions;
using contester.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class UpdateContestCommand : IRequest<ContestDto>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public bool IsPublic { get; set; }
    public List<Guid> CommissionMembers { get; set; } = null!;
}

public class UpdateContestCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IPermissionService permissionService,
    IFileService fileService,
    ScoreboardUpdateNotifier notifier)
    : IRequestHandler<UpdateContestCommand, ContestDto>
{
    private readonly IDirectoryService _directoryService = directoryService;

    public async Task<ContestDto> Handle(UpdateContestCommand request, CancellationToken cancellationToken)
    {
        var contest =  await context.Contests
            .Include(c => c.Participants)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }
        
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContests, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageContests);
        }
        
        contest.Name = request.Name;
        contest.StartDate = request.StartDate;
        contest.FinishDate = request.FinishDate;
        contest.IsPublic = request.IsPublic;

        contest.CommissionMembers = await context.Users
            .Where(u => request.CommissionMembers.Contains(u.Id))
            .ToListAsync(cancellationToken);
        if (contest.CommissionMembers.Count != request.CommissionMembers.Count)
        {
            throw new UserNotFoundException();
        }

        await fileService.SaveContestDescriptionToFileAsync(contest.Id, request.Description, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        await context.RefreshScoreboardEntriesAsync();

        await notifier.SendScoreboardUpdate(contest.Id);
        
        return mapper.Map<ContestDto>(contest);
    }
}