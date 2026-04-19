using AutoMapper;
using contester.Common.MediatR;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Queries;

public class GetSchemaDescriptionFilesQuery : IRequest<GetSchemaDescriptionFilesQueryResult>, IAuthorizedRequest
{
    public Guid SchemaDescriptionId { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission  RequiredPermission { get; set; } = Constants.Permission.ManageSchemaDescriptions;
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
