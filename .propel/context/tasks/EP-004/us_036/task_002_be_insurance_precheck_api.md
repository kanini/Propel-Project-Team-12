# Task - task_002_be_insurance_precheck_api

## Requirement Reference

- User Story: us_036
- Story Location: .propel/context/tasks/EP-004/us_036/us_036.md
- Acceptance Criteria:
  - AC-1: System validates insurance ID against InsuranceRecord reference data using regex pattern matching
  - AC-2: Valid insurance returns coverage type information (e.g., "PPO coverage")
  - AC-3: Invalid insurance ID (pattern mismatch) returns warning with descriptive message
  - AC-4: Provider not found in reference data returns "Provider not found in our records"
- Edge Cases:
  - Empty insurance reference data: Service returns "Validation unavailable" status and logs warning; intake proceeds
  - Insurance IDs with leading zeros or special formats: Regex patterns in InsuranceRecord accommodate varied formats; service trims whitespace before matching

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

Implement the backend insurance pre-check validation service and API endpoint (FR-021, TR-021). This task creates `IInsuranceValidationService` and `InsuranceValidationService` that query the `InsuranceRecords` table for active providers, match provider names using case-insensitive comparison, and validate insurance IDs against the stored `AcceptedIdPattern` regex. The service returns a structured result with validation status, coverage type, and descriptive message. A `POST /api/intake/insurance/validate` endpoint exposes this service. The endpoint also updates the `IntakeRecord.InsuranceValidationStatus` and `ValidatedInsuranceRecordId` when called within an active intake session. Regex evaluation uses a configurable timeout to prevent ReDoS attacks.

## Dependent Tasks

