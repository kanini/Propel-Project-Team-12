namespace PatientAccess.Data.Models;

/// <summary>
/// User role types for role-based access control (RBAC).
/// Maps to: 1=Patient, 2=Staff, 3=Admin
/// </summary>
public enum UserRole
{
    Patient = 1,
    Staff = 2,
    Admin = 3
}
