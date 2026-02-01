using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using contester.Infrastructure;
using Duende.IdentityServer;
using Microsoft.IdentityModel.Tokens;

namespace contester.Features.Authentication.Services;

public interface IJwtService
{
    string IssueAccessToken(string userId, string role);
    string IssueRefreshToken(string userId);
    bool ValidateAccessToken(string token);
    bool ValidateRefreshToken(string token);
    string? ExtractClaim(string token, string claimType);
    public Guid ExtractUserId(string token);
    public DateTime ExtractExpiry(string token);
}

public class JwtService(IConfigurationReaderService configuration) : IJwtService
{
    public string IssueAccessToken(string userId, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(configuration.GetJwtKey());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role),
            ]),
            Expires = DateTime.UtcNow.Add(configuration.GetAccessTokenTtl()),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration.GetJwtIssuer(),
            Audience = $"{configuration.GetJwtIssuer()}/access",
            TokenType = IdentityServerConstants.TokenTypes.AccessToken,
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string IssueRefreshToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(configuration.GetJwtKey());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId),
            ]),
            Expires = DateTime.UtcNow.Add(configuration.GetRefreshTokenTtl()),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration.GetJwtIssuer(),
            Audience = $"{configuration.GetJwtIssuer()}/identity",
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public bool ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration.GetJwtKey());
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration.GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = $"{configuration.GetJwtIssuer()}/access",
                ClockSkew = TimeSpan.Zero,
            }, out SecurityToken validatedToken);
            return true;
        }
        catch (SecurityTokenValidationException)
        {
            return false;
        }
    }

    public bool ValidateRefreshToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration.GetJwtKey());
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration.GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = $"{configuration.GetJwtIssuer()}/identity",
                ClockSkew = TimeSpan.Zero,

            }, out SecurityToken validatedToken);
            return true;
        }
        catch (SecurityTokenValidationException)
        {
            return false;
        }
    }

    public string? ExtractClaim(string token, string claimType)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration.GetJwtKey());
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration.GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudiences = [ $"{configuration.GetJwtIssuer()}/identity", $"{configuration.GetJwtIssuer()}/access" ],
                ClockSkew = TimeSpan.Zero,

            }, out SecurityToken validatedToken);
            return ((JwtSecurityToken)validatedToken).Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        }
        catch (SecurityTokenValidationException)
        {
            return null;
        }
    }
    
    public Guid ExtractUserId(string token)
    {
        var userIdString = ExtractClaim(token, "nameid");
        if (userIdString is null)
            throw new AuthenticationException("User id has not been provided in the token");
        return Guid.Parse(userIdString);
    }

    public DateTime ExtractExpiry(string token)
    {
        var expirationString = ExtractClaim(token, "exp");
        if (expirationString is null)
            throw new AuthenticationException("Expiration has not been provided in the token");
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dateTime.AddSeconds(Int64.Parse(expirationString));
    }
}
