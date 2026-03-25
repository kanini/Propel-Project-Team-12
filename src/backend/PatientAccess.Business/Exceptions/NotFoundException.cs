namespace PatientAccess.Business.Exceptions;

/// <summary>
/// Exception thrown when requested resource is not found.
/// Maps to HTTP 404 Not Found status code.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
