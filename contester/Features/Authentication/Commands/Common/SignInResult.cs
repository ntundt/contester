namespace contester.Features.Authentication.Commands.Common;

public class SignInResult
{
    public string AccessToken { get; set; } = null!;
    public int AccessTokenTtlSeconds { get; set; }
    public string RefreshToken { get; set; } = null!;
    public int RefreshTokenTtlSeconds { get; set; }
}
