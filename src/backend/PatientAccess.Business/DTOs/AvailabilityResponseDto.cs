namespace PatientAccess.Business.DTOs;

/// <summary>
/// Availability response DTO for calendar display (FR-007).
/// Contains available time slots for a specific date.
/// </summary>
public class AvailabilityResponseDto
{
    /// <summary>
    /// Date for availability query (date only, no time component).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// List of time slots for the specified date.
    /// </summary>
    public List<TimeSlotDto> TimeSlots { get; set; } = new();
}
