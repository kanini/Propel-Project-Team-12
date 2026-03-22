# Task - task_002_be_arrival_marking_api

## Requirement Reference

- User Story: US_031 - Patient Arrival Status Marking
- Story Location: .propel/context/tasks/EP-003/us_031/us_031.md
- Acceptance Criteria:
  - AC-1: Given I am on the Arrival Management page, When I search for a patient with a today's appointment, Then the patient's appointment details are displayed with a "Mark Arrived" button.
  - AC-2: Given I click "Mark Arrived", When the action is confirmed, Then the appointment status updates to "Arrived", the patient is added to the queue, and an audit log entry is created.
  - AC-3: Given a patient has no appointment today, When I search for them on the arrival page, Then the system indicates no appointment found and offers to create a walk-in booking.
  - AC-4: Given RBAC is enforced, When a Patient user attempts to self-check-in, Then the arrival marking functionality is not available — only Staff can mark arrivals.
- Edge Case:
  - What happens when Staff marks a patient arrived who was already marked? System displays "Patient already marked as arrived" and prevents duplicate status change.
  - How does the system handle marking arrival for a cancelled appointment? System prevents arrival marking for cancelled appointments with "Appointment was cancelled" message.

## Design References (Frontend Tasks Only)

| Reference Type         | Value |
| ---------------------- | ----- |
| **UI Impact**          | No    |
| **Figma URL**          | N/A   |
| **Wireframe Status**   | N/A   |
| **Wireframe Type**     | N/A   |
| **Wireframe Path/URL** | N/A   |
| **Screen Spec**        | N/A   |
| **UXR Requirements**   | N/A   |
| **Design Tokens**      | N/A   |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack

| Layer        | Technology                | Version |
| ------------ | ------------------------- | ------- |
| Frontend     | N/A                       | N/A     |
| Backend      | .NET ASP.NET Core Web API | 8.0     |
| Backend      | Entity Framework Core     | 8.x     |
| Database     | PostgreSQL                | 16.x    |
| Library      | PusherServer              | 5.x     |
| Library      | AutoMapper                | Latest  |
| AI/ML        | N/A                       | N/A     |
| Vector Store | N/A                       | N/A     |
| AI Gateway   | N/A                       | N/A     |
| Mobile       | N/A                       | N/A     |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)

| Reference Type           | Value |
| ------------------------ | ----- |
| **AI Impact**            | No    |
| **AIR Requirements**     | N/A   |
| **AI Pattern**           | N/A   |
| **Prompt Template Path** | N/A   |
| **Guardrails Config**    | N/A   |
| **Model Provider**       | N/A   |

> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement the backend API endpoints for Staff Arrival Marking functionality. This includes an appointment search endpoint filtered for today's date and patient query, and a mark-arrived endpoint that updates appointment status to "Arrived", records arrival time, and triggers Pusher event to add patient to the queue. All endpoints enforce Staff-only RBAC. The implementation includes comprehensive edge case handling (already arrived, cancelled appointment) and audit logging for all arrival marking operations.

## Dependent Tasks

- US_030 Task 002 (Queue Management API) - for Pusher event integration

## Impacted Components

