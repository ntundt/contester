using AutoMapper;
using contester.Features.Common.Exceptions;
using contester.Features.UserGroups;
using contester.Features.UserGroups.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Queries;

public class GetContestParticipantsQuery : IRequest<GetContestParticipantsQueryResult>
{
    public Guid ContestId { get; set; }
}

public class GetContestParticipantsQueryResult
{
    public List<PrincipalDto> ContestParticipants { get; set; } = null!;
}

public class GetContestParticipantsQueryHandler(
    ApplicationDbContext context,
    IUserGroupMapperService userGroupMapperService
) : IRequestHandler<GetContestParticipantsQuery, GetContestParticipantsQueryResult>
{
    public async Task<GetContestParticipantsQueryResult> Handle(GetContestParticipantsQuery request, CancellationToken ct)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.ParticipantsGroup)
            .Include(c => c.ContestApplications)
            .ThenInclude(ca => ca.User)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, ct)
            ?? throw new EntityNotFoundException(typeof(Contest), request.ContestId);

        var participants = await userGroupMapperService.GetUserGroupMembers(contest.ParticipantsGroupId, ct);

        return new GetContestParticipantsQueryResult
        {
            ContestParticipants = participants,
        };
    }
}
