using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for staff dashboard operations (US_068).
/// Provides metrics and queue preview data for staff operational hub.
/// </summary>
public interface IStaffDashboardService
{
    /// <summary>
    /// Retrieves dashboard metrics including today's appointments, queue size, and pending verifications (US_068, AC2).
    /// </summary>
    /// <returns>Dashboard metrics DTO with stat card data</returns>
    Task<StaffDashboardMetricsDto> GetDashboardMetricsAsync();

    /// <summary>
    /// Retrieves queue preview showing next N patients chronologically (US_068, AC4).
    /// </summary>
    /// <param name="count">Number of patients to return (default: 5)</param>
    /// <returns>List of queue preview DTOs with patient details and wait times</returns>
    Task<List<QueuePreviewDto>> GetQueuePreviewAsync(int count = 5);
}
