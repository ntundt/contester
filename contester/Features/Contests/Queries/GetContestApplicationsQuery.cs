using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Features.UserGroups;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Queries;

public class GetContestApplicationsQuery : IRequest<List<PrincipalDto>>, IAuthorizedRequest
{
    public Guid ContestId { get; set; }
    [JsonIgnore] 
    public Guid CallerId { get; set; }
    [JsonIgnore]
    public Constants.Permission RequiredPermission => Constants.Permission.ManageContestParticipants;
}

public class GetContestApplicationsQueryHandler(
    ApplicationDbContext context,
    IMapper mapper
) : IRequestHandler<GetContestApplicationsQuery, List<PrincipalDto>>
{
    public async Task<List<PrincipalDto>> Handle(GetContestApplicationsQuery request, CancellationToken ct)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(p => p.ContestApplications)
            .ThenInclude(ca => ca.User)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, ct);
        if (contest is null)
            throw new EntityNotFoundException(typeof(Contest), request.ContestId);
        
        return mapper.Map<List<PrincipalDto>>(contest.ContestApplications
            .Where(ca => !ca.IsApproved)
            .Select(ca => ca.User)
            .ToList());
    }
}
