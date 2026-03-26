# Task - task_001_be_noshow_risk_scoring_engine

## Requirement Reference

- User Story: us_038
- Story Location: .propel/context/tasks/EP-005/us_038/us_038.md
- Acceptance Criteria:
  - AC-1: Risk score (0-100) calculated within 100ms (NFR-016) using weighted factors: appointment lead time, previous no-show history, and confirmation response rate
  - AC-2: Each factor has a configurable weight and the calculation is deterministic (rule-based, not AI)
- Edge Cases:
  - First-time patients with no history: Default risk score uses only appointment lead time; no-show history weight evaluates to neutral (50/100)
  - Same-day walk-ins: Walk-in appointments receive a fixed low-risk score (0) since already on-site

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

Implement the rule-based no-show risk scoring engine as a standalone service. This creates `INoShowRiskService` and `NoShowRiskService` that calculate a deterministic risk score (0-100) using three configurable weighted factors: appointment lead time, previous no-show history ratio, and confirmation response rate (TR-020). Factor weights are read from `SystemSettings` (created by US_037 task_001) with appsettings.json fallback defaults. The engine handles edge cases for first-time patients (neutral history score) and walk-in appointments (fixed score of 0). The calculation is pure in-memory arithmetic after a single indexed DB read, meeting the 100ms NFR-016 performance requirement.

## Dependent Tasks

