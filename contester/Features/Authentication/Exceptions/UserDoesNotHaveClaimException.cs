namespace contester.Features.Authentication.Exceptions;

public class UserDoesNotHavePermissionException(Guid userId, string permissionName) : Exception
{
    public Guid UserId { get; private set; } = userId;
    public string PermissionName { get; private set; } = permissionName;

    public UserDoesNotHavePermissionException(Guid userId, Constants.Permission permission) : this(userId, permission.ToString())
    {
    }
}
