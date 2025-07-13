namespace contester.Features.Authentication.Exceptions;

public class NotSufficientPrivilegesException : Exception
{
    public NotSufficientPrivilegesException(string message) : base(message)
    {
    }
}