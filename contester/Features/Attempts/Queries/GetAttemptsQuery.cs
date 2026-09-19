using AutoMapper;
using contester.Common.Sieve;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace contester.Features.Attempts.Queries;

public class GetAttemptsQuery : IRequest<PaginatedResult<AttemptDto>>
{
    public SieveModel? SieveModel { get; set; } = null!;
    public Guid? ContestId { get; set; }
}

public class GetAttemptsQueryHandler(ApplicationDbContext context, IMapper mapper, SieveProcessor sieveProcessor)
    : IRequestHandler<GetAttemptsQuery, PaginatedResult<AttemptDto>>
{
    public async Task<PaginatedResult<AttemptDto>> Handle(GetAttemptsQuery request, CancellationToken cancellationToken)
    {
        var attempts = context.Attempts.AsNoTracking()
            .Include(x => x.Problem)
            .Include(x => x.Author).AsQueryable();
        if (request.ContestId != null)
        {
            attempts = attempts.Where(x => x.Problem.ContestId == request.ContestId);
        }
        int totalCount;
        if (request.SieveModel != null)
        {
            totalCount = await sieveProcessor.Apply(request.SieveModel, attempts, applyPagination: false)
                .CountAsync(cancellationToken);
            attempts = sieveProcessor.Apply(request.SieveModel, attempts);
        }
        else
        {
            totalCount = await attempts.CountAsync(cancellationToken);
        }
        var attemptsDto = await mapper.ProjectTo<AttemptDto>(attempts).ToListAsync(cancellationToken);
        return new PaginatedResult<AttemptDto>
        {
            Data = attemptsDto,
            TotalCount = totalCount,
        };
    }
}