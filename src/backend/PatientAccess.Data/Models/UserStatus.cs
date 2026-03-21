namespace PatientAccess.Data.Models;

/// <summary>
/// User account status values (FR-001).
/// Maps to: 0=Pending (email not verified), 1=Active, 2=Inactive (deactivated), 3=Locked (failed login attempts)
/// </summary>
public enum UserStatus
{
    Pending = 0,
    Active = 1,
    Inactive = 2,
    Locked = 3
}
