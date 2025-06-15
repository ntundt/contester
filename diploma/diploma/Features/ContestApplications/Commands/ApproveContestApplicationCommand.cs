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

public class ApproveContestApplicationCommandHandler(ApplicationDbContext context, IPermissionService permissionService)
    : IRequestHandler<ApproveContestApplicationCommand, Unit>
{
    private async Task<User> FinishRegistrationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new NotifyUserException("User not found");
        }

        user.UserRoleId = context.UserRoles.First(x => x.Name == "User").Id;
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<Unit> Handle(ApproveContestApplicationCommand request, CancellationToken cancellationToken)
    {
        var contestApplication = await context.ContestApplications
            .Include(ca => ca.User)
            .Include(ca => ca.Contest)
            .ThenInclude(c => c.Participants)
            .FirstOrDefaultAsync(ca => ca.Id == request.ContestApplicationId, cancellationToken);
        if (contestApplication is null)
        {
            throw new NotifyUserException("Contest application not found");
        }

        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContestParticipants, cancellationToken))
        {
            throw new NotifyUserException("You don't have enough permissions to manage contest participants");
        }

        contestApplication.Contest.Participants.Add(contestApplication.User);
        
        contestApplication.IsApproved = true;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}