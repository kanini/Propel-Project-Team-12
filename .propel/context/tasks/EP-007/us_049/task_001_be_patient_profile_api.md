# Task - task_001_be_patient_profile_api

## Requirement Reference
- User Story: US_049
- Story Location: .propel/context/tasks/EP-007/us_049/us_049.md
- Acceptance Criteria:
    - **AC1**: Given I am a patient on the Health Dashboard, When the page loads, Then my 360-Degree Patient View displays demographics, active conditions, current medications, allergies, vital trends (chart), and recent encounters within 2 seconds (NFR-002).
    - **AC2**: Given I am a Staff member viewing a patient, When the 360-Degree View loads, Then each data element displays an amber badge (AI-suggested) or green badge (staff-verified) per UXR-402.
    - **AC3**: Given the 360-Degree View is generating, When the AI assembles the unified summary (AIR-007), Then data is sourced from the PatientProfile aggregated entity and organized into logical sections.
- Edge Case:
    - What happens when vital trend data spans multiple years? Chart displays a scrollable timeline with zoom controls defaulting to the last 12 months.
    - How does the system handle partially verified data? Sections show a completion bar indicating percentage verified with "X of Y items verified" count.

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Entity Framework Core | 8.0 |
| Caching | Upstash Redis | Redis 7.x compatible |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement REST API endpoint for retrieving 360-Degree Patient View data from aggregated PatientProfile entity. This task creates GET endpoint that returns unified patient health summary including demographics, consolidated conditions, medications, allergies, vital trends (time-series data), and recent encounters, implements Redis caching for 2-second response time compliance (NFR-002), includes verification status badges for staff view (AI-suggested vs staff-verified), handles date range filtering for vital trends (default last 12 months), calculates profile completeness percentage, and enforces role-based access (patients see own profile, staff see any patient). The API optimizes query performance using eager loading and projection to meet sub-2-second retrieval requirement.

**Key Capabilities:**
- GET /api/patients/{patientId}/profile/360 endpoint
- Unified PatientProfile response DTO with all sections
- Redis caching (15-minute TTL, invalidate on aggregation)
- Verification status tracking (AI-suggested vs staff-verified)
- Vital trends date range filtering (last 12 months default)
- Profile completeness percentage calculation
- Role-based authorization (Patient: own profile, Staff: any patient)
- Eager loading optimization (< 2 seconds retrieval)
- Cache invalidation on data updates