- EP-004/us_033/task_002_be_intake_api — Provides IntakeController base, IntakeService, and intake session infrastructure
- EP-004/us_034/task_002_be_manual_intake_api — Provides manual submission flow that triggers insurance validation during submit

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/DTOs/InsuranceValidateRequestDto.cs` — Request DTO with provider name and insurance ID
- **NEW** `src/backend/PatientAccess.Business/DTOs/InsuranceValidateResponseDto.cs` — Response DTO with status, coverage type, provider name, and message
- **NEW** `src/backend/PatientAccess.Business/Interfaces/IInsuranceValidationService.cs` — Interface for insurance validation service
- **NEW** `src/backend/PatientAccess.Business/Services/InsuranceValidationService.cs` — Implementation querying InsuranceRecords and applying regex pattern matching
- **MODIFY** `src/backend/PatientAccess.Web/Controllers/IntakeController.cs` — Add `POST /api/intake/insurance/validate` endpoint
- **MODIFY** `src/backend/PatientAccess.Web/Extensions/ServiceCollectionExtensions.cs` — Register `IInsuranceValidationService` in DI container

## Implementation Plan

1. **Define InsuranceValidateRequestDto**:
   ```csharp
   public class InsuranceValidateRequestDto
   {
       [Required, StringLength(200)]
       public string ProviderName { get; set; } = string.Empty;

       [Required, StringLength(100)]
       public string InsuranceId { get; set; } = string.Empty;
   }
   ```
   Both fields required. `StringLength` limits match the InsuranceRecord column constraints.

2. **Define InsuranceValidateResponseDto**:
   ```csharp
   public class InsuranceValidateResponseDto
   {
       public string Status { get; set; } = string.Empty; // "Valid", "Invalid", "NotFound", "Unavailable"
       public string? CoverageType { get; set; } // "PPO", "HMO", etc.
       public string? ProviderName { get; set; }
       public string Message { get; set; } = string.Empty;
   }
   ```
   Status uses string values (not enum) for frontend simplicity. Message contains human-readable text for display.

3. **Define IInsuranceValidationService interface**:
   ```csharp
   public interface IInsuranceValidationService
   {
       Task<InsuranceValidateResponseDto> ValidateAsync(string providerName, string insuranceId);
   }
   ```
   Single method returning the full response DTO. Caller (controller or IntakeService) can use the result to update IntakeRecord fields.

4. **Implement InsuranceValidationService**:
   - Inject `PatientAccessDbContext` via constructor
   - Query `InsuranceRecords` table for active records matching provider name (case-insensitive `Contains` or `EqualityComparer`):
     ```csharp
     var provider = await _context.InsuranceRecords
         .Where(r => r.IsActive && r.ProviderName.ToLower() == providerName.Trim().ToLower())
         .FirstOrDefaultAsync();
     ```
   - If no active records exist in the table at all, return `{ Status = "Unavailable", Message = "Validation unavailable" }` and log warning (edge case)
   - If no matching provider found, return `{ Status = "NotFound", Message = "Provider not found in our records" }` (AC-4)
   - If provider found but `AcceptedIdPattern` is null, return `{ Status = "Valid", CoverageType = provider.CoverageType.ToString(), ProviderName = provider.ProviderName, Message = $"Insurance verified — {provider.CoverageType} coverage" }` (no pattern to validate against)
   - If provider found and `AcceptedIdPattern` is set, evaluate regex with timeout protection:
     ```csharp
     var regex = new Regex(provider.AcceptedIdPattern, RegexOptions.None, TimeSpan.FromSeconds(1));
     var match = regex.IsMatch(insuranceId.Trim());
     ```
   - If regex matches: return `{ Status = "Valid", CoverageType = ..., ProviderName = ..., Message = "Insurance verified — {CoverageType} coverage" }` (AC-2)
   - If regex does not match: return `{ Status = "Invalid", ProviderName = provider.ProviderName, Message = "Insurance could not be verified. Please check your insurance ID." }` (AC-3)
   - If `RegexMatchTimeoutException` is thrown: treat as Invalid and log warning about potentially malicious pattern

5. **Add POST endpoint to IntakeController**:
   ```csharp
   [HttpPost("insurance/validate")]
   [Authorize(Roles = "Patient")]
   public async Task<ActionResult<InsuranceValidateResponseDto>> ValidateInsurance(
       [FromBody] InsuranceValidateRequestDto request)
   ```
   - Call `IInsuranceValidationService.ValidateAsync(request.ProviderName, request.InsuranceId)`
   - Return 200 with the response DTO (always 200 — the status field indicates the validation outcome)
   - Optionally accept an `intakeId` query parameter; if provided, update the `IntakeRecord.InsuranceValidationStatus` and `ValidatedInsuranceRecordId` fields

6. **Register service in DI**: In `ServiceCollectionExtensions.cs`, add:
   ```csharp
   services.AddScoped<IInsuranceValidationService, InsuranceValidationService>();
   ```

7. **Integrate with manual intake submission** (cross-reference US_034 task_002): The existing `SubmitManualIntakeAsync` in `IntakeService` calls `IInsuranceValidationService.ValidateAsync` when insurance fields are provided, and sets `InsuranceValidationStatus` (Valid/Invalid) and `ValidatedInsuranceRecordId` on the `IntakeRecord` before saving. This integration point is documented here but implemented in US_034 task_002.

8. **Add audit logging**: Log insurance validation attempts via `AuditLogService` with action type `InsuranceValidationAttempt` including provider name, result status, and whether the intake was linked (NFR-007). No insurance ID logged to avoid PII concerns.

## Current Project State

```
src/backend/
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── IntakeController.cs    # US_033 + US_034 + US_035 endpoints
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs  # DI registrations
├── PatientAccess.Business/
│   ├── DTOs/
│   │   ├── StartIntakeRequestDto.cs        # US_033
│   │   ├── IntakeMessageRequestDto.cs      # US_033
│   │   ├── IntakeMessageResponseDto.cs     # US_033
│   │   ├── IntakeSummaryDto.cs             # US_033
│   │   ├── UpdateIntakeRequestDto.cs       # US_033
│   │   ├── ManualIntakeSubmitDto.cs        # US_034
│   │   ├── IntakeDraftDto.cs              # US_034
│   │   ├── IntakeValidationErrorDto.cs     # US_034
│   │   ├── SwitchIntakeModeRequestDto.cs   # US_035
│   │   ├── SwitchIntakeModeResponseDto.cs  # US_035
│   │   └── CompletedCategoriesDto.cs       # US_035
│   ├── Interfaces/
│   │   ├── IIntakeService.cs               # US_033 + US_034 + US_035
│   │   └── IAiIntakeService.cs             # US_033
│   └── Services/
│       ├── IntakeService.cs                # US_033 + US_034 + US_035
│       └── GeminiIntakeService.cs          # US_033
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── IntakeRecord.cs                 # DR-012 (InsuranceValidationStatus, ValidatedInsuranceRecordId FK)
│   │   ├── InsuranceRecord.cs              # DR-015 (ProviderName, AcceptedIdPattern, CoverageType, IsActive)
│   │   ├── InsuranceValidationStatus.cs    # Enum: NotValidated=1, Valid=2, Invalid=3
│   │   └── CoverageType.cs                # Enum: HMO=1, PPO=2, EPO=3, POS=4, Medicare=5, Medicaid=6, Other=7
│   ├── Configurations/
│   │   ├── InsuranceRecordConfiguration.cs # DR-015 table config
│   │   └── IntakeRecordConfiguration.cs    # FK_IntakeRecords_InsuranceRecords
│   └── DatabaseSeeder.cs                  # 10 seeded insurance providers with regex patterns
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/DTOs/InsuranceValidateRequestDto.cs | Request DTO with provider name and insurance ID fields |
| CREATE | src/backend/PatientAccess.Business/DTOs/InsuranceValidateResponseDto.cs | Response DTO with status, coverage type, provider name, message |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IInsuranceValidationService.cs | Interface with ValidateAsync method signature |
| CREATE | src/backend/PatientAccess.Business/Services/InsuranceValidationService.cs | Implementation querying InsuranceRecords, matching provider, evaluating regex |
| MODIFY | src/backend/PatientAccess.Web/Controllers/IntakeController.cs | Add POST /api/intake/insurance/validate endpoint |
| MODIFY | src/backend/PatientAccess.Web/Extensions/ServiceCollectionExtensions.cs | Register IInsuranceValidationService in DI container |

