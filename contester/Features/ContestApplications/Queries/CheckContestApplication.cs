using System.Text.Json.Serialization;
using contester.Common.MediatR;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ContestApplications.Queries;

public class CheckContestApplicationQuery : IRequest<CheckContestApplicationQueryResult>, IAuthenticatedRequest
{
    public Guid ContestId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
}

public class CheckContestApplicationQueryResult
{
    public bool AlreadyApplied { get; set; }
    public bool IsApplicationApproved { get; set; }
    public Guid? ApplicationId { get; set; }
}

public class CheckContestApplicationHandler(ApplicationDbContext context)
    : IRequestHandler<CheckContestApplicationQuery, CheckContestApplicationQueryResult>
{
    public async Task<CheckContestApplicationQueryResult> Handle(CheckContestApplicationQuery request, CancellationToken cancellationToken)
    {
        var application = await context.ContestApplications.AsNoTracking()
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
