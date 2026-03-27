using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Models;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using System.Diagnostics;
using MatchType = PatientAccess.Business.Models.MatchType;

namespace PatientAccess.Business.Services;

/// <summary>
/// Data aggregation service for consolidating extracted clinical data into patient profiles (AIR-005, FR-030, AC3, AC4).
/// Provides incremental aggregation, entity resolution integration, conflict detection, and profile completeness calculation.
/// </summary>
public class DataAggregationService : IDataAggregationService
{
    private readonly PatientAccessDbContext _context;
    private readonly IEntityResolutionService _entityResolution;
    private readonly IConflictDetectionService _conflictDetection;
    private readonly IPusherService _pusher;
    private readonly ILogger<DataAggregationService> _logger;

    // Profile completeness weights (must sum to 100%)
    private const decimal DEMOGRAPHICS_WEIGHT = 20m;
    private const decimal CONDITIONS_WEIGHT = 15m;
    private const decimal MEDICATIONS_WEIGHT = 15m;
    private const decimal ALLERGIES_WEIGHT = 10m;
    private const decimal VITALS_WEIGHT = 20m;
    private const decimal ENCOUNTERS_WEIGHT = 20m;

    public DataAggregationService(
        PatientAccessDbContext context,
        IEntityResolutionService entityResolution,
        IConflictDetectionService conflictDetection,
        IPusherService pusher,
        ILogger<DataAggregationService> logger)
    {
        _context = context;
        _entityResolution = entityResolution;
        _conflictDetection = conflictDetection;
        _pusher = pusher;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AggregationResultDto> AggregatePatientDataAsync(Guid patientId, Guid? documentId = null)
    {
        if (documentId.HasValue)
        {
            return await IncrementalAggregateAsync(patientId, documentId.Value);
        }
        else
        {
            return await ReaggregatePatientProfileAsync(patientId);
        }
    }

    /// <inheritdoc/>
    public async Task<AggregationResultDto> IncrementalAggregateAsync(Guid patientId, Guid documentId)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting incremental aggregation for Patient {PatientId}, Document {DocumentId}",
            patientId, documentId);

        var result = new AggregationResultDto
        {
            PatientId = patientId,
            AggregatedAt = DateTime.UtcNow,
            IsIncremental = true
        };

        try
        {
            // Start transaction for atomicity
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Ensure PatientProfile exists
                var profile = await GetOrCreatePatientProfileAsync(patientId);
                result.PatientProfileId = profile.Id;

                // Load extracted data for this document only (incremental)
                var extractedData = await _context.ExtractedClinicalData
                    .Where(e => e.PatientId == patientId && e.DocumentId == documentId)
                    .ToListAsync();

                if (!extractedData.Any())
                {
                    _logger.LogWarning("No extracted data found for Document {DocumentId}", documentId);
                    stopwatch.Stop();
                    result.ProcessingDurationMs = stopwatch.ElapsedMilliseconds;
                    return result;
                }

                // Group by data type
                var medications = extractedData.Where(e => e.DataType == ClinicalDataType.Medication).ToList();
                var diagnoses = extractedData.Where(e => e.DataType == ClinicalDataType.Diagnosis).ToList();
                var allergies = extractedData.Where(e => e.DataType == ClinicalDataType.Allergy).ToList();
                var vitals = extractedData.Where(e => e.DataType == ClinicalDataType.Vital).ToList();
                var labResults = extractedData.Where(e => e.DataType == ClinicalDataType.LabResult).ToList();

                // Aggregate each data type
                result.NewMedicationsCount = await AggregateMedicationsAsync(profile, medications);
                result.NewConditionsCount = await AggregateConditionsAsync(profile, diagnoses);
                result.NewAllergiesCount = await AggregateAllergiesAsync(profile, allergies);
                result.NewVitalsCount = await AggregateVitalsAsync(profile, vitals);
                result.NewEncountersCount = await AggregateEncountersAsync(profile, labResults);

                // Update profile metadata
                profile.LastAggregatedAt = DateTime.UtcNow;
                profile.TotalDocumentsProcessed++;
                profile.UpdatedAt = DateTime.UtcNow;

                // Calculate profile completeness
                profile.ProfileCompleteness = await CalculateProfileCompletenessAsync(profile.Id);
                result.ProfileCompleteness = profile.ProfileCompleteness;

                // Check for unresolved conflicts
                var unresolvedConflicts = await _context.DataConflicts
                    .Where(c => c.PatientProfileId == profile.Id && c.ResolutionStatus == "Unresolved")
                    .CountAsync();

                profile.HasUnresolvedConflicts = unresolvedConflicts > 0;
                result.HasUnresolvedConflicts = profile.HasUnresolvedConflicts;
                result.ConflictsDetected = unresolvedConflicts;
                result.TotalDocumentsProcessed = profile.TotalDocumentsProcessed;

                // Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                stopwatch.Stop();
                result.ProcessingDurationMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "Aggregation complete for Patient {PatientId}: {Conditions} conditions, {Medications} medications, " +
                    "{Allergies} allergies, {Vitals} vitals, {Encounters} encounters, {Conflicts} conflicts in {Duration}ms",
                    patientId, result.NewConditionsCount, result.NewMedicationsCount, result.NewAllergiesCount,
                    result.NewVitalsCount, result.NewEncountersCount, result.ConflictsDetected, result.ProcessingDurationMs);

