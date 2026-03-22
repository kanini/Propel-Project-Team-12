namespace PatientAccess.Business.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs, resulting in HTTP 409 Conflict response.
/// Used for appointment booking conflicts when a time slot is already booked.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException() : base()
    {
    }

    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
