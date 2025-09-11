using System.Security.Authentication;
using System.Security.Claims;
using System.Web;
using contester.Features.Users;
using contester.Services;
using Microsoft.AspNetCore.Identity;
using SignInResult = contester.Features.Authentication.Commands.Common.SignInResult;

namespace contester.Features.Authentication.Services;

public interface IAuthenticationService
{
    public SignInResult IssueCredentials(Guid userId, string roleName);
    public SignInResult ReIssueCredentials(string refreshToken, string roleName);
    public string GetEmailConfirmationUrl(Guid token);
    public string GetEmailAuthenticationUrl(Guid token);
    public string GetPasswordRecoveryUrl(Guid token);
    public string GetEmailSignInUrl(string email, Guid emailCode);
    public string GetEmailSignUpUrl(string email, Guid emailCode);
}

public class AuthenticationService(IConfigurationReaderService configuration, IJwtService jwtService) : IAuthenticationService
{
    public SignInResult IssueCredentials(Guid userId, string roleName)
    {
        return new SignInResult
        {
            AccessToken = jwtService.IssueAccessToken(userId.ToString(), roleName),
            AccessTokenTtlSeconds = (int)configuration.GetAccessTokenTtl().TotalSeconds,
            RefreshToken = jwtService.IssueRefreshToken(userId.ToString()),
            RefreshTokenTtlSeconds = (int)configuration.GetRefreshTokenTtl().TotalSeconds,
        };
    }
    
    public SignInResult ReIssueCredentials(string refreshToken, string roleName)
    {
        var userId = jwtService.ExtractUserId(refreshToken);
        return new SignInResult
        {
            AccessToken = jwtService.IssueAccessToken(userId.ToString(), roleName),
            AccessTokenTtlSeconds = (int)configuration.GetAccessTokenTtl().TotalSeconds,
            RefreshToken = refreshToken,
            RefreshTokenTtlSeconds = (int)(jwtService.ExtractExpiry(refreshToken) - DateTime.UtcNow).TotalSeconds,
        };
    }
    
    public string GetEmailConfirmationUrl(Guid token)
    {
        return $"{configuration.GetFrontendUrl()}/confirm-sign-up?token={token}";
    }

    public string GetEmailAuthenticationUrl(Guid token)
    {
        return $"{configuration.GetFrontendUrl()}/email-code-sign-up?token={token}";
    }

    public string GetPasswordRecoveryUrl(Guid token)
    {
        return $"{configuration.GetFrontendUrl()}/reset-password?token={token}";
    }

    public string GetEmailSignInUrl(string email, Guid emailCode)
    {
        return $"{configuration.GetFrontendUrl()}/email-code-sign-in?email={HttpUtility.UrlEncode(email)}&emailCode={emailCode}";
    }

    public string GetEmailSignUpUrl(string email, Guid emailCode)
    {
        return $"{configuration.GetFrontendUrl()}/email-code-sign-up?email={HttpUtility.UrlEncode(email)}&emailCode={emailCode}";
    }
}
