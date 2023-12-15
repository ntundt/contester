using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Contests.Exceptions;
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
}

public class UpdateContestCommandHandler : IRequestHandler<UpdateContestCommand, ContestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;
    
    public UpdateContestCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, IMapper mapper, IClaimService claimService)
    {
        _context = context;
        _directoryService = directoryService;
        _mapper = mapper;
        _claimService = claimService;
    }
    
    public async Task<ContestDto> Handle(UpdateContestCommand request, CancellationToken cancellationToken)
    {
        var contest =  await _context.Contests.FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }
        
        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageContests", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageContests");
        }
        
        contest.Name = request.Name;
        contest.StartDate = request.StartDate;
        contest.FinishDate = request.FinishDate;
        contest.IsPublic = request.IsPublic;
        await _directoryService.SaveContestDescriptionToFileAsync(contest.Id, request.Description, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ContestDto>(contest);
    }
}