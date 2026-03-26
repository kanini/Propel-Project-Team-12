/**
 * QueuePatientDto - Data Transfer Object for Queue Patient Display
 * Used for displaying patients in the same-day queue
 */

using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for queue patient information displayed in Queue Management interface
/// </summary>
public class QueuePatientDto
{
    /// <summary>
    /// Appointment ID (primary key)
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Patient ID (foreign key)
    /// </summary>
    [Required]
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// Patient full name
    /// </summary>
    [Required]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Appointment type: "Walk-in" or "Scheduled"
    /// </summary>
    [Required]
    public string AppointmentType { get; set; } = string.Empty;

    /// <summary>
    /// Provider full name
    /// </summary>
    [Required]
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Provider ID for filtering purposes
    /// </summary>
    [Required]
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Arrival time (when patient checked in as "Arrived"). Null if patient hasn't arrived yet.
    /// </summary>
    public DateTime? ArrivalTime { get; set; }

    /// <summary>
    /// Estimated wait time in minutes (current time - arrival time). 0 if patient hasn't arrived.
    /// </summary>
    [Required]
    public int EstimatedWaitTime { get; set; }

    /// <summary>
    /// Priority flag (true for emergency patients)
    /// </summary>
    [Required]
    public bool IsPriority { get; set; }
}
