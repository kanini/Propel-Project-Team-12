using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Enums;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using System.Text.Json;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for retrieving 360-Degree Patient View with sub-2-second performance (NFR-002).
/// Implements Redis caching with 15-minute TTL and verification badge mapping (UXR-402).
/// </summary>
public class PatientProfileService : IPatientProfileService
{
    private readonly PatientAccessDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PatientProfileService> _logger;
    private const int CacheExpirationMinutes = 15;

    public PatientProfileService(
        PatientAccessDbContext context,
        IDistributedCache cache,
        ILogger<PatientProfileService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PatientProfile360Dto> Get360ProfileAsync(
        Guid patientId,
        DateTime? vitalRangeStart = null,
        DateTime? vitalRangeEnd = null)
    {
        var startTime = DateTime.UtcNow;

        // Check cache first (NFR-002 optimization)
        var cacheKey = $"patient-profile-360:{patientId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation(
                "Cache hit for patient profile 360 (PatientId: {PatientId})",
                patientId);

            var cached = JsonSerializer.Deserialize<PatientProfile360Dto>(cachedData);
            if (cached != null)
            {
                return cached;
            }
        }

        _logger.LogInformation(
            "Cache miss for patient profile 360 (PatientId: {PatientId}). Querying database.",
            patientId);

        // Set default date ranges for vital trends (last 12 months)
        var rangeStart = vitalRangeStart ?? DateTime.UtcNow.AddMonths(-12);
        var rangeEnd = vitalRangeEnd ?? DateTime.UtcNow;

        // Query PatientProfile with eager loading for sub-2-second retrieval (NFR-002)
        var profile = await _context.PatientProfiles
            .AsNoTracking() // Read-only query optimization
            .Include(p => p.Patient)
            .Include(p => p.Conditions.Where(c => c.Status == "Active"))
            .Include(p => p.Medications.Where(m => m.Status == "Active"))
            .Include(p => p.Allergies.Where(a => a.Status == "Active"))
            .Include(p => p.VitalTrends.Where(v =>
                v.RecordedAt >= rangeStart && v.RecordedAt <= rangeEnd))
            .Include(p => p.Encounters.OrderByDescending(e => e.EncounterDate).Take(10))
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (profile == null)
        {
            throw new KeyNotFoundException(
                $"Patient profile not found for PatientId: {patientId}. " +
                "Upload clinical documents to build your health profile.");
        }

        // Build 360° DTO
        var dto = new PatientProfile360Dto
        {
            PatientId = profile.PatientId,
            Demographics = await BuildDemographicsSectionAsync(profile.Patient),
            Conditions = await BuildConditionsSectionAsync(profile.Conditions.ToList()),
            Medications = await BuildMedicationsSectionAsync(profile.Medications.ToList()),
            Allergies = await BuildAllergiesSectionAsync(profile.Allergies.ToList()),
            VitalTrends = BuildVitalTrendsSection(profile.VitalTrends.ToList(), rangeStart, rangeEnd),
            Encounters = BuildEncountersSection(profile.Encounters.ToList()),
            ProfileCompleteness = profile.ProfileCompleteness,
            LastAggregatedAt = profile.LastAggregatedAt,
            HasUnresolvedConflicts = profile.HasUnresolvedConflicts,
            TotalDocumentsProcessed = profile.TotalDocumentsProcessed
        };

