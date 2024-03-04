using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests.Commands;

public class CreateContestCommand : IRequest<ContestDto>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublic { get; set; }
    public Guid CallerId { get; set; }
    public List<Guid> Participants { get; set; } = null!;
}

public class CreateContestCommandHandler : IRequestHandler<CreateContestCommand, ContestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;
    private readonly IDirectoryService _directoryService;

    public CreateContestCommandHandler(ApplicationDbContext context, IMapper mapper, IClaimService claimService, IDirectoryService directoryService)
    {
        _context = context;
        _mapper = mapper;
        _claimService = claimService;
        _directoryService = directoryService;
    }

    public async Task<ContestDto> Handle(CreateContestCommand request, CancellationToken cancellationToken)
    {
        var author = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.CallerId, cancellationToken);
        if (author == null)
        {
            throw new UserNotFoundException();
        }
        
        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageContests", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageContests");
        }
        
        var contest = new Contest()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StartDate = request.StartDate,
            FinishDate = request.EndDate,
            IsPublic = request.IsPublic,
            AuthorId = author.Id,
            Participants = await _context.Users.Where(u => request.Participants.Contains(u.Id)).ToListAsync(cancellationToken),
            CommissionMembers = [author]
        };
        contest.DescriptionPath = _directoryService.GetContestDescriptionPath(contest.Id);
        
        await _directoryService.SaveContestDescriptionToFileAsync(contest.Id, request.Description, cancellationToken);
        
        _context.Contests.Add(contest);
        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ContestDto>(contest);
    }
}