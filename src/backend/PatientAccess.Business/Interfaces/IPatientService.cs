using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for patient search and minimal patient creation operations (US_029).
/// Supports walk-in booking workflow with fast patient search and quick registration.
/// </summary>
public interface IPatientService
{
    /// <summary>
    /// Searches for patients by name, email, or phone number (US_029, AC-1).
    /// Must return results within 300ms for optimal UX.
    /// </summary>
    /// <param name="query">Search term (minimum 2 characters)</param>
    /// <returns>List of matching patient records (max 20 results)</returns>
    Task<List<PatientSearchResultDto>> SearchPatientsAsync(string query);

    /// <summary>
    /// Creates a minimal patient record for walk-in registration (US_029, AC-2).
    /// If email is provided and already exists, returns existing patient record.
    /// Patient status is Active, password is empty (patient completes full registration later).
    /// </summary>
    /// <param name="dto">Minimal patient creation data (first name, last name, DOB, phone, optional email)</param>
    /// <returns>Created or existing patient record</returns>
    Task<PatientSearchResultDto> CreateMinimalPatientAsync(CreateMinimalPatientDto dto);
}
