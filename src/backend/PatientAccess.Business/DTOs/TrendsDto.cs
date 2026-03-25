namespace PatientAccess.Business.DTOs;

/// <summary>
/// Trend indicators for dashboard statistics (US_067).
/// Percentage changes compared to previous period.
/// </summary>
public class TrendsDto
{
    /// <summary>
    /// Percentage change in total appointments (current 6 months vs previous 6 months).
    /// Positive indicates increase, negative indicates decrease.
    /// </summary>
    public double TotalAppointmentsTrend { get; set; }

    /// <summary>
    /// Percentage change in upcoming appointments.
    /// Note: Trend may not be applicable for future projections.
    /// </summary>
    public double UpcomingAppointmentsTrend { get; set; }

    /// <summary>
    /// Percentage change in waitlist entries.
    /// Note: Requres historical tracking for accurate trends.
    /// </summary>
    public double WaitlistEntriesTrend { get; set; }
}
