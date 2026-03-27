using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using PatientAccess.Data.Models.Enums;
using System.Text.Json;

namespace PatientAccess.Business.Services;

/// <summary>
/// Conflict detection service for identifying critical data inconsistencies (US_048, FR-031, AIR-006).
/// Classifies conflicts by severity and provides resolution workflow.
/// </summary>
public class ConflictDetectionService : IConflictDetectionService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<ConflictDetectionService> _logger;

    public ConflictDetectionService(
        PatientAccessDbContext context,
        ILogger<ConflictDetectionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Detect medication conflicts (AC3: medications always Critical).
    /// </summary>
    public async Task<DataConflict?> DetectMedicationConflictsAsync(
        ConsolidatedMedication medication,
        List<ExtractedClinicalData> sources)
    {
        if (sources.Count < 2) return null;

        _logger.LogInformation("Detecting medication conflicts for {DrugName} across {SourceCount} sources",
            medication.DrugName, sources.Count);

        var medicationDataList = sources
            .Where(s => !string.IsNullOrWhiteSpace(s.StructuredData))
            .Select(s => JsonSerializer.Deserialize<MedicationData>(s.StructuredData!))
            .Where(m => m != null)
            .ToList();

        if (medicationDataList.Count < 2) return null;

        // Check for dosage mismatch
        var uniqueDosages = medicationDataList
            .Select(m => m!.Dosage?.Trim().ToLowerInvariant())
            .Distinct()
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .ToList();

        if (uniqueDosages.Count > 1)
        {
            var conflict = new DataConflict
            {
                Id = Guid.NewGuid(),
                PatientProfileId = medication.PatientProfileId,
                ConflictType = "MedicationDosageMismatch",
                EntityType = "Medication",
                EntityId = medication.Id,
                Description = $"Medication '{medication.DrugName}' has conflicting dosages: {string.Join(" vs ", uniqueDosages)}",
                Severity = ConflictSeverity.Critical, // AC3: All medication conflicts are Critical
                SourceDataIds = sources.Select(s => s.ExtractedDataId).ToList(),
                ResolutionStatus = "Unresolved",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataConflicts.Add(conflict);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Critical medication dosage conflict detected: {Description}", conflict.Description);
            return conflict;
        }

        // Check for frequency mismatch
        var uniqueFrequencies = medicationDataList
            .Select(m => m!.Frequency?.Trim().ToLowerInvariant())
            .Distinct()
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToList();

        if (uniqueFrequencies.Count > 1)
        {
            var conflict = new DataConflict
            {
                Id = Guid.NewGuid(),
                PatientProfileId = medication.PatientProfileId,
                ConflictType = "MedicationFrequencyMismatch",
                EntityType = "Medication",
                EntityId = medication.Id,
                Description = $"Medication '{medication.DrugName}' has conflicting frequencies: {string.Join(" vs ", uniqueFrequencies)}",
                Severity = ConflictSeverity.Critical,
                SourceDataIds = sources.Select(s => s.ExtractedDataId).ToList(),
                ResolutionStatus = "Unresolved",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataConflicts.Add(conflict);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Critical medication frequency conflict detected: {Description}", conflict.Description);
            return conflict;
        }

        return null;
    }

    /// <summary>
    /// Detect allergy conflicts (Severe/Critical allergies classified as Critical).
    /// </summary>
    public async Task<DataConflict?> DetectAllergyConflictsAsync(
        ConsolidatedAllergy allergy,
        List<ExtractedClinicalData> sources)
    {
        if (sources.Count < 2) return null;

        _logger.LogInformation("Detecting allergy conflicts for {AllergenName} across {SourceCount} sources",
            allergy.AllergenName, sources.Count);

        var allergyDataList = sources
            .Where(s => !string.IsNullOrWhiteSpace(s.StructuredData))
            .Select(s => JsonSerializer.Deserialize<AllergyData>(s.StructuredData!))
            .Where(a => a != null)
            .ToList();

        if (allergyDataList.Count < 2) return null;

        // Check for severity mismatch
        var uniqueSeverities = allergyDataList
            .Select(a => a!.Severity?.Trim().ToLowerInvariant())
            .Distinct()
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (uniqueSeverities.Count > 1)
        {
            // Classify severity based on highest level mentioned
            var hasCritical = uniqueSeverities.Any(s => s != null && (s.Contains("severe") || s.Contains("critical")));
            var conflictSeverity = hasCritical ? ConflictSeverity.Critical : ConflictSeverity.Warning;

            var conflict = new DataConflict
            {
                Id = Guid.NewGuid(),
                PatientProfileId = allergy.PatientProfileId,
                ConflictType = "AllergySeverityMismatch",
                EntityType = "Allergy",
                EntityId = allergy.Id,
                Description = $"Allergy '{allergy.AllergenName}' has conflicting severity levels: {string.Join(" vs ", uniqueSeverities)}",
                Severity = conflictSeverity,
                SourceDataIds = sources.Select(s => s.ExtractedDataId).ToList(),
                ResolutionStatus = "Unresolved",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataConflicts.Add(conflict);
            await _context.SaveChangesAsync();

            _logger.LogWarning("{Severity} allergy conflict detected: {Description}",
                conflictSeverity, conflict.Description);
            return conflict;
        }

        return null;
    }

    /// <summary>
    /// Detect condition conflicts (ICD-10 code mismatches classified as Warning).
    /// </summary>
    public async Task<DataConflict?> DetectConditionConflictsAsync(
        ConsolidatedCondition condition,
        List<ExtractedClinicalData> sources)
    {
        if (sources.Count < 2) return null;

        _logger.LogInformation("Detecting condition conflicts for {ConditionName} across {SourceCount} sources",
            condition.ConditionName, sources.Count);

        var conditionDataList = sources
            .Where(s => !string.IsNullOrWhiteSpace(s.StructuredData))
            .Select(s => JsonSerializer.Deserialize<ConditionData>(s.StructuredData!))
            .Where(c => c != null)
            .ToList();

        if (conditionDataList.Count < 2) return null;

        // Check for ICD-10 code mismatch
        var uniqueIcd10Codes = conditionDataList
            .Select(c => c!.Icd10Code?.Trim().ToUpperInvariant())
            .Distinct()
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();

        if (uniqueIcd10Codes.Count > 1)
        {
            var conflict = new DataConflict
            {
                Id = Guid.NewGuid(),
                PatientProfileId = condition.PatientProfileId,
                ConflictType = "DiagnosisCodeMismatch",
                EntityType = "Condition",
                EntityId = condition.Id,
                Description = $"Condition '{condition.ConditionName}' has conflicting ICD-10 codes: {string.Join(" vs ", uniqueIcd10Codes)}",
                Severity = ConflictSeverity.Warning, // Diagnosis conflicts are Warning level
                SourceDataIds = sources.Select(s => s.ExtractedDataId).ToList(),
                ResolutionStatus = "Unresolved",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataConflicts.Add(conflict);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Warning-level diagnosis conflict detected: {Description}", conflict.Description);
            return conflict;
        }

        return null;
    }

    /// <summary>
    /// Classify conflict severity based on entity type.
    /// </summary>
    public Task<ConflictSeverity> ClassifyConflictSeverityAsync(string entityType, string conflictType)
    {
        // AC3: All medication conflicts are Critical
        if (entityType.Equals("Medication", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ConflictSeverity.Critical);
        }

        // Severe/Critical allergies are Critical
        if (entityType.Equals("Allergy", StringComparison.OrdinalIgnoreCase) &&
            conflictType.Contains("Severe", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ConflictSeverity.Critical);
        }

        // Diagnosis conflicts are Warning
        if (entityType.Equals("Condition", StringComparison.OrdinalIgnoreCase) ||
            entityType.Equals("Diagnosis", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(ConflictSeverity.Warning);
        }

        // Default to Info for other conflicts (vitals, etc.)
        return Task.FromResult(ConflictSeverity.Info);
    }

    /// <summary>
    /// Resolve a conflict with staff attestation.
    /// </summary>
    public async Task<DataConflictDto> ResolveConflictAsync(Guid conflictId, Guid staffUserId, string resolution)
    {
        var conflict = await _context.DataConflicts
            .FirstOrDefaultAsync(c => c.Id == conflictId);

        if (conflict == null)
        {
            throw new InvalidOperationException($"Conflict {conflictId} not found");
        }

        if (conflict.ResolutionStatus == "Resolved")
        {
            _logger.LogWarning("Conflict {ConflictId} already resolved", conflictId);
        }

        conflict.ResolutionStatus = "Resolved";
        conflict.ResolvedBy = staffUserId;
        conflict.ResolvedAt = DateTime.UtcNow;
        conflict.Description = $"{conflict.Description} | Resolution: {resolution}";

        await _context.SaveChangesAsync();

        _logger.LogInformation("Conflict {ConflictId} resolved by staff user {UserId}", conflictId, staffUserId);

        return MapToDto(conflict);
    }

    /// <summary>
    /// Get conflicts with filtering and pagination.
    /// </summary>
    public async Task<List<DataConflictDto>> GetConflictsAsync(
        int patientProfileId,
        ConflictSeverity? severity = null,
        bool unresolvedOnly = true,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.DataConflicts
            .Where(c => c.PatientProfileId == patientProfileId);

        if (unresolvedOnly)
        {
            query = query.Where(c => c.ResolutionStatus == "Unresolved");
        }

        if (severity.HasValue)
        {
            query = query.Where(c => c.Severity == severity.Value);
        }

        // Order by severity (Critical first), then by creation date (newest first)
        var conflicts = await query
            .OrderBy(c => c.Severity) // Critical=0, Warning=1, Info=2
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return conflicts.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get conflict summary statistics for a patient profile.
    /// </summary>
    public async Task<ConflictSummaryDto> GetConflictSummaryAsync(int patientProfileId)
    {
        var unresolvedConflicts = await _context.DataConflicts
            .Where(c => c.PatientProfileId == patientProfileId && c.ResolutionStatus == "Unresolved")
            .ToListAsync();

        var summary = new ConflictSummaryDto
        {
            TotalUnresolved = unresolvedConflicts.Count,
            CriticalCount = unresolvedConflicts.Count(c => c.Severity == ConflictSeverity.Critical),
            WarningCount = unresolvedConflicts.Count(c => c.Severity == ConflictSeverity.Warning),
            InfoCount = unresolvedConflicts.Count(c => c.Severity == ConflictSeverity.Info),
            OldestConflictDate = unresolvedConflicts
                .OrderBy(c => c.CreatedAt)
                .Select(c => (DateTime?)c.CreatedAt)
                .FirstOrDefault()
        };

        return summary;
    }

    private static DataConflictDto MapToDto(DataConflict conflict)
    {
        return new DataConflictDto
        {
            Id = conflict.Id,
            PatientProfileId = conflict.PatientProfileId,
            ConflictType = conflict.ConflictType,
            EntityType = conflict.EntityType,
            EntityId = conflict.EntityId,
            Description = conflict.Description,
            Severity = conflict.Severity,
            SourceDataIds = conflict.SourceDataIds,
            ResolutionStatus = conflict.ResolutionStatus,
            ResolvedBy = conflict.ResolvedBy,
            ResolvedAt = conflict.ResolvedAt,
            CreatedAt = conflict.CreatedAt
        };
    }

    // Internal data models for JSON deserialization
    private class MedicationData
    {
        public string? DrugName { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
    }

    private class AllergyData
    {
        public string? AllergenName { get; set; }
        public string? Severity { get; set; }
        public string? Reaction { get; set; }
    }

    private class ConditionData
    {
        public string? ConditionName { get; set; }
        public string? Icd10Code { get; set; }
        public string? Status { get; set; }
    }
}
