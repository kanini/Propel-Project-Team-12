using PatientAccess.Business.Enums;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Demographics section for 360° Patient View (FR-032).
/// </summary>
public class DemographicsSectionDto
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? EmergencyContact { get; set; }
}

/// <summary>
/// Conditions section for 360° Patient View (FR-032).
/// </summary>
public class ConditionsSectionDto
{
    public List<ConditionItemDto> ActiveConditions { get; set; } = new();
    public int VerifiedCount { get; set; }
    public int TotalCount { get; set; }
}

public class ConditionItemDto
{
    public Guid Id { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public string? ICD10Code { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Resolved, Historical
    public DateTime? DiagnosisDate { get; set; }
    public string? Severity { get; set; }
    public VerificationBadge Badge { get; set; } // AI-suggested or Staff-verified (UXR-402)
    public List<Guid> SourceDocumentIds { get; set; } = new();
}

/// <summary>
/// Medications section for 360° Patient View (FR-032).
/// </summary>
public class MedicationsSectionDto
{
    public List<MedicationItemDto> ActiveMedications { get; set; } = new();
    public int VerifiedCount { get; set; }
    public int TotalCount { get; set; }
}

public class MedicationItemDto
{
    public Guid Id { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? RouteOfAdministration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Discontinued, Historical
    public bool HasConflict { get; set; }
    public VerificationBadge Badge { get; set; } // AI-suggested or Staff-verified (UXR-402)
    public List<Guid> SourceDocumentIds { get; set; } = new();
}

/// <summary>
/// Allergies section for 360° Patient View (FR-032).
/// </summary>
public class AllergiesSectionDto
{
    public List<AllergyItemDto> ActiveAllergies { get; set; } = new();
    public int VerifiedCount { get; set; }
    public int TotalCount { get; set; }
}

public class AllergyItemDto
{
    public Guid Id { get; set; }
    public string AllergenName { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime? OnsetDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Resolved
    public VerificationBadge Badge { get; set; } // AI-suggested or Staff-verified (UXR-402)
    public List<Guid> SourceDocumentIds { get; set; } = new();
}

/// <summary>
/// Vital trends section for 360° Patient View with time-series data (FR-032).
/// </summary>
public class VitalTrendsSectionDto
{
    public List<VitalDataPointDto> BloodPressure { get; set; } = new();
    public List<VitalDataPointDto> HeartRate { get; set; } = new();
    public List<VitalDataPointDto> Temperature { get; set; } = new();
    public List<VitalDataPointDto> Weight { get; set; } = new();
    public DateTime RangeStart { get; set; } // Default: 12 months ago
    public DateTime RangeEnd { get; set; } // Default: today
}

public class VitalDataPointDto
{
    public DateTime RecordedAt { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public Guid SourceDocumentId { get; set; }
}

/// <summary>
/// Encounters section for 360° Patient View (FR-032).
/// </summary>
public class EncountersSectionDto
{
    public List<EncounterItemDto> RecentEncounters { get; set; } = new();
    public int TotalCount { get; set; }
}

public class EncounterItemDto
{
    public Guid Id { get; set; }
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public string? Facility { get; set; }
    public string? ChiefComplaint { get; set; }
    public List<Guid> SourceDocumentIds { get; set; } = new();
}

/// <summary>
/// Top-level 360° Patient View DTO aggregating all health sections (FR-032, AIR-007).
/// Optimized for sub-2-second retrieval (NFR-002) with Redis caching.
/// </summary>
public class PatientProfile360Dto
{
    public Guid PatientId { get; set; }
    public DemographicsSectionDto Demographics { get; set; } = new();
    public ConditionsSectionDto Conditions { get; set; } = new();
    public MedicationsSectionDto Medications { get; set; } = new();
    public AllergiesSectionDto Allergies { get; set; } = new();
    public VitalTrendsSectionDto VitalTrends { get; set; } = new();
    public EncountersSectionDto Encounters { get; set; } = new();

    /// <summary>
    /// Profile completeness percentage (0-100%) based on data coverage.
    /// </summary>
    public decimal ProfileCompleteness { get; set; }

    public DateTime LastAggregatedAt { get; set; }
    public bool HasUnresolvedConflicts { get; set; }
    public int TotalDocumentsProcessed { get; set; }
}
