using System.Text.Json.Serialization;
using contester.Features.Common.Exceptions;
using contester.Features.ContestApplications.Exceptions;
using contester.Features.Contests;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ContestApplications.Commands;

public class ApplyForContestCommand : IRequest<Unit>
{
    public Guid ContestId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
}

public class ApplyForContestCommandHandler(ApplicationDbContext context) : IRequestHandler<ApplyForContestCommand, Unit>
{
    public async Task<Unit> Handle(ApplyForContestCommand request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.ContestApplications)
            .FirstOrDefaultAsync(x => x.Id == request.ContestId, cancellationToken);
        if (contest is null)
            throw new EntityNotFoundException(typeof(Contest), request.ContestId);

        if (contest.ContestApplications.Any(ca => ca.UserId == request.CallerId))
            throw new AlreadyAppliedForContestException();
        
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
