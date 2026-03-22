# Task - task_002_be_queue_management_api

## Requirement Reference

- User Story: US_030 - Same-Day Queue Management Interface
- Story Location: .propel/context/tasks/EP-003/us_030/us_030.md
- Acceptance Criteria:
  - AC-1: Given I am on the Queue Management page, When the page loads, Then same-day patients are displayed in chronological order (arrival time) with patient name, appointment type, provider, arrival time, and estimated wait time.
  - AC-2: Given new patients are added to the queue, When a walk-in or arrival is registered, Then the queue updates in real-time via Pusher Channels without requiring page refresh.
  - AC-3: Given I need to adjust priority, When I flag a patient for priority (e.g., emergency), Then the patient moves to the appropriate position in the queue and wait times recalculate.
  - AC-4: Given the queue is empty, When no patients are waiting, Then an empty state illustration displays with a guiding CTA (e.g., "No patients in queue. Book a walk-in?").
- Edge Case:
  - What happens when the Pusher connection drops? Queue should periodically poll (every 30 seconds) as fallback and display a "Live updates paused" indicator.
  - How does the system handle queue for multiple providers? Queue can be filtered by provider with a default "All Providers" view.

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

Implement the backend API endpoints and Pusher Channels integration for Staff Queue Management functionality. This includes a queue retrieval endpoint returning same-day patients with status "Arrived" in chronological order, a priority update endpoint for flagging emergency patients, and Pusher event triggers for real-time queue updates. All endpoints enforce Staff-only RBAC. The implementation integrates Pusher Server SDK to broadcast "patient-added", "patient-removed", and "priority-changed" events to connected clients, enabling real-time queue synchronization without polling.

## Dependent Tasks

- None (database schema and Appointments table already exist)

## Impacted Components

- **NEW**: `src/backend/PatientAccess.Business/Services/QueueManagementService.cs` - Business logic for queue operations
- **NEW**: `src/backend/PatientAccess.Business/DTOs/QueuePatientDto.cs` - DTO for queue patient data
- **NEW**: `src/backend/PatientAccess.Business/DTOs/UpdatePriorityDto.cs` - DTO for priority flag update request
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IQueueManagementService.cs` - Queue service interface
- **NEW**: `src/backend/PatientAccess.Business/Services/PusherService.cs` - Pusher event broadcasting service
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IPusherService.cs` - Pusher service interface
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/StaffController.cs` - Add queue endpoints (GET /api/staff/queue, PATCH /api/staff/queue/{id}/priority)
- **MODIFY**: `src/backend/PatientAccess.Business/Services/AppointmentService.cs` - Trigger Pusher event on appointment arrival status change
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register PusherService as singleton, configure Pusher credentials from appsettings

## Implementation Plan

1. **Install and Configure Pusher Server SDK**
   - Install PusherServer NuGet package: `dotnet add package PusherServer`
   - Add Pusher configuration to `appsettings.json`: AppId, Key, Secret, Cluster
   - Create `IPusherService` interface with `TriggerEvent(string channel, string eventName, object data)` method
   - Implement `PusherService` wrapping Pusher client initialization and event triggering
   - Register `PusherService` as singleton in Program.cs with configuration binding

2. **Create DTOs for Queue Management**
   - Create `QueuePatientDto` with fields: Id, PatientId, PatientName, AppointmentType, ProviderName, ArrivalTime, EstimatedWaitTime, IsPriority
   - Create `UpdatePriorityDto` with field: IsPriority (boolean)
   - Add validation attributes where applicable

3. **Implement Queue Retrieval Endpoint**
   - Create `IQueueManagementService` interface with `GetSameDayQueue(Guid? providerId)` method
   - Implement `QueueManagementService`:
     - Query Appointments table for today's date with Status = "Arrived"
     - Optional filter by ProviderId if provided
     - ORDER BY IsPriority DESC, ArrivalTime ASC (priority patients first, then chronological)
     - Join with Patients and Providers tables to get names
     - Calculate EstimatedWaitTime: current time - arrival time (in minutes)
     - Map to List<QueuePatientDto>
   - Create `GET /api/staff/queue?providerId={guid}` endpoint in StaffController
   - Apply [Authorize(Roles = "Staff")] attribute
   - Return List<QueuePatientDto> with 200 OK

4. **Implement Priority Update Endpoint**
   - Add `UpdatePatientPriority(Guid appointmentId, bool isPriority)` method to IQueueManagementService
   - Implement in QueueManagementService:
     - Validate appointment exists and status is "Arrived"
     - Update Appointment.IsPriority field
     - Save to database
     - Trigger Pusher event "priority-changed" with updated queue patient data
     - Log audit entry for priority change
   - Create `PATCH /api/staff/queue/{id}/priority` endpoint in StaffController
   - Apply [Authorize(Roles = "Staff")] attribute
   - Accept UpdatePriorityDto in request body
   - Return updated QueuePatientDto with 200 OK

5. **Integrate Pusher Event Broadcasting**
   - Inject IPusherService into AppointmentService and QueueManagementService
   - Modify AppointmentService.UpdateAppointmentStatus:
     - When status changes to "Arrived", trigger Pusher event "patient-added" with queue patient data
     - Channel: "queue-updates"
   - Implement Pusher trigger in QueueManagementService.UpdatePatientPriority:
     - After priority update, trigger "priority-changed" event with updated queue patient data
     - Channel: "queue-updates"
   - Add Pusher event "patient-removed" trigger when appointment status changes from "Arrived" to "Completed" or "Cancelled"

6. **Implement Error Handling and Edge Cases**
   - Handle invalid appointment ID - return 404 Not Found
   - Handle appointment not in "Arrived" status - return 400 Bad Request
   - Handle Pusher API failures - log error but don't fail request (graceful degradation)
   - Log all errors to structured logging

7. **Add Database Index for Performance**
   - Add index on Appointments(Status, ArrivalTime) for fast same-day queue queries
   - Ensure query executes in < 100ms for queue retrieval

8. **Add Unit Tests**
   - Write unit tests for QueueManagementService.GetSameDayQueue (mock EF context)
   - Write unit tests for QueueManagementService.UpdatePatientPriority
   - Write unit tests for PusherService event triggering (mock Pusher client)

## Current Project State

```
src/backend/PatientAccess.Web/
├── Controllers/
│   ├── AppointmentsController.cs            # Existing appointment endpoints
│   ├── StaffController.cs                   # Existing staff endpoints (walk-in booking)
│   └── ...
└── Program.cs                               # Startup configuration

