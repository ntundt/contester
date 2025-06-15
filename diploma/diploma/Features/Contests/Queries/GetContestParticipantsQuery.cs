using AutoMapper;
using diploma.Data;
using diploma.Features.Contests.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests.Queries;

public class GetContestParticipantsQuery : IRequest<GetContestParticipantsQueryResult>
{
    public Guid ContestId { get; set; }
}

public class GetContestParticipantsQueryResult
{
    public List<ContestParticipantDto> ContestParticipants { get; set; } = null!;
}

public class GetContestParticipantsQueryHandler(ApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetContestParticipantsQuery, GetContestParticipantsQueryResult>
{
    public async Task<GetContestParticipantsQueryResult> Handle(GetContestParticipantsQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.Participants)
            .Include(c => c.ContestApplications)
            .ThenInclude(ca => ca.User)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken)
            ?? throw new ContestNotFoundException(request.ContestId);

        var participants = contest.Participants
            .Select(mapper.Map<ContestParticipantDto>)
            .ToList();

        participants.ForEach(p => p.IsApplicationApproved = true);

        if (!contest.IsPublic)
        {
            var unapprovedParticipants = contest.ContestApplications
                .Where(ca => !ca.IsApproved)
                .Select(ca => 
                {
                    var p = mapper.Map<ContestParticipantDto>(ca.User);
                    p.IsApplicationApproved = false;
                    p.ApplicationId = ca.Id;
                    return p;
                })
                .ToList();
            
            participants.AddRange(unapprovedParticipants);
        }

        var result = new GetContestParticipantsQueryResult
        {
            ContestParticipants = [.. participants.OrderBy(p => p.Id)]
        };
        return result;
    }
}
