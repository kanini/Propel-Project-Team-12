namespace PatientAccess.Data.Models;

/// <summary>
/// Discrete availability window for providers (DR-002).
/// Includes optimistic concurrency token for concurrent booking protection.
/// Updated for US_024 - relationship with Appointment managed from Appointment side.
/// </summary>
public class TimeSlot
{
    public Guid TimeSlotId { get; set; }

    public Guid ProviderId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsBooked { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token to handle concurrent time slot bookings.
    /// </summary>
    public uint RowVersion { get; set; }

    // Navigation properties
    public Provider Provider { get; set; } = null!;
}
