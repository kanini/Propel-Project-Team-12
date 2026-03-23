# Task - task_001_be_conflict_detection_service

## Requirement Reference
- User Story: US_048
- Story Location: .propel/context/tasks/EP-007/us_048/us_048.md
- Acceptance Criteria:
    - **AC1**: Given aggregation finds conflicting data (e.g., Medication A from Document 1 vs Medication B for same condition from Document 2), When the conflict is detected, Then it is flagged with severity level, both source references, and requires staff verification.
    - **AC3**: Given a conflict involves medications, When the conflict is categorized, Then medication conflicts are marked as "Critical" (highest severity) due to patient safety implications.
    - **AC4**: Given the AI resolves ambiguity by itself, When it determines one source is more authoritative, Then the suggestion is still flagged for staff review — no auto-resolution of clinical conflicts.
- Edge Case:
    - What happens when a patient has 50+ data points with multiple conflicts? Conflicts are prioritized by severity (Critical > Warning > Info) and paginated.
    - How does the system handle conflicts between AI-extracted data and staff-verified data? Staff-verified data takes precedence; the conflict is auto-resolved with audit trail.

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
| Real-time | Pusher .NET Server | 5.x |
| Monitoring | Application Insights | Latest |
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

Implement conflict detection service to identify critical data inconsistencies across patient documents during aggregation. This task creates severity-based conflict classification (Critical for medications/allergies, Warning for diagnoses, Info for vitals), automated conflict flagging when entity resolution detects mismatches, source document traceability for all conflicts, staff-verified data precedence logic (auto-resolve conflicts when one source is staff-verified), and REST API endpoints for retrieving, prioritizing, and resolving conflicts. The service integrates with DataAggregationService to detect conflicts during incremental aggregation and sends real-time Pusher notifications when critical conflicts are detected.

**Key Capabilities:**
- Severity-based classification (Critical, Warning, Info)
- Medication conflict detection (different doses, contraindications)
- Allergy conflict detection (different severity levels)
- Diagnosis conflict detection (inconsistent ICD-10 codes)
- Vital sign outlier detection (conflicting ranges)
- Staff-verified data precedence
- Conflict prioritization and pagination
- Real-time Pusher notifications for critical conflicts
- REST API for conflict management
- Audit trail for conflict resolution

