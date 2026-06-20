using AutoMapper;
using contester.Common.Sieve;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace contester.Features.UserGroups.Queries;

public class GetUserGroupsQuery : IRequest<PaginatedResult<PrincipalDto>>
{
    public SieveModel? SieveModel { get; set; }
}

public class GetGroupsQueryHandler(
    IMapper mapper,
    ApplicationDbContext context,
    SieveProcessor sieve): IRequestHandler<GetUserGroupsQuery, PaginatedResult<PrincipalDto>>
{
    public async Task<PaginatedResult<PrincipalDto>> Handle(GetUserGroupsQuery request, CancellationToken ct)
    {
        var count = await context.UserGroups.CountAsync(ct);
        
        if (request.SieveModel is null)
            return new PaginatedResult<PrincipalDto>
            {
                Data = mapper.Map<List<PrincipalDto>>(await context.UserGroups.AsNoTracking().ToListAsync(ct)),
                TotalCount = count,
            };

        return new PaginatedResult<PrincipalDto>
        {
            Data = mapper.Map<List<PrincipalDto>>(
                sieve.Apply(
                    request.SieveModel,
                    context.UserGroups.AsNoTracking()
                )
            ),
            TotalCount = count,
        };
    }
}
