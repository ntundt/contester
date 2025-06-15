using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using diploma.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace diploma.Features.Attempts.Queries;

public class GetAttemptsQuery : IRequest<GetAttemptsQueryResult>
{
    public SieveModel? SieveModel { get; set; } = null!;
    public Guid? ContestId { get; set; }
}

public class GetAttemptsQueryResult
{
    public List<AttemptDto> Attempts { get; set; } = null!;
    public int TotalCount { get; set; }
}

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class GetAttemptsQueryHandler(ApplicationDbContext context, IMapper mapper, SieveProcessor sieveProcessor)
    : IRequestHandler<GetAttemptsQuery, GetAttemptsQueryResult>
{
    public async Task<GetAttemptsQueryResult> Handle(GetAttemptsQuery request, CancellationToken cancellationToken)
    {
        var attempts = context.Attempts.AsNoTracking()
            .Include(x => x.Problem)
            .Include(x => x.Author).AsQueryable();
        if (request.ContestId != null)
        {
            attempts = attempts.Where(x => x.Problem.ContestId == request.ContestId);
        }
        if (request.SieveModel != null)
        {
            attempts = sieveProcessor.Apply(request.SieveModel, attempts);
        }
        var attemptsDto = await mapper.ProjectTo<AttemptDto>(attempts).ToListAsync(cancellationToken);
        var result = new GetAttemptsQueryResult()
        {
            Attempts = attemptsDto,
            TotalCount = attempts.Count(),
        };
        return result;
    }
}