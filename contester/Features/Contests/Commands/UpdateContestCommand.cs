using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Contests.Exceptions;
using contester.Features.Scoreboard.Services;
using contester.Features.Users.Exceptions;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Commands;

public class UpdateContestCommand : IRequest<ContestDto>, IAuthorizedRequest
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime FinishDate { get; set; }
    public bool IsPublic { get; set; }
    public List<Guid> CommissionMembers { get; set; } = null!;
    public Constants.Permission RequiredPermission { get; set; } =  Constants.Permission.ManageContests;
}

public class UpdateContestCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IFileService fileService,
    ScoreboardUpdateNotifier notifier,
    IScoreboardService scoreboardService)
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
        
        await scoreboardService.RefreshScoreboardEntriesAsync(contest.Id, cancellationToken);

        await notifier.SendScoreboardUpdate(contest.Id);
        
        return mapper.Map<ContestDto>(contest);
    }
}