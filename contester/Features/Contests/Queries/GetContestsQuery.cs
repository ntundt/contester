using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Services;
using contester.Services;
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
    IMapper mapper,
    SieveProcessor sieveProcessor,
    IPermissionService permissionService,
    IFileService fileService)
    : IRequestHandler<GetContestsQuery, GetContestsQueryResult>
{
    private readonly IMapper _mapper = mapper;
    private readonly IPermissionService _permissionService = permissionService;

    public Task<GetContestsQueryResult> Handle(GetContestsQuery request, CancellationToken cancellationToken)
    {
        var contests = context.Contests.AsNoTracking();
        
        if (request.Sieve != null)
        {
            contests = sieveProcessor.Apply(request.Sieve, contests);
        }

        contests = contests.Include(c => c.Participants)
            .Include(c => c.CommissionMembers);

        var result = contests.Select(c => new ContestParticipationDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = fileService.ReadApplicationDirectoryFileAllText(c.DescriptionPath),
                IsPublic = c.IsPublic,
                CreatedAt = c.CreatedAt,
                StartDate = c.StartDate,
                FinishDate = c.FinishDate,
                AuthorId = c.AuthorId,
                UserParticipates = c.Participants.Any(p => p.Id == request.UserId),
            }).ToList();
        
        return Task.FromResult(new GetContestsQueryResult { Contests = result });
    }
}
