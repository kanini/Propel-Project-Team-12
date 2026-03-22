namespace PatientAccess.Data.Models;

/// <summary>
/// Patient appointment scheduling entity with status lifecycle (DR-002).
/// Includes primary TimeSlot reference and optional PreferredSlot for dynamic slot swap (FR-010).
/// </summary>
public class Appointment
{
    public Guid AppointmentId { get; set; }

    public Guid PatientId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid TimeSlotId { get; set; }

    public DateTime ScheduledDateTime { get; set; }

    public AppointmentStatus Status { get; set; }

    public string VisitReason { get; set; } = string.Empty;

    public bool IsWalkIn { get; set; }

    public bool ConfirmationReceived { get; set; }

    public decimal? NoShowRiskScore { get; set; }

    public int CancellationNoticeHours { get; set; } = 24;

    public string? Notes { get; set; }

    public Guid? PreferredSlotId { get; set; }

    public string ConfirmationNumber { get; set; } = string.Empty;

    public string? PdfFilePath { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
    public TimeSlot? PreferredSlot { get; set; }
}