## Dependent Tasks
- EP-007: US_047: task_001_db_patient_profile_schema (PatientProfile entities)
- EP-007: US_047: task_003_be_aggregation_service (PatientProfile data)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/IPatientProfileService.cs` - Profile retrieval interface
- **NEW**: `src/backend/PatientAccess.Business/Services/PatientProfileService.cs` - Profile retrieval implementation
- **NEW**: `src/backend/PatientAccess.Business/DTOs/PatientProfile360Dto.cs` - 360° view DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/DemographicsSectionDto.cs` - Demographics DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ConditionsSectionDto.cs` - Conditions DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/MedicationsSectionDto.cs` - Medications DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/AllergiesSectionDto.cs` - Allergies DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/VitalTrendsSectionDto.cs` - Vital trends DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/EncountersSectionDto.cs` - Encounters DTO
- **NEW**: `src/backend/PatientAccess.Web/Controllers/PatientProfileController.cs` - Profile API controller
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register profile service, configure Redis caching
- **NEW**: `src/backend/PatientAccess.Tests/Services/PatientProfileServiceTests.cs` - Unit tests

## Implementation Plan

1. **Create PatientProfile360Dto Structure**
   - Top-level DTO:
     ```csharp
     public class PatientProfile360Dto
     {
         public int PatientId { get; set; }
         public DemographicsSectionDto Demographics { get; set; }
         public ConditionsSectionDto Conditions { get; set; }
         public MedicationsSectionDto Medications { get; set; }
         public AllergiesSectionDto Allergies { get; set; }
         public VitalTrendsSectionDto VitalTrends { get; set; }
         public EncountersSectionDto Encounters { get; set; }
         public decimal ProfileCompleteness { get; set; } // 0-100%
         public DateTime LastAggregatedAt { get; set; }
         public bool HasUnresolvedConflicts { get; set; }
     }
     ```

2. **Create Section DTOs**
   - **DemographicsSectionDto**: PatientId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, EmergencyContact
   - **ConditionsSectionDto**:
     ```csharp
     public class ConditionsSectionDto
     {
         public List<ConditionItemDto> ActiveConditions { get; set; }
         public int VerifiedCount { get; set; }
         public int TotalCount { get; set; }
     }
     public class ConditionItemDto
     {
         public Guid Id { get; set; }
         public string ConditionName { get; set; }
         public string ICD10Code { get; set; }
         public string Status { get; set; } // Active, Resolved
         public DateTime? DiagnosisDate { get; set; }
         public VerificationBadge Badge { get; set; } // AI-suggested or Staff-verified
     }
     ```
   - **MedicationsSectionDto**: Similar structure with ActiveMedications list, DrugName, Dosage, Frequency, Status
   - **AllergiesSectionDto**: Similar structure with ActiveAllergies list, AllergenName, Severity, Reaction
   - **VitalTrendsSectionDto**:
     ```csharp
     public class VitalTrendsSectionDto
     {
         public List<VitalDataPointDto> BloodPressure { get; set; }
         public List<VitalDataPointDto> HeartRate { get; set; }
         public List<VitalDataPointDto> Temperature { get; set; }
         public List<VitalDataPointDto> Weight { get; set; }
         public DateTime RangeStart { get; set; } // Default: 12 months ago
         public DateTime RangeEnd { get; set; } // Default: today
     }
     public class VitalDataPointDto
     {
         public DateTime RecordedAt { get; set; }
         public string Value { get; set; }
         public string Unit { get; set; }
     }
     ```
   - **EncountersSectionDto**: List of EncounterItemDto with EncounterDate, Type, Provider, Facility

3. **Create VerificationBadge Enum**
   - Enum:
     ```csharp
     public enum VerificationBadge
     {
         AISuggested = 0,  // Amber badge
         StaffVerified = 1  // Green badge
     }
     ```
   - Map from ExtractedClinicalData.VerificationStatus: "Suggested" → AISuggested, "Verified" → StaffVerified

4. **Implement PatientProfileService.Get360ProfileAsync**
   - Method signature: `Task<PatientProfile360Dto> Get360ProfileAsync(int patientId, DateTime? vitalRangeStart = null, DateTime? vitalRangeEnd = null)`
   - Query PatientProfile with eager loading:
     ```csharp
     var profile = await context.PatientProfiles
         .Include(p => p.Patient)
         .Include(p => p.Conditions.Where(c => c.Status == "Active"))
         .Include(p => p.Medications.Where(m => m.Status == "Active"))
         .Include(p => p.Allergies.Where(a => a.Status == "Active"))
         .Include(p => p.VitalTrends)
         .Include(p => p.Encounters.OrderByDescending(e => e.EncounterDate).Take(10))
         .FirstOrDefaultAsync(p => p.PatientId == patientId);
     ```
   - Filter vital trends by date range (default: last 12 months)
   - Project to PatientProfile360Dto
   - Calculate verified counts for each section
   - Return unified DTO

5. **Implement Redis Caching**
   - Install `StackExchange.Redis` NuGet package
   - Configure Redis in `Program.cs`:
     ```csharp
     services.AddStackExchangeRedisCache(options =>
     {
         options.Configuration = configuration["Redis:ConnectionString"];
         options.InstanceName = "PatientAccess:";
     });
     ```
   - Wrap Get360ProfileAsync with caching:
     - Cache key: `patient-profile-360:{patientId}`
     - TTL: 15 minutes
     - On cache miss: query database, store in cache
     - On cache hit: return cached data
   - Invalidate cache:
     - When DataAggregationService updates PatientProfile
     - When ConflictResolutionService resolves conflict
     - When staff verifies data

6. **Create PatientProfileController**
   - Endpoint: `GET /api/patients/{patientId}/profile/360`
   - Query params:
     - vitalRangeStart (DateTime, optional, default: 12 months ago)
     - vitalRangeEnd (DateTime, optional, default: today)
   - Authorization:
     - Patient role: Can only retrieve own profile (check JWT patientId claim)
     - Staff role: Can retrieve any patient profile
     - Admin role: Can retrieve any patient profile
   - Response: 200 OK with PatientProfile360Dto, 404 Not Found if PatientProfile doesn't exist
   - Performance logging: Log warning if query exceeds 1.5 seconds

7. **Implement Role-Based Access Control**
   - Use `[Authorize]` attribute with role policy
   - In controller action:
     ```csharp
     var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
     var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
     
     if (currentUserRole == "Patient")
     {
         // Ensure patient can only access own profile
         if (int.Parse(currentUserId) != patientId)
         {
             return Forbid();
         }
     }
     ```

8. **Optimize Query Performance**
   - Use `.AsNoTracking()` for read-only queries (improve performance)
   - Use projection instead of mapping entire entities
   - Add database indexes (already created in US_047 task_001):
     - ConsolidatedCondition: PatientProfileId, Status
     - ConsolidatedMedication: PatientProfileId, Status
     - ConsolidatedAllergy: PatientProfileId, Status
     - VitalTrend: PatientProfileId, VitalType, RecordedAt (composite)
   - Measure query time with Application Insights
   - Target: < 1.5 seconds database query, < 2 seconds total with serialization

9. **Handle Empty Profile State**
   - If PatientProfile doesn't exist: return 404 Not Found with message "Patient profile not found. Upload clinical documents to build your health profile."
   - If PatientProfile exists but all sections are empty: return 200 OK with empty lists and ProfileCompleteness = 0%

10. **Add Comprehensive Telemetry**
    - Track metrics in Application Insights:
      * `patient_profile_retrieval_time_ms` (query time)
      * `patient_profile_cache_hit_rate` (percentage)
      * `patient_profile_completeness_avg` (average completeness)
    - Log profile retrieval with patient ID, role, cache hit/miss
    - Track custom events:
      * `PatientProfileRetrieved` (patient ID, role, completeness)
      * `Patient Profile360CacheHit` (patient ID)
      * `PatientProfile360SlowQuery` (patient ID, query time)

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── DataAggregationService.cs (from US_047)
│   │   └── ConflictDetectionService.cs (from US_048)
│   └── DTOs/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── PatientProfile.cs (from US_047)
│   │   ├── ConsolidatedCondition.cs (from US_047)
│   │   └── Patient.cs (from EP-001)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    ├── Program.cs
    └── Controllers/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/IPatientProfileService.cs | Profile retrieval interface |
| CREATE | src/backend/PatientAccess.Business/Services/PatientProfileService.cs | Profile retrieval implementation |
| CREATE | src/backend/PatientAccess.Business/DTOs/PatientProfile360Dto.cs | 360° view DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/DemographicsSectionDto.cs | Demographics section DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/ConditionsSectionDto.cs | Conditions section DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/MedicationsSectionDto.cs | Medications section DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/AllergiesSectionDto.cs | Allergies section DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/VitalTrendsSectionDto.cs | Vital trends section DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/EncountersSectionDto.cs | Encounters section DTO |
| CREATE | src/backend/PatientAccess.Business/Enums/VerificationBadge.cs | Verification badge enum |
| CREATE | src/backend/PatientAccess.Web/Controllers/PatientProfileController.cs | Profile API controller |
| CREATE | src/backend/PatientAccess.Tests/Services/PatientProfileServiceTests.cs | Unit tests |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register profile service, configure Redis |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Documentation
- **Controllers**: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- **Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
- **Caching**: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed

### Entity Framework Core Documentation
- **Eager Loading**: https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager
- **Performance**: https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying

### StackExchange.Redis Documentation
- **Getting Started**: https://stackexchange.github.io/StackExchange.Redis/
- **Distributed Caching**: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed

### Design Requirements
- **FR-032**: System MUST generate 360-Degree Patient View displaying unified patient health summary (spec.md)
- **FR-033**: System MUST display 360-Degree Patient View as read-only for patients (spec.md)
- **AIR-007**: System MUST generate 360-Degree Patient View from aggregated clinical data (design.md)
- **NFR-002**: System MUST retrieve and display 360-Degree Patient View within 2 seconds (design.md)
- **NFR-003**: System MUST implement role-based access control (design.md)
- **UXR-402**: System MUST visually distinguish AI-suggested data (amber badge) from staff-verified data (green badge) (figma_spec.md)

### Existing Codebase Patterns
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/DataAggregationService.cs`
- **Controller Pattern**: `src/backend/PatientAccess.Web/Controllers/DocumentsController.cs`
- **DTO Pattern**: `src/backend/PatientAccess.Business/DTOs/AggregationResultDto.cs`

