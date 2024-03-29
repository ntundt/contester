using AutoMapper;
using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Contests.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests;

public class GetContestSettingsQuery : IRequest<ContestSettingsDto>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetContestSettingsQueryHandler : IRequestHandler<GetContestSettingsQuery, ContestSettingsDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;

    public GetContestSettingsQueryHandler(ApplicationDbContext context, IMapper mapper, IClaimService claimService)
    {
        _context = context;
        _mapper = mapper;
        _claimService = claimService;
    }

    public async Task<ContestSettingsDto> Handle(GetContestSettingsQuery request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests.AsNoTracking()
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken)
            ?? throw new ContestNotFoundException(request.ContestId);

        return _mapper.Map<ContestSettingsDto>(contest);
    }
}