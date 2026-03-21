namespace PatientAccess.Data.Models;

/// <summary>
/// Insurance coverage type classification (DR-015).
/// </summary>
public enum CoverageType
{
    HMO = 1,
    PPO = 2,
    EPO = 3,
    POS = 4,
    Medicare = 5,
    Medicaid = 6,
    Other = 7
}