## Build Commands
```powershell
# Add Redis caching package
cd src/backend
dotnet add PatientAccess.Web package StackExchange.Redis

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run unit tests
dotnet test --filter "FullyQualifiedName~PatientProfileServiceTests"

# Run application
cd PatientAccess.Web
dotnet run

# Test profile API endpoint
$token = "your-patient-or-staff-jwt-token"
$patientId = 123
Invoke-WebRequest -Uri "http://localhost:5000/api/patients/$patientId/profile/360" -Method Get -Headers @{ Authorization = "Bearer $token" }
```

## Implementation Validation Strategy
- [ ] Unit tests pass (profile retrieval, caching, authorization)
- [ ] Integration tests pass (end-to-end 360° view retrieval)
- [ ] Profile retrieval completes within 2 seconds (NFR-002)
- [ ] Redis caching works (cache hit returns data <100ms)
- [ ] Cache invalidates on PatientProfile updates
- [ ] Role-based authorization works (Patient: own profile only, Staff: any patient)
- [ ] Verification badges correctly mapped (AISuggested vs StaffVerified)
- [ ] Vital trends filtered by date range (default: last 12 months)
- [ ] Profile completeness calculated correctly (0-100%)
- [ ] Empty profile returns 404 with appropriate message
- [ ] Verified counts calculated correctly for each section
- [ ] Query performance optimized (eager loading, projection, AsNoTracking)
- [ ] Application Insights logs retrieval metrics
- [ ] API returns 403 Forbidden when patient tries to access other patient's profile

## Implementation Checklist
- [ ] Create PatientProfile360Dto with all section DTOs
- [ ] Create VerificationBadge enum (AISuggested, StaffVerified)
- [ ] Implement Get360ProfileAsync with eager loading and date range filtering
- [ ] Add Redis caching with 15-minute TTL
- [ ] Create PatientProfileController with GET /api/patients/{patientId}/profile/360 endpoint
- [ ] Implement role-based authorization (Patient: own profile, Staff: any patient)
- [ ] Optimize query performance with AsNoTracking and projection
- [ ] Calculate verified counts for conditions, medications, allergies sections
- [ ] Handle empty profile state (404 with message)
- [ ] Add cache invalidation logic in DataAggregationService
- [ ] Register PatientProfileService and Redis in Program.cs
- [ ] Add telemetry tracking for retrieval time, cache hit rate, completeness
- [ ] Write unit tests for profile retrieval, caching, authorization
- [ ] Validate 2-second retrieval requirement with performance tests
