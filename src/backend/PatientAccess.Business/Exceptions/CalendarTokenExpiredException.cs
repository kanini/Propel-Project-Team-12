namespace PatientAccess.Business.Exceptions;

/// <summary>
/// Exception thrown when calendar OAuth token refresh fails (US_039 - EC-2).
/// Indicates user must re-authorize calendar connection.
/// Results in HTTP 401 Unauthorized with re-authorization prompt.
/// </summary>
public class CalendarTokenExpiredException : Exception
{
    public CalendarTokenExpiredException() : base()
    {
    }

    public CalendarTokenExpiredException(string message) : base(message)
    {
    }

    public CalendarTokenExpiredException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
