namespace diploma.Features.Authentication.Exceptions;

public class UserDoesNotHavePermissionException : Exception
{
    public Guid UserId { get; private set; }
    public string ClaimName { get; private set; }

    public UserDoesNotHavePermissionException(Guid userId, string claimName)
    {
        UserId = userId;
        ClaimName = claimName;
    }

    public UserDoesNotHavePermissionException(Guid userId, Constants.Permission permission)
    {
        UserId = userId;
        ClaimName = permission.ToString();
    }
}