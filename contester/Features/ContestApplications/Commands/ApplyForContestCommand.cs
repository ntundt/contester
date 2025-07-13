using contester.Data;
using contester.Exceptions;
using contester.Features.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ContestApplications.Commands;

public class ApplyForContestCommand : IRequest<Unit>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class ApplyForContestCommandHandler(ApplicationDbContext context) : IRequestHandler<ApplyForContestCommand, Unit>
{
    public async Task<Unit> Handle(ApplyForContestCommand request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ContestId, cancellationToken);
        if (contest is null)
        {
            throw new NotifyUserException("Contest not found");
        }

        var contestApplication = new ContestApplication
        {
            Id = Guid.NewGuid(),
            ContestId = contest.Id,
            UserId = request.CallerId,
            IsApproved = false,
        };

        context.ContestApplications.Add(contestApplication);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
