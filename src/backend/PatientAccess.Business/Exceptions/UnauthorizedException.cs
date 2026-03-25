namespace PatientAccess.Business.Exceptions;

/// <summary>
/// Exception thrown when user is not authorized for an action.
/// Maps to HTTP 403 Forbidden status code.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
