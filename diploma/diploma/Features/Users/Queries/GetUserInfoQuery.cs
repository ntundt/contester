using AutoMapper;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Users.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Users.Queries;

public class GetUserInfoQuery : IRequest<UserDto>
{
    public Guid Id { get; set; }
    public Guid CallerId { get; set; }
}

public class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQuery, UserDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;

    public GetUserInfoQueryHandler(ApplicationDbContext dbContext, IMapper mapper, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _permissionService = permissionService;
    }

    public async Task<UserDto> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        if (request.CallerId != request.Id && !await _permissionService.UserHasPermissionAsync(request.CallerId, "ManageContestParticipants", cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, "ManageContestParticipants");
        }

        var result = _mapper.Map<UserDto>(user);
        return result;
    }
}