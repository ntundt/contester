using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.ContestApplications.Commands;

public class ApplyForContestCommand : IRequest<Unit>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class ApplyForContestCommandHandler : IRequestHandler<ApplyForContestCommand, Unit>
{
    private readonly ApplicationDbContext _context;

    public ApplyForContestCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ApplyForContestCommand request, CancellationToken cancellationToken)
    {
        var contest = await _context.Contests.AsNoTracking()
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

        _context.ContestApplications.Add(contestApplication);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
