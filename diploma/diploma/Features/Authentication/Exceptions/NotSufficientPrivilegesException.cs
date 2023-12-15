namespace diploma.Features.Authentication.Exceptions;

public class NotSufficientPrivilegesException : Exception
{
    public NotSufficientPrivilegesException(string message) : base(message)
    {
    }
}