namespace PatientAccess.Data.Models;

/// <summary>
/// Preferred time of day for waitlist entries.
/// Maps to: 1=Morning, 2=Afternoon, 3=Evening, 4=Anytime
/// </summary>
public enum PreferredTimeOfDay
{
    Morning = 1,
    Afternoon = 2,
    Evening = 3,
    Anytime = 4
}