- **NEW**: `src/backend/PatientAccess.Business/Services/ArrivalManagementService.cs` - Business logic for arrival operations
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ArrivalSearchResultDto.cs` - DTO for appointment search results
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IArrivalManagementService.cs` - Arrival service interface
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/StaffController.cs` - Add arrival endpoints (GET /api/staff/arrivals/search, POST /api/staff/arrivals/{id}/mark-arrived)
- **MODIFY**: `src/backend/PatientAccess.Business/Services/AppointmentService.cs` - Update appointment status to "Arrived" and record arrival time
- **MODIFY**: `src/backend/PatientAccess.Business/Services/AuditService.cs` - Log arrival marking actions

## Implementation Plan

1. **Create DTOs for Arrival Management**
   - Create `ArrivalSearchResultDto` with fields: AppointmentId, PatientId, PatientName, DateOfBirth, AppointmentDateTime, ProviderName, VisitReason, Status
   - Add validation attributes where applicable

2. **Implement Appointment Search Endpoint**
   - Create `IArrivalManagementService` interface with `SearchTodayAppointments(string query)` method
   - Implement `ArrivalManagementService`:
     - Query Appointments table WHERE DATE(ScheduledDatetime) = TODAY
     - Filter by patient name, email, or phone (LIKE %query%)
     - Include Patient and Provider data via EF Core joins
     - Return empty list if no results
     - Map to List<ArrivalSearchResultDto>
   - Create `GET /api/staff/arrivals/search?query={term}&date={today}` endpoint in StaffController
   - Apply [Authorize(Roles = "Staff")] attribute
   - Return List<ArrivalSearchResultDto> with 200 OK

3. **Implement Mark Arrived Endpoint**
   - Add `MarkAppointmentArrived(Guid appointmentId)` method to IArrivalManagementService
   - Implement in ArrivalManagementService:
     - Validate appointment exists (return 404 if not found)
     - Check appointment status:
       - If already "Arrived", return 409 Conflict with message "Patient already marked as arrived"
       - If "Cancelled", return 400 Bad Request with message "Appointment was cancelled"
       - If "Scheduled" or "Confirmed", proceed to mark arrived
     - Update Appointment status to "Arrived"
     - Record current timestamp in ArrivalTime field
     - Save to database using transaction
     - Inject IPusherService and trigger "patient-added" event to "queue-updates" channel
     - Log audit entry for arrival marking (user ID, appointment ID, timestamp)
   - Create `POST /api/staff/arrivals/{id}/mark-arrived` endpoint in StaffController
   - Apply [Authorize(Roles = "Staff")] attribute
   - Return updated ArrivalSearchResultDto with 200 OK

4. **Implement Edge Case Handling**
   - Handle null/empty search query - return empty list
   - Handle appointment not found - return 404 Not Found with message
   - Handle already arrived - return 409 Conflict with descriptive message
   - Handle cancelled appointment - return 400 Bad Request with descriptive message
   - Log all errors to structured logging (Seq/Application Insights)
   - Return ProblemDetails for 4xx/5xx responses

5. **Add RBAC Enforcement**
   - Apply [Authorize(Roles = "Staff")] to all arrival endpoints
   - Verify JWT token contains Staff role claim
   - Return 403 Forbidden if user is not Staff
   - Add audit logging for all arrival operations

6. **Integrate Audit Logging**
   - Log all arrival marking operations with:
     - User ID (Staff member who marked arrival)
     - Appointment ID
     - Patient ID
     - Action: "MarkArrived"
     - Timestamp
     - Result (success or failure with reason)
   - Use existing AuditService or create audit log entry directly

7. **Optimize Query Performance**
   - Add database index on Appointments(ScheduledDatetime, Status) for fast today's appointment queries
   - Ensure search query executes in < 200ms

8. **Add Unit Tests**
   - Write unit tests for ArrivalManagementService.SearchTodayAppointments (mock EF context)
   - Write unit tests for ArrivalManagementService.MarkAppointmentArrived with edge case scenarios (already arrived, cancelled)

## Current Project State

```
src/backend/PatientAccess.Web/
├── Controllers/
│   ├── AppointmentsController.cs            # Existing appointment endpoints
│   ├── StaffController.cs                   # Existing staff endpoints (walk-in, queue)
│   └── ...
└── Program.cs                               # Startup configuration

src/backend/PatientAccess.Business/
├── Services/
│   ├── AppointmentService.cs                # Appointment business logic
│   ├── PatientService.cs                    # Patient business logic
│   ├── QueueManagementService.cs            # Queue management (from US_030)
│   ├── PusherService.cs                     # Pusher event broadcasting (from US_030)
│   ├── AuditService.cs                      # Audit logging
│   └── ...
├── Interfaces/
│   ├── IAppointmentService.cs               # Appointment service interface
│   ├── IPusherService.cs                    # Pusher service interface (from US_030)
│   └── ...
└── DTOs/                                    # Data transfer objects

