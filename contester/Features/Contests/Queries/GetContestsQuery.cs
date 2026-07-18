using contester.Features.UserGroups.Services;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace contester.Features.Contests.Queries;

public class GetContestsQuery : IRequest<GetContestsQueryResult>
{
    public Guid? UserId { get; set; }
    public SieveModel? Sieve { get; set; }
}

public class GetContestsQueryResult
{
    public List<ContestParticipationDto> Contests { get; set; } = null!;
}

public class GetContestsQueryHandler(
    ApplicationDbContext context,
    SieveProcessor sieveProcessor,
    IUserGroupService userGroupService,
    IFileService fileService)
    : IRequestHandler<GetContestsQuery, GetContestsQueryResult>
{
    public async Task<GetContestsQueryResult> Handle(GetContestsQuery request, CancellationToken ct)
    {
        var contestsQuery = context.Contests.AsNoTracking();

        if (request.Sieve != null)
        {
            contestsQuery = sieveProcessor.Apply(request.Sieve, contestsQuery);
        }

        var contests = await contestsQuery
            .Include(c => c.ParticipantsGroup)
            .Include(c => c.CommissionMembers)
            .ToListAsync(ct);

        var result = new List<ContestParticipationDto>(contests.Count);
        foreach (var contest in contests)
        {
            result.Add(new ContestParticipationDto
            {
                Id = contest.Id,
                Name = contest.Name,
                Description = await fileService.ReadApplicationDirectoryFileAllTextAsync(contest.DescriptionPath, ct),
                IsPublic = contest.IsPublic,
                CreatedAt = contest.CreatedAt,
                StartDate = contest.StartDate,
                FinishDate = contest.FinishDate,
                AuthorId = contest.AuthorId,
                UserParticipates = request.UserId.HasValue
                    && await userGroupService.UserIsGroupMember(contest.ParticipantsGroupId, request.UserId.Value, ct),
            });
        }

        return new GetContestsQueryResult { Contests = result };
    }
}
