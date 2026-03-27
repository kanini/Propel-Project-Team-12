using PatientAccess.Business.DTOs;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for aggregating extracted clinical data into consolidated patient profiles (AIR-005, FR-030).
/// Provides incremental aggregation, entity resolution integration, and profile completeness calculation.
/// </summary>
public interface IDataAggregationService
{
    /// <summary>
    /// Aggregates clinical data for a patient, optionally scoped to a specific document.
    /// Uses entity resolution to de-duplicate and detect conflicts.
    /// </summary>
    /// <param name="patientId">Patient ID (Guid from Users table)</param>
    /// <param name="documentId">Optional document ID to aggregate only data from that document</param>
    /// <returns>Aggregation result with statistics and conflict details</returns>
    Task<AggregationResultDto> AggregatePatientDataAsync(Guid patientId, Guid? documentId = null);

    /// <summary>
    /// Incrementally aggregates data from a newly processed document without reprocessing existing data (AC4).
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="documentId">Document ID to aggregate</param>
    /// <returns>Aggregation result for the incremental update</returns>
    Task<AggregationResultDto> IncrementalAggregateAsync(Guid patientId, Guid documentId);

    /// <summary>
    /// Gets existing PatientProfile or creates new one if it doesn't exist.
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <returns>Existing or newly created PatientProfile</returns>
    Task<PatientProfile> GetOrCreatePatientProfileAsync(Guid patientId);

    /// <summary>
    /// Calculates profile completeness score (0-100%) based on data coverage (AC3).
    /// </summary>
    /// <param name="patientProfileId">PatientProfile ID</param>
    /// <returns>Completeness percentage (0-100)</returns>
    Task<decimal> CalculateProfileCompletenessAsync(int patientProfileId);

    /// <summary>
    /// Re-aggregates all data for a patient (admin use case for data fixes or algorithm updates).
    /// Deletes existing consolidated data and re-processes from scratch.
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <returns>Aggregation result after full re-aggregation</returns>
    Task<AggregationResultDto> ReaggregatePatientProfileAsync(Guid patientId);
}
