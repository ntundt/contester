using contester.Common.MediatR;
using contester.Features.Authentication.Services;
using contester.Features.Common.Exceptions;
using MediatR;

namespace contester.Infrastructure.MediatRBehaviors;

public class AuthorizationBehavior<TRequest, TResponse>(
    IPermissionService permissionService
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAuthorizedRequest rq)
            if (!await permissionService.UserHasPermissionAsync(rq.CallerId, rq.RequiredPermission, cancellationToken))
                throw new NotifyUserException("You do not have a permission to do that");
        
        return await next();
    }
}
