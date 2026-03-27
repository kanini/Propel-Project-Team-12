using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Models;
using PatientAccess.Business.Utilities;
using PatientAccess.Data.Models;
using System.Text.Json;
using MatchType = PatientAccess.Business.Models.MatchType;

namespace PatientAccess.Business.Services;

/// <summary>
/// Entity resolution service for de-duplicating clinical data across documents (AIR-005, FR-030, FR-031).
/// Uses fuzzy string matching and rule-based logic to identify duplicates and conflicts.
/// </summary>
public class EntityResolutionService : IEntityResolutionService
{
    private readonly ILogger<EntityResolutionService> _logger;

    // Similarity thresholds
    private const int EXACT_MATCH_THRESHOLD = 100;
    private const int HIGH_SIMILARITY_THRESHOLD = 90;
    private const int POTENTIAL_MATCH_MIN_THRESHOLD = 70;
    private const int POTENTIAL_MATCH_MAX_THRESHOLD = 89;

    public EntityResolutionService(ILogger<EntityResolutionService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, EntityMatchResult>> ResolveMedicationDuplicatesAsync(List<ExtractedClinicalData> medications)
    {
        _logger.LogInformation("Resolving medication duplicates for {Count} entries", medications.Count);

        var results = new Dictionary<Guid, EntityMatchResult>();
        var processed = new HashSet<Guid>();

        for (int i = 0; i < medications.Count; i++)
        {
            var medication1 = medications[i];
            if (processed.Contains(medication1.ExtractedDataId))
                continue;

            var matchResult = new EntityMatchResult
            {
                SourceEntityIds = new List<Guid> { medication1.ExtractedDataId },
                MatchType = MatchType.NoMatch
            };

            // Parse medication data from StructuredData JSON
            var med1Data = ParseMedicationData(medication1);

            for (int j = i + 1; j < medications.Count; j++)
            {
                var medication2 = medications[j];
                if (processed.Contains(medication2.ExtractedDataId))
                    continue;

                var med2Data = ParseMedicationData(medication2);

                // Compare drug names using fuzzy matching
                var drugNameSimilarity = FuzzyMatcher.CalculateSimilarity(
                    FuzzyMatcher.NormalizeMedicationName(med1Data.DrugName),
                    FuzzyMatcher.NormalizeMedicationName(med2Data.DrugName)
                );

                // If drug names are similar enough, check dosage and frequency
                if (drugNameSimilarity >= POTENTIAL_MATCH_MIN_THRESHOLD)
                {
                    var dosageMatch = FuzzyMatcher.IsExactMatch(med1Data.Dosage, med2Data.Dosage);
                    var frequencyMatch = FuzzyMatcher.IsExactMatch(med1Data.Frequency, med2Data.Frequency);

                    if (drugNameSimilarity >= HIGH_SIMILARITY_THRESHOLD && dosageMatch && frequencyMatch)
                    {
                        // High confidence duplicate
                        matchResult.IsMatch = true;
                        matchResult.MatchType = drugNameSimilarity == EXACT_MATCH_THRESHOLD
                            ? MatchType.ExactMatch
                            : MatchType.HighSimilarity;
                        matchResult.SimilarityScore = drugNameSimilarity;
                        matchResult.SourceEntityIds.Add(medication2.ExtractedDataId);
                        processed.Add(medication2.ExtractedDataId);
                    }
                    else if (drugNameSimilarity >= HIGH_SIMILARITY_THRESHOLD && (!dosageMatch || !frequencyMatch))
                    {
                        // Same drug, different dose/frequency = CONFLICT
                        matchResult.IsMatch = true;
                        matchResult.HasConflict = true;
                        matchResult.MatchType = MatchType.HighSimilarity;
                        matchResult.SimilarityScore = drugNameSimilarity;
                        matchResult.ConflictDetails = $"Medication dosage/frequency mismatch: {med1Data.DrugName} " +
                            $"({med1Data.Dosage} {med1Data.Frequency}) vs ({med2Data.Dosage} {med2Data.Frequency})";
                        matchResult.RequiresManualReview = true;
                        matchResult.SourceEntityIds.Add(medication2.ExtractedDataId);

                        _logger.LogWarning("Medication conflict detected: {ConflictDetails}", matchResult.ConflictDetails);
                    }
                    else if (drugNameSimilarity >= POTENTIAL_MATCH_MIN_THRESHOLD)
                    {
                        // Potential match - requires manual review
                        matchResult.IsMatch = true;
                        matchResult.MatchType = MatchType.PotentialMatch;
                        matchResult.SimilarityScore = drugNameSimilarity;
                        matchResult.RequiresManualReview = true;
                        matchResult.SourceEntityIds.Add(medication2.ExtractedDataId);
                    }
                }
            }

            results[medication1.ExtractedDataId] = matchResult;
        }

        _logger.LogInformation("Medication resolution complete: {TotalEntries} entries, {DuplicateGroups} duplicate groups, {Conflicts} conflicts",
            medications.Count, results.Count(r => r.Value.IsMatch && !r.Value.HasConflict), results.Count(r => r.Value.HasConflict));

        return await Task.FromResult(results);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, EntityMatchResult>> ResolveConditionDuplicatesAsync(List<ExtractedClinicalData> conditions)
    {
        _logger.LogInformation("Resolving condition duplicates for {Count} entries", conditions.Count);

        var results = new Dictionary<Guid, EntityMatchResult>();
        var processed = new HashSet<Guid>();

        for (int i = 0; i < conditions.Count; i++)
        {
            var condition1 = conditions[i];
            if (processed.Contains(condition1.ExtractedDataId))
                continue;

            var matchResult = new EntityMatchResult
            {
                SourceEntityIds = new List<Guid> { condition1.ExtractedDataId },
                MatchType = MatchType.NoMatch
            };

            var cond1Data = ParseConditionData(condition1);

            for (int j = i + 1; j < conditions.Count; j++)
            {
                var condition2 = conditions[j];
                if (processed.Contains(condition2.ExtractedDataId))
                    continue;

                var cond2Data = ParseConditionData(condition2);

                // ICD-10 code exact match = highest confidence
                if (!string.IsNullOrWhiteSpace(cond1Data.ICD10Code) &&
                    !string.IsNullOrWhiteSpace(cond2Data.ICD10Code) &&
                    FuzzyMatcher.IsExactMatch(cond1Data.ICD10Code, cond2Data.ICD10Code))
                {
                    matchResult.IsMatch = true;
                    matchResult.MatchType = MatchType.ExactMatch;
                    matchResult.SimilarityScore = EXACT_MATCH_THRESHOLD;
                    matchResult.SourceEntityIds.Add(condition2.ExtractedDataId);
                    processed.Add(condition2.ExtractedDataId);
                    continue;
                }

                // Condition name fuzzy matching
                var nameSimilarity = FuzzyMatcher.CalculateSimilarity(cond1Data.ConditionName, cond2Data.ConditionName);

                if (nameSimilarity >= HIGH_SIMILARITY_THRESHOLD)
                {
                    // High similarity in condition name
                    if (!string.IsNullOrWhiteSpace(cond1Data.ICD10Code) &&
                        !string.IsNullOrWhiteSpace(cond2Data.ICD10Code) &&
                        !FuzzyMatcher.IsExactMatch(cond1Data.ICD10Code, cond2Data.ICD10Code))
                    {
                        // Same condition name but different ICD-10 codes = potential conflict
                        matchResult.IsMatch = true;
                        matchResult.HasConflict = true;
                        matchResult.MatchType = MatchType.HighSimilarity;
                        matchResult.SimilarityScore = nameSimilarity;
                        matchResult.ConflictDetails = $"Condition name matches but ICD-10 codes differ: " +
                            $"{cond1Data.ConditionName} ({cond1Data.ICD10Code}) vs ({cond2Data.ICD10Code})";
                        matchResult.RequiresManualReview = true;
                        matchResult.SourceEntityIds.Add(condition2.ExtractedDataId);
                    }
                    else
                    {
                        // High similarity condition - likely duplicate
                        matchResult.IsMatch = true;
                        matchResult.MatchType = nameSimilarity == EXACT_MATCH_THRESHOLD
                            ? MatchType.ExactMatch
                            : MatchType.HighSimilarity;
                        matchResult.SimilarityScore = nameSimilarity;
                        matchResult.SourceEntityIds.Add(condition2.ExtractedDataId);
                        processed.Add(condition2.ExtractedDataId);
                    }
                }
                else if (nameSimilarity >= POTENTIAL_MATCH_MIN_THRESHOLD)
                {
                    // Potential match - requires manual review
                    matchResult.IsMatch = true;
                    matchResult.MatchType = MatchType.PotentialMatch;
                    matchResult.SimilarityScore = nameSimilarity;
                    matchResult.RequiresManualReview = true;
                    matchResult.SourceEntityIds.Add(condition2.ExtractedDataId);
                }
            }

            results[condition1.ExtractedDataId] = matchResult;
        }

        _logger.LogInformation("Condition resolution complete: {TotalEntries} entries, {DuplicateGroups} duplicate groups, {Conflicts} conflicts",
            conditions.Count, results.Count(r => r.Value.IsMatch && !r.Value.HasConflict), results.Count(r => r.Value.HasConflict));

        return await Task.FromResult(results);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, EntityMatchResult>> ResolveAllergyDuplicatesAsync(List<ExtractedClinicalData> allergies)
    {
        _logger.LogInformation("Resolving allergy duplicates for {Count} entries", allergies.Count);

        var results = new Dictionary<Guid, EntityMatchResult>();
        var processed = new HashSet<Guid>();

        for (int i = 0; i < allergies.Count; i++)
        {
            var allergy1 = allergies[i];
            if (processed.Contains(allergy1.ExtractedDataId))
                continue;

            var matchResult = new EntityMatchResult
            {
                SourceEntityIds = new List<Guid> { allergy1.ExtractedDataId },
                MatchType = MatchType.NoMatch
            };

            var allergy1Data = ParseAllergyData(allergy1);

            for (int j = i + 1; j < allergies.Count; j++)
            {
                var allergy2 = allergies[j];
                if (processed.Contains(allergy2.ExtractedDataId))
                    continue;

                var allergy2Data = ParseAllergyData(allergy2);

                // Compare allergen names
                var allergenSimilarity = FuzzyMatcher.CalculateSimilarity(allergy1Data.AllergenName, allergy2Data.AllergenName);

                if (allergenSimilarity >= HIGH_SIMILARITY_THRESHOLD)
                {
                    var severityMatch = FuzzyMatcher.IsExactMatch(allergy1Data.Severity, allergy2Data.Severity);

                    if (severityMatch)
                    {
                        // Same allergen, same severity = duplicate
                        matchResult.IsMatch = true;
                        matchResult.MatchType = allergenSimilarity == EXACT_MATCH_THRESHOLD
                            ? MatchType.ExactMatch
                            : MatchType.HighSimilarity;
                        matchResult.SimilarityScore = allergenSimilarity;
                        matchResult.SourceEntityIds.Add(allergy2.ExtractedDataId);
                        processed.Add(allergy2.ExtractedDataId);
                    }
                    else
                    {
                        // Same allergen, different severity = CONFLICT
                        matchResult.IsMatch = true;
                        matchResult.HasConflict = true;
                        matchResult.MatchType = MatchType.HighSimilarity;
                        matchResult.SimilarityScore = allergenSimilarity;
                        matchResult.ConflictDetails = $"Allergy severity mismatch: {allergy1Data.AllergenName} " +
                            $"({allergy1Data.Severity}) vs ({allergy2Data.Severity})";
                        matchResult.RequiresManualReview = true;
                        matchResult.SourceEntityIds.Add(allergy2.ExtractedDataId);

                        _logger.LogWarning("Allergy conflict detected: {ConflictDetails}", matchResult.ConflictDetails);
                    }
                }
                else if (allergenSimilarity >= POTENTIAL_MATCH_MIN_THRESHOLD)
                {
                    // Potential match - requires manual review
                    matchResult.IsMatch = true;
                    matchResult.MatchType = MatchType.PotentialMatch;
                    matchResult.SimilarityScore = allergenSimilarity;
                    matchResult.RequiresManualReview = true;
                    matchResult.SourceEntityIds.Add(allergy2.ExtractedDataId);
                }
            }

            results[allergy1.ExtractedDataId] = matchResult;
        }

        _logger.LogInformation("Allergy resolution complete: {TotalEntries} entries, {DuplicateGroups} duplicate groups, {Conflicts} conflicts",
            allergies.Count, results.Count(r => r.Value.IsMatch && !r.Value.HasConflict), results.Count(r => r.Value.HasConflict));

        return await Task.FromResult(results);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<Guid, EntityMatchResult>> ResolveEncounterDuplicatesAsync(List<ExtractedClinicalData> encounters)
    {
        _logger.LogInformation("Resolving encounter duplicates for {Count} entries", encounters.Count);

        var results = new Dictionary<Guid, EntityMatchResult>();
        var processed = new HashSet<Guid>();

        for (int i = 0; i < encounters.Count; i++)
        {
            var encounter1 = encounters[i];
            if (processed.Contains(encounter1.ExtractedDataId))
                continue;

            var matchResult = new EntityMatchResult
            {
                SourceEntityIds = new List<Guid> { encounter1.ExtractedDataId },
                MatchType = MatchType.NoMatch
            };

            var enc1Data = ParseEncounterData(encounter1);

            for (int j = i + 1; j < encounters.Count; j++)
            {
                var encounter2 = encounters[j];
                if (processed.Contains(encounter2.ExtractedDataId))
                    continue;

                var enc2Data = ParseEncounterData(encounter2);

                // Check if encounters are on the same day
                var sameDate = enc1Data.EncounterDate.Date == enc2Data.EncounterDate.Date;
                var typeMatch = FuzzyMatcher.IsExactMatch(enc1Data.EncounterType, enc2Data.EncounterType);
                var providerSimilarity = FuzzyMatcher.CalculateSimilarity(enc1Data.Provider, enc2Data.Provider);
                var facilitySimilarity = FuzzyMatcher.CalculateSimilarity(enc1Data.Facility, enc2Data.Facility);

                if (sameDate && typeMatch &&
                    (providerSimilarity >= 80 || facilitySimilarity >= 80))
                {
                    // Same date + type + provider/facility = likely duplicate
                    var overallSimilarity = (providerSimilarity + facilitySimilarity) / 2;

                    matchResult.IsMatch = true;
                    matchResult.MatchType = overallSimilarity >= HIGH_SIMILARITY_THRESHOLD
                        ? MatchType.HighSimilarity
                        : MatchType.PotentialMatch;
                    matchResult.SimilarityScore = overallSimilarity;
                    matchResult.SourceEntityIds.Add(encounter2.ExtractedDataId);
                    matchResult.RequiresManualReview = overallSimilarity < HIGH_SIMILARITY_THRESHOLD;

                    if (matchResult.MatchType == MatchType.HighSimilarity)
                    {
                        processed.Add(encounter2.ExtractedDataId);
                    }
                }
            }

            results[encounter1.ExtractedDataId] = matchResult;
        }

        _logger.LogInformation("Encounter resolution complete: {TotalEntries} entries, {DuplicateGroups} duplicate groups",
            encounters.Count, results.Count(r => r.Value.IsMatch));

        return await Task.FromResult(results);
    }

    /// <inheritdoc/>
    public async Task<EntitySimilarity> CalculateEntitySimilarityAsync(ExtractedClinicalData entity1, ExtractedClinicalData entity2)
    {
        var similarity = new EntitySimilarity
        {
            EntityId = entity1.ExtractedDataId,
            ComparisonEntityId = entity2.ExtractedDataId
        };

        // Calculate field-level similarities based on data type
        switch (entity1.DataType)
        {
            case ClinicalDataType.Medication:
                var med1 = ParseMedicationData(entity1);
                var med2 = ParseMedicationData(entity2);

                var drugNameScore = FuzzyMatcher.CalculateSimilarity(med1.DrugName, med2.DrugName);
                var dosageScore = FuzzyMatcher.IsExactMatch(med1.Dosage, med2.Dosage) ? 100 : 0;
                var frequencyScore = FuzzyMatcher.IsExactMatch(med1.Frequency, med2.Frequency) ? 100 : 0;

                similarity.FieldSimilarityScores["DrugName"] = drugNameScore;
                similarity.FieldSimilarityScores["Dosage"] = dosageScore;
                similarity.FieldSimilarityScores["Frequency"] = frequencyScore;
                similarity.SimilarityScore = (drugNameScore + dosageScore + frequencyScore) / 3;

                if (drugNameScore >= HIGH_SIMILARITY_THRESHOLD) similarity.MatchedFields.Add("DrugName");
                if (dosageScore == 0) similarity.ConflictingFields.Add("Dosage");
                if (frequencyScore == 0) similarity.ConflictingFields.Add("Frequency");
                break;

            case ClinicalDataType.Diagnosis:
                var cond1 = ParseConditionData(entity1);
                var cond2 = ParseConditionData(entity2);

                var condNameScore = FuzzyMatcher.CalculateSimilarity(cond1.ConditionName, cond2.ConditionName);
                var icd10Score = FuzzyMatcher.IsExactMatch(cond1.ICD10Code, cond2.ICD10Code) ? 100 : 0;

                similarity.FieldSimilarityScores["ConditionName"] = condNameScore;
                similarity.FieldSimilarityScores["ICD10Code"] = icd10Score;
                similarity.SimilarityScore = (condNameScore + icd10Score) / 2;

                if (condNameScore >= HIGH_SIMILARITY_THRESHOLD) similarity.MatchedFields.Add("ConditionName");
                if (icd10Score == 100) similarity.MatchedFields.Add("ICD10Code");
                else if (!string.IsNullOrWhiteSpace(cond1.ICD10Code) && !string.IsNullOrWhiteSpace(cond2.ICD10Code))
                    similarity.ConflictingFields.Add("ICD10Code");
                break;

            case ClinicalDataType.Allergy:
                var allergy1 = ParseAllergyData(entity1);
                var allergy2 = ParseAllergyData(entity2);

                var allergenScore = FuzzyMatcher.CalculateSimilarity(allergy1.AllergenName, allergy2.AllergenName);
                var severityScore = FuzzyMatcher.IsExactMatch(allergy1.Severity, allergy2.Severity) ? 100 : 0;

                similarity.FieldSimilarityScores["AllergenName"] = allergenScore;
                similarity.FieldSimilarityScores["Severity"] = severityScore;
                similarity.SimilarityScore = (allergenScore + severityScore) / 2;

                if (allergenScore >= HIGH_SIMILARITY_THRESHOLD) similarity.MatchedFields.Add("AllergenName");
                if (severityScore == 0) similarity.ConflictingFields.Add("Severity");
                break;
        }

        return await Task.FromResult(similarity);
    }

    // Helper methods to parse structured data
    private MedicationData ParseMedicationData(ExtractedClinicalData data)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(data.StructuredData))
            {
                return JsonSerializer.Deserialize<MedicationData>(data.StructuredData) ?? new MedicationData();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse medication structured data for {DataId}", data.ExtractedDataId);
        }

        return new MedicationData
        {
            DrugName = data.DataValue,
            Dosage = data.DataKey.Contains("dosage") ? data.DataValue : "",
            Frequency = data.DataKey.Contains("frequency") ? data.DataValue : ""
        };
    }

    private ConditionData ParseConditionData(ExtractedClinicalData data)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(data.StructuredData))
            {
                return JsonSerializer.Deserialize<ConditionData>(data.StructuredData) ?? new ConditionData();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse condition structured data for {DataId}", data.ExtractedDataId);
        }

        return new ConditionData
        {
            ConditionName = data.DataValue,
            ICD10Code = data.DataKey.Contains("icd") ? data.DataValue : ""
        };
    }

    private AllergyData ParseAllergyData(ExtractedClinicalData data)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(data.StructuredData))
            {
                return JsonSerializer.Deserialize<AllergyData>(data.StructuredData) ?? new AllergyData();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse allergy structured data for {DataId}", data.ExtractedDataId);
        }

        return new AllergyData
        {
            AllergenName = data.DataValue,
            Severity = data.DataKey.Contains("severity") ? data.DataValue : "Unknown"
        };
    }

    private EncounterData ParseEncounterData(ExtractedClinicalData data)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(data.StructuredData))
            {
                return JsonSerializer.Deserialize<EncounterData>(data.StructuredData) ?? new EncounterData();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse encounter structured data for {DataId}", data.ExtractedDataId);
        }

        return new EncounterData
        {
            EncounterDate = DateTime.UtcNow,
            EncounterType = data.DataKey,
            Provider = data.DataValue
        };
    }

    // Internal data models for parsing
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

    private class EncounterData
    {
        public DateTime EncounterDate { get; set; }
        public string EncounterType { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public string? Facility { get; set; }
    }
}
