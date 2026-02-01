using AutoMapper;
using contester.Features.Common.Exceptions;
using contester.Features.Authentication.Services;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Queries;

public class GetSchemaDescriptionFilesQuery : IRequest<GetSchemaDescriptionFilesQueryResult>
{
    public Guid SchemaDescriptionId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetSchemaDescriptionFilesQueryResult
{
    public List<SchemaDescriptionFileDto> SchemaDescriptionFiles { get; set; } = null!;
}

public class GetSchemaDescriptionFilesQueryHandler(ApplicationDbContext context, IMapper mapper, IPermissionService permissionService)
    : IRequestHandler<GetSchemaDescriptionFilesQuery, GetSchemaDescriptionFilesQueryResult>
{
    public async Task<GetSchemaDescriptionFilesQueryResult> Handle(GetSchemaDescriptionFilesQuery request, CancellationToken cancellationToken)
    {
        var schemaDescriptionFiles = await context.SchemaDescriptionFiles.AsNoTracking()
            .Where(s => s.SchemaDescriptionId == request.SchemaDescriptionId)
            .ToListAsync(cancellationToken);

        if (!await permissionService.UserHasPermissionAsync(request.CallerId,
                Constants.Permission.ManageSchemaDescriptions, cancellationToken))
            throw new NotifyUserException("You do not have a permission to do that");
        
        return new GetSchemaDescriptionFilesQueryResult
        {
            SchemaDescriptionFiles = mapper.Map<List<SchemaDescriptionFileDto>>(schemaDescriptionFiles)
        };
    }
}
