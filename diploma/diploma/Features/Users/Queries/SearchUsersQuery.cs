using AutoMapper;
using diploma.Data;
using diploma.Features.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class SearchUsersQuery : IRequest<IEnumerable<UserDto>>
{
    public string Search { get; set; }
}

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchUsersQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (request.Search == null) return new List<UserDto>();

        var users = await _context.Users.AsNoTracking()
            .Where(u => (
                u.FirstName + " " + 
                u.LastName + " " +
                u.Patronymic + " " +
                u.Email).IndexOf(request.Search!, StringComparison.CurrentCultureIgnoreCase) >= 0)
            .Take(10)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<UserDto>>(users);
    }
}
