namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for patient dashboard statistics (US_067).
/// Contains appointment counts and trend indicators.
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Total appointments completed in past 6 months.
    /// </summary>
    public int TotalAppointments { get; set; }

    /// <summary>
    /// Upcoming confirmed appointments in next 30 days.
    /// </summary>
    public int UpcomingAppointments { get; set; }

    /// <summary>
    /// Current active waitlist entries count.
    /// </summary>
    public int WaitlistEntries { get; set; }

    /// <summary>
    /// Total documents uploaded by the patient.
    /// </summary>
    public int TotalDocuments { get; set; }

    /// <summary>
    /// Number of documents that have been successfully processed (status = Completed).
    /// </summary>
    public int CompletedDocuments { get; set; }

    /// <summary>
    /// Trend indicators for each statistic.
    /// </summary>
    public TrendsDto Trends { get; set; } = new TrendsDto();
}
