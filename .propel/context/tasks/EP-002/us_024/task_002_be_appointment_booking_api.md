# Task - task_002_be_appointment_booking_api

## Requirement Reference
- User Story: US_024
- Story Location: .propel/context/tasks/EP-002/us_024/us_024.md
- Acceptance Criteria:
    - AC-2: API responds within 500ms at 95th percentile (NFR-001)
    - AC-3: Booking validates slot availability using FOR UPDATE lock to prevent double-booking
    - AC-4: Concurrent booking conflict returns 409 Conflict status code
- Edge Case:
    - Slow API responses handled with timeout (server must respond within reasonable time)

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

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact
>
> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Backend | Entity Framework Core | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Npgsql.EntityFrameworkCore.PostgreSQL | 8.x |
| AI/ML | N/A | N/A |
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

> **Mobile Impact Legend:**
> - **Yes**: Task involves mobile app development (native or cross-platform)
> - **No**: Task is web, backend, or infrastructure only
>
> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview
Implement RESTful API endpoints for appointment booking with provider availability calendar. Create GET /api/providers/{providerId}/availability endpoints for monthly and daily time slot retrieval meeting 500ms P95 response time (NFR-001). Implement POST /api/appointments endpoint with pessimistic locking (SELECT FOR UPDATE) to prevent double-booking race conditions. Handle concurrent booking conflicts with 409 Conflict response when slot already booked. Support optional preferred slot swap feature by storing swap preferences in Appointments table. Implement comprehensive error handling for validation failures, database constraints, and deadlock scenarios.

## Dependent Tasks
- task_003_db_appointment_indexes.md - Database indexes for performance

## Impacted Components
- Backend (.NET): New components to be created
  - `src/backend/PatientAccess.Business/Services/AppointmentService.cs` (NEW)
  - `src/backend/PatientAccess.Business/Interfaces/IAppointmentService.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/AvailabilityResponseDto.cs` (NEW)
  - `src/backend/ PatientAccess.Business/DTOs/TimeSlotDto.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/CreateAppointmentRequestDto.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/AppointmentResponseDto.cs` (NEW)
  - `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs` (NEW)
  - `src/backend/PatientAccess.Business/Models/Appointment.cs` (NEW or UPDATE)

## Implementation Plan
1. **Create EF Core entity models**:
   - Update or create Appointment entity with properties: AppointmentId, PatientId, ProviderId, TimeSlotId, ScheduledDateTime, VisitReason, Status (Scheduled/Confirmed/Arrived/Completed/Cancelled/NoShow), PreferredSlotId (nullable), CreatedAt
   - Add navigation properties: Appointment.Patient, Appointment.Provider, Appointment.TimeSlot, Appointment.PreferredSlot

2. **Create DTOs for data transfer**:
   - TimeSlotDto: Id, StartTime, EndTime, IsBooked, Duration
   - AvailabilityResponseDto: Date, TimeSlots (List<TimeSlotDto>)
   - CreateAppointmentRequestDto: ProviderId, TimeSlotId, VisitReason, PreferredSlotId (nullable)
   - AppointmentResponseDto: Id, ProviderId, ProviderName, ScheduledDateTime, VisitReason, Status, ConfirmationNumber

3. **Implement IAppointmentService interface**:
   - Task<AvailabilityResponseDto> GetMonthlyAvailabilityAsync(Guid providerId, int year, int month)
   - Task<AvailabilityResponseDto> GetDailyAvailabilityAsync(Guid providerId, DateTime date)
   - Task<AppointmentResponseDto> CreateAppointmentAsync(Guid patientId, CreateAppointmentRequestDto request)

4. **Build GetMonthlyAvailabilityAsync logic**:
   - Query TimeSlots for providerId WHERE StartTime between first day of month and last day of month
   - Group by date, return dates with at least one available slot (IsBooked = false)
   - Use AsNoTracking() for read-only performance
   - Optimize query with index on (ProviderId, StartTime, IsBooked)

5. **Build GetDailyAvailabilityAsync logic**:
   - Query TimeSlots for providerId and specific date
   - Return all time slots with IsBooked status
   - Meet 500ms P95 response time target (NFR-001)
   - Use caching for frequently accessed dates (optional enhancement)

