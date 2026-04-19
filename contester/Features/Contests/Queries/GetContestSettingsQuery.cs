using AutoMapper;
using contester.Features.Authentication.Services;
using contester.Features.Contests.Exceptions;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests.Queries;

public class GetContestSettingsQuery : IRequest<ContestSettingsDto>
{
    public Guid ContestId { get; set; }
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
            ?? throw new ContestNotFoundException(request.ContestId);

        return mapper.Map<ContestSettingsDto>(contest);
    }
}
