using AutoMapper;
using diploma.Data;
using diploma.Features.Contests.Exceptions;
using diploma.Features.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests.Queries;

public class GetContestParticipantsQuery : IRequest<GetContestParticipantsQueryResult>
{
    public Guid ContestId { get; set; }   
}

public class GetContestParticipantsQueryResult
{
    public List<UserDto> ContestParticipants { get; set; } = null!;
}

public class GetContestParticipantsQueryHandler : IRequestHandler<GetContestParticipantsQuery, GetContestParticipantsQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetContestParticipantsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<GetContestParticipantsQueryResult> Handle(GetContestParticipantsQuery request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests.AsNoTracking()
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }
        
        var contestParticipants = contest.Participants;

        var result = new GetContestParticipantsQueryResult
        {
            ContestParticipants = _mapper.Map<List<UserDto>>(contestParticipants),
        };
        return result;
    }
}