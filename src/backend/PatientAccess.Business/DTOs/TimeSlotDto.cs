namespace PatientAccess.Business.DTOs;

/// <summary>
/// Time slot data transfer object for availability display (FR-007).
/// Represents a single bookable time window.
/// </summary>
public class TimeSlotDto
{
    /// <summary>
    /// Unique time slot identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Slot start time in UTC.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Slot end time in UTC.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Indicates if slot is already booked.
    /// </summary>
    public bool IsBooked { get; set; }

    /// <summary>
    /// Duration in minutes calculated from start and end times.
    /// </summary>
    public int Duration => (int)(EndTime - StartTime).TotalMinutes;
}
