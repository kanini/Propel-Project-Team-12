using Microsoft.Extensions.Logging;
using Moq;
using PatientAccess.Business.Models;
using PatientAccess.Business.Services;
using PatientAccess.Business.Utilities;
using PatientAccess.Data.Models;
using Xunit;
using MatchType = PatientAccess.Business.Models.MatchType;

namespace PatientAccess.Tests.Services;

/// <summary>
/// Unit tests for EntityResolutionService (US_047, AIR-005).
/// Tests medication, condition, allergy, and encounter duplicate detection with conflict flagging.
/// </summary>
public class EntityResolutionServiceTests
{
    private readonly EntityResolutionService _service;
    private readonly Mock<ILogger<EntityResolutionService>> _loggerMock;

    public EntityResolutionServiceTests()
    {
        _loggerMock = new Mock<ILogger<EntityResolutionService>>();
        _service = new EntityResolutionService(_loggerMock.Object);
    }

    #region Medication Matching Tests

    [Fact]
    public async Task ResolveMedicationDuplicates_ExactMatch_ReturnsExactMatchType()
    {
        // Arrange
        var medications = new List<ExtractedClinicalData>
        {
            CreateMedicationData("Lisinopril 10mg", "10mg", "once daily"),
            CreateMedicationData("Lisinopril 10mg", "10mg", "once daily")
        };

        // Act
        var results = await _service.ResolveMedicationDuplicatesAsync(medications);

        // Assert
        Assert.True(results[medications[0].ExtractedDataId].IsMatch);
        Assert.Equal(MatchType.ExactMatch, results[medications[0].ExtractedDataId].MatchType);
        Assert.False(results[medications[0].ExtractedDataId].HasConflict);
        Assert.Equal(2, results[medications[0].ExtractedDataId].SourceEntityIds.Count);
    }

    [Fact]
    public async Task ResolveMedicationDuplicates_DifferentDosage_DetectsConflict()
    {
        // Arrange
        var medications = new List<ExtractedClinicalData>
        {
            CreateMedicationData("Lisinopril", "10mg", "once daily"),
            CreateMedicationData("Lisinopril", "20mg", "once daily")
        };

        // Act
        var results = await _service.ResolveMedicationDuplicatesAsync(medications);

        // Assert
        Assert.True(results[medications[0].ExtractedDataId].IsMatch);
        Assert.True(results[medications[0].ExtractedDataId].HasConflict);
        Assert.True(results[medications[0].ExtractedDataId].RequiresManualReview);
        Assert.Contains("dosage/frequency mismatch", results[medications[0].ExtractedDataId].ConflictDetails ?? "");
    }

    [Fact]
    public async Task ResolveMedicationDuplicates_DifferentFrequency_DetectsConflict()
    {
        // Arrange
        var medications = new List<ExtractedClinicalData>
        {
            CreateMedicationData("Metformin", "500mg", "twice daily"),
            CreateMedicationData("Metformin", "500mg", "three times daily")
        };

        // Act
        var results = await _service.ResolveMedicationDuplicatesAsync(medications);

        // Assert
        Assert.True(results[medications[0].ExtractedDataId].IsMatch);
        Assert.True(results[medications[0].ExtractedDataId].HasConflict);
        Assert.True(results[medications[0].ExtractedDataId].RequiresManualReview);
    }

    [Fact]
    public async Task ResolveMedicationDuplicates_SimilarNames_HighSimilarityMatch()
    {
        // Arrange - minor spelling variation
        var medications = new List<ExtractedClinicalData>
        {
            CreateMedicationData("Metformin HCL", "500mg", "twice daily"),
            CreateMedicationData("Metformin Hydrochloride", "500mg", "twice daily")
        };

        // Act
        var results = await _service.ResolveMedicationDuplicatesAsync(medications);

        // Assert
        Assert.True(results[medications[0].ExtractedDataId].IsMatch);
        Assert.True(results[medications[0].ExtractedDataId].SimilarityScore >= 70);
    }