6. **Implement CreateAppointmentAsync with pessimistic locking**:
   - Start database transaction
   - SELECT TimeSlot FOR UPDATE WHERE TimeSlotId = request.TimeSlotId (EF Core: FromSqlRaw with FOR UPDATE)
   - Check IsBooked = false; if true, rollback and throw ConflictException (409)
   - Set TimeSlot.IsBooked = true
   - Create Appointment record with Status = "Scheduled"
   - Generate unique ConfirmationNumber (8-character alphanumeric)
   - Commit transaction
   - Return AppointmentResponseDto

7. **Create AppointmentsController API endpoints**:
   - GET /api/providers/{providerId}/availability?month={month}&year={year} -> GetMonthlyAvailabilityAsync
   - GET /api/providers/{providerId}/availability?date={date} -> GetDailyAvailabilityAsync
   - POST /api/appointments -> CreateAppointmentAsync
   - [Authorize] attribute for authenticated users only
   - Extract patientId from authenticated user claims
   - Return 201 Created with Location header for successful bookings
   - Return 409 Conflict for concurrent booking conflicts (AC-4)
   - Return 400 Bad Request for validation failures (invalid TimeSlotId, ProviderId)
   - Return 500 Internal Server Error for database exceptions

8. **Add comprehensive error handling**:
   - Custom ConflictException for 409 responses
   - Validation: TimeSlotId exists, ProviderId exists, VisitReason not empty, VisitReason max 200 chars
   - Deadlock detection and retry logic (max 3 retries with exponential backoff)
   - Log all booking attempts (success/failure) with ILogger

