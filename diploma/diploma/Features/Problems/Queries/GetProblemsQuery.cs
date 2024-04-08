using AutoMapper;
using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Contests.Exceptions;
using diploma.Features.Contests.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Problems.Queries;

public class GetProblemsQuery : IRequest<GetProblemsQueryResult>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetProblemsQueryResult
{
    public List<ProblemDto> Problems { get; set; } = null!;
}

public class GetProblemsQueryHandler : IRequestHandler<GetProblemsQuery, GetProblemsQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly IContestService _contestService;
    
    public GetProblemsQueryHandler(ApplicationDbContext context, IMapper mapper, IPermissionService permissionService,
        IContestService contestService)
    {
        _context = context;
        _mapper = mapper;
        _permissionService = permissionService;
        _contestService = contestService;
    }
    
    public async Task<GetProblemsQueryResult> Handle(GetProblemsQuery request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests.AsNoTracking()
            .Include(c => c.Participants)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);

        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }

        if (!contest.IsPublic)
        {
            if (!_contestService.ContestGoingOn(contest)
                && !await _permissionService.UserHasPermissionAsync(request.CallerId, "ManageContests", cancellationToken) 
                && contest.Participants.All(p => p.Id != request.CallerId)
                && contest.CommissionMembers.All(cm => cm.Id != request.CallerId))
            {
                throw new NotifyUserException("You do not have permission to view this contest's problems.");
            }
        }

        var problems = await _context.Problems.AsNoTracking()
            .Include(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files.Where(f => !f.HasProblems))
            .Where(p => p.ContestId == request.ContestId)
            .OrderBy(p => p.Ordinal)
            .ToListAsync(cancellationToken);
        return new GetProblemsQueryResult
        {
            Problems = _mapper.Map<List<ProblemDto>>(problems)
        };
    }
}