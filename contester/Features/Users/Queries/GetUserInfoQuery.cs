using AutoMapper;
using contester.Data;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Users.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Users.Queries;

public class GetUserInfoQuery : IRequest<UserDto>
{
    public Guid Id { get; set; }
    public Guid CallerId { get; set; }
}

public class GetUserInfoQueryHandler(
    ApplicationDbContext dbContext,
    IMapper mapper,
    IPermissionService permissionService)
    : IRequestHandler<GetUserInfoQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        if (request.CallerId != request.Id && !await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageContestParticipants, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageContestParticipants);
        }

        var result = mapper.Map<UserDto>(user);
        return result;
    }
}
