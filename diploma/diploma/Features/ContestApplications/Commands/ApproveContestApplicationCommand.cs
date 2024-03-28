using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.ContestApplications.Commands;

public class ApproveContestApplicationCommand : IRequest<Unit>
{
    public Guid ContestApplicationId { get; set; }
    public Guid CallerId { get; set; }
}

public class ApproveContestApplicationCommandHandler : IRequestHandler<ApproveContestApplicationCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly IClaimService _claimService;

    public ApproveContestApplicationCommandHandler(ApplicationDbContext context, IClaimService claimService)
    {
        _context = context;
        _claimService = claimService;
    }

    private async Task<User> FinishRegistrationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            throw new NotifyUserException("User not found");
        }

        user.UserRoleId = _context.UserRoles.First(x => x.Name == "User").Id;
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<Unit> Handle(ApproveContestApplicationCommand request, CancellationToken cancellationToken)
    {
        var contestApplication = await _context.ContestApplications
            .Include(ca => ca.User)
            .Include(ca => ca.Contest)
            .ThenInclude(c => c.Participants)
            .FirstOrDefaultAsync(ca => ca.Id == request.ContestApplicationId, cancellationToken);
        if (contestApplication is null)
        {
            throw new NotifyUserException("Contest application not found");
        }

        if (!await _claimService.UserHasClaimAsync(request.CallerId, "ManageContestParticipants"))
        {
            throw new NotifyUserException("You don't have enough permissions to manage contest participants");
        }

        contestApplication.Contest.Participants.Add(contestApplication.User);
        
        contestApplication.IsApproved = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}