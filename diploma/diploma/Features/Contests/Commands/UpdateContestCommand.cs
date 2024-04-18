using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Contests.Exceptions;
using diploma.Features.Users.Exceptions;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests.Commands;

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

public class UpdateContestCommandHandler : IRequestHandler<UpdateContestCommand, ContestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    
    public UpdateContestCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, IMapper mapper, IPermissionService permissionService)
    {
        _context = context;
        _directoryService = directoryService;
        _mapper = mapper;
        _permissionService = permissionService;
    }
    
    public async Task<ContestDto> Handle(UpdateContestCommand request, CancellationToken cancellationToken)
    {
        var contest =  await _context.Contests
            .Include(c => c.Participants)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }
        
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContests, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageContests);
        }
        
        contest.Name = request.Name;
        contest.StartDate = request.StartDate;
        contest.FinishDate = request.FinishDate;
        contest.IsPublic = request.IsPublic;

        contest.CommissionMembers = await _context.Users
            .Where(u => request.CommissionMembers.Contains(u.Id))
            .ToListAsync(cancellationToken);
        if (contest.CommissionMembers.Count != request.CommissionMembers.Count)
        {
            throw new UserNotFoundException();
        }

        await _directoryService.SaveContestDescriptionToFileAsync(contest.Id, request.Description, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ContestDto>(contest);
    }
}