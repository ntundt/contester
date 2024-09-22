using System.Security.Claims;

namespace diploma.Features.Authentication.Services;

public interface IAuthorizationService
{
    public Guid GetUserId();
    public Guid? TryGetUserId();
}

public class AuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public AuthorizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public Guid GetUserId()
    {
        if (_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier) == null)
        {
            throw new InvalidOperationException("User is not authenticated");
        }

        var userId = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        return Guid.Parse(userId);
    }

    public Guid? TryGetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        return userId != null ? Guid.Parse(userId) : (Guid?)null;
    }
}
