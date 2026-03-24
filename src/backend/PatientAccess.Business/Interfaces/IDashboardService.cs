using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for patient dashboard statistics and data aggregation (US_067).
/// Provides summary metrics and trend indicators for dashboard display.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Retrieves dashboard statistics for authenticated patient.
    /// Calculates appointment counts and trend percentages.
    /// Results are cached for 5 minutes (NFR-001).
    /// </summary>
    /// <param name="userId">Patient user ID from JWT claims</param>
    /// <returns>Dashboard statistics with trend indicators</returns>
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId);
}