Database Schema:
├── Appointments (patient_id, provider_id, status, scheduled_datetime, arrival_time)
├── Patients (user_id FK, name, contact info)
├── Providers (name, specialty)
└── AuditLogs (user_id, action_type, resource_id, timestamp, action_details)
```

## Expected Changes

| Action | File Path                                                                  | Description                                                                                                              |
| ------ | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| CREATE | src/backend/PatientAccess.Business/DTOs/ArrivalSearchResultDto.cs          | DTO for arrival search results (AppointmentId, PatientName, DOB, AppointmentDateTime, ProviderName, VisitReason, Status) |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IArrivalManagementService.cs | Interface for arrival management methods (SearchTodayAppointments, MarkAppointmentArrived)                               |
| CREATE | src/backend/PatientAccess.Business/Services/ArrivalManagementService.cs    | Arrival management business logic with Pusher integration for queue updates                                              |
| MODIFY | src/backend/PatientAccess.Web/Controllers/StaffController.cs               | Add GET /api/staff/arrivals/search and POST /api/staff/arrivals/{id}/mark-arrived endpoints with Staff RBAC              |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs          | Add ArrivalTime field update when status changes to "Arrived"                                                            |

## External References

- Entity Framework Core DateTime Filtering: https://learn.microsoft.com/en-us/ef/core/querying/filters
- ASP.NET Core ProblemDetails: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails
- Pusher Server .NET SDK: https://github.com/pusher/pusher-http-dotnet
- .NET 8 DateOnly/TimeOnly: https://learn.microsoft.com/en-us/dotnet/api/system.dateonly?view=net-8.0

## Build Commands

- `dotnet build src/backend/PatientAccess.sln` - Build solution
- `dotnet run --project src/backend/PatientAccess.Web/PatientAccess.Web.csproj` - Run API locally
- `dotnet test src/backend/PatientAccess.Tests/PatientAccess.Tests.csproj` - Run unit tests
- `dotnet ef migrations add ArrivalManagementIndices --project src/backend/PatientAccess.Web` - Create migration for arrival indices
- `dotnet ef database update --project src/backend/PatientAccess.Web` - Apply migration

## Implementation Validation Strategy

- [ ] Unit tests pass for ArrivalManagementService.SearchTodayAppointments
- [ ] Unit tests pass for ArrivalManagementService.MarkAppointmentArrived with edge case scenarios
- [ ] Integration tests pass for arrival search endpoint (verify today's date filter)
- [ ] Integration tests pass for mark-arrived endpoint (verify status update, arrival time, Pusher event)
- [ ] Edge case handling verified:
  - [ ] Already arrived returns 409 Conflict
  - [ ] Cancelled appointment returns 400 Bad Request
  - [ ] Invalid appointment ID returns 404 Not Found
- [ ] Pusher event "patient-added" successfully broadcast to "queue-updates" channel
- [ ] RBAC enforcement verified - non-Staff users receive 403 Forbidden
- [ ] Database index on Appointments(ScheduledDatetime, Status) created and used
- [ ] Query performance measured - search < 200ms
- [ ] Audit logs capture all arrival marking operations with user ID and timestamp

## Implementation Checklist

- [ ] Create ArrivalSearchResultDto with validation attributes
- [ ] Create IArrivalManagementService interface with SearchTodayAppointments and MarkAppointmentArrived methods
- [ ] Implement ArrivalManagementService.SearchTodayAppointments:
  - [ ] Query Appointments WHERE DATE(ScheduledDatetime) = TODAY
  - [ ] Filter by patient name, email, or phone (LIKE %query%)
  - [ ] Include Patient and Provider via EF Core joins
  - [ ] Map to List<ArrivalSearchResultDto>
  - [ ] Return empty list if no results
- [ ] Implement ArrivalManagementService.MarkAppointmentArrived:
  - [ ] Validate appointment exists (404 if not found)
  - [ ] Check status: If "Arrived", return 409 Conflict
  - [ ] Check status: If "Cancelled", return 400 Bad Request
  - [ ] Update status to "Arrived"
  - [ ] Record current timestamp in ArrivalTime field
  - [ ] Save to database using transaction
  - [ ] Trigger Pusher "patient-added" event to "queue-updates" channel
  - [ ] Log audit entry (user ID, appointment ID, action, timestamp)
- [ ] Create GET /api/staff/arrivals/search endpoint in StaffController with [Authorize(Roles = "Staff")]
- [ ] Create POST /api/staff/arrivals/{id}/mark-arrived endpoint in StaffController with [Authorize(Roles = "Staff")]
- [ ] Add database index on Appointments(ScheduledDatetime, Status) via migration
- [ ] Implement error handling for null/empty search query (return empty list)
- [ ] Implement error handling for appointment not found (404 with message)
- [ ] Implement error handling for already arrived (409 Conflict with descriptive message)
- [ ] Implement error handling for cancelled appointment (400 Bad Request with descriptive message)
- [ ] Add structured logging for all arrival operations
- [ ] Write unit tests for SearchTodayAppointments (mock EF context, verify date filter)
- [ ] Write unit tests for MarkAppointmentArrived (mock EF context, test already-arrived scenario)
- [ ] Write unit tests for MarkAppointmentArrived (test cancelled appointment scenario)
- [ ] Write integration tests for arrival search endpoint (verify results)
- [ ] Write integration tests for mark-arrived endpoint (verify status, arrival time, Pusher event)
- [ ] Verify RBAC enforcement with role-based test cases
- [ ] Measure query performance for SearchTodayAppointments (< 200ms target)
- [ ] Test Pusher event broadcasting using Pusher dashboard debug console
