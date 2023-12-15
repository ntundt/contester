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

public class GetSchemaDescriptionFilesQueryHandler : IRequestHandler<GetSchemaDescriptionFilesQuery, GetSchemaDescriptionFilesQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetSchemaDescriptionFilesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<GetSchemaDescriptionFilesQueryResult> Handle(GetSchemaDescriptionFilesQuery request, CancellationToken cancellationToken)
    {
        var schemaDescriptionFiles = await _context.SchemaDescriptionFiles.AsNoTracking()
            .Where(s => s.SchemaDescriptionId == request.SchemaDescriptionId)
            .ToListAsync(cancellationToken);

        return new GetSchemaDescriptionFilesQueryResult
        {
            SchemaDescriptionFiles = _mapper.Map<List<SchemaDescriptionFileDto>>(schemaDescriptionFiles)
        };
    }
}