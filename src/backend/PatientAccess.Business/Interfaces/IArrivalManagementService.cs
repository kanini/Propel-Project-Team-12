/**
 * IArrivalManagementService Interface for Patient Arrival Operations
 * Provides business logic for arrival search and status marking
 */

using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for managing patient arrivals
/// </summary>
public interface IArrivalManagementService
{
    /// <summary>
    /// Search for appointments scheduled for today matching the query.
    /// If query is null or empty, returns all appointments for the date.
    /// </summary>
    /// <param name="query">Optional search term (patient name, email, or phone). Returns all if null/empty.</param>
    /// <param name="date">Date to search for appointments (defaults to today)</param>
    /// <returns>Task<List<ArrivalSearchResultDto>> - List of matching appointments</returns>
    Task<List<ArrivalSearchResultDto>> SearchTodayAppointmentsAsync(string? query = null, DateTime? date = null);

    /// <summary>
    /// Mark an appointment as arrived and record arrival time
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Task<ArrivalSearchResultDto> - Updated appointment data</returns>
    Task<ArrivalSearchResultDto> MarkAppointmentArrivedAsync(Guid appointmentId);
}
