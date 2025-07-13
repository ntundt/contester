using System.Security.Claims;

namespace contester.Features.Authentication.Services;

public interface IAuthorizationService
{
    public Guid GetUserId();
    public Guid? TryGetUserId();
}

public class AuthorizationService(IHttpContextAccessor httpContextAccessor) : IAuthorizationService
{
    public Guid GetUserId()
    {
        if (httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier) == null)
        {
            throw new InvalidOperationException("User is not authenticated");
        }

        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        return Guid.Parse(userId);
    }

    public Guid? TryGetUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        return userId != null ? Guid.Parse(userId) : (Guid?)null;
    }
}
