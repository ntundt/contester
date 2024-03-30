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

public class RemoveContestParticipantCommand : IRequest<ContestDto>
{
    public Guid CallerId { get; set; }
    public Guid ContestId { get; set; }
    public Guid ParticipantId { get; set; }
}

public class RemoveContestParticipantCommandHandler : IRequestHandler<RemoveContestParticipantCommand, ContestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;

    public RemoveContestParticipantCommandHandler(ApplicationDbContext context, IMapper mapper, IPermissionService permissionService)
    {
        _context = context;
        _mapper = mapper;
        _permissionService = permissionService;
    }

    public async Task<ContestDto> Handle(RemoveContestParticipantCommand request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests
            .Include(c => c.Participants)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        if (contest == null)
        {
            throw new ContestNotFoundException(request.ContestId);
        }

        var participant = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ParticipantId, cancellationToken);
        if (participant == null)
        {
            throw new UserNotFoundException();
        }

        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, "ManageContestParticipants", cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, "ManageContestParticipants");
        }

        var contestApplication = _context.ContestApplications
            .FirstOrDefault(ca => ca.ContestId == contest.Id && ca.UserId == participant.Id);
        if (contestApplication != null)
        {
            contestApplication.IsApproved = false;
        }

        contest.Participants.Remove(participant);
        await _context.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<ContestDto>(contest);
        return result;
    }
}