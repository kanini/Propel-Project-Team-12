namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for staff dashboard metrics (US_068, AC2).
/// Contains aggregated statistics displayed in dashboard stat cards.
/// </summary>
public class StaffDashboardMetricsDto
{
    /// <summary>
    /// Total number of appointments scheduled for today (all statuses except Cancelled).
    /// </summary>
    public int TodayAppointments { get; set; }

    /// <summary>
    /// Current number of patients in the waiting queue with status 'Waiting'.
    /// </summary>
    public int CurrentQueueSize { get; set; }

    /// <summary>
    /// Number of unverified clinical data items with VerificationStatus = 'AISuggested'.
    /// Placeholder count until EP-009 clinical verification workflow is implemented.
    /// </summary>
    public int PendingVerifications { get; set; }
}
