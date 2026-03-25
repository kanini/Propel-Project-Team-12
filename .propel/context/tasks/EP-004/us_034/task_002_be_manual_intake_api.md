# Task - task_002_be_manual_intake_api

## Requirement Reference

- User Story: us_034
- Story Location: .propel/context/tasks/EP-004/us_034/us_034.md
- Acceptance Criteria:
  - AC-1: Structured form sections stored — backend accepts and persists medical history, medications, allergies, visit concerns, and insurance data
  - AC-2: IntakeRecord saved with mode "ManualForm" linked to appointment — `POST /api/intake/{id}/submit` creates/updates record
  - AC-3: Inline validation errors — backend returns 400 with field-level validation errors for required/invalid fields
  - AC-4: Contextual help — backend provides static tooltip content (not a separate AC for BE, but API supports metadata)
- Edge Cases:
  - Form submission fails mid-save: API is idempotent; re-submission with same session ID updates existing record
  - Extremely long text: Backend enforces character limits (2000 chars for free-text fields) and returns validation error

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

## Applicable Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET 8 ASP.NET Core Web API | .NET 8.0 |
| Database | PostgreSQL with pgvector | PostgreSQL 16, pgvector 0.5+ |
| Caching | Upstash Redis | Redis 7.x compatible |
| AI/ML | N/A | N/A |

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

## Mobile References (Mobile Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Extend the Intake API (created in US_033 task_002) with the manual form submission endpoint. This task adds `POST /api/intake/{id}/submit` to accept the full structured form data, validate all required fields with field-level error responses, serialize data into the `IntakeRecord` JSONB columns, and mark the record with mode `ManualForm`. It also adds a draft save endpoint (`PUT /api/intake/{id}/draft`) for partial saves during multi-step form progress. The insurance pre-check validation is performed during submission by calling the existing insurance validation service.

## Dependent Tasks

