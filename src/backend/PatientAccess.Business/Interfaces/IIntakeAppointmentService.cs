using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for intake appointment operations (US_037).
/// Retrieves appointments requiring intake for the patient portal intake selection screen.
/// </summary>
public interface IIntakeAppointmentService
{
    /// <summary>
    /// Retrieves a patient's upcoming appointments that require intake (US_037, AC-1).
    /// Returns only future appointments with RequiresIntake = true.
    /// Includes intake status (Pending/InProgress/Completed) based on IntakeRecord presence.
    /// Results are cached for 2 minutes to reduce database load.
    /// </summary>
    /// <param name="patientId">Patient unique identifier (legacy int format)</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of intake appointment DTOs ordered by appointment date ascending</returns>
    Task<List<IntakeAppointmentDto>> GetPatientIntakeAppointmentsAsync(
        int patientId, 
        CancellationToken cancellationToken = default);
        
    /// <summary>
    /// Retrieves a patient's upcoming appointments that require intake (US_037, AC-1).
    /// Overload accepting GUID directly from JWT claims.
    /// </summary>
    /// <param name="patientGuid">Patient unique identifier from JWT claims</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of intake appointment DTOs ordered by appointment date ascending</returns>
    Task<List<IntakeAppointmentDto>> GetPatientIntakeAppointmentsAsync(
        Guid patientGuid, 
        CancellationToken cancellationToken = default);
}
