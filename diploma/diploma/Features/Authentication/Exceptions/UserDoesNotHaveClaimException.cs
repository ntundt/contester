namespace diploma.Features.Authentication.Exceptions;

public class UserDoesNotHaveClaimException : Exception
{
    public Guid UserId { get; private set; }
    public string ClaimName { get; private set; }

    public UserDoesNotHaveClaimException(Guid userId, string claimName)
    {
        UserId = userId;
        ClaimName = claimName;
    }
}