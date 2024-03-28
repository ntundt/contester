using diploma.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.ContestApplications.Queries;

public class CheckContestApplicationQuery : IRequest<CheckContestApplicationQueryResult>
{
    public Guid ContestId { get; set; }
    public Guid CallerId { get; set; }
}

public class CheckContestApplicationQueryResult
{
    public bool AlreadyApplied { get; set; }
    public bool IsApplicationApproved { get; set; }
    public Guid? ApplicationId { get; set; }
}

public class CheckContestApplicationHandler : IRequestHandler<CheckContestApplicationQuery, CheckContestApplicationQueryResult>
{
    private readonly ApplicationDbContext _context;

    public CheckContestApplicationHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<CheckContestApplicationQueryResult> Handle(CheckContestApplicationQuery request, CancellationToken cancellationToken)
    {
        var application = await _context.ContestApplications.AsNoTracking()
            .FirstOrDefaultAsync(ca => ca.ContestId == request.ContestId && ca.UserId == request.CallerId, cancellationToken);

        if (application is null)
        {
            return new CheckContestApplicationQueryResult
            {
                AlreadyApplied = false,
                IsApplicationApproved = false,
                ApplicationId = null
            };
        }

        return new CheckContestApplicationQueryResult
        {
            AlreadyApplied = true,
            IsApplicationApproved = application.IsApproved,
            ApplicationId = application.Id
        };
    }
}
