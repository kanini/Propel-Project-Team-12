# Task - task_003_be_aggregation_service

## Requirement Reference
- User Story: US_047
- Story Location: .propel/context/tasks/EP-007/us_047/us_047.md
- Acceptance Criteria:
    - **AC3**: Given aggregation produces a de-duplicated profile, When the PatientProfile entity is updated, Then it contains consolidated conditions, medications, allergies, vital trends, and encounters from all processed documents.
    - **AC4**: Given a new document is processed for an existing patient, When extraction completes, Then the aggregation service incrementally updates the patient profile without reprocessing previously aggregated data.
- Edge Case:
    - What happens when entity resolution cannot determine if two entries are duplicates? Items are merged with both source references and flagged for staff review.
    - How does the system handle data from documents uploaded years apart? Temporal context is preserved — older vitals do not overwrite newer ones.

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
| Background Jobs | Hangfire | 1.8.x |
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

Implement data aggregation service to consolidate extracted clinical data from multiple documents into a unified PatientProfile using entity resolution for de-duplication. This task creates incremental aggregation logic that processes only new documents, applies entity resolution to detect duplicates and conflicts, persists consolidated data to PatientProfile entities (conditions, medications, allergies, vital trends, encounters), flags data conflicts for staff review, calculates profile completeness scores, and maintains temporal integrity (preserving historical vitals without overwriting). The service integrates with Hangfire for background processing and Pusher for real-time status updates.

**Key Capabilities:**
- Incremental aggregation (process only new ExtractedClinicalData)
- Entity resolution-based de-duplication
- Conflict detection and flagging
- PatientProfile creation and updates
- Profile completeness calculation (0-100%)
- Temporal integrity (preserve older data points)
- Batch processing for efficiency
- Transaction support (rollback on failure)
- Real-time Pusher notifications
- Background job integration

