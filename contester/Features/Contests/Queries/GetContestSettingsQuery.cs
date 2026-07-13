using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Common.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Queries;

public class GetContestSettingsQuery : IRequest<ContestSettingsDto>, IAuthenticatedRequest
{
    public Guid ContestId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
}

public class GetContestSettingsQueryHandler(
    ApplicationDbContext context,
    IMapper mapper)
    : IRequestHandler<GetContestSettingsQuery, ContestSettingsDto>
{
    public async Task<ContestSettingsDto> Handle(GetContestSettingsQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Contest), request.ContestId);

        return mapper.Map<ContestSettingsDto>(contest);
    }
}
