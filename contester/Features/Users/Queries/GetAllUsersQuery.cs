using AutoMapper;
using contester.Common.MediatR;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace contester.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<List<AdminPanelUserDto>>, IAuthorizedRequest
{
    public SieveModel? SieveModel { get; set; }
    public Guid CallerId { get; set; }
    public Constants.Permission RequiredPermission { get; set; } = Constants.Permission.ManageContestParticipants;
}

public class GetAllUsersQueryHandler(
    ApplicationDbContext context,
    IMapper mapper,
    SieveProcessor sieveProcessor)
    : IRequestHandler<GetAllUsersQuery, List<AdminPanelUserDto>>
{
    public async Task<List<AdminPanelUserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .Where(u => u.Id != request.CallerId);

        if (request.SieveModel is not null)
        {
            users = sieveProcessor.Apply(request.SieveModel, users, applyPagination: false);
        }

        return mapper.Map<List<AdminPanelUserDto>>(await users.ToListAsync(cancellationToken));
    }
}