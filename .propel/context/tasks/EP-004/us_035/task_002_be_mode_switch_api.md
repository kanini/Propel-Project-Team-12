# Task - task_002_be_mode_switch_api

## Requirement Reference

- User Story: us_035
- Story Location: .propel/context/tasks/EP-004/us_035/us_035.md
- Acceptance Criteria:
  - AC-1: AI→Manual switch — backend updates IntakeMode, preserves all JSONB data, returns current data for form pre-population
  - AC-2: Manual→AI switch — backend updates IntakeMode, returns completed categories so AI can skip them
  - AC-3: Post-submission editing — backend accepts partial field updates via PATCH without requiring staff role
  - AC-4: Data mappings preserved — backend validates no data loss during mode change by comparing field counts before/after
- Edge Cases:
  - Unmapped AI data: Backend stores overflow in a dedicated `AdditionalNotes` field within MedicalHistory JSONB
  - Partial intake switch: Backend persists current partial data before changing mode

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

Extend the Intake API (from US_033/US_034) with mode switching and post-submission editing endpoints. This task adds `PATCH /api/intake/{id}/mode` to switch between AIConversational and ManualForm modes while preserving all JSONB data, `GET /api/intake/{id}/completed-categories` to inform the AI which sections are already filled, and extends the existing `PATCH /api/intake/{id}` to support post-submission inline field edits by Patient role without staff assistance (FR-020). The mode switch operation is atomic — it persists any pending data, updates the `IntakeMode` enum, and returns the full current intake data in a single transaction.

## Dependent Tasks

- EP-004/us_033/task_002_be_intake_api — Provides IntakeController, IntakeService, IIntakeService, DTOs, and the base PATCH endpoint
- EP-004/us_034/task_002_be_manual_intake_api — Provides ManualIntakeSubmitDto, draft save, and field validation logic

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/DTOs/SwitchIntakeModeRequestDto.cs` — Request DTO for mode switch (target mode enum)
- **NEW** `src/backend/PatientAccess.Business/DTOs/SwitchIntakeModeResponseDto.cs` — Response DTO with full intake data and completed categories
- **NEW** `src/backend/PatientAccess.Business/DTOs/CompletedCategoriesDto.cs` — Response DTO listing which intake categories have data
- **MODIFY** `src/backend/PatientAccess.Web/Controllers/IntakeController.cs` — Add PATCH mode, GET completed-categories endpoints; extend PATCH for post-submission edits
- **MODIFY** `src/backend/PatientAccess.Business/Services/IntakeService.cs` — Add SwitchModeAsync, GetCompletedCategoriesAsync, extend UpdateIntakeAsync for post-submission
- **MODIFY** `src/backend/PatientAccess.Business/Interfaces/IIntakeService.cs` — Add method signatures

## Implementation Plan

1. **Define SwitchIntakeModeRequestDto**:
   ```csharp
   public class SwitchIntakeModeRequestDto
   {
       [Required]
       public IntakeMode TargetMode { get; set; }
   }
   ```
   Simple DTO with the target mode enum value (`AIConversational` or `ManualForm`).

2. **Define SwitchIntakeModeResponseDto**: Contains the full current intake data returned after mode switch so the frontend can pre-populate the target interface:
   ```csharp
   public class SwitchIntakeModeResponseDto
   {
       public Guid IntakeRecordId { get; set; }
       public IntakeMode CurrentMode { get; set; }
       public string? ChiefComplaint { get; set; }
       public string? SymptomHistory { get; set; }
       public string? CurrentMedications { get; set; }
       public string? KnownAllergies { get; set; }
       public string? MedicalHistory { get; set; }
       public List<string> CompletedCategories { get; set; } = new();
       public int Progress { get; set; }
   }
   ```

3. **Define CompletedCategoriesDto**: Lightweight response listing filled categories for the AI to skip:
   ```csharp
   public class CompletedCategoriesDto
   {
       public List<string> CompletedCategories { get; set; } = new();
       public int Progress { get; set; }
   }
   ```

4. **Implement SwitchModeAsync in IntakeService**:
   - Load `IntakeRecord` by ID; verify it belongs to the authenticated patient
   - Verify the intake is not yet completed (cannot switch mode after final submission). Return 400 if `IsCompleted == true`.
   - Verify the target mode is different from the current mode (no-op if same)
   - Update `IntakeMode` to the target value
   - Set `UpdatedAt = DateTime.UtcNow`
   - Save changes within a single transaction
   - Compute `CompletedCategories` by checking which JSONB columns are non-null and non-empty
   - Calculate `Progress` as percentage of filled categories (5 total: medications, allergies, symptoms, history, concerns)
   - Return `SwitchIntakeModeResponseDto` with all current data and completed categories
   - Log audit event with action type `IntakeModeSwitched` including previous and new mode

5. **Implement GetCompletedCategoriesAsync in IntakeService**:
   - Load `IntakeRecord`; verify patient ownership
   - Check each JSONB column (`CurrentMedications`, `KnownAllergies`, `SymptomHistory`, `MedicalHistory`, `ChiefComplaint`)
   - Return `CompletedCategoriesDto` with list of non-empty category names and progress percentage

6. **Extend UpdateIntakeAsync for post-submission editing** (FR-020):
   - The existing `PATCH /api/intake/{id}` (from US_033) accepts `UpdateIntakeRequestDto` for partial updates
   - Extend to allow updates even when `IsCompleted == true` (previously blocked)
   - Add a guard: only allow edits before the appointment's scheduled datetime. Fetch the linked `Appointment` and compare `ScheduledAt` with `DateTime.UtcNow`. Return 400 with message "Cannot edit intake after appointment time" if past.
   - On successful edit, set `UpdatedAt = DateTime.UtcNow` but keep `IsCompleted = true`
   - Log audit event with action type `IntakePostSubmitEdit` including changed fields

7. **Add endpoints to IntakeController**:
   - `PATCH /api/intake/{id}/mode` — Accepts `SwitchIntakeModeRequestDto`, returns `SwitchIntakeModeResponseDto`. Secured with `[Authorize(Roles = "Patient")]`.
   - `GET /api/intake/{id}/completed-categories` — Returns `CompletedCategoriesDto`. Secured with `[Authorize(Roles = "Patient")]`.
   - Extend existing `PATCH /api/intake/{id}` — Remove the `IsCompleted` guard to allow post-submission edits (with appointment time check).

8. **Add audit logging**: Log mode switch events and post-submission edits via existing `AuditLogService` with action types `IntakeModeSwitched` and `IntakePostSubmitEdit` (NFR-007).

## Current Project State

```
src/backend/
├── PatientAccess.Web/
│   └── Controllers/
│       └── IntakeController.cs    # US_033 (start, message, summary, update, complete)
│                                  # US_034 (submit, draft)
├── PatientAccess.Business/
│   ├── DTOs/
│   │   ├── StartIntakeRequestDto.cs        # US_033
│   │   ├── IntakeMessageRequestDto.cs      # US_033
│   │   ├── IntakeMessageResponseDto.cs     # US_033
│   │   ├── IntakeSummaryDto.cs             # US_033
│   │   ├── UpdateIntakeRequestDto.cs       # US_033
│   │   ├── ManualIntakeSubmitDto.cs        # US_034
│   │   ├── IntakeDraftDto.cs              # US_034
│   │   └── IntakeValidationErrorDto.cs     # US_034
│   ├── Interfaces/
│   │   ├── IIntakeService.cs               # US_033 + US_034
│   │   └── IAiIntakeService.cs             # US_033
│   └── Services/
│       ├── IntakeService.cs                # US_033 + US_034
│       └── GeminiIntakeService.cs          # US_033
├── PatientAccess.Data/
│   └── Models/
│       └── IntakeRecord.cs                 # Existing (IntakeMode enum, JSONB columns)
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/DTOs/SwitchIntakeModeRequestDto.cs | Request DTO with target IntakeMode enum |
| CREATE | src/backend/PatientAccess.Business/DTOs/SwitchIntakeModeResponseDto.cs | Response DTO with full intake data and completed categories |
| CREATE | src/backend/PatientAccess.Business/DTOs/CompletedCategoriesDto.cs | Response DTO listing completed intake categories |
| MODIFY | src/backend/PatientAccess.Business/Interfaces/IIntakeService.cs | Add SwitchModeAsync, GetCompletedCategoriesAsync signatures |
| MODIFY | src/backend/PatientAccess.Business/Services/IntakeService.cs | Implement mode switch, completed categories, extend post-submission editing |
| MODIFY | src/backend/PatientAccess.Web/Controllers/IntakeController.cs | Add PATCH mode, GET completed-categories; extend PATCH for post-submit edits |

