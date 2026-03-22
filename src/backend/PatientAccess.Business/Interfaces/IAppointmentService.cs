using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for appointment booking and availability queries (FR-007, FR-008).
/// Supports real-time availability calendar and appointment creation with conflict detection.
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// Retrieves monthly availability for a provider (FR-007).
    /// Returns dates with at least one available time slot.
    /// </summary>
    /// <param name="providerId">Provider unique identifier</param>
    /// <param name="year">Year for availability query</param>
    /// <param name="month">Month for availability query (1-12)</param>
    /// <returns>List of availability responses by date</returns>
    Task<List<AvailabilityResponseDto>> GetMonthlyAvailabilityAsync(Guid providerId, int year, int month);

    /// <summary>
    /// Retrieves daily availability for a provider (FR-007).
    /// Returns all time slots (booked and available) for the specified date.
    /// Must respond within 500ms at P95 (NFR-001).
    /// </summary>
    /// <param name="providerId">Provider unique identifier</param>
    /// <param name="date">Date for availability query</param>
    /// <returns>Availability response with time slots</returns>
    Task<AvailabilityResponseDto> GetDailyAvailabilityAsync(Guid providerId, DateTime date);

    /// <summary>
    /// Creates a new appointment with pessimistic locking (FR-008).
    /// Uses SELECT FOR UPDATE to prevent double-booking race conditions.
    /// Generates unique confirmation number.
    /// </summary>
    /// <param name="patientId">Patient unique identifier from authenticated user claims</param>
    /// <param name="request">Appointment creation request</param>
    /// <returns>Appointment response with confirmation details</returns>
    /// <exception cref="ConflictException">Thrown when time slot is already booked</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<AppointmentResponseDto> CreateAppointmentAsync(Guid patientId, CreateAppointmentRequestDto request);

    /// <summary>
    /// Cancels an existing appointment with cancellation policy enforcement (FR-011, US_027).
    /// Validates cancellation window based on configurable advance notice hours.
    /// Releases time slot and updates appointment status to Cancelled.
    /// </summary>
    /// <param name="appointmentId">Appointment unique identifier</param>
    /// <param name="patientId">Patient unique identifier for ownership verification</param>
    /// <returns>True if cancellation succeeded</returns>
    /// <exception cref="PolicyViolationException">Thrown when cancellation violates advance notice policy</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when patient doesn't own the appointment</exception>
    /// <exception cref="ArgumentException">Thrown when appointment not found or already cancelled</exception>
    Task<bool> CancelAsync(Guid appointmentId, Guid patientId);

    /// <summary>
    /// Reschedules an existing appointment to a new time slot (FR-011, US_027).
    /// Uses atomic transaction to release original slot and book new slot.
    /// Implements pessimistic locking to prevent double-booking.
    /// </summary>
    /// <param name="appointmentId">Appointment unique identifier</param>
    /// <param name="patientId">Patient unique identifier for ownership verification</param>
    /// <param name="newTimeSlotId">New time slot unique identifier</param>
    /// <returns>Updated appointment response</returns>
    /// <exception cref="ConflictException">Thrown when new time slot is already booked</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when patient doesn't own the appointment</exception>
    /// <exception cref="ArgumentException">Thrown when appointment or new slot not found</exception>
    Task<AppointmentResponseDto> RescheduleAsync(Guid appointmentId, Guid patientId, Guid newTimeSlotId);

    /// <summary>
    /// Retrieves all appointments for a patient (FR-011, US_027).
    /// Used by My Appointments page to display upcoming and past appointments.
    /// </summary>
    /// <param name="patientId">Patient unique identifier</param>
    /// <returns>List of appointment responses</returns>
    Task<List<AppointmentResponseDto>> GetPatientAppointmentsAsync(Guid patientId);

    /// <summary>
    /// Retrieves appointment entity by ID for internal use (US_028).
    /// Includes navigation properties for PDF generation and verification.
    /// </summary>
    /// <param name="appointmentId">Appointment unique identifier</param>
    /// <returns>Appointment entity with navigation properties, or null if not found</returns>
    Task<PatientAccess.Data.Models.Appointment?> GetAppointmentByIdInternalAsync(Guid appointmentId);
}
