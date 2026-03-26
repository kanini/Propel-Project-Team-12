# Task - task_002_be_risk_scoring_integration

## Requirement Reference

- User Story: us_038
- Story Location: .propel/context/tasks/EP-005/us_038/us_038.md
- Acceptance Criteria:
  - AC-1: No-show risk score (0-100) calculated when booking is confirmed, within 100ms (NFR-016)
  - AC-4: When a no-show is recorded or a confirmation is received, NoShowHistory record is updated and future risk scores reflect the new data
- Edge Cases:
  - First-time patients with no history: NoShowHistory record created on first booking with neutral defaults
  - Same-day walk-ins: Already handled in AppointmentService (NoShowRiskScore = 0)

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

Integrate the `INoShowRiskService` scoring engine into the appointment booking flow and implement NoShowHistory lifecycle updates. On appointment booking, the service calculates a risk score and persists it on the `Appointment.NoShowRiskScore` field. The `AppointmentResponseDto` is extended to include the risk score for API consumers. NoShowHistory records are created for first-time patients and updated when appointments transition to `NoShow` status (incrementing `NoShowCount`) or when confirmations are received (updating `ConfirmationResponseRate`). The risk scoring weights configuration section is added to `appsettings.json` as fallback defaults. `INoShowRiskService` is registered in the DI container.

## Dependent Tasks

- EP-005/us_038/task_001_be_noshow_risk_scoring_engine — Provides INoShowRiskService and NoShowRiskService
- EP-005/us_037/task_001_db_reminder_configuration_schema — Provides SystemSettings table (for weight config storage)

## Impacted Components

- **MODIFY** `src/backend/PatientAccess.Business/Services/AppointmentService.cs` — Call INoShowRiskService on booking; update NoShowHistory on status changes
- **MODIFY** `src/backend/PatientAccess.Business/DTOs/AppointmentResponseDto.cs` — Add NoShowRiskScore and RiskLevel fields
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register INoShowRiskService in DI container
- **MODIFY** `src/backend/PatientAccess.Web/appsettings.json` — Add RiskScoringWeights default configuration section

## Implementation Plan

1. **Add NoShowRiskScore to AppointmentResponseDto**:
   ```csharp
   /// <summary>
   /// Calculated no-show risk score (0-100). Null for legacy appointments without scoring.
   /// </summary>
   public decimal? NoShowRiskScore { get; set; }

   /// <summary>
   /// Risk level derived from score: "Low" (<40), "Medium" (40-70), "High" (>70). Null when score is null.
   /// </summary>
   public string? RiskLevel { get; set; }
   ```
   Nullable to support existing appointments that don't have scores yet.

2. **Integrate risk scoring into BookAppointmentAsync**:
   - Inject `INoShowRiskService` into `AppointmentService` constructor
   - After appointment record creation (inside the transaction, after `SaveChangesAsync`):
     ```csharp
     var riskResult = await _noShowRiskService.CalculateRiskScoreAsync(
         patientId,
         timeSlot.StartTime,
         isWalkIn: false);
     appointment.NoShowRiskScore = riskResult.Score;
     await _context.SaveChangesAsync();
     ```
   - Include `NoShowRiskScore` and `RiskLevel` in the `AppointmentResponseDto` mapping
   - Ensure risk calculation + DB update stays within the existing transaction

3. **Ensure NoShowHistory record exists for first-time patients**:
   - In `CalculateRiskScoreAsync` (or in the integration layer), when no `NoShowHistory` record found:
     ```csharp
     // Create initial NoShowHistory for first-time patients
     var history = new NoShowHistory
     {
         PatientId = patientId,
         TotalAppointments = 0,
         NoShowCount = 0,
         ConfirmationResponseRate = null,
         AverageLeadTimeHours = null,
         LastCalculatedRiskScore = null,
         CreatedAt = DateTime.UtcNow
     };
     _context.NoShowHistory.Add(history);
     await _context.SaveChangesAsync();
     ```
   - This ensures future bookings have a history record to update

4. **Implement NoShowHistory update on appointment status change to NoShow (AC-4)**:
   - Add method to `AppointmentService` (or create a dedicated update method):
     ```csharp
     private async Task UpdateNoShowHistoryForNoShowAsync(Guid patientId)
     {
         var history = await _context.NoShowHistory
             .FirstOrDefaultAsync(h => h.PatientId == patientId);
         if (history != null)
         {
             history.NoShowCount++;
             history.TotalAppointments++;
             history.UpdatedAt = DateTime.UtcNow;
         }
     }
     ```
   - Call this when appointment status transitions to `AppointmentStatus.NoShow`
   - If the existing cancellation/status-change flow doesn't have a dedicated method yet, add status change handling

