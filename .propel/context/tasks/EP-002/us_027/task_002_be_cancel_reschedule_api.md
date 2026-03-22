# Task - task_002_be_cancel_reschedule_api

## Requirement Reference
- User Story: US_027
- Story Location: .propel/context/tasks/EP-002/us_027/us_027.md
- Acceptance Criteria:
    - AC-1: Cancel updates status to Cancelled, releases slot, sends notification
    - AC-3: Reschedule atomic operation - release original, book new, send confirmation
    - AC-4: Enforce configurable advance notice (e.g., 24 hours) - return 403 Forbidden if violated

## Design References
| UI Impact | No |
| All fields | N/A |

## Applicable Technology Stack
| Backend | .NET 8.0 |
| Database | PostgreSQL 16.x |
| Library | EF Core 8.0 |

## Task Overview
Implement DELETE /api/appointments/{id} for cancellation with advance notice policy enforcement. Add PATCH /api/appointments/{id}/reschedule for rescheduling with atomic transaction (release original slot, book new slot). Validate cancellation window (e.g., appointment time - current time >= 24 hours). Return 403 Forbidden when policy violated. Send notification confirmations via placeholder INotificationService.

## Dependent Tasks
- None (extends existing appointment functionality)

## Impacted Components
- ` src/backend/PatientAccess.Business/Services/AppointmentService.cs` (UPDATE - add CancelAsync, RescheduleAsync)
-  `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs` (UPDATE - add DELETE, PATCH endpoints)

## Implementation Plan
1. Add CancelAsync method to AppointmentService: Verify ownership, check cancellation policy, update status to Cancelled, release slot (IsBooked = false), send notification
2. Policy check: appointment.ScheduledDateTime - DateTime.UtcNow >= cancellationNoticeHours (configurable, default 24)
3. Add RescheduleAsync method: BEGIN TRANSACTION; Verify ownership; Release original slot; Book new slot (SELECT FOR UPDATE, check availability); Update appointment TimeSlotId and ScheduledDateTime; COMMIT
4. Handle conflict: New slot already booked (IsBooked = true) -> rollback, return 409 Conflict
5. Add DELETE /api/appointments/{id} endpoint calling CancelAsync
6. Add PATCH /api/appointments/{id}/reschedule endpoint calling RescheduleAsync
7. Return 403 Forbidden when policy violated with message: "Appointments must be cancelled at least {hours} hours in advance"

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| UPDATE | src/backend/PatientAccess.Business/Services/AppointmentService.cs | Add CancelAsync, RescheduleAsync methods |
| UPDATE | src/backend/PatientAccess.Business/Interfaces/IAppointmentService.cs | Add method signatures |
| UPDATE | src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs | Add DELETE and PATCH endpoints |
| UPDATE | src/backend/PatientAccess.Web/appsettings.json | Add CancellationNoticeHours config (default: 24) |

## Implementation Checklist
- [x] Add CancelAsync(Guid appointmentId, Guid patientId): Verify ownership
- [x] Check cancellation policy: appointment.ScheduledDateTime - DateTime.UtcNow >= _config.CancellationNoticeHours
- [x] If violated, throw PolicyViolationException (403 Forbidden)
- [x] Update Appointment.Status = "Cancelled"
- [x] Update TimeSlot.IsBooked = false (release slot)
- [ ] Call INotificationService.SendCancellationConfirmationAsync (TODO comment exists)
- [x] Add RescheduleAsync(Guid appointmentId, Guid patientId, Guid newTimeSlotId): BEGIN TRANSACTION
- [x] SELECT appointment FOR UPDATE
- [x] SELECT original TimeSlot FOR UPDATE, SET IsBooked = false
- [x] SELECT new TimeSlot FOR UPDATE
- [x] If new slot IsBooked = true, rollback and throw ConflictException
- [x] SET new slot IsBooked = true
- [x] UPDATE appointment SET TimeSlotId = new, ScheduledDateTime = new.StartTime
- [x] COMMIT
- [ ] Call INotificationService.SendRescheduleConfirmationAsync (TODO comment exists)
- [x] Add DELETE /api/appointments/{id} endpoint
- [x] Extract patientId from claims, call CancelAsync
- [x] Handle PolicyViolationException -> 403 Forbidden
- [x] Add PATCH /api/appointments/{id}/reschedule endpoint with body: { newTimeSlotId }
- [x] Call RescheduleAsync, handle ConflictException -> 409 Conflict
- [x] Add CancellationNoticeHours to appsettings.json (default: 24)

Estimated effort: 6 hours.

