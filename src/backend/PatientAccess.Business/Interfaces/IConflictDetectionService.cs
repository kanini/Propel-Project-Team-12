using PatientAccess.Business.DTOs;
using PatientAccess.Data.Models;
using PatientAccess.Data.Models.Enums;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for detecting and managing data conflicts (US_048, FR-031, AIR-006).
/// </summary>
public interface IConflictDetectionService
{
    /// <summary>
    /// Detect medication conflicts (dosage, frequency, route mismatches).
    /// </summary>
    Task<DataConflict?> DetectMedicationConflictsAsync(
        ConsolidatedMedication medication,
        List<ExtractedClinicalData> sources);

    /// <summary>
    /// Detect allergy conflicts (severity, reaction mismatches).
    /// </summary>
    Task<DataConflict?> DetectAllergyConflictsAsync(
        ConsolidatedAllergy allergy,
        List<ExtractedClinicalData> sources);

    /// <summary>
    /// Detect condition conflicts (ICD-10 code, status mismatches).
    /// </summary>
    Task<DataConflict?> DetectConditionConflictsAsync(
        ConsolidatedCondition condition,
        List<ExtractedClinicalData> sources);

    /// <summary>
    /// Classify conflict severity based on entity type and conflict details.
    /// </summary>
    Task<ConflictSeverity> ClassifyConflictSeverityAsync(string entityType, string conflictType);

    /// <summary>
    /// Resolve a conflict by marking it as resolved with staff attestation.
    /// </summary>
    Task<DataConflictDto> ResolveConflictAsync(Guid conflictId, Guid staffUserId, string resolution);

    /// <summary>
    /// Get conflicts for a patient profile with optional filtering.
    /// </summary>
    Task<List<DataConflictDto>> GetConflictsAsync(
        int patientProfileId,
        ConflictSeverity? severity = null,
        bool unresolvedOnly = true,
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Get conflict summary stats for a patient profile.
    /// </summary>
    Task<ConflictSummaryDto> GetConflictSummaryAsync(int patientProfileId);
}