        // Cache the result with 15-minute TTL
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto),
            cacheOptions);

        var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogInformation(
            "Patient profile 360 retrieved and cached (PatientId: {PatientId}, Elapsed: {Elapsed}ms)",
            patientId, elapsed);

        // Log warning if query exceeds 1.5 seconds (NFR-002 threshold)
        if (elapsed > 1500)
        {
            _logger.LogWarning(
                "Slow query detected for patient profile 360 (PatientId: {PatientId}, Elapsed: {Elapsed}ms)",
                patientId, elapsed);
        }

        return dto;
    }

    public async Task InvalidateCacheAsync(Guid patientId)
    {
        var cacheKey = $"patient-profile-360:{patientId}";
        await _cache.RemoveAsync(cacheKey);

        _logger.LogInformation(
            "Invalidated cache for patient profile 360 (PatientId: {PatientId})",
            patientId);
    }

    // Private helper methods for building DTOs

    private async Task<DemographicsSectionDto> BuildDemographicsSectionAsync(User patient)
    {
        // Split name into first/last (simple heuristic - use space as delimiter)
        var nameParts = patient.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : patient.Name;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        return new DemographicsSectionDto
        {
            PatientId = patient.UserId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = patient.DateOfBirth?.ToDateTime(TimeOnly.MinValue), // Convert DateOnly? to DateTime?
            Gender = null, // Not tracked in User entity - can be added later
            PhoneNumber = patient.Phone,
            Email = patient.Email,
            EmergencyContact = null // Not tracked in User entity - can be added later
        };
    }

    private async Task<ConditionsSectionDto> BuildConditionsSectionAsync(
        List<ConsolidatedCondition> conditions)
    {
        var conditionItems = new List<ConditionItemDto>();
        int verifiedCount = 0;

        foreach (var condition in conditions)
        {
            var badge = await DetermineVerificationBadgeAsync(condition.SourceDataIds);
            if (badge == VerificationBadge.StaffVerified)
            {
                verifiedCount++;
            }

            conditionItems.Add(new ConditionItemDto
            {
                Id = condition.Id,
                ConditionName = condition.ConditionName,
                ICD10Code = condition.ICD10Code,
                Status = condition.Status,
                DiagnosisDate = condition.DiagnosisDate,
                Severity = condition.Severity,
                Badge = badge,
                SourceDocumentIds = condition.SourceDocumentIds
            });
        }

        return new ConditionsSectionDto
        {
            ActiveConditions = conditionItems,
            VerifiedCount = verifiedCount,
            TotalCount = conditions.Count
        };
    }

    private async Task<MedicationsSectionDto> BuildMedicationsSectionAsync(
        List<ConsolidatedMedication> medications)
    {
        var medicationItems = new List<MedicationItemDto>();
        int verifiedCount = 0;

        foreach (var medication in medications)
        {
            var badge = await DetermineVerificationBadgeAsync(medication.SourceDataIds);
            if (badge == VerificationBadge.StaffVerified)
            {
                verifiedCount++;
            }

            medicationItems.Add(new MedicationItemDto
            {
                Id = medication.Id,
                DrugName = medication.DrugName,
                Dosage = medication.Dosage,
                Frequency = medication.Frequency,
                RouteOfAdministration = medication.RouteOfAdministration,
                StartDate = medication.StartDate,
                EndDate = medication.EndDate,
                Status = medication.Status,
                HasConflict = medication.HasConflict,
                Badge = badge,
                SourceDocumentIds = medication.SourceDocumentIds
            });
        }

        return new MedicationsSectionDto
        {
            ActiveMedications = medicationItems,
            VerifiedCount = verifiedCount,
            TotalCount = medications.Count
        };
    }

    private async Task<AllergiesSectionDto> BuildAllergiesSectionAsync(
        List<ConsolidatedAllergy> allergies)
    {
        var allergyItems = new List<AllergyItemDto>();
        int verifiedCount = 0;

        foreach (var allergy in allergies)
        {
            var badge = await DetermineVerificationBadgeAsync(allergy.SourceDataIds);
            if (badge == VerificationBadge.StaffVerified)
            {
                verifiedCount++;
            }

            allergyItems.Add(new AllergyItemDto
            {
                Id = allergy.Id,
                AllergenName = allergy.AllergenName,
                Reaction = allergy.Reaction,
                Severity = allergy.Severity,
                OnsetDate = allergy.OnsetDate,
                Status = allergy.Status,
                Badge = badge,
                SourceDocumentIds = allergy.SourceDocumentIds
            });
        }

        return new AllergiesSectionDto
        {
            ActiveAllergies = allergyItems,
            VerifiedCount = verifiedCount,
            TotalCount = allergies.Count
        };
    }

    private VitalTrendsSectionDto BuildVitalTrendsSection(
        List<VitalTrend> vitalTrends,
        DateTime rangeStart,
        DateTime rangeEnd)
    {
        return new VitalTrendsSectionDto
        {
            BloodPressure = vitalTrends
                .Where(v => v.VitalType.Contains("Blood Pressure", StringComparison.OrdinalIgnoreCase))
                .Select(v => new VitalDataPointDto
                {
                    RecordedAt = v.RecordedAt,
                    Value = v.Value,
                    Unit = v.Unit,
                    SourceDocumentId = v.SourceDocumentId
                })
                .OrderBy(v => v.RecordedAt)
                .ToList(),

            HeartRate = vitalTrends
                .Where(v => v.VitalType.Contains("Heart Rate", StringComparison.OrdinalIgnoreCase))
                .Select(v => new VitalDataPointDto
                {
                    RecordedAt = v.RecordedAt,
                    Value = v.Value,
                    Unit = v.Unit,
                    SourceDocumentId = v.SourceDocumentId
                })
                .OrderBy(v => v.RecordedAt)
                .ToList(),

            Temperature = vitalTrends
                .Where(v => v.VitalType.Contains("Temperature", StringComparison.OrdinalIgnoreCase))
                .Select(v => new VitalDataPointDto
                {
                    RecordedAt = v.RecordedAt,
                    Value = v.Value,
                    Unit = v.Unit,
                    SourceDocumentId = v.SourceDocumentId
                })
                .OrderBy(v => v.RecordedAt)
                .ToList(),

            Weight = vitalTrends
                .Where(v => v.VitalType.Contains("Weight", StringComparison.OrdinalIgnoreCase))
                .Select(v => new VitalDataPointDto
                {
                    RecordedAt = v.RecordedAt,
                    Value = v.Value,
                    Unit = v.Unit,
                    SourceDocumentId = v.SourceDocumentId
                })
                .OrderBy(v => v.RecordedAt)
                .ToList(),

            RangeStart = rangeStart,
            RangeEnd = rangeEnd
        };
    }

    private EncountersSectionDto BuildEncountersSection(List<ConsolidatedEncounter> encounters)
    {
        return new EncountersSectionDto
        {
            RecentEncounters = encounters
                .OrderByDescending(e => e.EncounterDate)
                .Take(10) // Most recent 10 encounters
                .Select(e => new EncounterItemDto
                {
                    Id = e.Id,
                    EncounterDate = e.EncounterDate,
                    EncounterType = e.EncounterType,
                    Provider = e.Provider,
                    Facility = e.Facility,
                    ChiefComplaint = e.ChiefComplaint,
                    SourceDocumentIds = e.SourceDocumentIds
                })
                .ToList(),
            TotalCount = encounters.Count
        };
    }

    /// <summary>
    /// Determines verification badge based on source ExtractedClinicalData records (UXR-402).
    /// Returns StaffVerified only if ALL source data records are verified.
    /// Returns AISuggested if any source data is still AI-suggested.
    /// </summary>
    private async Task<VerificationBadge> DetermineVerificationBadgeAsync(List<Guid> sourceDataIds)
    {
        if (!sourceDataIds.Any())
        {
            return VerificationBadge.AISuggested; // Default to AI-suggested if no sources
        }

        var sourceData = await _context.ExtractedClinicalData
            .AsNoTracking()
            .Where(e => sourceDataIds.Contains(e.ExtractedDataId))
            .Select(e => e.VerificationStatus)
            .ToListAsync();

        // All sources must be StaffVerified to display green badge
        var allVerified = sourceData.All(vs => vs == VerificationStatus.StaffVerified);

        return allVerified ? VerificationBadge.StaffVerified : VerificationBadge.AISuggested;
    }
}
