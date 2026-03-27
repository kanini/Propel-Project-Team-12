using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Models;
using PatientAccess.Business.Services;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using Xunit;

namespace PatientAccess.Tests.Services;

/// <summary>
/// Unit tests for DataAggregationService (EP-007, Task 003).
/// Tests incremental aggregation, profile completeness calculation, and conflict handling.
/// </summary>
public class DataAggregationServiceTests : IDisposable
{
    private readonly PatientAccessDbContext _context;
    private readonly Mock<IEntityResolutionService> _entityResolutionMock;
    private readonly Mock<IPusherService> _pusherMock;
    private readonly Mock<ILogger<DataAggregationService>> _loggerMock;
    private readonly DataAggregationService _service;

    public DataAggregationServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<PatientAccessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        _context = new PatientAccessDbContext(options);
        _entityResolutionMock = new Mock<IEntityResolutionService>();
        _pusherMock = new Mock<IPusherService>();
        _loggerMock = new Mock<ILogger<DataAggregationService>>();

        var conflictDetectionMock = new Mock<IConflictDetectionService>();
        _service = new DataAggregationService(
            _context,
            _entityResolutionMock.Object,
            conflictDetectionMock.Object,
            _pusherMock.Object,
            _loggerMock.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Name = "John Doe",
            Email = "john.doe@test.com",
            DateOfBirth = new DateOnly(1980, 5, 15),
            Role = UserRole.Patient,
            PasswordHash = "hash",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetOrCreatePatientProfileAsync Tests

    [Fact]
    public async Task GetOrCreatePatientProfileAsync_WhenProfileExists_ReturnsExistingProfile()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var existingProfile = new PatientProfile
        {
            PatientId = userId,
            LastAggregatedAt = DateTime.UtcNow.AddDays(-1),
            TotalDocumentsProcessed = 5,
            ProfileCompleteness = 75,
            HasUnresolvedConflicts = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.PatientProfiles.Add(existingProfile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetOrCreatePatientProfileAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingProfile.Id, result.Id);
        Assert.Equal(5, result.TotalDocumentsProcessed);
        Assert.Equal(75, result.ProfileCompleteness);
    }

    [Fact]
    public async Task GetOrCreatePatientProfileAsync_WhenProfileDoesNotExist_CreatesNewProfile()
    {
        // Arrange
        var userId = _context.Users.First().UserId;

        // Act
        var result = await _service.GetOrCreatePatientProfileAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.PatientId);
        Assert.Equal(0, result.TotalDocumentsProcessed);
        Assert.Equal(0, result.ProfileCompleteness);
        Assert.False(result.HasUnresolvedConflicts);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.NotEqual(default, result.LastAggregatedAt);
    }

    #endregion

    #region CalculateProfileCompletenessAsync Tests

    [Fact]
    public async Task CalculateProfileCompletenessAsync_EmptyProfile_Returns20PercentForDemographics()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var profile = await _service.GetOrCreatePatientProfileAsync(userId);

        // Act
        var completeness = await _service.CalculateProfileCompletenessAsync(profile.Id);