    [Fact]
    public async Task ResolveMedicationDuplicates_DifferentMedications_NoMatch()
    {
        // Arrange
        var medications = new List<ExtractedClinicalData>
        {
            CreateMedicationData("Lisinopril", "10mg", "once daily"),
            CreateMedicationData("Atorvastatin", "20mg", "once daily")
        };

        // Act
        var results = await _service.ResolveMedicationDuplicatesAsync(medications);

        // Assert
        Assert.False(results[medications[0].ExtractedDataId].IsMatch);
        Assert.Equal(MatchType.NoMatch, results[medications[0].ExtractedDataId].MatchType);
    }

    #endregion

    #region Condition Matching Tests

    [Fact]
    public async Task ResolveConditionDuplicates_ExactICD10Match_ReturnsExactMatchType()
    {
        // Arrange
        var conditions = new List<ExtractedClinicalData>
        {
            CreateConditionData("Type 2 Diabetes Mellitus", "E11.9"),
            CreateConditionData("Diabetes Type 2", "E11.9")
        };

        // Act
        var results = await _service.ResolveConditionDuplicatesAsync(conditions);

        // Assert
        Assert.True(results[conditions[0].ExtractedDataId].IsMatch);
        Assert.Equal(MatchType.ExactMatch, results[conditions[0].ExtractedDataId].MatchType);
        Assert.Equal(100, results[conditions[0].ExtractedDataId].SimilarityScore);
    }

    [Fact]
    public async Task ResolveConditionDuplicates_SameNameDifferentICD10_DetectsConflict()
    {
        // Arrange
        var conditions = new List<ExtractedClinicalData>
        {
            CreateConditionData("Hypertension", "I10"),
            CreateConditionData("Hypertension", "I11.0")
        };

        // Act
        var results = await _service.ResolveConditionDuplicatesAsync(conditions);

        // Assert
        Assert.True(results[conditions[0].ExtractedDataId].IsMatch);
        Assert.True(results[conditions[0].ExtractedDataId].HasConflict);
        Assert.Contains("ICD-10 codes differ", results[conditions[0].ExtractedDataId].ConflictDetails ?? "");
    }

    [Fact]
    public async Task ResolveConditionDuplicates_SynonymNames_HighSimilarity()
    {
        // Arrange
        var conditions = new List<ExtractedClinicalData>
        {
            CreateConditionData("Diabetes Mellitus", null),
            CreateConditionData("Diabetes", null)
        };

        // Act
        var results = await _service.ResolveConditionDuplicatesAsync(conditions);

        // Assert
        Assert.True(results[conditions[0].ExtractedDataId].IsMatch);
        Assert.True(results[conditions[0].ExtractedDataId].SimilarityScore >= 70);
    }

    [Fact]
    public async Task ResolveConditionDuplicates_DifferentConditions_NoMatch()
    {
        // Arrange
        var conditions = new List<ExtractedClinicalData>
        {
            CreateConditionData("Diabetes", "E11.9"),
            CreateConditionData("Hypertension", "I10")
        };

        // Act
        var results = await _service.ResolveConditionDuplicatesAsync(conditions);

        // Assert
        Assert.False(results[conditions[0].ExtractedDataId].IsMatch);
    }

    #endregion

    #region Allergy Matching Tests

    [Fact]
    public async Task ResolveAllergyDuplicates_ExactMatch_ReturnsDuplicate()
    {
        // Arrange
        var allergies = new List<ExtractedClinicalData>
        {
            CreateAllergyData("Penicillin", "Severe"),
            CreateAllergyData("Penicillin", "Severe")
        };

        // Act
        var results = await _service.ResolveAllergyDuplicatesAsync(allergies);

        // Assert
        Assert.True(results[allergies[0].ExtractedDataId].IsMatch);
        Assert.False(results[allergies[0].ExtractedDataId].HasConflict);
    }