## Dependent Tasks
- EP-007: US_047: task_001_db_patient_profile_schema (DataConflict entity)
- EP-007: US_047: task_002_be_entity_resolution_service (EntityResolutionService for conflict detection)
- EP-007: US_047: task_003_be_aggregation_service (DataAggregationService integration)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/IConflictDetectionService.cs` - Conflict detection interface
- **NEW**: `src/backend/PatientAccess.Business/Services/ConflictDetectionService.cs` - Conflict detection implementation
- **NEW**: `src/backend/PatientAccess.Business/DTOs/DataConflictDto.cs` - Conflict DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ConflictSummaryDto.cs` - Conflict summary DTO
- **NEW**: `src/backend/PatientAccess.Business/Enums/ConflictSeverity.cs` - Severity enum
- **NEW**: `src/backend/PatientAccess.Web/Controllers/ConflictsController.cs` - Conflict REST API
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DataAggregationService.cs` - Integrate conflict detection
- **MODIFY**: `src/backend/PatientAccess.Business/Services/PusherService.cs` - Add conflict notification event
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register conflict detection service
- **NEW**: `src/backend/PatientAccess.Tests/Services/ConflictDetectionServiceTests.cs` - Unit tests

## Implementation Plan

1. **Create ConflictSeverity Enum**
   - Values:
     - Critical = 0 (medications, allergies with severe safety implications)
     - Warning = 1 (diagnoses, conditions with clinical significance)
     - Info = 2 (vitals, minor discrepancies)
   - Use integer values for database storage and ordering

2. **Create IConflictDetectionService Interface**
   - Methods:
     - `DetectMedicationConflictsAsync(ConsolidatedMedication medication, List<ExtractedClinicalData> sources)` - Detect medication conflicts
     - `DetectAllergyConflictsAsync(ConsolidatedAllergy allergy, List<ExtractedClinicalData> sources)` - Detect allergy conflicts
     - `DetectConditionConflictsAsync(ConsolidatedCondition condition, List<ExtractedClinicalData> sources)` - Detect diagnosis conflicts
     - `ClassifyConflictSeverityAsync(string entityType, DataConflict conflict)` - Calculate severity
     - `ResolveConflictAsync(Guid conflictId, int staffUserId, string resolution)` - Mark conflict as resolved
     - `GetConflictsAsync(int patientProfileId, ConflictSeverity? severity = null, bool unresolvedOnly = true)` - Retrieve conflicts

3. **Implement DetectMedicationConflictsAsync**
   - Check for:
     - **Dosage Mismatch**: Same drug, different doses (e.g., "10mg" vs "20mg")
     - **Frequency Mismatch**: Same drug, same dose, different frequency (e.g., "twice daily" vs "once daily")
     - **Route Mismatch**: Same drug, different routes (e.g., "oral" vs "IV")
   - If mismatch detected:
     - Create DataConflict record
     - ConflictType = "MedicationDosageMismatch" or "MedicationFrequencyMismatch"
     - Severity = Critical (always for medications)
     - Description = "Medication '{DrugName}' has conflicting dosage: {Dose1} (Document A) vs {Dose2} (Document B)"
     - SourceDataIds = List of conflicting ExtractedClinicalData IDs
     - ResolutionStatus = "Unresolved"
   - Return conflict details

4. **Implement DetectAllergyConflictsAsync**
   - Check for:
     - **Severity Mismatch**: Same allergen, different severity (e.g., "Moderate" vs "Severe")
     - **Reaction Mismatch**: Same allergen, different reactions
   - Classification:
     - Severity "Critical" or "Severe" mismatch → Critical conflict
     - Severity "Mild" or "Moderate" mismatch → Warning conflict
   - Create DataConflict record with appropriate severity

5. **Implement DetectConditionConflictsAsync**
   - Check for:
     - **ICD-10 Code Mismatch**: Same condition name, different ICD-10 codes
     - **Status Mismatch**: Same condition, different status (e.g., "Active" vs "Resolved")
   - Classification:
     - Different ICD-10 codes → Warning (requires clinical review)
     - Status mismatch → Info (may be temporal change)
   - Create DataConflict record

6. **Implement Staff-Verified Data Precedence Logic**
   - When comparing two ExtractedClinicalData records:
     - Check VerificationStatus field
     - If one is "Verified" and other is "Suggested":
       * Auto-resolve: keep verified data, discard suggested
       * Create DataConflict record with ResolutionStatus = "Resolved"
       * ResolvedBy = system (special user ID = 0)
       * Description = "Auto-resolved: Staff-verified data takes precedence over AI-suggested data"
       * Log audit trail
   - Even auto-resolved conflicts are stored for audit purposes

7. **Integrate with DataAggregationService**
   - Modify `IncrementalAggregateAsync` to call conflict detection:
     ```csharp
     // After entity resolution detects potential duplicate with mismatch
     if (entityMatchResult.HasConflict)
     {
         var conflict = await conflictDetectionService.DetectMedicationConflictsAsync(
             consolidatedMedication, 
             sourceDataPoints);
         
         // Update PatientProfile.HasUnresolvedConflicts if severity is Critical
         if (conflict.Severity == ConflictSeverity.Critical)
         {
             patientProfile.HasUnresolvedConflicts = true;
         }
     }
     ```

8. **Create ConflictsController REST API**
   - Endpoints:
     - **GET /api/patients/{patientId}/conflicts** - Get all conflicts for patient
       * Query params: severity, unresolvedOnly, page, pageSize
       * Returns paginated ConflictSummaryDto (count by severity, total unresolved)
     - **GET /api/conflicts/{conflictId}** - Get conflict details
       * Returns DataConflictDto with source references
     - **POST /api/conflicts/{conflictId}/resolve** - Resolve conflict
       * Request body: resolution (string), chosenEntityId (Guid)
       * Updates ResolutionStatus, ResolvedBy, ResolvedAt
       * Returns updated DataConflictDto
     - **GET /api/patients/{patientId}/conflicts/summary** - Get conflict summary
       * Returns count by severity, oldest unresolved conflict timestamp
   - Authorization: Require Staff role for all endpoints

9. **Enhance PusherService with Conflict Notifications**
   - Add method: `SendConflictDetectedAsync(int patientId, DataConflictDto conflict)`
   - Event channel: `private-patient-{patientId}`
   - Event name: `critical-conflict-detected`
   - Event payload:
     ```json
     {
       "conflictId": "guid",
       "severity": "Critical",
       "entityType": "Medication",
       "description": "Medication 'Aspirin' has conflicting dosage...",
       "detectedAt": "2026-03-23T10:30:00Z"
     }
     ```
   - Only send Pusher event for Critical severity conflicts

10. **Add Comprehensive Telemetry**
    - Track metrics in Application Insights:
      * `conflict_detected_count` (count by severity)
      * `conflict_resolution_time_ms` (time to resolve)
      * `auto_resolved_conflicts_count` (staff-verified precedence)
    - Log conflict detection events with:
      * Patient ID, conflict type, severity, source document IDs
      * Entity type (medication, allergy, condition)
    - Track custom events:
      * `CriticalConflictDetected` (requires immediate staff attention)
      * `ConflictAutoResolved` (staff-verified data precedence)

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── DataAggregationService.cs (from US_047, to be enhanced)
│   │   ├── EntityResolutionService.cs (from US_047)
│   │   └── PusherService.cs (from EP-006-I, to be enhanced)
│   └── DTOs/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── DataConflict.cs (from US_047)
│   │   ├── ConsolidatedMedication.cs (from US_047)
│   │   └── ExtractedClinicalData.cs (from EP-006-II)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    ├── Program.cs
    └── Controllers/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/IConflictDetectionService.cs | Conflict detection interface |
| CREATE | src/backend/PatientAccess.Business/Services/ConflictDetectionService.cs | Conflict detection implementation |
| CREATE | src/backend/PatientAccess.Business/DTOs/DataConflictDto.cs | Conflict DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/ConflictSummaryDto.cs | Conflict summary DTO |
| CREATE | src/backend/PatientAccess.Business/Enums/ConflictSeverity.cs | Severity enum |
| CREATE | src/backend/PatientAccess.Web/Controllers/ConflictsController.cs | Conflict REST API |
| CREATE | src/backend/PatientAccess.Tests/Services/ConflictDetectionServiceTests.cs | Unit tests |
| MODIFY | src/backend/PatientAccess.Business/Services/DataAggregationService.cs | Integrate conflict detection |
| MODIFY | src/backend/PatientAccess.Business/Services/PusherService.cs | Add conflict notification event |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register conflict detection service |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Design Requirements
- **FR-031**: System MUST explicitly highlight critical data conflicts requiring staff verification before clinical use (spec.md)
- **AIR-006**: System MUST detect and highlight critical data conflicts across patient documents for staff verification (design.md)
- **NFR-003**: System MUST implement role-based access control (Staff role for conflict resolution) (design.md)
- **NFR-007**: System MUST log all data access, changes, and authentication events for audit compliance (design.md)

### ASP.NET Core Documentation
- **Controllers**: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- **Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles

### Entity Framework Core Documentation
- **Enums**: https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions

### Existing Codebase Patterns
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/DataAggregationService.cs`
- **Controller Pattern**: `src/backend/PatientAccess.Web/Controllers/DocumentsController.cs`
- **Pusher Event Pattern**: `src/backend/PatientAccess.Business/Services/PusherService.cs`

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build

