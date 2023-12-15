using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Contests.Exceptions;
using diploma.Features.Users.Exceptions;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Contests.Commands;

public class AddContestParticipantCommand : IRequest<ContestDto>
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public string ParticipantEmail { get; set; }
}

public class AddContestParticipantCommandHandler : IRequestHandler<AddContestParticipantCommand, ContestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IClaimService _claimService;

    public AddContestParticipantCommandHandler(ApplicationDbContext context, IMapper mapper, IClaimService claimService)
    {
        _context = context;
        _mapper = mapper;
        _claimService = claimService;
    }

    public async Task<ContestDto> Handle(AddContestParticipantCommand request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }

        var participant = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.ParticipantEmail, cancellationToken);
        if (participant == null)
        {
            throw new UserNotFoundException();
        }

        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageContestParticipants", cancellationToken))
        {
            throw new UserDoesNotHaveClaimException(request.CallerId, "ManageContestParticipants");
        }

        contest.Participants.Add(participant);
        await _context.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<ContestDto>(contest);
        return result;
    }
}