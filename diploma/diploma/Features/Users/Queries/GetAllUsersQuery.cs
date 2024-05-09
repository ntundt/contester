using AutoMapper;
using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace diploma.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<List<AdminPanelUserDto>>
{
    public SieveModel? SieveModel { get; set; }
    public Guid CallerId { get; set; }
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<AdminPanelUserDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly SieveProcessor _sieveProcessor;
    private readonly IPermissionService _permissionService;

    public GetAllUsersQueryHandler(ApplicationDbContext context, IMapper mapper, SieveProcessor sieveProcessor,
        IPermissionService permissionService)
    {
        _context = context;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _permissionService = permissionService;
    }

    public async Task<List<AdminPanelUserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContestParticipants))
        {
            throw new NotifyUserException("You don't have permission to view this page");
        }

        var users = _context.Users.AsNoTracking()
            .Include(u => u.UserRole)
            .Where(u => u.Id != request.CallerId);

        if (request.SieveModel is not null)
        {
            users = _sieveProcessor.Apply(request.SieveModel, users, applyPagination: false);
        }

        return _mapper.Map<List<AdminPanelUserDto>>(await users.ToListAsync(cancellationToken));
    }
}