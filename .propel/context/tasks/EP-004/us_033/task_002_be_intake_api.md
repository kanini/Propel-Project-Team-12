# Task - task_002_be_intake_api

## Requirement Reference

- User Story: us_033
- Story Location: .propel/context/tasks/EP-004/us_033/us_033.md
- Acceptance Criteria:
  - AC-1: AI conversational interface loads with welcome message — backend provides `POST /api/intake/start` returning session ID and welcome message
  - AC-2: AI extracts structured data from natural language — backend orchestrates message to AI service and returns structured data with confidence
  - AC-3: Summary displayed for review — backend provides `GET /api/intake/{id}/summary` returning aggregated extracted data
  - AC-4: Confidence below 70% triggers fallback suggestion — backend returns confidence score and fallback flag
  - AC-5: Token budget enforcement (AIR-O01) — backend validates request does not exceed 4000 tokens
- Edge Cases:
  - AI cannot understand response: Backend returns clarification prompt; tracks consecutive failures
  - Patient provides minimal responses: Backend returns follow-up prompts from AI service

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
| AI/ML | N/A (delegated to AI task layer) | N/A |

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

> Backend task consumes AI service via interface abstraction (`IAiIntakeService`). Actual LLM integration is in task_003.

## Mobile References (Mobile Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Implement the backend API endpoints for the AI Conversational Intake flow (UC-007). This task creates the `IntakeController` with four endpoints: start session, send message, update intake data, and complete intake. The service layer (`IntakeService`) manages intake session lifecycle, persists data to the `IntakeRecords` table, and delegates AI NLU processing to an `IAiIntakeService` interface (implemented in task_003). Includes conversation state tracking, consecutive failure counting, and confidence threshold evaluation.

## Dependent Tasks

