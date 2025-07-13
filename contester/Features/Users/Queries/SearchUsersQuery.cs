using AutoMapper;
using contester.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Users.Queries;

public class SearchUsersQuery : IRequest<IEnumerable<UserDto>>
{
    public string? Search { get; set; } = null!;
}

public class SearchUsersQueryHandler(ApplicationDbContext context, IMapper mapper)
    : IRequestHandler<SearchUsersQuery, IEnumerable<UserDto>>
{
    public async Task<IEnumerable<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (request.Search == null) return new List<UserDto>();

        var users = (await context.Users.AsNoTracking()
                .ToListAsync(cancellationToken))
            .Where(u => (
                u.FirstName + " " + 
                u.LastName + " " +
                u.Patronymic + " " +
                u.Email).IndexOf(request.Search!, StringComparison.CurrentCultureIgnoreCase) >= 0)
            .Take(10);

        return mapper.Map<IEnumerable<UserDto>>(users);
    }
}
