using AutoMapper;
using contester.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace contester.Features.SchemaDescriptions.Queries;

public class GetSchemaDescriptionsQuery : IRequest <GetSchemaDescriptionsQueryResult>
{
    public SieveModel? SieveModel { get; set; }
}

public class GetSchemaDescriptionsQueryResult
{
    public List<SchemaDescriptionDto> SchemaDescriptions { get; set; } = null!;
}

public class GetSchemaDescriptionsQueryHandler(
    ApplicationDbContext context,
    IMapper mapper,
    SieveProcessor sieveProcessor)
    : IRequestHandler<GetSchemaDescriptionsQuery, GetSchemaDescriptionsQueryResult>
{
    public async Task<GetSchemaDescriptionsQueryResult> Handle(GetSchemaDescriptionsQuery request, CancellationToken cancellationToken)
    {
        var schemaDescriptions = await context.SchemaDescriptions.AsNoTracking()
            .Include(s => s.Files)
            .ToListAsync(cancellationToken);
        
        var schemaDescriptionDtos = mapper.Map<List<SchemaDescriptionDto>>(schemaDescriptions);
        
        schemaDescriptionDtos = sieveProcessor.Apply(request.SieveModel, schemaDescriptionDtos.AsQueryable(), applyPagination: false)
            .ToList();

        return new GetSchemaDescriptionsQueryResult
        {
            SchemaDescriptions = schemaDescriptionDtos
        };
    }
}