                // Send real-time notification
                await _pusher.SendAggregationCompleteAsync(patientId, result);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Aggregation failed for Patient {PatientId}, rolling back transaction", patientId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during aggregation for Patient {PatientId}", patientId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PatientProfile> GetOrCreatePatientProfileAsync(Guid patientId)
    {
        var profile = await _context.PatientProfiles
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (profile == null)
        {
            _logger.LogInformation("Creating new PatientProfile for Patient {PatientId}", patientId);

            profile = new PatientProfile
            {
                PatientId = patientId,
                LastAggregatedAt = DateTime.UtcNow,
                TotalDocumentsProcessed = 0,
                HasUnresolvedConflicts = false,
                ProfileCompleteness = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.PatientProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        return profile;
    }

    /// <inheritdoc/>
    public async Task<decimal> CalculateProfileCompletenessAsync(int patientProfileId)
    {
        var profile = await _context.PatientProfiles
            .Include(p => p.Patient)
            .Include(p => p.Conditions)
            .Include(p => p.Medications)
            .Include(p => p.Allergies)
            .Include(p => p.VitalTrends)
            .Include(p => p.Encounters)
            .FirstOrDefaultAsync(p => p.Id == patientProfileId);

        if (profile == null)
        {
            return 0;
        }

        decimal completeness = 0;

        // Demographics (from User/Patient table) - 20%
        if (!string.IsNullOrWhiteSpace(profile.Patient.Name) &&
            !string.IsNullOrWhiteSpace(profile.Patient.Email) &&
            profile.Patient.DateOfBirth.HasValue)
        {
            completeness += DEMOGRAPHICS_WEIGHT;
        }

        // At least 1 condition - 15%
        if (profile.Conditions.Any())
        {
            completeness += CONDITIONS_WEIGHT;
        }

        // At least 1 medication - 15%
        if (profile.Medications.Any())
        {
            completeness += MEDICATIONS_WEIGHT;
        }

        // At least 1 allergy - 10%
        if (profile.Allergies.Any())
        {
            completeness += ALLERGIES_WEIGHT;
        }

        // At least 5 vital trends - 20%
        if (profile.VitalTrends.Count >= 5)
        {
            completeness += VITALS_WEIGHT;
        }
        else if (profile.VitalTrends.Any())
        {
            // Partial credit proportional to vital count
            completeness += VITALS_WEIGHT * (profile.VitalTrends.Count / 5m);
        }

        // At least 1 encounter - 20%
        if (profile.Encounters.Any())
        {
            completeness += ENCOUNTERS_WEIGHT;
        }

        return Math.Round(completeness, 2);
    }

    /// <inheritdoc/>
    public async Task<AggregationResultDto> ReaggregatePatientProfileAsync(Guid patientId)
    {
        _logger.LogInformation("Starting full re-aggregation for Patient {PatientId}", patientId);

        // Delete existing consolidated data
        var profile = await _context.PatientProfiles
            .Include(p => p.Conditions)
            .Include(p => p.Medications)
            .Include(p => p.Allergies)
            .Include(p => p.VitalTrends)
            .Include(p => p.Encounters)
            .Include(p => p.Conflicts)
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (profile != null)
        {
            _context.ConsolidatedConditions.RemoveRange(profile.Conditions);
            _context.ConsolidatedMedications.RemoveRange(profile.Medications);
            _context.ConsolidatedAllergies.RemoveRange(profile.Allergies);
            _context.VitalTrends.RemoveRange(profile.VitalTrends);
            _context.ConsolidatedEncounters.RemoveRange(profile.Encounters);
            _context.DataConflicts.RemoveRange(profile.Conflicts);

            profile.TotalDocumentsProcessed = 0;
            profile.ProfileCompleteness = 0;
            profile.HasUnresolvedConflicts = false;

            await _context.SaveChangesAsync();
        }

        // Get all documents for patient and aggregate them one by one
        var documentIds = await _context.ClinicalDocuments
            .Where(d => d.PatientId == patientId && d.ProcessingStatus == ProcessingStatus.Completed)
            .Select(d => d.DocumentId)
            .ToListAsync();

        AggregationResultDto? finalResult = null;

        foreach (var docId in documentIds)
        {
            finalResult = await IncrementalAggregateAsync(patientId, docId);
        }

        if (finalResult != null)
        {
            finalResult.IsIncremental = false; // Mark as full re-aggregation
        }

        return finalResult ?? new AggregationResultDto
        {
            PatientId = patientId,
            AggregatedAt = DateTime.UtcNow,
            IsIncremental = false
        };
    }

    // Private helper methods for aggregating each data type

    private async Task<int> AggregateMedicationsAsync(PatientProfile profile, List<ExtractedClinicalData> medications)
    {
        if (!medications.Any()) return 0;

        // Load existing medications
        var existing = await _context.ConsolidatedMedications
            .Where(m => m.PatientProfileId == profile.Id)
            .ToListAsync();

        // Combine new and existing for entity resolution
        var allMedicationData = existing.Any()
            ? await _context.ExtractedClinicalData
                .Where(e => e.PatientId == profile.PatientId && e.DataType == ClinicalDataType.Medication)
                .ToListAsync()
            : medications;

        // Run entity resolution
        var matches = await _entityResolution.ResolveMedicationDuplicatesAsync(allMedicationData);

        int newCount = 0;

        foreach (var medication in medications)
        {
            var match = matches.GetValueOrDefault(medication.ExtractedDataId);

            if (match == null || match.MatchType == MatchType.NoMatch)
            {
                // New medication
                var newMed = CreateConsolidatedMedication(profile.Id, medication);
                _context.ConsolidatedMedications.Add(newMed);
                newCount++;
            }
            else if (match.HasConflict)
            {
                // Conflict detected - create both entries and flag
                var conflictMed = CreateConsolidatedMedication(profile.Id, medication);
                conflictMed.HasConflict = true;
                _context.ConsolidatedMedications.Add(conflictMed);

                await CreateDataConflictAsync(profile.Id, "MedicationDosageMismatch", "Medication",
                    conflictMed.Id, match.ConflictDetails ?? "Medication conflict detected", match.SourceEntityIds);

                newCount++;
            }
            else if (match.IsMatch)
            {
                // Duplicate - update existing with new source reference
                var existingMed = existing.FirstOrDefault(e =>
                    match.SourceEntityIds.Contains(e.SourceDataIds.FirstOrDefault()));

                if (existingMed != null)
                {
                    existingMed.SourceDataIds.Add(medication.ExtractedDataId);
                    existingMed.SourceDocumentIds.Add(medication.DocumentId);
                    existingMed.DuplicateCount++;
                    existingMed.LastUpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return newCount;
    }

    private async Task<int> AggregateConditionsAsync(PatientProfile profile, List<ExtractedClinicalData> conditions)
    {
        if (!conditions.Any()) return 0;

        var existing = await _context.ConsolidatedConditions
            .Where(c => c.PatientProfileId == profile.Id)
            .ToListAsync();

        var allConditionData = existing.Any()
            ? await _context.ExtractedClinicalData
                .Where(e => e.PatientId == profile.PatientId && e.DataType == ClinicalDataType.Diagnosis)
                .ToListAsync()
            : conditions;

        var matches = await _entityResolution.ResolveConditionDuplicatesAsync(allConditionData);

        int newCount = 0;

        foreach (var condition in conditions)
        {
            var match = matches.GetValueOrDefault(condition.ExtractedDataId);

            if (match == null || match.MatchType == MatchType.NoMatch)
            {
                var newCond = CreateConsolidatedCondition(profile.Id, condition);
                _context.ConsolidatedConditions.Add(newCond);
                newCount++;
            }
            else if (match.HasConflict)
            {
                var conflictCond = CreateConsolidatedCondition(profile.Id, condition);
                _context.ConsolidatedConditions.Add(conflictCond);

                await CreateDataConflictAsync(profile.Id, "DiagnosisMismatch", "Condition",
                    conflictCond.Id, match.ConflictDetails ?? "Condition conflict detected", match.SourceEntityIds);

                newCount++;
            }
            else if (match.IsMatch)
            {
                var existingCond = existing.FirstOrDefault(e =>
                    match.SourceEntityIds.Contains(e.SourceDataIds.FirstOrDefault()));

                if (existingCond != null)
                {
                    existingCond.SourceDataIds.Add(condition.ExtractedDataId);
                    existingCond.SourceDocumentIds.Add(condition.DocumentId);
                    existingCond.DuplicateCount++;
                    existingCond.LastUpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return newCount;
    }

    private async Task<int> AggregateAllergiesAsync(PatientProfile profile, List<ExtractedClinicalData> allergies)
    {
        if (!allergies.Any()) return 0;

        var existing = await _context.ConsolidatedAllergies
            .Where(a => a.PatientProfileId == profile.Id)
            .ToListAsync();

        var allAllergyData = existing.Any()
            ? await _context.ExtractedClinicalData
                .Where(e => e.PatientId == profile.PatientId && e.DataType == ClinicalDataType.Allergy)
                .ToListAsync()
            : allergies;

        var matches = await _entityResolution.ResolveAllergyDuplicatesAsync(allAllergyData);

        int newCount = 0;

        foreach (var allergy in allergies)
        {
            var match = matches.GetValueOrDefault(allergy.ExtractedDataId);

            if (match == null || match.MatchType == MatchType.NoMatch)
            {
                var newAllergy = CreateConsolidatedAllergy(profile.Id, allergy);
                _context.ConsolidatedAllergies.Add(newAllergy);
                newCount++;
            }
            else if (match.HasConflict)
            {
                var conflictAllergy = CreateConsolidatedAllergy(profile.Id, allergy);
                _context.ConsolidatedAllergies.Add(conflictAllergy);

                await CreateDataConflictAsync(profile.Id, "AllergyMismatch", "Allergy",
                    conflictAllergy.Id, match.ConflictDetails ?? "Allergy conflict detected", match.SourceEntityIds);

                newCount++;
            }
            else if (match.IsMatch)
            {
                var existingAllergy = existing.FirstOrDefault(e =>
                    match.SourceEntityIds.Contains(e.SourceDataIds.FirstOrDefault()));

                if (existingAllergy != null)
                {
                    existingAllergy.SourceDataIds.Add(allergy.ExtractedDataId);
                    existingAllergy.SourceDocumentIds.Add(allergy.DocumentId);
                    existingAllergy.DuplicateCount++;
                    existingAllergy.LastUpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return newCount;
    }

    private async Task<int> AggregateVitalsAsync(PatientProfile profile, List<ExtractedClinicalData> vitals)
    {
        if (!vitals.Any()) return 0;

        // Vitals are NOT de-duplicated - preserve all temporal data (AC4 edge case)
        foreach (var vital in vitals)
        {
            var vitalTrend = new VitalTrend
            {
                PatientProfileId = profile.Id,
                VitalType = vital.DataKey, // e.g., "BloodPressure", "HeartRate"
                Value = vital.DataValue,
                Unit = ExtractUnit(vital.DataValue),
                RecordedAt = vital.ExtractedAt,
                SourceDocumentId = vital.DocumentId,
                SourceDataId = vital.ExtractedDataId,
                CreatedAt = DateTime.UtcNow
            };

            _context.VitalTrends.Add(vitalTrend);
        }

        return vitals.Count;
    }

    private async Task<int> AggregateEncountersAsync(PatientProfile profile, List<ExtractedClinicalData> labResults)
    {
        // Encounters may be extracted from lab results or other sources
        // For now, create simple encounter records
        if (!labResults.Any()) return 0;

        // Group lab results by date to create encounters
        var encountersByDate = labResults
            .GroupBy(l => l.ExtractedAt.Date)
            .ToList();

        int newCount = 0;

        foreach (var group in encountersByDate)
        {
            var encounter = new ConsolidatedEncounter
            {
                PatientProfileId = profile.Id,
                EncounterDate = group.Key,
                EncounterType = "LabWork",
                SourceDocumentIds = group.Select(g => g.DocumentId).Distinct().ToList(),
                SourceDataIds = group.Select(g => g.ExtractedDataId).ToList(),
                IsDuplicate = false,
                DuplicateCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.ConsolidatedEncounters.Add(encounter);
            newCount++;
        }

        return newCount;
    }

    private ConsolidatedMedication CreateConsolidatedMedication(int profileId, ExtractedClinicalData data)
    {
        var medicationData = System.Text.Json.JsonSerializer.Deserialize<MedicationData>(data.StructuredData ?? "{}");

        return new ConsolidatedMedication
        {
            PatientProfileId = profileId,
            DrugName = medicationData?.DrugName ?? data.DataValue,
            Dosage = medicationData?.Dosage ?? "",
            Frequency = medicationData?.Frequency ?? "",
            Status = "Active",
            SourceDocumentIds = new List<Guid> { data.DocumentId },
            SourceDataIds = new List<Guid> { data.ExtractedDataId },
            IsDuplicate = false,
            DuplicateCount = 0,
            HasConflict = false,
            FirstRecordedAt = data.ExtractedAt,
            LastUpdatedAt = data.ExtractedAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ConsolidatedCondition CreateConsolidatedCondition(int profileId, ExtractedClinicalData data)
    {
        var conditionData = System.Text.Json.JsonSerializer.Deserialize<ConditionData>(data.StructuredData ?? "{}");

        return new ConsolidatedCondition
        {
            PatientProfileId = profileId,
            ConditionName = conditionData?.ConditionName ?? data.DataValue,
            ICD10Code = conditionData?.ICD10Code,
            Status = "Active",
            SourceDocumentIds = new List<Guid> { data.DocumentId },
            SourceDataIds = new List<Guid> { data.ExtractedDataId },
            IsDuplicate = false,
            DuplicateCount = 0,
            FirstRecordedAt = data.ExtractedAt,
            LastUpdatedAt = data.ExtractedAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ConsolidatedAllergy CreateConsolidatedAllergy(int profileId, ExtractedClinicalData data)
    {
        var allergyData = System.Text.Json.JsonSerializer.Deserialize<AllergyData>(data.StructuredData ?? "{}");

        return new ConsolidatedAllergy
        {
            PatientProfileId = profileId,
            AllergenName = allergyData?.AllergenName ?? data.DataValue,
            Severity = allergyData?.Severity ?? "Unknown",
            Status = "Active",
            SourceDocumentIds = new List<Guid> { data.DocumentId },
            SourceDataIds = new List<Guid> { data.ExtractedDataId },
            IsDuplicate = false,
            DuplicateCount = 0,
            FirstRecordedAt = data.ExtractedAt,
            LastUpdatedAt = data.ExtractedAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task CreateDataConflictAsync(int profileId, string conflictType, string entityType,
        Guid entityId, string description, List<Guid> sourceDataIds)
    {
        // Classify conflict severity using ConflictDetectionService (US_048, AC3)
        var severity = await _conflictDetection.ClassifyConflictSeverityAsync(entityType, conflictType);

        var conflict = new DataConflict
        {
            Id = Guid.NewGuid(),
            PatientProfileId = profileId,
            ConflictType = conflictType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description ?? $"{conflictType} detected",
            Severity = severity,
            SourceDataIds = sourceDataIds,
            ResolutionStatus = "Unresolved",
            CreatedAt = DateTime.UtcNow
        };

        _context.DataConflicts.Add(conflict);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Data conflict detected ({Severity}): {ConflictType} for Profile {ProfileId}",
            severity, conflictType, profileId);

        // Send Pusher notification for Critical conflicts (US_048)
        if (severity == Data.Models.Enums.ConflictSeverity.Critical)
        {
            try
            {
                var profile = await _context.PatientProfiles
                    .FirstOrDefaultAsync(p => p.Id == profileId);

                if (profile != null)
                {
                    var conflictDto = new DataConflictDto
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
                        CreatedAt = conflict.CreatedAt
                    };

                    await _pusher.SendConflictDetectedAsync(profile.PatientId, conflictDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Pusher notification for critical conflict {ConflictId}", conflict.Id);
                // Don't fail aggregation if Pusher notification fails
            }
        }
    }

    private string ExtractUnit(string value)
    {
        // Simple unit extraction from value (e.g., "120/80 mmHg" -> "mmHg")
        var parts = value.Split(' ');
        return parts.Length > 1 ? parts[^1] : "";
    }

    // Internal data models for JSON deserialization
    private class MedicationData
    {
        public string DrugName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
    }

    private class ConditionData
    {
        public string ConditionName { get; set; } = string.Empty;
        public string? ICD10Code { get; set; }
    }

    private class AllergyData
    {
        public string AllergenName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
}
