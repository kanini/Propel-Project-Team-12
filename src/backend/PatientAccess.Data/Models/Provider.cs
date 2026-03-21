namespace PatientAccess.Data.Models;

/// <summary>
/// Healthcare provider reference entity (DR-002).
/// No login capability in Phase 1 — reference entity only.
/// </summary>
public class Provider
{
    public Guid ProviderId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? LicenseNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