- EP-004/us_033/task_002_be_intake_api — Provides `IntakeController`, `IntakeService`, `IIntakeService`, and all DTOs. The start session and complete endpoints already exist. This task extends with manual-specific submit and draft endpoints.

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/DTOs/ManualIntakeSubmitDto.cs` — DTO for structured manual form submission with all section fields
- **NEW** `src/backend/PatientAccess.Business/DTOs/IntakeDraftDto.cs` — DTO for partial draft saves
- **NEW** `src/backend/PatientAccess.Business/DTOs/IntakeValidationErrorDto.cs` — DTO mapping field names to validation error messages
- **MODIFY** `src/backend/PatientAccess.Web/Controllers/IntakeController.cs` — Add `POST /api/intake/{id}/submit` and `PUT /api/intake/{id}/draft` endpoints
- **MODIFY** `src/backend/PatientAccess.Business/Services/IntakeService.cs` — Add `SubmitManualIntakeAsync` and `SaveDraftAsync` methods with field-level validation
- **MODIFY** `src/backend/PatientAccess.Business/Interfaces/IIntakeService.cs` — Add method signatures for manual submit and draft save

## Implementation Plan

1. **Define ManualIntakeSubmitDto**: Create a strongly-typed DTO representing the complete manual form:
   ```csharp
   public class ManualIntakeSubmitDto
   {
       // Medical History section
       [StringLength(2000)]
       public string? MedicalConditions { get; set; }

       [Required, StringLength(2000)]
       public string CurrentMedications { get; set; }

       [Required, StringLength(1000)]
       public string DrugAllergies { get; set; }

       [StringLength(2000)]
       public string? SurgicalHistory { get; set; }

       public List<string>? FamilyHistory { get; set; }  // checkbox values

       public string? SmokingStatus { get; set; }  // "yes"|"no"|"former"
       public string? AlcoholConsumption { get; set; }  // "never"|"occasional"|"regular"

       // Visit Concerns section
       [Required, StringLength(2000)]
       public string ChiefComplaint { get; set; }

       [StringLength(2000)]
       public string? AdditionalConcerns { get; set; }

       // Insurance section
       [StringLength(200)]
       public string? InsuranceProviderName { get; set; }

       [StringLength(100)]
       public string? InsuranceId { get; set; }
   }
   ```
   Use data annotations for server-side validation. `[Required]` on mandatory fields; `[StringLength]` to enforce character limits.

2. **Define IntakeDraftDto**: A permissive DTO (all fields optional) for saving partial progress at any form step. Mirrors `ManualIntakeSubmitDto` but without `[Required]` attributes. Includes a `CurrentStep` integer (0-3) to track which section the patient was on.

3. **Define IntakeValidationErrorDto**: A response DTO containing a dictionary of field-level errors:
   ```csharp
   public class IntakeValidationErrorDto
   {
       public Dictionary<string, string[]> FieldErrors { get; set; } = new();
   }
   ```
   Returned with HTTP 400 when validation fails, enabling the frontend to display inline errors per field.

4. **Extend IIntakeService**: Add two method signatures:
   - `Task<Result> SubmitManualIntakeAsync(Guid intakeId, Guid patientId, ManualIntakeSubmitDto dto)`
   - `Task SaveDraftAsync(Guid intakeId, Guid patientId, IntakeDraftDto dto)`

5. **Implement SubmitManualIntakeAsync in IntakeService**:
   - Load existing `IntakeRecord` by ID; verify it belongs to the authenticated patient (return 403 otherwise)
   - Verify `IntakeMode` is `ManualForm` (return 400 if mismatched)
   - Perform field-level validation beyond data annotations: check `CurrentMedications` is non-empty, `DrugAllergies` is non-empty, `ChiefComplaint` is non-empty
   - If insurance fields are provided, call the existing insurance pre-check validation service (FR-021) to validate against `InsuranceRecord` reference data
   - Serialize each section into the corresponding JSONB column on `IntakeRecord`:
     - `CurrentMedications` → `CurrentMedications` column
     - `DrugAllergies` → `KnownAllergies` column
     - `MedicalConditions` + `SurgicalHistory` + `FamilyHistory` + lifestyle → `MedicalHistory` column (as structured JSON)
     - `ChiefComplaint` + `AdditionalConcerns` → `ChiefComplaint` + `SymptomHistory` columns
   - Set `InsuranceValidationStatus` based on pre-check result
   - Set `IsCompleted = true` and `CompletedAt = DateTime.UtcNow`
   - Save changes and log audit event via `AuditLogService`

6. **Implement SaveDraftAsync in IntakeService**:
   - Load existing `IntakeRecord`; verify ownership
   - Serialize provided (non-null) fields into JSONB columns without marking complete
   - Update `UpdatedAt` timestamp
   - This endpoint is idempotent — multiple calls update the same record

7. **Add endpoints to IntakeController**:
   - `POST /api/intake/{id}/submit` — Accepts `ManualIntakeSubmitDto`. Calls `SubmitManualIntakeAsync`. Returns 200 on success, 400 with `IntakeValidationErrorDto` on validation failure, 403 on unauthorized access.
   - `PUT /api/intake/{id}/draft` — Accepts `IntakeDraftDto`. Calls `SaveDraftAsync`. Returns 204 on success.
   - Both endpoints secured with `[Authorize(Roles = "Patient")]`.

8. **Add audit logging**: Log manual intake submission events with action type `ManualIntakeSubmitted` and draft save events with `IntakeDraftSaved` via existing `AuditLogService` (NFR-007).

## Current Project State

```
src/backend/
├── PatientAccess.Web/
│   └── Controllers/
│       └── IntakeController.cs    # US_033 task_002 (start, message, summary, update, complete)
├── PatientAccess.Business/
│   ├── DTOs/
│   │   ├── StartIntakeRequestDto.cs        # US_033
│   │   ├── IntakeMessageRequestDto.cs      # US_033
│   │   ├── IntakeMessageResponseDto.cs     # US_033
│   │   ├── IntakeSummaryDto.cs             # US_033
│   │   └── UpdateIntakeRequestDto.cs       # US_033
│   ├── Interfaces/
│   │   └── IIntakeService.cs               # US_033 (start, process, summary, update, complete)
│   └── Services/
│       └── IntakeService.cs                # US_033
├── PatientAccess.Data/
│   └── Models/
│       └── IntakeRecord.cs                 # Existing entity with JSONB columns
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/DTOs/ManualIntakeSubmitDto.cs | DTO for full manual form submission with data annotations |
| CREATE | src/backend/PatientAccess.Business/DTOs/IntakeDraftDto.cs | DTO for partial draft saves (all fields optional) |
| CREATE | src/backend/PatientAccess.Business/DTOs/IntakeValidationErrorDto.cs | Field-level validation error response DTO |
| MODIFY | src/backend/PatientAccess.Business/Interfaces/IIntakeService.cs | Add SubmitManualIntakeAsync and SaveDraftAsync signatures |
| MODIFY | src/backend/PatientAccess.Business/Services/IntakeService.cs | Implement manual submit with validation and draft save logic |
| MODIFY | src/backend/PatientAccess.Web/Controllers/IntakeController.cs | Add POST submit and PUT draft endpoints |

## External References

- [ASP.NET Core Model Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-8.0)
- [ASP.NET Core Data Annotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-8.0)
- [System.Text.Json Serialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)
- UC-008 Sequence Diagram: .propel/context/docs/models.md#UC-008

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj
```

## Implementation Validation Strategy

- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] Manual submit endpoint returns 200 with valid form data and creates IntakeRecord (mode=ManualForm)
- [ ] Manual submit endpoint returns 400 with field-level validation errors on missing required fields
- [ ] Character limit enforcement returns validation error for fields exceeding 2000 chars
- [ ] Draft save endpoint returns 204 and persists partial data without marking complete
- [ ] Insurance pre-check triggered on submit when insurance fields are provided
- [ ] Unauthorized access to another patient's intake returns 403
- [ ] Audit logs created for submit and draft save events
- [ ] Idempotent re-submission updates existing record without creating duplicates

## Implementation Checklist

- [ ] Create `ManualIntakeSubmitDto` with data annotations for required fields and character limits
- [ ] Create `IntakeDraftDto` with all-optional fields and `CurrentStep` tracking
- [ ] Create `IntakeValidationErrorDto` for field-level error responses
- [ ] Add `SubmitManualIntakeAsync` and `SaveDraftAsync` to `IIntakeService` interface
- [ ] Implement `SubmitManualIntakeAsync` with field validation, JSONB serialization, and insurance pre-check
- [ ] Implement `SaveDraftAsync` with partial update logic and idempotent handling
- [ ] Add `POST /api/intake/{id}/submit` and `PUT /api/intake/{id}/draft` endpoints to `IntakeController`
- [ ] Add audit logging for manual intake submit and draft save events
