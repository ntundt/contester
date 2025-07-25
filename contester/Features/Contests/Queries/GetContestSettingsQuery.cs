using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Services;
using contester.Features.Contests.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Contests;

public class GetContestSettingsQuery : IRequest<ContestSettingsDto>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetContestSettingsQueryHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IPermissionService permissionService)
    : IRequestHandler<GetContestSettingsQuery, ContestSettingsDto>
{
    private readonly IPermissionService _permissionService = permissionService;

    public async Task<ContestSettingsDto> Handle(GetContestSettingsQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken)
            ?? throw new ContestNotFoundException(request.ContestId);

        return mapper.Map<ContestSettingsDto>(contest);
    }
}