## Dependent Tasks
- EP-007: US_047: task_001_db_patient_profile_schema (PatientProfile entities)
- EP-007: US_047: task_002_be_entity_resolution_service (EntityResolutionService)
- EP-006-II: US_045: task_003_be_verification_workflow_integration (DocumentProcessingService)
- EP-006-I: US_042: task_002_be_chunked_upload_api (PusherService)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/IDataAggregationService.cs` - Aggregation service interface
- **NEW**: `src/backend/PatientAccess.Business/Services/DataAggregationService.cs` - Aggregation implementation
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/PatientProfileAggregationJob.cs` - Hangfire job
- **NEW**: `src/backend/PatientAccess.Business/DTOs/AggregationResultDto.cs` - Aggregation result DTO
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs` - Trigger aggregation after extraction
- **MODIFY**: `src/backend/PatientAccess.Business/Services/PusherService.cs` - Add aggregation complete event
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register aggregation service
- **NEW**: `src/backend/PatientAccess.Tests/Services/DataAggregationServiceTests.cs` - Unit tests

## Implementation Plan

1. **Create IDataAggregationService Interface**
   - Methods:
     - `AggregatePatientDataAsync(int patientId, Guid? documentId = null)` - Aggregate data for patient (optionally scoped to document)
     - `GetOrCreatePatientProfileAsync(int patientId)` - Ensure PatientProfile exists
     - `CalculateProfileCompletenessAsync(int patientProfileId)` - Calculate completeness score
     - `IncrementalAggregateAsync(int patientId, Guid documentId)` - Aggregate only new document data

2. **Implement DataAggregationService**
   - Inject dependencies:
     - ApplicationDbContext
     - IEntityResolutionService
     - IPusherService
     - ILogger
   - Implement core aggregation logic

3. **Implement GetOrCreatePatientProfileAsync**
   - Check if PatientProfile exists for patient
   - If not exists: create new PatientProfile with initial values
   - Return existing or newly created PatientProfile
   - Set CreatedAt, LastAggregatedAt timestamps

4. **Implement IncrementalAggregateAsync Logic**
   - Load ExtractedClinicalData for specified document
   - Group data by DataType (Vital, Medication, Allergy, Diagnosis, LabResult)
   - For each data type:
     - **Conditions**:
       * Load existing ConsolidatedConditions for patient
       * Call EntityResolutionService.ResolveConditionDuplicatesAsync
       * If exact match: update existing record with new source references
       * If new condition: create ConsolidatedCondition
       * If conflict: create DataConflict record
     - **Medications**:
       * Load existing ConsolidatedMedications
       * Call EntityResolutionService.ResolveMedicationDuplicatesAsync
       * If exact match: merge with DuplicateCount++
       * If dosage conflict: create both entries with HasConflict=true, create DataConflict
       * If new medication: create ConsolidatedMedication
     - **Allergies**:
       * Load existing ConsolidatedAllergies
       * Call EntityResolutionService.ResolveAllergyDuplicatesAsync
       * Handle severity conflicts similar to medication dosage
     - **Vitals**:
       * Create VitalTrend records (no de-duplication)
       * Preserve temporal order (RecordedAt timestamp)
       * Never overwrite older vitals
     - **Encounters** (from LabResult extractions):
       * Load existing ConsolidatedEncounters
       * Call EntityResolutionService.ResolveEncounterDuplicatesAsync
       * Merge duplicates from same date/type/provider
   - Update PatientProfile.LastAggregatedAt timestamp
   - Update PatientProfile.TotalDocumentsProcessed count
   - Calculate and update ProfileCompleteness
   - Save changes within transaction

5. **Implement CalculateProfileCompletenessAsync**
   - Query PatientProfile and related entities
   - Calculate completeness score (0-100%) based on:
     - Demographics (from Patient table): 20%
     - At least 1 condition: 15%
     - At least 1 medication: 15%
     - At least 1 allergy: 10%
     - At least 5 vital trends: 20%
     - At least 1 encounter: 20%
   - Return completeness percentage
   - Update PatientProfile.ProfileCompleteness

6. **Implement Conflict Detection and Flagging**
   - When EntityResolutionService detects conflict:
     - Create DataConflict record with:
       * ConflictType (e.g., "MedicationDosageMismatch")
       * EntityType (e.g., "Medication")
       * EntityId (FK to ConsolidatedMedication)
       * Description (human-readable conflict details)
       * SourceDataIds (conflicting ExtractedClinicalData IDs)
       * ResolutionStatus = "Unresolved"
     - Set PatientProfile.HasUnresolvedConflicts = true
     - Log conflict details for audit

7. **Integrate with DocumentProcessingService**
   - Modify `ProcessDocumentAsync` to call aggregation after extraction completes
   - Add after extraction persistence:
     ```csharp
     // After extraction completes successfully
     await dataAggregationService.IncrementalAggregateAsync(document.PatientId, documentId);
     ```
   - Handle aggregation errors: log details, continue (don't fail document processing)

8. **Enhance PusherService with Aggregation Events**
   - Add method: `SendAggregationCompleteAsync(int patientId, AggregationResultDto result)`
   - Event channel: `private-patient-{patientId}`
   - Event name: `patient-profile-updated`
   - Event payload:
     ```json
     {
       "patientId": 123,
       "profileCompleteness": 87.5,
       "newConditionsCount": 2,
       "newMedicationsCount": 3,
       "newAllergiesCount": 1,
       "newVitalsCount": 5,
       "conflictsDetected": 1,
       "aggregatedAt": "2026-03-23T10:30:00Z"
     }
     ```

9. **Create PatientProfileAggregationJob**
   - Background job for manual re-aggregation (admin use case)
   - Method: `ReaggregatePatientProfileAsync(int patientId)`
   - Logic:
     - Delete existing consolidated data for patient
     - Re-aggregate all ExtractedClinicalData for patient
     - Recalculate profile completeness
     - Send Pusher notification
   - Use case: Fix data issues, apply algorithm updates

10. **Add Comprehensive Telemetry**
    - Log aggregation start/complete with patient ID
    - Track metrics in Application Insights:
      * `aggregation_duration_ms` (processing time)
      * `aggregation_conditions_merged` (count)
      * `aggregation_medications_merged` (count)
      * `aggregation_conflicts_detected` (count)
      * `profile_completeness_score` (percentage)
    - Log conflicts detected with entity types and source references
    - Track incremental vs. full aggregation performance

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── DocumentProcessingService.cs (from EP-006-I, to be enhanced)
│   │   ├── EntityResolutionService.cs (from task_002)
│   │   └── PusherService.cs (from EP-006-I, to be enhanced)
│   └── BackgroundJobs/
│       └── DocumentProcessingJob.cs (from EP-006-I)
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── PatientProfile.cs (from task_001)
│   │   ├── ConsolidatedCondition.cs (from task_001)
│   │   ├── ConsolidatedMedication.cs (from task_001)
│   │   └── ExtractedClinicalData.cs (from EP-006-II)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/IDataAggregationService.cs | Aggregation service interface |
| CREATE | src/backend/PatientAccess.Business/Services/DataAggregationService.cs | Aggregation implementation |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/PatientProfileAggregationJob.cs | Manual re-aggregation job |
| CREATE | src/backend/PatientAccess.Business/DTOs/AggregationResultDto.cs | Aggregation result DTO |
| CREATE | src/backend/PatientAccess.Tests/Services/DataAggregationServiceTests.cs | Unit tests |
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs | Trigger aggregation after extraction |
| MODIFY | src/backend/PatientAccess.Business/Services/PusherService.cs | Add aggregation complete event |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register aggregation service |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Entity Framework Core Documentation
- **Transactions**: https://learn.microsoft.com/en-us/ef/core/saving/transactions
- **Concurrency**: https://learn.microsoft.com/en-us/ef/core/saving/concurrency
- **Change Tracking**: https://learn.microsoft.com/en-us/ef/core/change-tracking/

### Hangfire Documentation
- **Background Jobs**: https://docs.hangfire.io/en/latest/background-methods/index.html
- **Dependency Injection**: https://docs.hangfire.io/en/latest/background-methods/using-ioc-containers.html

### Design Requirements
- **FR-030**: System MUST aggregate extracted data into a de-duplicated, consolidated patient profile (spec.md)
- **FR-031**: System MUST explicitly highlight critical data conflicts requiring staff verification (spec.md)
- **FR-032**: System MUST generate 360-Degree Patient View displaying unified patient health summary (spec.md)
- **AIR-005**: System MUST aggregate extracted data using entity resolution pattern (design.md)
- **AG-003**: Reduce clinical preparation time from 20+ minutes to under 2 minutes through automated data aggregation (design.md)
- **DR-001**: Database schema MUST support referential integrity with foreign keys (design.md)

### Existing Codebase Patterns
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs`
- **Background Job Pattern**: `src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs`
- **Pusher Event Pattern**: `src/backend/PatientAccess.Business/Services/PusherService.cs`

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build

