namespace PatientAccess.Data.Models;

/// <summary>
/// Patient appointment scheduling entity with status lifecycle (DR-002).
/// </summary>
public class Appointment
{
    public Guid AppointmentId { get; set; }

    public Guid PatientId { get; set; }

    public Guid ProviderId { get; set; }

    public DateTime ScheduledDateTime { get; set; }

    public AppointmentStatus Status { get; set; }

    public string VisitReason { get; set; } = string.Empty;

    public bool IsWalkIn { get; set; }

    public bool ConfirmationReceived { get; set; }

    public decimal? NoShowRiskScore { get; set; }

    public int CancellationNoticeHours { get; set; } = 24;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