        // Assert
        Assert.Equal(20, completeness); // Demographics only (name + email + DOB)
    }

    [Fact]
    public async Task CalculateProfileCompletenessAsync_WithAllData_Returns100Percent()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var profile = await _service.GetOrCreatePatientProfileAsync(userId);

        // Add conditions
        _context.ConsolidatedConditions.Add(new ConsolidatedCondition
        {
            PatientProfileId = profile.Id,
            ConditionName = "Hypertension",
            ICD10Code = "I10",
            Status = "Active",
            FirstRecordedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        // Add medications
        _context.ConsolidatedMedications.Add(new ConsolidatedMedication
        {
            PatientProfileId = profile.Id,
            DrugName = "Lisinopril",
            Dosage = "10mg",
            Frequency = "once daily",
            Status = "Active",
            FirstRecordedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        // Add allergies
        _context.ConsolidatedAllergies.Add(new ConsolidatedAllergy
        {
            PatientProfileId = profile.Id,
            AllergenName = "Penicillin",
            Reaction = "Rash",
            Severity = "Moderate",
            FirstRecordedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        // Add 5 vitals
        for (int i = 0; i < 5; i++)
        {
            _context.VitalTrends.Add(new VitalTrend
            {
                PatientProfileId = profile.Id,
                VitalType = "Blood Pressure",
                Value = "120/80",
                Unit = "mmHg",
                RecordedAt = DateTime.UtcNow.AddDays(-i),
                SourceDataId = Guid.NewGuid(),
                SourceDocumentId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });
        }

        // Add encounter
        _context.ConsolidatedEncounters.Add(new ConsolidatedEncounter
        {
            PatientProfileId = profile.Id,
            EncounterDate = DateTime.UtcNow,
            EncounterType = "Outpatient",
            Provider = "Dr. Smith",
            Facility = "General Hospital",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Act
        var completeness = await _service.CalculateProfileCompletenessAsync(profile.Id);

        // Assert
        Assert.Equal(100, completeness); // 20% demographics + 15% conditions + 15% medications + 10% allergies + 20% vitals + 20% encounters
    }

    [Fact]
    public async Task CalculateProfileCompletenessAsync_WithPartialVitals_ReturnsPartialVitalsScore()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var profile = await _service.GetOrCreatePatientProfileAsync(userId);

        // Add only 3 vitals (partial)
        for (int i = 0; i < 3; i++)
        {
            _context.VitalTrends.Add(new VitalTrend
            {
                PatientProfileId = profile.Id,
                VitalType = "Heart Rate",
                Value = "75",
                Unit = "bpm",
                RecordedAt = DateTime.UtcNow.AddDays(-i),
                SourceDataId = Guid.NewGuid(),
                SourceDocumentId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        // Act
        var completeness = await _service.CalculateProfileCompletenessAsync(profile.Id);

        // Assert - 20% demographics + 12% vitals (60% of 20% because 3/5 vitals)
        Assert.Equal(32, completeness);
    }

    #endregion

    #region IncrementalAggregateAsync Tests

    [Fact]
    public async Task IncrementalAggregateAsync_WithNewMedications_CreatesConsolidatedMedication()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var documentId = Guid.NewGuid();

        // Create clinical document
        var document = new ClinicalDocument
        {
            DocumentId = documentId,
            PatientId = userId,
            FileName = "test-doc.pdf",
            ProcessingStatus = ProcessingStatus.Completed,
            UploadedAt = DateTime.UtcNow
        };
        _context.ClinicalDocuments.Add(document);

        // Create extracted medication data
        var extractedDataId = Guid.NewGuid();
        var medicationData = new
        {
            drugName = "Metformin",
            dosage = "500mg",
            frequency = "twice daily"
        };

        var extractedData = new ExtractedClinicalData
        {
            ExtractedDataId = extractedDataId,
            DocumentId = documentId,
            DataType = ClinicalDataType.Medication,
            StructuredData = System.Text.Json.JsonSerializer.Serialize(medicationData),
            ConfidenceScore = 95.5m,
            VerificationStatus = VerificationStatus.AISuggested,
            ExtractedAt = DateTime.UtcNow
        };
        _context.ExtractedClinicalData.Add(extractedData);
        await _context.SaveChangesAsync();

        // Mock entity resolution (no duplicates)
        _entityResolutionMock
            .Setup(x => x.ResolveMedicationDuplicatesAsync(It.IsAny<List<ExtractedClinicalData>>()))
            .ReturnsAsync(new Dictionary<Guid, EntityMatchResult>
            {
                {
                    extractedDataId,
                    new EntityMatchResult
                    {
                        IsMatch = false,
                        MatchType = PatientAccess.Business.Models.MatchType.NoMatch,
                        SimilarityScore = 0,
                        HasConflict = false,
                        SourceEntityIds = new List<Guid> { extractedDataId }
                    }
                }
            });

        // Mock Pusher
        _pusherMock
            .Setup(x => x.SendAggregationCompleteAsync(userId, It.IsAny<AggregationResultDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IncrementalAggregateAsync(userId, documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.NewMedicationsCount);
        Assert.Equal(0, result.ConflictsDetected);
        Assert.True(result.IsIncremental);

        var medications = await _context.ConsolidatedMedications
            .Where(m => m.PatientProfile.PatientId == userId)
            .ToListAsync();

        Assert.Single(medications);
        Assert.Equal("Metformin", medications[0].DrugName);
        Assert.Equal("500mg", medications[0].Dosage);
        Assert.Equal("twice daily", medications[0].Frequency);
        Assert.False(medications[0].HasConflict);
    }

    [Fact]
    public async Task IncrementalAggregateAsync_WithDuplicateMedication_UpdatesExistingMedication()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var profile = await _service.GetOrCreatePatientProfileAsync(userId);

        // Create existing consolidated medication
        var existingMedicationId = Guid.NewGuid();
        var existingMedication = new ConsolidatedMedication
        {
            Id = existingMedicationId,
            PatientProfileId = profile.Id,
            DrugName = "Lisinopril",
            Dosage = "10mg",
            Frequency = "once daily",
            Status = "Active",
            FirstRecordedAt = DateTime.UtcNow.AddDays(-30),
            LastUpdatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            SourceDataIds = new List<Guid> { Guid.NewGuid() },
            SourceDocumentIds = new List<Guid> { Guid.NewGuid() },
            DuplicateCount = 1
        };
        _context.ConsolidatedMedications.Add(existingMedication);
        await _context.SaveChangesAsync();

        // Create new document with duplicate medication
        var documentId = Guid.NewGuid();
        var document = new ClinicalDocument
        {
            DocumentId = documentId,
            PatientId = userId,
            FileName = "duplicate-doc.pdf",
            ProcessingStatus = ProcessingStatus.Completed,
            UploadedAt = DateTime.UtcNow
        };
        _context.ClinicalDocuments.Add(document);

        var extractedDataId = Guid.NewGuid();
        var medicationData = new
        {
            drugName = "Lisinopril",
            dosage = "10mg",
            frequency = "once daily"
        };

        var extractedData = new ExtractedClinicalData
        {
            ExtractedDataId = extractedDataId,
            DocumentId = documentId,
            DataType = ClinicalDataType.Medication,
            StructuredData = System.Text.Json.JsonSerializer.Serialize(medicationData),
            ConfidenceScore = 92.0m,
            VerificationStatus = VerificationStatus.AISuggested,
            ExtractedAt = DateTime.UtcNow
        };
        _context.ExtractedClinicalData.Add(extractedData);
        await _context.SaveChangesAsync();

        // Mock entity resolution (exact match found)
        _entityResolutionMock
            .Setup(x => x.ResolveMedicationDuplicatesAsync(It.IsAny<List<ExtractedClinicalData>>()))
            .ReturnsAsync(new Dictionary<Guid, EntityMatchResult>
            {
                {
                    extractedDataId,
                    new EntityMatchResult
                    {
                        IsMatch = true,
                        MatchType = PatientAccess.Business.Models.MatchType.ExactMatch,
                        SimilarityScore = 100,
                        HasConflict = false,
                        SourceEntityIds = new List<Guid> { extractedDataId }
                    }
                }
            });

        _pusherMock
            .Setup(x => x.SendAggregationCompleteAsync(userId, It.IsAny<AggregationResultDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IncrementalAggregateAsync(userId, documentId);

        // Assert
        Assert.Equal(0, result.NewMedicationsCount); // No new medication, just updated existing
        Assert.Equal(0, result.ConflictsDetected);

        var medications = await _context.ConsolidatedMedications
            .Where(m => m.PatientProfile.PatientId == userId)
            .ToListAsync();

        Assert.Single(medications); // Still only one medication
        Assert.Equal(2, medications[0].DuplicateCount); // Count incremented
    }

    [Fact]
    public async Task IncrementalAggregateAsync_WithMedicationConflict_CreatesDataConflict()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var documentId = Guid.NewGuid();

        var document = new ClinicalDocument
        {
            DocumentId = documentId,
            PatientId = userId,
            FileName = "conflict-doc.pdf",
            ProcessingStatus = ProcessingStatus.Completed,
            UploadedAt = DateTime.UtcNow
        };
        _context.ClinicalDocuments.Add(document);

        var extractedDataId = Guid.NewGuid();
        var medicationData = new
        {
            drugName = "Metformin",
            dosage = "1000mg", // Conflict with existing 500mg dosage
            frequency = "twice daily"
        };

        var extractedData = new ExtractedClinicalData
        {
            ExtractedDataId = extractedDataId,
            DocumentId = documentId,
            DataType = ClinicalDataType.Medication,
            StructuredData = System.Text.Json.JsonSerializer.Serialize(medicationData),
            ConfidenceScore = 88.0m,
            VerificationStatus = VerificationStatus.AISuggested,
            ExtractedAt = DateTime.UtcNow
        };
        _context.ExtractedClinicalData.Add(extractedData);
        await _context.SaveChangesAsync();

        // Mock entity resolution (conflict detected)
        _entityResolutionMock
            .Setup(x => x.ResolveMedicationDuplicatesAsync(It.IsAny<List<ExtractedClinicalData>>()))
            .ReturnsAsync(new Dictionary<Guid, EntityMatchResult>
            {
                {
                    extractedDataId,
                    new EntityMatchResult
                    {
                        IsMatch = true,
                        MatchType = PatientAccess.Business.Models.MatchType.HighSimilarity,
                        SimilarityScore = 85,
                        HasConflict = true,
                        ConflictDetails = "Dosage mismatch: 500mg vs 1000mg",
                        RequiresManualReview = true,
                        SourceEntityIds = new List<Guid> { extractedDataId }
                    }
                }
            });

        _pusherMock
            .Setup(x => x.SendAggregationCompleteAsync(userId, It.IsAny<AggregationResultDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IncrementalAggregateAsync(userId, documentId);

        // Assert
        Assert.Equal(1, result.NewMedicationsCount);
        Assert.Equal(1, result.ConflictsDetected);
        Assert.True(result.HasUnresolvedConflicts);

        var medications = await _context.ConsolidatedMedications
            .Where(m => m.PatientProfile.PatientId == userId)
            .ToListAsync();

        Assert.Single(medications);
        Assert.True(medications[0].HasConflict);

        var conflicts = await _context.DataConflicts
            .Where(c => c.PatientProfileId == result.PatientProfileId)
            .ToListAsync();

        Assert.Single(conflicts);
        Assert.Equal("Medication", conflicts[0].EntityType);
        Assert.Equal("Unresolved", conflicts[0].ResolutionStatus);
    }

    [Fact]
    public async Task IncrementalAggregateAsync_WithVitals_PreservesAllVitalsWithoutDeduplication()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var documentId = Guid.NewGuid();

        var document = new ClinicalDocument
        {
            DocumentId = documentId,
            PatientId = userId,
            FileName = "vitals-doc.pdf",
            ProcessingStatus = ProcessingStatus.Completed,
            UploadedAt = DateTime.UtcNow
        };
        _context.ClinicalDocuments.Add(document);

        // Add 3 vital readings (same type, different times)
        var vitalDataIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var vitalDataId = Guid.NewGuid();
            vitalDataIds.Add(vitalDataId);

            var vitalData = new
            {
                vitalType = "Blood Pressure",
                value = $"{120 + i * 5}/{80 + i * 2}",
                unit = "mmHg",
                recordedAt = DateTime.UtcNow.AddHours(-i).ToString("o")
            };

            var extractedData = new ExtractedClinicalData
            {
                ExtractedDataId = vitalDataId,
                DocumentId = documentId,
                DataType = ClinicalDataType.Vital,
                StructuredData = System.Text.Json.JsonSerializer.Serialize(vitalData),
                ConfidenceScore = 98.0m,
                VerificationStatus = VerificationStatus.AISuggested,
                ExtractedAt = DateTime.UtcNow
            };
            _context.ExtractedClinicalData.Add(extractedData);
        }

        await _context.SaveChangesAsync();

        _pusherMock
            .Setup(x => x.SendAggregationCompleteAsync(userId, It.IsAny<AggregationResultDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IncrementalAggregateAsync(userId, documentId);

        // Assert
        Assert.Equal(3, result.NewVitalsCount);

        var vitals = await _context.VitalTrends
            .Where(v => v.PatientProfile.PatientId == userId)
            .OrderBy(v => v.RecordedAt)
            .ToListAsync();

        // ALL 3 vitals should be preserved (no de-duplication per AC4 edge case)
        Assert.Equal(3, vitals.Count);
        Assert.All(vitals, v => Assert.Equal("Blood Pressure", v.VitalType));
        Assert.All(vitals, v => Assert.Equal("mmHg", v.Unit));
    }

    [Fact]
    public async Task IncrementalAggregateAsync_SendsPusherNotification()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var documentId = Guid.NewGuid();

        var document = new ClinicalDocument
        {
            DocumentId = documentId,
            PatientId = userId,
            FileName = "test-doc.pdf",
            ProcessingStatus = ProcessingStatus.Completed,
            UploadedAt = DateTime.UtcNow
        };
        _context.ClinicalDocuments.Add(document);
        await _context.SaveChangesAsync();

        _pusherMock
            .Setup(x => x.SendAggregationCompleteAsync(userId, It.IsAny<AggregationResultDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IncrementalAggregateAsync(userId, documentId);

        // Assert
        _pusherMock.Verify(
            x => x.SendAggregationCompleteAsync(
                userId,
                It.Is<AggregationResultDto>(dto =>
                    dto.PatientId == userId &&
                    dto.IsIncremental == true)),
            Times.Once);
    }

    #endregion

    #region ReaggregatePatientProfileAsync Tests

    [Fact]
    public async Task ReaggregatePatientProfileAsync_DeletesExistingDataAndReaggregates()
    {
        // Arrange
        var userId = _context.Users.First().UserId;
        var profile = await _service.GetOrCreatePatientProfileAsync(userId);

        // Add existing consolidated data
        _context.ConsolidatedMedications.Add(new ConsolidatedMedication
        {
            PatientProfileId = profile.Id,
            DrugName = "Old Medication",
            Dosage = "10mg",
            Frequency = "once daily",
            Status = "Active",
            FirstRecordedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        _pusherMock
            .Setup(x => x.SendAggregationCompleteAsync(userId, It.IsAny<AggregationResultDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ReaggregatePatientProfileAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsIncremental);

        // Old data should be deleted
        var medications = await _context.ConsolidatedMedications
            .Where(m => m.PatientProfileId == profile.Id)
            .ToListAsync();

        Assert.Empty(medications); // No documents to re-aggregate from, so empty
    }

    #endregion
}
