using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Services;
using diploma.Features.Users;
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
    public List<ContestParticipationDto> Contests { get; set; } = null!;
}

public class GetContestsQueryHandler : IRequestHandler<GetContestsQuery, GetContestsQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly SieveProcessor _sieveProcessor;
    private readonly IPermissionService _permissionService;
    
    public GetContestsQueryHandler(ApplicationDbContext context, IMapper mapper, SieveProcessor sieveProcessor, IPermissionService permissionService)
    {
        _context = context;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _permissionService = permissionService;
    }
/*
    private async Task<IQueryable<Contest>> GetAvailableContests(IQueryable<Contest> contests, Guid? userId)
    {
        if (userId == null)
        {
            return contests.Where(c => c.IsPublic);
        }
        
        if (await _permissionService.UserHasClaimAsync(userId.Value, "ManageContests"))
        {
            return contests;
        }
        
        return contests.Where(c => c.IsPublic || c.Author.Id == userId || c.Participants.Any(p => p.Id == userId));
    }
*/
    public Task<GetContestsQueryResult> Handle(GetContestsQuery request, CancellationToken cancellationToken)
    {
        var contests = _context.Contests.AsNoTracking();
        
        if (request.Sieve != null)
        {
            contests = _sieveProcessor.Apply(request.Sieve, contests);
        }

        contests = contests.Include(c => c.Participants)
            .Include(c => c.CommissionMembers);
        
        var result = contests.Select(c => new ContestParticipationDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = File.ReadAllText(c.DescriptionPath),
            IsPublic = c.IsPublic,
            CreatedAt = c.CreatedAt,
            StartDate = c.StartDate,
            FinishDate = c.FinishDate,
            AuthorId = c.AuthorId,
            UserParticipates = c.Participants.Any(p => p.Id == request.UserId),
        }).ToList();
        
        return Task.FromResult(new GetContestsQueryResult { Contests = result });
    }
}