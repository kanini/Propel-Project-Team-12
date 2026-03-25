using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for intake session operations (US_033).
/// Manages intake lifecycle from start to completion.
/// </summary>
public interface IIntakeService
{
    /// <summary>
    /// Starts a new intake session for an appointment (AC-1).
    /// </summary>
    /// <param name="patientId">Patient GUID from JWT claims</param>
    /// <param name="request">Start request with appointment ID and mode</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session ID and welcome message</returns>
    Task<StartIntakeResponseDto> StartSessionAsync(
        Guid patientId,
        StartIntakeRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a patient message and returns AI response with extracted data (AC-2).
    /// </summary>
    /// <param name="patientId">Patient GUID for authorization</param>
    /// <param name="request">Message request with session ID and message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI response with extracted data and confidence metrics</returns>
    Task<IntakeMessageResponseDto> ProcessMessageAsync(
        Guid patientId,
        IntakeMessageRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the intake summary for review (AC-3).
    /// </summary>
    /// <param name="patientId">Patient GUID for authorization</param>
    /// <param name="sessionId">Intake session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated intake summary</returns>
    Task<IntakeSummaryDto> GetSummaryAsync(
        Guid patientId,
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates intake data (for patient edits in summary).
    /// </summary>
    /// <param name="patientId">Patient GUID for authorization</param>
    /// <param name="sessionId">Intake session ID</param>
    /// <param name="request">Partial update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateIntakeAsync(
        Guid patientId,
        string sessionId,
        UpdateIntakeRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes and submits the intake session.
    /// </summary>
    /// <param name="patientId">Patient GUID for authorization</param>
    /// <param name="sessionId">Intake session ID</param>
    /// <param name="request">Optional final summary data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion confirmation</returns>
    Task<CompleteIntakeResponseDto> CompleteIntakeAsync(
        Guid patientId,
        string sessionId,
        CompleteIntakeRequestDto? request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an existing intake session by ID.
    /// </summary>
    /// <param name="patientId">Patient GUID for authorization</param>
    /// <param name="sessionId">Intake session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session data or null if not found</returns>
    Task<IntakeSessionDto?> GetSessionAsync(
        Guid patientId,
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches intake mode between AI and manual.
    /// </summary>
    /// <param name="patientId">Patient GUID for authorization</param>
    /// <param name="sessionId">Intake session ID</param>
    /// <param name="newMode">New mode ("ai" or "manual")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Switch confirmation with data preservation status</returns>
    Task<SwitchModeResponseDto> SwitchModeAsync(
        Guid patientId,
        string sessionId,
        string newMode,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for intake session information.
/// </summary>
public class IntakeSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public Guid AppointmentId { get; set; }
    public string Mode { get; set; } = "ai";
    public string Status { get; set; } = "active";
    public int Progress { get; set; }
}

/// <summary>
/// Response DTO for mode switch operation.
/// </summary>
public class SwitchModeResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public bool DataPreserved { get; set; } = true;
}