# Run unit tests
dotnet test --filter "FullyQualifiedName~ConflictDetectionServiceTests"

# Run all tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run

# Test conflict API endpoint
$token = "your-staff-jwt-token"
$patientId = 123
Invoke-WebRequest -Uri "http://localhost:5000/api/patients/$patientId/conflicts" -Method Get -Headers @{ Authorization = "Bearer $token" }
```

## Implementation Validation Strategy
- [ ] Unit tests pass (medication, allergy, condition conflict detection)
- [ ] Integration tests pass (end-to-end conflict detection during aggregation)
- [ ] Medication conflicts classified as Critical severity
- [ ] Allergy conflicts with "Severe" classified as Critical
- [ ] Diagnosis conflicts classified as Warning severity
- [ ] Staff-verified data takes precedence (auto-resolved conflicts)
- [ ] Conflicts API returns paginated results sorted by severity
- [ ] Conflict resolution updates ResolutionStatus and audit fields
- [ ] Pusher notification sent only for Critical conflicts
- [ ] Application Insights logs conflict detection metrics
- [ ] Conflict summary endpoint returns correct counts
- [ ] Authorization enforces Staff role requirement
- [ ] Pagination works correctly for patients with 50+ conflicts

## Implementation Checklist
- [ ] Create ConflictSeverity enum with Critical, Warning, Info values
- [ ] Create IConflictDetectionService interface with conflict detection methods
- [ ] Implement DetectMedicationConflictsAsync with dosage/frequency checks
- [ ] Implement DetectAllergyConflictsAsync with severity mismatch detection
- [ ] Implement DetectConditionConflictsAsync with ICD-10 code comparison
- [ ] Implement staff-verified data precedence logic (auto-resolve)
- [ ] Integrate conflict detection with DataAggregationService.IncrementalAggregateAsync
- [ ] Create ConflictsController with GET/POST endpoints
- [ ] Add pagination support for conflicts API (50+ conflicts edge case)
- [ ] Enhance PusherService with SendConflictDetectedAsync for Critical conflicts
- [ ] Register ConflictDetectionService in Program.cs dependency injection
- [ ] Add telemetry tracking for conflict detection to Application Insights
- [ ] Write unit tests for each conflict detection method
- [ ] Validate Staff role authorization on all conflict endpoints
