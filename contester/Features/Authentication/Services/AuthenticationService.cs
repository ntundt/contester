using contester.Features.Users;
using contester.Services;
using Microsoft.AspNetCore.Identity;

namespace contester.Features.Authentication.Services;

public interface IAuthenticationService
{
    public string HashPassword(string password);
    public bool VerifyPassword(string password, string hash);
    public string GetEmailConfirmationUrl(Guid token);
    public string GetPasswordRecoveryUrl(Guid token);
}

public class AuthenticationService(IConfigurationReaderService configuration) : IAuthenticationService
{
    public string HashPassword(string password)
    {
        var hasher = new PasswordHasher<User>();
        
        return hasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var hasher = new PasswordHasher<User>();

        return hasher.VerifyHashedPassword(null!, hash, password) == PasswordVerificationResult.Success;
    }

    public string GetEmailConfirmationUrl(Guid token)
    {
        return $"{configuration.GetFrontendUrl()}/confirm-sign-up?token={token}";
    }
    
    public string GetPasswordRecoveryUrl(Guid token)
    {
        return $"{configuration.GetFrontendUrl()}/reset-password?token={token}";
    }
}