src/backend/PatientAccess.Business/
├── Services/
│   ├── AppointmentService.cs                # Appointment business logic
│   ├── PatientService.cs                    # Patient business logic
│   └── ...
├── Interfaces/
│   ├── IAppointmentService.cs               # Appointment service interface
│   └── ...
└── DTOs/                                    # Data transfer objects

Database Schema:
├── Appointments (patient_id, provider_id, status, is_walkin, is_priority, arrival_time, scheduled_datetime)
├── Patients (user_id FK, name, contact info)
└── Providers (name, specialty)
```

## Expected Changes

| Action | File Path                                                                | Description                                                                                                        |
| ------ | ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------ |
| CREATE | src/backend/PatientAccess.Business/DTOs/QueuePatientDto.cs               | DTO for queue patient (Id, PatientName, AppointmentType, ProviderName, ArrivalTime, EstimatedWaitTime, IsPriority) |
| CREATE | src/backend/PatientAccess.Business/DTOs/UpdatePriorityDto.cs             | DTO for priority update request (IsPriority boolean)                                                               |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IQueueManagementService.cs | Interface for queue management methods (GetSameDayQueue, UpdatePatientPriority)                                    |
| CREATE | src/backend/PatientAccess.Business/Services/QueueManagementService.cs    | Queue management business logic with Pusher integration                                                            |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IPusherService.cs          | Interface for Pusher event broadcasting (TriggerEvent method)                                                      |
| CREATE | src/backend/PatientAccess.Business/Services/PusherService.cs             | Pusher client wrapper for event broadcasting to "queue-updates" channel                                            |
| MODIFY | src/backend/PatientAccess.Web/Controllers/StaffController.cs             | Add GET /api/staff/queue and PATCH /api/staff/queue/{id}/priority endpoints with Staff RBAC                        |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs        | Trigger Pusher "patient-added" event when status changes to "Arrived"                                              |
| MODIFY | src/backend/PatientAccess.Web/Program.cs                                 | Register PusherService as singleton with configuration from appsettings.json                                       |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json                           | Add Pusher configuration section (AppId, Key, Secret, Cluster)                                                     |

## External References

- Pusher Server .NET SDK: https://github.com/pusher/pusher-http-dotnet
- Pusher Channels Documentation: https://pusher.com/docs/channels/server_api/http-api/
- Entity Framework Core Include/Join: https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager
- ASP.NET Core Configuration: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0
- Free Pusher Plan: https://pusher.com/channels/pricing (200K messages/day, 100 concurrent connections)

## Build Commands

- `dotnet add src/backend/PatientAccess.Business/PatientAccess.Business.csproj package PusherServer` - Install Pusher SDK
- `dotnet build src/backend/PatientAccess.sln` - Build solution
- `dotnet run --project src/backend/PatientAccess.Web/PatientAccess.Web.csproj` - Run API locally
- `dotnet test src/backend/PatientAccess.Tests/PatientAccess.Tests.csproj` - Run unit tests
- `dotnet ef migrations add QueueManagementIndices --project src/backend/PatientAccess.Web` - Create migration for queue indices
- `dotnet ef database update --project src/backend/PatientAccess.Web` - Apply migration

## Implementation Validation Strategy

- [ ] Unit tests pass for QueueManagementService.GetSameDayQueue with provider filter scenarios
- [ ] Unit tests pass for QueueManagementService.UpdatePatientPriority
- [ ] Integration tests pass for queue retrieval endpoint (verify chronological order and priority sorting)
- [ ] Integration tests pass for priority update endpoint (verify Pusher event triggered)
- [ ] Pusher events successfully broadcast to "queue-updates" channel (verify with Pusher dashboard)
- [ ] RBAC enforcement verified - non-Staff users receive 403 Forbidden
- [ ] Database index on Appointments(Status, ArrivalTime) created and used by query optimizer
- [ ] Query performance measured - queue retrieval < 100ms
- [ ] Error handling returns ProblemDetails for 4xx/5xx responses
- [ ] Graceful degradation - API succeeds even if Pusher API fails (logged error)

## Implementation Checklist

- [ ] Install PusherServer NuGet package
- [ ] Add Pusher configuration section to appsettings.json (AppId, Key, Secret, Cluster)
- [ ] Create IPusherService interface with TriggerEvent method
- [ ] Implement PusherService class:
  - [ ] Initialize Pusher client with configuration
  - [ ] Implement TriggerEvent method wrapping pusher.TriggerAsync
  - [ ] Add error handling and logging for Pusher failures
- [ ] Register PusherService as singleton in Program.cs with IOptions binding
- [ ] Create QueuePatientDto with validation attributes
- [ ] Create UpdatePriorityDto with validation attributes
- [ ] Create IQueueManagementService interface with GetSameDayQueue and UpdatePatientPriority methods
- [ ] Implement QueueManagementService.GetSameDayQueue:
  - [ ] Query Appointments WHERE Status = "Arrived" AND DATE(ScheduledDatetime) = TODAY
  - [ ] Optional filter by ProviderId if provided
  - [ ] Include Patients and Providers via EF Core Include/Join
  - [ ] ORDER BY IsPriority DESC, ArrivalTime ASC
  - [ ] Calculate EstimatedWaitTime (current time - arrival time in minutes)
  - [ ] Map to List<QueuePatientDto>
- [ ] Implement QueueManagementService.UpdatePatientPriority:
  - [ ] Validate appointment exists and Status = "Arrived"
  - [ ] Update Appointment.IsPriority field
  - [ ] Save to database
  - [ ] Trigger Pusher "priority-changed" event
  - [ ] Log audit entry
- [ ] Create GET /api/staff/queue endpoint in StaffController with [Authorize(Roles = "Staff")]
- [ ] Create PATCH /api/staff/queue/{id}/priority endpoint in StaffController with [Authorize(Roles = "Staff")]
- [ ] Modify AppointmentService.UpdateAppointmentStatus:
  - [ ] Inject IPusherService
  - [ ] Trigger "patient-added" event when status changes to "Arrived"
  - [ ] Trigger "patient-removed" event when status changes from "Arrived" to "Completed"/"Cancelled"
- [ ] Add database index on Appointments(Status, ArrivalTime) via migration
- [ ] Implement error handling for invalid appointment ID (404 Not Found)
- [ ] Implement error handling for invalid appointment status (400 Bad Request)
- [ ] Implement graceful degradation for Pusher failures (log error, don't fail request)
- [ ] Write unit tests for GetSameDayQueue (mock EF context, verify ordering)
- [ ] Write unit tests for UpdatePatientPriority (mock EF context and Pusher service)
- [ ] Write unit tests for PusherService.TriggerEvent (mock Pusher client)
- [ ] Write integration tests for queue endpoint (verify data and sorting)
- [ ] Write integration tests for priority update endpoint (verify Pusher event)
- [ ] Test Pusher event broadcasting using Pusher dashboard debug console
- [ ] Verify RBAC enforcement with role-based test cases
- [ ] Measure query performance for GetSameDayQueue (< 100ms target)
