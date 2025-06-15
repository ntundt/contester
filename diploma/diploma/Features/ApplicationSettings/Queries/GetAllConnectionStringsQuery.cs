using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.ApplicationSettings.Queries;

public class GetAllConnectionStringsQuery : IRequest<List<ConnectionString>>
{
    public Guid CallerId { get; set; }
}

public class GetAllConnectionStringsQueryHandler(
    ApplicationDbContext context,
    IPermissionService permissionService) : IRequestHandler<GetAllConnectionStringsQuery, List<ConnectionString>>
{
    public async Task<List<ConnectionString>> Handle(GetAllConnectionStringsQuery request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId,
                Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new NotifyUserException("You do not have permission to view connection strings");
        }
        
        return await context.ConnectionStrings.AsNoTracking().ToListAsync(cancellationToken);
    }
}