- EP-005/us_037/task_001_db_reminder_configuration_schema — Provides SystemSettings table for configurable weight storage (shared key-value config store)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Interfaces/INoShowRiskService.cs` — Interface for risk score calculation
- **NEW** `src/backend/PatientAccess.Business/Services/NoShowRiskService.cs` — Rule-based scoring engine with configurable weights
- **NEW** `src/backend/PatientAccess.Business/DTOs/NoShowRiskScoreDto.cs` — Response DTO with score, risk level, and factor breakdown

## Implementation Plan

1. **Define NoShowRiskScoreDto**:
   ```csharp
   public class NoShowRiskScoreDto
   {
       public decimal Score { get; set; }           // 0-100
       public string RiskLevel { get; set; } = string.Empty; // "Low", "Medium", "High"
       public decimal LeadTimeFactor { get; set; }   // 0-100 raw factor
       public decimal HistoryFactor { get; set; }    // 0-100 raw factor
       public decimal ConfirmationFactor { get; set; } // 0-100 raw factor
   }
   ```
   Includes factor breakdown for audit/debugging. `RiskLevel` derived from score: Low (<40), Medium (40-70), High (>70).

2. **Define INoShowRiskService interface**:
   ```csharp
   namespace PatientAccess.Business.Interfaces;

   public interface INoShowRiskService
   {
       Task<NoShowRiskScoreDto> CalculateRiskScoreAsync(
           Guid patientId,
           DateTime scheduledDateTime,
           bool isWalkIn);
   }
   ```
   Single method — takes patient context and appointment timing, returns scored result.

3. **Implement NoShowRiskService — weight configuration**:
   - Inject `PatientAccessDbContext`, `IConfiguration`, `ILogger<NoShowRiskService>`
   - Read weights from SystemSettings (keys: `Risk.LeadTimeWeight`, `Risk.HistoryWeight`, `Risk.ConfirmationWeight`)
   - Fallback to `appsettings.json` section `RiskScoringWeights` if SystemSettings not populated
   - Default weights: LeadTime=0.30, History=0.45, Confirmation=0.25 (sum to 1.0)
   - Validate weights sum to 1.0 (or normalize if not)

4. **Implement lead time factor calculation**:
   ```
   LeadTimeHours = (ScheduledDateTime - DateTime.UtcNow).TotalHours
   Factor mapping:
     <2h:     20  (imminent, low forgetting risk)
     2-24h:   30
     24-72h:  50
     72-168h: 60
     >168h:   80  (far out, high forgetting risk)
   ```

5. **Implement no-show history factor calculation**:
   - Query `NoShowHistory` by PatientId (unique index, O(1) lookup)
   - If no record exists (first-time patient — EC-1): return 50 (neutral)
   - If record exists: calculate `noShowRate = NoShowCount / TotalAppointments`
   ```
   Rate mapping:
     0 no-shows:   20
     <10%:         20
     10-25%:       40
     25-50%:       60
     >50%:         90
   ```

6. **Implement confirmation response rate factor**:
   - Read `ConfirmationResponseRate` from same NoShowHistory record
   - If null or no history (first-time — EC-1): return 50 (neutral)
   ```
   Rate mapping:
     >90%:   15  (very responsive, low risk)
     70-90%: 35
     50-70%: 55
     <50%:   75  (unresponsive, high risk)
   ```

7. **Implement walk-in override and final score**:
   - If `isWalkIn == true` (EC-2): return `new NoShowRiskScoreDto { Score = 0, RiskLevel = "Low", ... }` immediately
   - Final score: `Score = (W1 × LeadTimeFactor) + (W2 × HistoryFactor) + (W3 × ConfirmationFactor)`
   - Clamp to 0-100 range
   - Derive RiskLevel: Low (<40), Medium (40-70), High (>70)
   - Log score calculation at Debug level for audit purposes

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── Appointment.cs              # EXISTS — has NoShowRiskScore decimal(5,2) nullable
│   │   ├── NoShowHistory.cs            # EXISTS — patient-level aggregated metrics
│   │   ├── AppointmentStatus.cs        # EXISTS — includes NoShow=6
│   │   └── ...
│   ├── Configurations/
│   │   ├── NoShowHistoryConfiguration.cs # EXISTS — unique index on PatientId
│   │   └── ...
│   └── PatientAccessDbContext.cs         # EXISTS — DbSet<NoShowHistory>
├── PatientAccess.Business/
│   ├── Interfaces/                       # No INoShowRiskService
│   ├── Services/
│   │   ├── AppointmentService.cs        # EXISTS — walk-ins already set NoShowRiskScore=0
│   │   └── ...
│   └── DTOs/                             # No NoShowRiskScoreDto
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Interfaces/INoShowRiskService.cs | Interface with CalculateRiskScoreAsync method |
| CREATE | src/backend/PatientAccess.Business/Services/NoShowRiskService.cs | Rule-based scoring engine with 3 weighted factors |
| CREATE | src/backend/PatientAccess.Business/DTOs/NoShowRiskScoreDto.cs | Response DTO with score, risk level, factor breakdown |

## External References

- .NET 8 Performance Best Practices: https://learn.microsoft.com/en-us/aspnet/core/performance/overview
- EF Core Query Performance: https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying
- No-Show Risk Scoring Research: Rule-based weighted scoring per TR-020 specification

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.Business
```

## Implementation Validation Strategy

- [x] `INoShowRiskService` interface compiles with correct method signature
- [x] `NoShowRiskService` reads configurable weights from SystemSettings with appsettings fallback
- [x] Score calculation returns value within 0-100 range for all input combinations
- [x] First-time patient (no NoShowHistory record) returns neutral history/confirmation factors (50)
- [x] Walk-in appointment returns fixed score of 0 immediately
- [x] RiskLevel correctly derived: Low (<40), Medium (40-70), High (>70)
- [x] Single DB query for NoShowHistory (verify via logging or profiling meets 100ms NFR-016)

## Implementation Checklist

- [x] Create `NoShowRiskScoreDto` with score, risk level, and factor breakdown fields
- [x] Create `INoShowRiskService` interface with `CalculateRiskScoreAsync` method
- [x] Implement configurable weight reading from SystemSettings with appsettings fallback
- [x] Implement lead time factor calculation with hour-range mapping
- [x] Implement no-show history factor with rate-based mapping and first-time patient neutral handling
- [x] Implement confirmation response rate factor with neutral default for missing data
- [x] Implement walk-in override (return 0 immediately) and final weighted score clamping
