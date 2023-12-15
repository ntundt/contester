using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using diploma.Data;
using diploma.Services;
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
public class GetAttemptsQueryHandler : IRequestHandler<GetAttemptsQuery, GetAttemptsQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly SieveProcessor _sieveProcessor;

    public GetAttemptsQueryHandler(ApplicationDbContext context, IMapper mapper, SieveProcessor sieveProcessor)
    {
        _context = context;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
    }
    
    public async Task<GetAttemptsQueryResult> Handle(GetAttemptsQuery request, CancellationToken cancellationToken)
    {
        var attempts = _context.Attempts.AsNoTracking()
            .Include(x => x.Problem)
            .Include(x => x.Author).AsQueryable();
        if (request.ContestId != null)
        {
            attempts = attempts.Where(x => x.Problem.ContestId == request.ContestId);
        }
        if (request.SieveModel != null)
        {
            attempts = _sieveProcessor.Apply(request.SieveModel, attempts);
        }
        var attemptsDto = await _mapper.ProjectTo<AttemptDto>(attempts).ToListAsync(cancellationToken);
        var result = new GetAttemptsQueryResult()
        {
            Attempts = attemptsDto,
            TotalCount = attempts.Count(),
        };
        return result;
    }
}