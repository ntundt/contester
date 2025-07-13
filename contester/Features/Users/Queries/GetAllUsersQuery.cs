using AutoMapper;
using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace contester.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<List<AdminPanelUserDto>>
{
    public SieveModel? SieveModel { get; set; }
    public Guid CallerId { get; set; }
}

public class GetAllUsersQueryHandler(
    ApplicationDbContext context,
    IMapper mapper,
    SieveProcessor sieveProcessor,
    IPermissionService permissionService)
    : IRequestHandler<GetAllUsersQuery, List<AdminPanelUserDto>>
{
    public async Task<List<AdminPanelUserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContestParticipants, cancellationToken))
        {
            throw new NotifyUserException("You don't have permission to view this page");
        }

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