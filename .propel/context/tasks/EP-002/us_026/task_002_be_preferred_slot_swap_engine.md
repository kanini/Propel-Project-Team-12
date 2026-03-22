# Task - task_002_be_preferred_slot_swap_engine

## Requirement Reference
- User Story: US_026
- Story Location: .propel/context/tasks/EP-002/us_026/us_026.md
- Acceptance Criteria:
    - AC-2: Automatic swap when preferred slot becomes available (cancellation or schedule change)
    - AC-3: Race condition handling - swap fails gracefully if another patient books preferred slot first
    - AC-4: Support canceling swap preference
    - AC-5: Atomic operations - release original slot, book preferred slot, update calendar

## Design References
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| All other fields | N/A |

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Backend | Entity Framework Core | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Hangfire (Background Jobs) | 1.8.x |

## Task Overview
Implement background slot swap detection engine that monitors TimeSlots for cancellations/schedule changes. When a slot becomes available (IsBooked = false), query Appointments WHERE PreferredSlotId = [slot] AND Status = 'Scheduled' ORDER BY CreatedAt ASC (FIFO). Execute atomic swap: BEGIN TRANSACTION; UPDATE original slot IsBooked=false; UPDATE preferred slot IsBooked=true; UPDATE appointment ScheduledDateTime, TimeSlotId; COMMIT. Handle race conditions with SELECT FOR UPDATE. Send swap confirmation notification. Add PATCH /api/appointments/{id}/swap endpoint to cancel swap preference.

## Dependent Tasks
- None (extends appointment booking functionality)

## Impacted Components
- Backend (.NET):
  - `src/backend/PatientAccess.Business/Services/SlotSwapService.cs` (NEW)
  - `src/backend/PatientAccess.Business/Interfaces/ISlotSwapService.cs` (NEW)
  - `src/backend/PatientAccess.Business/BackgroundJobs/SlotAvailabilityMonitor.cs` (NEW - Hangfire recurring job)
  - `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs` (UPDATE - add PATCH /swap endpoint)

## Implementation Plan
1. Create ISlotSwapService: Task<bool> ExecuteSwapAsync(Guid appointmentId, Guid preferredSlotId)
2. Implement SlotSwapService with atomic transaction logic (original slot release + preferred slot booking)
3. Create SlotAvailabilityMonitor Hangfire job: Runs every 5 minutes, queries slots WHERE IsBooked = false AND EXISTS(Appointments.PreferredSlotId = slot)
4. For each available preferred slot: Find first appointment with swap preference (FIFO), call ExecuteSwapAsync
5. Add transaction with SELECT FOR UPDATE to prevent race conditions
6. Handle swap failure gracefully: Log error, retain original booking
7. Send notification: INotificationService.SendSwapConfirmationAsync
8. Add PATCH /api/appointments/{id}/swap endpoint: Set PreferredSlotId = null to cancel swap preference

## Expected Changes
| Action | File Path | Description |
|--------|-------------|-----------  |
| CREATE | src/backend/PatientAccess.Business/Services/SlotSwapService.cs | Atomic swap execution with transaction |
| CREATE | src/backend/PatientAccess.Business/Interfaces/ISlotSwapService.cs | Service interface for swap logic |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/SlotAvailabilityMonitor.cs | Hangfire recurring job for swap detection |
| UPDATE | src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs | Add PATCH /api/appointments/{id}/swap endpoint |
| UPDATE | src/backend/PatientAccess.Web/Program.cs | Configure Hangfire, register SlotAvailabilityMonitor job |

## Implementation Checklist
- [ ] Create ISlotSwapService with ExecuteSwapAsync(Guid appointmentId, Guid preferredSlotId)
- [ ] Implement SlotSwapService.ExecuteSwapAsync with BEGIN TRANSACTION
- [ ] SELECT appointment and slots FOR UPDATE (pessimistic locking)
- [ ] Check preferred slot IsBooked = false (if true, abort swap gracefully)
- [ ] UPDATE original TimeSlot SET IsBooked = false
- [ ] UPDATE preferred TimeSlot SET IsBooked = true
- [ ] UPDATE Appointment SET TimeSlotId = preferred, ScheduledDateTime = preferred.StartTime, PreferredSlotId = null
- [ ] COMMIT transaction
- [ ] Call INotificationService.SendSwapConfirmationAsync(appointment.PatientId)
- [ ] Create SlotAvailabilityMonitor Hangfire recurring job (every 5 minutes)
- [ ] Query: SELECT * FROM "TimeSlots" WHERE "IsBooked" = false AND EXISTS (SELECT 1 FROM "Appointments" WHERE "PreferredSlotId" = "TimeSlots"."TimeSlotId")
- [ ] For each slot: Get first appointment with swap preference ORDER BY CreatedAt ASC (FIFO)
- [ ] Call ExecuteSwapAsync
- [ ] Add PATCH /api/appointments/{id}/swap endpoint
- [ ] Verify ownership: WHERE AppointmentId = {id} AND PatientId = {patientId}
- [ ] Update Appointment SET PreferredSlotId = null
- [ ] Return 200 OK
- [ ] Register ISlotSwapService -> SlotSwapService in DI
- [ ] Configure Hangfire in Program.cs
- [ ] Schedule SlotAvailabilityMonitor: RecurringJob.AddOrUpdate(() => monitor.Run(), Cron.MinuteInterval(5))

Task truncated for brevity. Estimated effort: 8 hours.

