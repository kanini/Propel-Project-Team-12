namespace PatientAccess.Business.Exceptions;

/// <summary>
/// Exception thrown when a policy or business rule is violated (US_027 - FR-011).
/// Maps to HTTP 403 Forbidden status code.
/// Examples: Attempting to cancel within restricted window, insufficient permissions for action.
/// </summary>
public class PolicyViolationException : Exception
{
    public PolicyViolationException()
    {
    }

    public PolicyViolationException(string message) : base(message)
    {
    }

    public PolicyViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