# Run unit tests
dotnet test --filter "FullyQualifiedName~DataAggregationServiceTests"

# Run all tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Unit tests pass (incremental aggregation, conflict detection, completeness calculation)
- [ ] Integration tests pass (end-to-end aggregation workflow)
- [ ] PatientProfile created automatically for new patients
- [ ] Incremental aggregation processes only new document data
- [ ] Entity resolution correctly de-duplicates medications, conditions, allergies
- [ ] Conflicts detected and flagged (medication dosage mismatches)
- [ ] VitalTrends preserve temporal order (no overwriting older data)
- [ ] Profile completeness calculated correctly (0-100%)
- [ ] Transaction rollback works on aggregation failure
- [ ] Pusher notification sent with aggregation result summary
- [ ] DocumentProcessingService triggers aggregation after extraction
- [ ] Application Insights logs aggregation metrics
- [ ] Performance acceptable (aggregate 100+ data points <5 seconds)
- [ ] Re-aggregation job works for admin data fixes

## Implementation Checklist
- [ ] Create IDataAggregationService interface with IncrementalAggregateAsync method
- [ ] Implement GetOrCreatePatientProfileAsync to ensure profile exists
- [ ] Implement IncrementalAggregateAsync with entity resolution integration
- [ ] Add conflict detection logic for medications, conditions, allergies
- [ ] Implement CalculateProfileCompletenessAsync with weighted scoring
- [ ] Create VitalTrend records preserving temporal order (no de-duplication)
- [ ] Wrap aggregation in database transaction with rollback on failure
- [ ] Enhance DocumentProcessingService to trigger aggregation after extraction
- [ ] Enhance PusherService with SendAggregationCompleteAsync event
- [ ] Create PatientProfileAggregationJob for manual re-aggregation
- [ ] Register DataAggregationService in Program.cs dependency injection
- [ ] Add telemetry tracking for aggregation metrics to Application Insights
- [ ] Write unit tests for incremental aggregation, conflict detection, completeness
- [ ] Write integration tests for end-to-end aggregation workflow