## External References

- [ASP.NET Core 8 Web API Controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0)
- [.NET Regex with Timeout (ReDoS Protection)](https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-regex#use-time-out-values)
- [EF Core Query Filtering](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- UC-008 Sequence Diagram (insurance validation step): .propel/context/docs/models.md#UC-008

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj
```

## Implementation Validation Strategy

- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] Valid provider + valid ID returns status "Valid" with correct CoverageType string
- [ ] Valid provider + invalid ID returns status "Invalid" with warning message
- [ ] Unknown provider returns status "NotFound" with descriptive message
- [ ] Empty InsuranceRecords table returns status "Unavailable" (edge case)
- [ ] Regex timeout (ReDoS) returns "Invalid" and logs warning instead of throwing
- [ ] Insurance IDs with leading zeros and special characters pass when regex allows them
- [ ] Case-insensitive provider name matching works ("aetna" matches "Aetna")
- [ ] Endpoint returns 200 for all validation outcomes (status field differentiates)
- [ ] No insurance ID logged in audit events (PII protection)

## Implementation Checklist

- [ ] Create `InsuranceValidateRequestDto` with required provider name and insurance ID
- [ ] Create `InsuranceValidateResponseDto` with status, coverage type, provider name, and message
- [ ] Define `IInsuranceValidationService` interface with `ValidateAsync` method
- [ ] Implement `InsuranceValidationService` with provider lookup, regex matching, and timeout protection
- [ ] Add `POST /api/intake/insurance/validate` endpoint to IntakeController
- [ ] Register `IInsuranceValidationService` in DI container
- [ ] Add audit logging for validation attempts without PII
- [ ] Handle empty reference data edge case with "Unavailable" status and warning log