5. **Implement NoShowHistory update on confirmation received (AC-4)**:
   - When `ConfirmationReceived` flag is set on an appointment:
     ```csharp
     private async Task UpdateNoShowHistoryForConfirmationAsync(Guid patientId)
     {
         var history = await _context.NoShowHistory
             .FirstOrDefaultAsync(h => h.PatientId == patientId);
         if (history != null)
         {
             // Recalculate confirmation response rate
             history.TotalAppointments++;
             var totalConfirmable = history.TotalAppointments;
             var confirmedCount = /* calculate from total - noshow or track separately */;
             history.ConfirmationResponseRate = (confirmedCount / (decimal)totalConfirmable) * 100;
             history.UpdatedAt = DateTime.UtcNow;
         }
     }
     ```
   - Increment `TotalAppointments` on each completed appointment cycle
   - Update `ConfirmationResponseRate` based on confirmed vs total ratio

6. **Add RiskScoringWeights to appsettings.json**:
   ```json
   "RiskScoringWeights": {
       "LeadTimeWeight": 0.30,
       "HistoryWeight": 0.45,
       "ConfirmationWeight": 0.25,
       "_comment": "Configurable weights for no-show risk scoring (TR-020, FR-023). Weights must sum to 1.0. Can be overridden via SystemSettings table."
   }
   ```

7. **Register INoShowRiskService in DI container**:
   ```csharp
   builder.Services.AddScoped<INoShowRiskService, NoShowRiskService>(); // US_038 - No-show risk scoring
   ```
   Scoped lifetime — uses DbContext (which is scoped).

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/
│   │   └── INoShowRiskService.cs       # FROM task_001
│   ├── Services/
│   │   ├── AppointmentService.cs        # EXISTS — creates appointments, walk-ins set NoShowRiskScore=0
│   │   └── NoShowRiskService.cs         # FROM task_001
│   └── DTOs/
│       ├── AppointmentResponseDto.cs    # EXISTS — no risk score fields yet
│       └── NoShowRiskScoreDto.cs        # FROM task_001
├── PatientAccess.Data/
│   └── Models/
│       ├── Appointment.cs               # EXISTS — NoShowRiskScore decimal(5,2) nullable
│       └── NoShowHistory.cs             # EXISTS — patient-level metrics
├── PatientAccess.Web/
│   ├── Program.cs                       # EXISTS — DI registrations
│   └── appsettings.json                 # EXISTS — no RiskScoringWeights section
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Business/DTOs/AppointmentResponseDto.cs | Add NoShowRiskScore and RiskLevel nullable fields |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs | Call INoShowRiskService on booking; add NoShowHistory update methods for NoShow status and confirmation |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register INoShowRiskService as Scoped in DI |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add RiskScoringWeights configuration section |

## External References

- EF Core Change Tracking: https://learn.microsoft.com/en-us/ef/core/change-tracking/
- ASP.NET Core DI Lifetimes: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#service-lifetimes
- Appointment Status Lifecycle: design.md Domain Entities — Appointment (Scheduled → Confirmed → Arrived → Completed/Cancelled/No-Show)

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [x] `AppointmentResponseDto` includes `NoShowRiskScore` and `RiskLevel` fields
- [x] Booking endpoint returns risk score in response for new appointments
- [x] Walk-in appointments still get NoShowRiskScore = 0 (existing behavior preserved)
- [x] First-time patient booking creates NoShowHistory record with neutral defaults
- [x] Marking an appointment as NoShow increments `NoShowCount` in NoShowHistory
- [x] Receiving a confirmation updates `ConfirmationResponseRate` in NoShowHistory
- [x] `INoShowRiskService` resolves from DI without runtime errors
- [x] Solution builds without warnings

## Implementation Checklist

- [x] Add `NoShowRiskScore` and `RiskLevel` nullable properties to `AppointmentResponseDto`
- [x] Inject `INoShowRiskService` into `AppointmentService` and call on booking
- [x] Create NoShowHistory record for first-time patients when none exists
- [x] Implement NoShowHistory update when appointment status changes to NoShow
- [x] Implement NoShowHistory update when appointment confirmation is received
- [x] Add `RiskScoringWeights` configuration section to `appsettings.json`
- [x] Register `INoShowRiskService` as Scoped in `Program.cs` DI container