## External References

- [ASP.NET Core 8 Web API Controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0)
- [Entity Framework Core Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- [ASP.NET Core Model Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-8.0)
- UC-007 Sequence Diagram (mode switch extension): .propel/context/docs/models.md#UC-007
- UC-008 Sequence Diagram (mode switch extension): .propel/context/docs/models.md#UC-008

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj
```

## Implementation Validation Strategy

- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] Mode switch from AIConversational to ManualForm preserves all JSONB data and returns full record
- [ ] Mode switch from ManualForm to AIConversational returns completed categories list
- [ ] Mode switch on completed intake returns 400 error
- [ ] Post-submission PATCH updates individual fields while keeping IsCompleted = true
- [ ] Post-submission PATCH after appointment time returns 400 error
- [ ] Completed categories endpoint returns accurate list based on non-empty JSONB columns
- [ ] Unauthorized access to another patient's intake returns 403
- [ ] Audit logs created for mode switch and post-submission edit events

## Implementation Checklist

- [ ] Create `SwitchIntakeModeRequestDto` with required target mode field
- [ ] Create `SwitchIntakeModeResponseDto` with full intake data and completed categories
- [ ] Create `CompletedCategoriesDto` with category list and progress percentage
- [ ] Implement `SwitchModeAsync` with atomic mode update, data preservation, and category computation
- [ ] Implement `GetCompletedCategoriesAsync` by inspecting JSONB column non-emptiness
- [ ] Extend `UpdateIntakeAsync` to allow post-submission edits with appointment time guard (FR-020)
- [ ] Add `PATCH /api/intake/{id}/mode` and `GET /api/intake/{id}/completed-categories` endpoints
- [ ] Add audit logging for mode switch and post-submission edit events