    [Fact]
    public async Task ResolveAllergyDuplicates_DifferentSeverity_DetectsConflict()
    {
        // Arrange
        var allergies = new List<ExtractedClinicalData>
        {
            CreateAllergyData("Penicillin", "Moderate"),
            CreateAllergyData("Penicillin", "Severe")
        };

        // Act
        var results = await _service.ResolveAllergyDuplicatesAsync(allergies);

        // Assert
        Assert.True(results[allergies[0].ExtractedDataId].IsMatch);
        Assert.True(results[allergies[0].ExtractedDataId].HasConflict);
        Assert.Contains("severity mismatch", results[allergies[0].ExtractedDataId].ConflictDetails ?? "");
        Assert.True(results[allergies[0].ExtractedDataId].RequiresManualReview);
    }

    [Fact]
    public async Task ResolveAllergyDuplicates_SimilarAllergenNames_HighSimilarity()
    {
        // Arrange
        var allergies = new List<ExtractedClinicalData>
        {
            CreateAllergyData("Penicillin G", "Moderate"),
            CreateAllergyData("Penicillin", "Moderate")
        };

        // Act
        var results = await _service.ResolveAllergyDuplicatesAsync(allergies);

        // Assert
        Assert.True(results[allergies[0].ExtractedDataId].IsMatch);
        Assert.True(results[allergies[0].ExtractedDataId].SimilarityScore >= 70);
    }

    #endregion

    #region Encounter Matching Tests

    [Fact]
    public async Task ResolveEncounterDuplicates_SameDateTypeProvider_HighSimilarity()
    {
        // Arrange
        var encounterDate = DateTime.UtcNow.Date;
        var encounters = new List<ExtractedClinicalData>
        {
            CreateEncounterData(encounterDate, "Outpatient", "Dr. Smith", "General Hospital"),
            CreateEncounterData(encounterDate, "Outpatient", "Dr. Smith", "General Hospital")
        };

        // Act
        var results = await _service.ResolveEncounterDuplicatesAsync(encounters);

        // Assert
        Assert.True(results[encounters[0].ExtractedDataId].IsMatch);
        Assert.True(results[encounters[0].ExtractedDataId].SimilarityScore >= 80);
    }

    [Fact]
    public async Task ResolveEncounterDuplicates_DifferentDates_NoMatch()
    {
        // Arrange
        var encounters = new List<ExtractedClinicalData>
        {
            CreateEncounterData(DateTime.UtcNow.Date, "Outpatient", "Dr. Smith", "General Hospital"),
            CreateEncounterData(DateTime.UtcNow.Date.AddDays(-1), "Outpatient", "Dr. Smith", "General Hospital")
        };

        // Act
        var results = await _service.ResolveEncounterDuplicatesAsync(encounters);

        // Assert
        Assert.False(results[encounters[0].ExtractedDataId].IsMatch);
    }

    [Fact]
    public async Task ResolveEncounterDuplicates_DifferentTypes_NoMatch()
    {
        // Arrange
        var encounterDate = DateTime.UtcNow.Date;
        var encounters = new List<ExtractedClinicalData>
        {
            CreateEncounterData(encounterDate, "Outpatient", "Dr. Smith", "General Hospital"),
            CreateEncounterData(encounterDate, "Emergency", "Dr. Smith", "General Hospital")
        };

        // Act
        var results = await _service.ResolveEncounterDuplicatesAsync(encounters);

        // Assert
        Assert.False(results[encounters[0].ExtractedDataId].IsMatch);
    }

    #endregion

    #region FuzzyMatcher Utility Tests

    [Fact]
    public void FuzzyMatcher_ExactMatch_Returns100Similarity()
    {
        // Act
        var similarity = FuzzyMatcher.CalculateSimilarity("Lisinopril", "Lisinopril");

        // Assert
        Assert.Equal(100, similarity);
    }