## Current Project State
```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   └── (AppointmentService.cs to be created)
│   ├── Interfaces/
│   │   └── (IAppointmentService.cs to be created)
│   ├── DTOs/
│   │   └── (AvailabilityResponseDto.cs, TimeSlotDto.cs, CreateAppointmentRequestDto.cs, AppointmentResponseDto.cs to be created)
│   ├── Models/
│   │   └── (Appointment.cs to be created or updated)
│   └── Exceptions/
│       └── (ConflictException.cs to be created)
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── (AppointmentsController.cs to be created)
│   └── Program.cs (to be modified for DI registration)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Models/Appointment.cs | EF Core entity for Appointments table |
| CREATE | src/backend/PatientAccess.Business/DTOs/TimeSlotDto.cs | DTO for time slot data |
| CREATE | src/backend/PatientAccess.Business/DTOs/AvailabilityResponseDto.cs | DTO for availability calendar response |
| CREATE | src/backend/PatientAccess.Business/DTOs/CreateAppointmentRequestDto.cs | DTO for appointment creation request |
| CREATE | src/backend/PatientAccess.Business/DTOs/AppointmentResponseDto.cs | DTO for appointment creation response |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAppointmentService.cs | Service interface for appointment and availability logic |
| CREATE | src/backend/PatientAccess.Business/Services/AppointmentService.cs | Service implementation with FOR UPDATE locking |
| CREATE | src/backend/PatientAccess.Business/Exceptions/ConflictException.cs | Custom exception for 409 Conflict responses |
| CREATE | src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs | REST API controller for availability and booking endpoints |
| MODIFY | src/backend/PatientAccess.Business/Data/ApplicationDbContext.cs | Add DbSet<Appointment> |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IAppointmentService -> AppointmentService in DI |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [EF Core Pessimistic Locking](https://learn.microsoft.com/en-us/ef/core/saving/transactions#controlling-transactions)
- [PostgreSQL SELECT FOR UPDATE](https://www.postgresql.org/docs/16/sql-select.html#SQL-FOR-UPDATE-SHARE)
- [ASP.NET Core Conflict Handling](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors)
- [EF Core FromSqlRaw](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries)
- [Transaction Deadlock Handling](https://www.npgsql.org/doc/types/datetime.html)
- [ASP.NET Core Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)

## Build Commands
- `dotnet build` - Build solution
- `dotnet test` - Run xUnit tests
- `dotnet run --project src/backend/PatientAccess.Web` - Start API server
- `dotnet ef migrations add AddAppointments --project src/backend/PatientAccess.Business` - Generate EF Core migration

## Implementation Validation Strategy
- [ ] Unit tests pass for AppointmentService.CreateAppointmentAsync
- [ ] Integration tests pass for GET /api/providers/{id}/availability endpoints
- [ ] Integration tests pass for POST /api/appointments endpoint
- [ ] API response time meets 500ms P95 target (NFR-001) - load test with k6 or Apache Bench
- [ ] Concurrent booking conflict returns 409 Conflict (AC-4)
- [ ] FOR UPDATE lock prevents double-booking (concurrent test with 2 simultaneous requests)
- [ ] ConfirmationNumber generation verified (unique, 8-character alphanumeric)
- [ ] Validation errors return 400 Bad Request with detailed error messages
- [ ] Unauthorized requests return 401 Unauthorized
- [ ] Deadlock retry logic tested (simulate deadlock scenario)
- [ ] PreferredSlotId optional field stored correctly when provided

## Implementation Checklist
- [X] Create or update Appointment.cs entity with AppointmentId, PatientId, ProviderId, TimeSlotId, ScheduledDateTime, VisitReason, Status, PreferredSlotId, ConfirmationNumber, CreatedAt
- [X] Add navigation properties: Patient, Provider, TimeSlot, PreferredSlot
- [X] Create TimeSlotDto.cs with Id, StartTime, EndTime, IsBooked, Duration
- [X] Create AvailabilityResponseDto.cs with Date, TimeSlots (List<TimeSlotDto>)
- [X] Create CreateAppointmentRequestDto.cs with ProviderId, TimeSlotId, VisitReason, PreferredSlotId?
- [X] Create AppointmentResponseDto.cs with Id, ProviderId, ProviderName, ScheduledDateTime, VisitReason, Status, ConfirmationNumber
- [X] Define IAppointmentService interface with GetMonthlyAvailabilityAsync, GetDailyAvailabilityAsync, CreateAppointmentAsync
- [X] Implement GetMonthlyAvailabilityAsync: Query TimeSlots WHERE ProviderId = {id} AND StartTime between month range
- [X] Implement GetDailyAvailabilityAsync: Query TimeSlots WHERE ProviderId = {id} AND DATE(StartTime) = {date}
- [X] Use AsNoTracking() for availability queries (read-only)
- [X] Implement CreateAppointmentAsync with transaction: BEGIN; SELECT ... FOR UPDATE; UPDATE; INSERT; COMMIT;
- [X] Use EF Core FromSqlRaw for SELECT FOR UPDATE query: SELECT * FROM "TimeSlots" WHERE "TimeSlotId" = {id} FOR UPDATE
- [X] Check IsBooked = false; if true, throw ConflictException
- [X] Set TimeSlot.IsBooked = true
- [X] Create Appointment record with Status = "Scheduled"
- [X] Generate ConfirmationNumber using Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()
- [X] Commit transaction, rollback on exception
- [X] Create ConflictException.cs inheriting from Exception
- [X] Create AppointmentsController.cs with GET /api/providers/{providerId}/availability
- [X] Add query param logic: ?month={}&year={} -> GetMonthlyAvailabilityAsync, ?date={} -> GetDailyAvailabilityAsync
- [X] Create POST /api/appointments endpoint calling CreateAppointmentAsync
- [X] Extract patientId from User.Claims (ClaimTypes.NameIdentifier)
- [X] Add [Authorize] attribute to all endpoints
- [X] Return 201 Created with Location: /api/appointments/{appointmentId}
- [X] Map ConflictException to 409 Conflict response
- [X] Add ModelState validation for CreateAppointmentRequestDto
- [X] Add try-catch for deadlock detection, retry up to 3 times with exponential backoff
- [X] Register DbSet<Appointment> in ApplicationDbContext.cs
- [X] Register IAppointmentService -> AppointmentService in Program.cs DI
- [ ] Write unit tests: CreateAppointmentAsync throws ConflictException when slot booked
- [ ] Write unit tests: CreateAppointmentAsync creates appointment when slot available
- [ ] Write integration tests: GET /api/providers/{id}/availability returns 200 OK with time slots
- [ ] Write integration tests: POST /api/appointments returns 201 Created
- [ ] Write integration tests: Concurrent POST requests, one returns 409 Conflict
- [ ] Load test with k6: Verify 500ms P95 response time
- [ ] Manual test: Simulate deadlock scenario (2 simultaneous bookings on same slot)
