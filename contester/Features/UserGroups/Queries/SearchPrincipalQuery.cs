using AutoMapper;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.UserGroups.Queries;

public class SearchPrincipalQuery : IRequest<List<PrincipalDto>>
{
    public required string SearchString { get; set; }    
}

public class SearchPrincipalQueryHandler(
    IMapper mapper,
    ApplicationDbContext context) : IRequestHandler<SearchPrincipalQuery, List<PrincipalDto>>
{
    public async Task<List<PrincipalDto>> Handle(SearchPrincipalQuery request, CancellationToken ct)
    {
        var users = await context.Users.AsNoTracking()
            .Where(u => (
                u.FirstName + " " +
                u.LastName + " " +
                u.Patronymic + " " +
                u.Email).ToLower().IndexOf(request.SearchString.ToLower()) >= 0)
            .Take(10)
            .ToListAsync(ct);

        var group = await context.UserGroups.AsNoTracking()
            .Where(ug => ug.Name.ToLower().Contains(request.SearchString.ToLower()))
            .Take(10)
            .ToListAsync(ct);

        return mapper.Map<List<PrincipalDto>>(group)
            .Concat(mapper.Map<List<PrincipalDto>>(users))
            .ToList();
    }
}