    [Fact]
    public void FuzzyMatcher_CaseInsensitive_Returns100Similarity()
    {
        // Act
        var similarity = FuzzyMatcher.CalculateSimilarity("lisinopril", "LISINOPRIL");

        // Assert
        Assert.Equal(100, similarity);
    }

    [Fact]
    public void FuzzyMatcher_NormalizeString_RemovesPunctuation()
    {
        // Act
        var normalized = FuzzyMatcher.NormalizeString("Metformin-HCL 500mg!");

        // Assert
        Assert.DoesNotContain("-", normalized);
        Assert.DoesNotContain("!", normalized);
        Assert.Equal("metformin hcl 500mg", normalized);
    }

    [Fact]
    public void FuzzyMatcher_IsHighSimilarity_DetectsMinorSpellingDifference()
    {
        // Act
        var isHighSimilarity = FuzzyMatcher.IsHighSimilarity("Metformin", "Metfornin", 85);

        // Assert
        Assert.True(isHighSimilarity);
    }

    [Fact]
    public void FuzzyMatcher_IsPotentialMatch_DetectsModeratelyDifferent()
    {
        // Act
        var isPotentialMatch = FuzzyMatcher.IsPotentialMatch("Amoxicillin", "Amoxicilin", 70, 89);

        // Assert
        Assert.True(isPotentialMatch);
    }

    [Fact]
    public void FuzzyMatcher_NormalizeMedicationName_RemovesForms()
    {
        // Act
        var normalized = FuzzyMatcher.NormalizeMedicationName("Lisinopril 10mg tablets");

        // Assert
        Assert.DoesNotContain("tablets", normalized);
        Assert.DoesNotContain("mg", normalized);
        Assert.Equal("lisinopril 10", normalized);
    }

    #endregion

    #region Helper Methods

    private ExtractedClinicalData CreateMedicationData(string drugName, string dosage, string frequency)
    {
        return new ExtractedClinicalData
        {
            ExtractedDataId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DataType = ClinicalDataType.Medication,
            DataKey = "medication",
            DataValue = drugName,
            StructuredData = $"{{\"DrugName\":\"{drugName}\",\"Dosage\":\"{dosage}\",\"Frequency\":\"{frequency}\"}}",
            ConfidenceScore = 95.0m,
            VerificationStatus = VerificationStatus.AISuggested,
            ExtractedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ExtractedClinicalData CreateConditionData(string conditionName, string? icd10Code)
    {
        return new ExtractedClinicalData
        {
            ExtractedDataId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DataType = ClinicalDataType.Diagnosis,
            DataKey = "diagnosis",
            DataValue = conditionName,
            StructuredData = $"{{\"ConditionName\":\"{conditionName}\",\"ICD10Code\":\"{icd10Code}\"}}",
            ConfidenceScore = 90.0m,
            VerificationStatus = VerificationStatus.AISuggested,
            ExtractedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ExtractedClinicalData CreateAllergyData(string allergenName, string severity)
    {
        return new ExtractedClinicalData
        {
            ExtractedDataId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DataType = ClinicalDataType.Allergy,
            DataKey = "allergy",
            DataValue = allergenName,
            StructuredData = $"{{\"AllergenName\":\"{allergenName}\",\"Severity\":\"{severity}\"}}",
            ConfidenceScore = 92.0m,
            VerificationStatus = VerificationStatus.AISuggested,
            ExtractedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    private ExtractedClinicalData CreateEncounterData(DateTime encounterDate, string encounterType, string provider, string facility)
    {
        return new ExtractedClinicalData
        {
            ExtractedDataId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            DataType = ClinicalDataType.LabResult,
            DataKey = "encounter",
            DataValue = encounterType,
            StructuredData = $"{{\"EncounterDate\":\"{encounterDate:O}\",\"EncounterType\":\"{encounterType}\",\"Provider\":\"{provider}\",\"Facility\":\"{facility}\"}}",
            ConfidenceScore = 88.0m,
            VerificationStatus = VerificationStatus.AISuggested,
            ExtractedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
