namespace PatientAccess.Business.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// Maps to HTTP 400 Bad Request status code.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
    {
    }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