- None (AI integration interface is defined here; implementation is in task_003)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Web/Controllers/IntakeController.cs` — REST API controller
- **NEW** `src/backend/PatientAccess.Business/Services/IntakeService.cs` — Business logic service
- **NEW** `src/backend/PatientAccess.Business/Interfaces/IIntakeService.cs` — Service interface
- **NEW** `src/backend/PatientAccess.Business/Interfaces/IAiIntakeService.cs` — AI service interface (consumed by IntakeService, implemented in task_003)
- **NEW** `src/backend/PatientAccess.Business/DTOs/StartIntakeRequestDto.cs` — Request DTO for session start
- **NEW** `src/backend/PatientAccess.Business/DTOs/IntakeMessageRequestDto.cs` — Request DTO for chat message
- **NEW** `src/backend/PatientAccess.Business/DTOs/IntakeMessageResponseDto.cs` — Response DTO with AI reply and extracted data
- **NEW** `src/backend/PatientAccess.Business/DTOs/IntakeSummaryDto.cs` — Response DTO for intake summary
- **NEW** `src/backend/PatientAccess.Business/DTOs/UpdateIntakeRequestDto.cs` — Request DTO for manual edits to intake data
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register IntakeService and IAiIntakeService in DI
- **EXISTING** `src/backend/PatientAccess.Data/Models/IntakeRecord.cs` — Existing entity (no changes needed)

## Implementation Plan

1. **Define DTOs**: Create request/response DTOs matching the API contract from the UC-007 sequence diagram:
   - `StartIntakeRequestDto`: `AppointmentId` (required)
   - `IntakeMessageRequestDto`: `SessionId`, `Message` (patient text, max 2000 chars)
   - `IntakeMessageResponseDto`: `AiMessage`, `ExtractedData` (structured object), `ConfidenceScore`, `Category` (current question category), `Progress` (0-100), `SuggestManualFallback` (bool)
   - `IntakeSummaryDto`: Aggregated `Medications[]`, `Allergies[]`, `MedicalHistory[]`, `Symptoms[]`, `Concerns[]`
   - `UpdateIntakeRequestDto`: Partial update fields for patient edits

2. **Define service interfaces**:
   - `IIntakeService`: `StartSessionAsync`, `ProcessMessageAsync`, `GetSummaryAsync`, `UpdateIntakeAsync`, `CompleteIntakeAsync`
   - `IAiIntakeService`: `GenerateNextQuestionAsync(conversationHistory, category)`, `ExtractStructuredDataAsync(patientMessage, context)` — returns structured data and confidence score

3. **Implement IntakeService**:
   - `StartSessionAsync`: Validate appointment belongs to authenticated patient. Create `IntakeRecord` with mode `AIConversational`. Return session ID and welcome message.
   - `ProcessMessageAsync`: Load conversation history from intake record. Delegate to `IAiIntakeService.ExtractStructuredDataAsync`. If confidence >= 70%, persist extracted data to the appropriate JSONB column. If confidence < 70%, increment consecutive failure counter; if >= 3 failures, set `SuggestManualFallback = true`. Generate next question via `IAiIntakeService.GenerateNextQuestionAsync`. Calculate progress based on filled categories.
   - `GetSummaryAsync`: Load intake record and deserialize all JSONB columns into `IntakeSummaryDto`.
   - `UpdateIntakeAsync`: Allow partial updates to extracted data JSONB columns (for patient edits in summary).
   - `CompleteIntakeAsync`: Mark `IsCompleted = true`, set `CompletedAt = DateTime.UtcNow`.

4. **Implement IntakeController** with JWT `[Authorize(Roles = "Patient")]`:
   - `POST /api/intake/start` — Accepts `StartIntakeRequestDto`, returns session info
   - `POST /api/intake/{id}/message` — Accepts `IntakeMessageRequestDto`, returns `IntakeMessageResponseDto`
   - `GET /api/intake/{id}/summary` — Returns `IntakeSummaryDto`
   - `PATCH /api/intake/{id}` — Accepts `UpdateIntakeRequestDto`, updates extracted data
   - `POST /api/intake/{id}/complete` — Marks intake as complete
   - Include `[ValidateAntiForgeryToken]` or equivalent CSRF protection for state-changing endpoints.

5. **Register DI services** in `Program.cs`: Add `IIntakeService` → `IntakeService` and `IAiIntakeService` → placeholder/stub until task_003 is implemented.

6. **Add input validation**: Validate `Message` length <= 2000 characters on `IntakeMessageRequestDto`. Validate `AppointmentId` exists and belongs to the authenticated patient. Return 403 for unauthorized access attempts.

7. **Add audit logging**: Log intake session creation, message processing, and completion events via existing `AuditLogService` pattern (NFR-007).

## Current Project State

```
src/backend/
├── PatientAccess.Web/
│   ├── Controllers/
│   │   ├── AppointmentsController.cs
│   │   ├── AuthController.cs
│   │   ├── PatientController.cs
│   │   └── ...
│   └── Program.cs
├── PatientAccess.Business/
│   ├── DTOs/
│   │   ├── CreateAppointmentRequestDto.cs
│   │   └── ...
│   ├── Interfaces/
│   │   ├── IAppointmentService.cs
│   │   └── ...
│   └── Services/
│       ├── AppointmentService.cs
│       └── ...
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── IntakeRecord.cs        # Existing entity with JSONB fields
│   │   └── ...
│   └── PatientAccessDbContext.cs   # Already has DbSet<IntakeRecord>
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/DTOs/StartIntakeRequestDto.cs | Request DTO for POST /api/intake/start |
| CREATE | src/backend/PatientAccess.Business/DTOs/IntakeMessageRequestDto.cs | Request DTO for POST /api/intake/{id}/message |
| CREATE | src/backend/PatientAccess.Business/DTOs/IntakeMessageResponseDto.cs | Response DTO with AI reply, extracted data, confidence |
| CREATE | src/backend/PatientAccess.Business/DTOs/IntakeSummaryDto.cs | Response DTO for aggregated intake summary |
| CREATE | src/backend/PatientAccess.Business/DTOs/UpdateIntakeRequestDto.cs | Request DTO for PATCH /api/intake/{id} |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IIntakeService.cs | Intake service interface |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAiIntakeService.cs | AI intake service interface (implemented in task_003) |
| CREATE | src/backend/PatientAccess.Business/Services/IntakeService.cs | Intake business logic |
| CREATE | src/backend/PatientAccess.Web/Controllers/IntakeController.cs | REST API controller for intake endpoints |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IIntakeService and IAiIntakeService in DI container |

## External References

- [ASP.NET Core 8 Web API Controllers](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0)
- [Entity Framework Core 8 JSON Columns](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#json-columns)
- [ASP.NET Core Model Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-8.0)
- UC-007 Sequence Diagram: .propel/context/docs/models.md#UC-007

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj
```

## Implementation Validation Strategy

- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] All five intake endpoints return correct HTTP status codes
- [ ] Conversation state persisted correctly across multiple messages
- [ ] Confidence threshold evaluation returns `SuggestManualFallback` when < 70%
- [ ] Consecutive failure counter resets on successful extraction
- [ ] Unauthorized access to another patient's intake returns 403
- [ ] Audit logs created for session start and completion events

## Implementation Checklist

- [ ] Create DTOs for intake start, message, response, summary, and update operations
- [ ] Define `IIntakeService` and `IAiIntakeService` interfaces with method signatures
- [ ] Implement `IntakeService` with session lifecycle, message processing, and confidence evaluation
- [ ] Implement `IntakeController` with five endpoints secured by JWT Patient role authorization
- [ ] Register services in DI container in `Program.cs`
- [ ] Add input validation (message length, appointment ownership, required fields)
- [ ] Add audit logging for intake session events via `AuditLogService`
- [ ] Implement consecutive failure tracking and manual fallback suggestion logic
