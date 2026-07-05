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
        var contests = context.Contests.AsNoTracking();
        
        if (request.Sieve != null)
        {
            contests = sieveProcessor.Apply(request.Sieve, contests);
        }

        contests = contests.Include(c => c.ParticipantsGroup)
            .Include(c => c.CommissionMembers);

        var result = contests.AsEnumerable().Select(async c => new ContestParticipationDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = await fileService.ReadApplicationDirectoryFileAllTextAsync(c.DescriptionPath, ct),
                IsPublic = c.IsPublic,
                CreatedAt = c.CreatedAt,
                StartDate = c.StartDate,
                FinishDate = c.FinishDate,
                AuthorId = c.AuthorId,
                UserParticipates = request.UserId.HasValue && await userGroupService.UserIsGroupMember(c.ParticipantsGroupId, request.UserId.Value, ct),
            }).ToList();
        
        return new GetContestsQueryResult { Contests = [..await Task.WhenAll(result)] };
    }
}
