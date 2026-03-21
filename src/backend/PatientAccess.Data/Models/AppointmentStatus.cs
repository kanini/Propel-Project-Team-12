namespace PatientAccess.Data.Models;

/// <summary>
/// Appointment status lifecycle values (DR-002).
/// Maps to: 1=Scheduled, 2=Confirmed, 3=Arrived, 4=Completed, 5=Cancelled, 6=NoShow
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    Arrived = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
}
