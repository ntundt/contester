using AutoMapper;
using diploma.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.SchemaDescriptions.Queries;

public class GetSchemaDescriptionFilesQuery : IRequest<GetSchemaDescriptionFilesQueryResult>
{
    public Guid SchemaDescriptionId { get; set; }
}

public class GetSchemaDescriptionFilesQueryResult
{
    public List<SchemaDescriptionFileDto> SchemaDescriptionFiles { get; set; } = null!;
}

public class GetSchemaDescriptionFilesQueryHandler(ApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetSchemaDescriptionFilesQuery, GetSchemaDescriptionFilesQueryResult>
{
    public async Task<GetSchemaDescriptionFilesQueryResult> Handle(GetSchemaDescriptionFilesQuery request, CancellationToken cancellationToken)
    {
        var schemaDescriptionFiles = await context.SchemaDescriptionFiles.AsNoTracking()
            .Where(s => s.SchemaDescriptionId == request.SchemaDescriptionId)
            .ToListAsync(cancellationToken);

        return new GetSchemaDescriptionFilesQueryResult
        {
            SchemaDescriptionFiles = mapper.Map<List<SchemaDescriptionFileDto>>(schemaDescriptionFiles)
        };
    }
}
