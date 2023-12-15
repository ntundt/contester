using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace diploma.Features.Contests.Queries;

public class GetContestsQuery : IRequest<GetContestsQueryResult>
{
    public Guid? UserId { get; set; }
    public SieveModel? Sieve { get; set; }
}

public class GetContestsQueryResult
{
    public List<ContestDto> Contests { get; set; } = null!;
}

public class GetContestsQueryHandler : IRequestHandler<GetContestsQuery, GetContestsQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly SieveProcessor _sieveProcessor;
    private readonly IClaimService _claimService;
    
    public GetContestsQueryHandler(ApplicationDbContext context, IMapper mapper, SieveProcessor sieveProcessor, IClaimService claimService)
    {
        _context = context;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _claimService = claimService;
    }
    
    private async Task<IQueryable<Contest>> GetAvailableContests(IQueryable<Contest> contests, Guid? userId)
    {
        if (userId == null)
        {
            return contests.Where(c => c.IsPublic);
        }
        
        if (await _claimService.UserHasClaimAsync(userId.Value, "ManageContests"))
        {
            return contests;
        }
        
        return contests.Where(c => c.IsPublic || c.Author.Id == userId || c.Participants.Any(p => p.Id == userId));
    }

    public async Task<GetContestsQueryResult> Handle(GetContestsQuery request, CancellationToken cancellationToken)
    {
        var contests = _context.Contests.AsNoTracking();

        contests = await GetAvailableContests(contests, request.UserId);
        
        if (request.Sieve != null)
        {
            contests = _sieveProcessor.Apply(request.Sieve, contests);
        }
        
        var result = _mapper.ProjectTo<ContestDto>(contests);
        
        
        return new GetContestsQueryResult
        {
            Contests = await result.ToListAsync(cancellationToken)
        };
    }
}