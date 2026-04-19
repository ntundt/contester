using contester.Common.MediatR;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.ApplicationSettings.Queries;

public class GetAllConnectionStringsQuery : IRequest<List<ConnectionString>>, IAuthorizedRequest
{
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageSchemaDescriptions;
}

public class GetAllConnectionStringsQueryHandler(
    ApplicationDbContext context) : IRequestHandler<GetAllConnectionStringsQuery, List<ConnectionString>>
{
    public async Task<List<ConnectionString>> Handle(GetAllConnectionStringsQuery request, CancellationToken cancellationToken)
    {
        return await context.ConnectionStrings.AsNoTracking().ToListAsync(cancellationToken);
    }
}
