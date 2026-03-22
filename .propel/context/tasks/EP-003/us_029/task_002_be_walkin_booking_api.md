# Task - task_002_be_walkin_booking_api

## Requirement Reference

- User Story: US_029 - Staff Walk-in Booking
- Story Location: .propel/context/tasks/EP-003/us_029/us_029.md
- Acceptance Criteria:
  - AC-1: Given I am a Staff user on the Walk-in Booking page, When I search for an existing patient, Then the system returns matching records by name, email, or phone within 300ms.
  - AC-2: Given the walk-in patient is not registered, When I click "Create New Patient", Then a minimal registration form appears (name, DOB, phone) and the account is created with status "active" — patient completes full registration later.
  - AC-3: Given I have identified the patient, When I select a provider and available time slot and enter the visit reason, Then the appointment is created with a "Walk-in" flag and an optional confirmation is sent if contact info is available.
  - AC-4: Given there is no same-day availability, When I attempt to book a walk-in, Then the system offers to add the patient to the same-day queue (UC-012).
- Edge Case:
  - What happens when trying to create a patient with an existing email? System displays match and allows Staff to select the existing record instead.
  - How does the system handle walk-in booking for a patient-only-restricted feature? Walk-in booking is exclusively Staff-accessible; patient role users cannot access this page.

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
| Library      | BCrypt.Net-Next           | Latest  |
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

Implement the backend API endpoints and business logic for Staff Walk-in Booking functionality. This includes a patient search endpoint with fast query performance (< 300ms), a minimal patient creation endpoint for walk-in scenarios, and a walk-in appointment booking endpoint with the "IsWalkin" flag. All endpoints enforce Staff-only RBAC via [Authorize(Roles = "Staff")] attributes. The implementation leverages existing database schema (Users, Patients, Appointments, TimeSlots) and follows the three-layer architecture (Controller → Service → Data Access).

## Dependent Tasks

- None (database schema already exists from prior setup)

## Impacted Components

- **NEW**: `src/backend/PatientAccess.Web/Controllers/StaffController.cs` - Staff-specific endpoints for patient search, patient creation
- **NEW**: `src/backend/PatientAccess.Business/Services/WalkinBookingService.cs` - Business logic for walk-in appointment creation
- **NEW**: `src/backend/PatientAccess.Business/DTOs/PatientSearchResultDto.cs` - DTO for patient search results
- **NEW**: `src/backend/PatientAccess.Business/DTOs/CreateMinimalPatientDto.cs` - DTO for minimal patient creation
- **NEW**: `src/backend/PatientAccess.Business/DTOs/CreateWalkinAppointmentDto.cs` - DTO for walk-in booking request
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs` - Add walk-in booking endpoint or extend existing
- **MODIFY**: `src/backend/PatientAccess.Business/Interfaces/IPatientService.cs` - Add SearchPatients and CreateMinimalPatient methods
- **MODIFY**: `src/backend/PatientAccess.Business/Services/PatientService.cs` - Implement patient search and minimal creation logic
- **MODIFY**: `src/backend/PatientAccess.Business/Interfaces/IAppointmentService.cs` - Add CreateWalkinAppointment method
- **MODIFY**: `src/backend/PatientAccess.Business/Services/AppointmentService.cs` - Implement walk-in booking logic

## Implementation Plan

1. **Create DTOs for Walk-in Booking**
   - Create `PatientSearchResultDto` with fields: Id, FullName, DateOfBirth, Email, Phone, LastAppointmentDate
   - Create `CreateMinimalPatientDto` with fields: FirstName, LastName, DateOfBirth, Phone, Email (optional)
   - Create `CreateWalkinAppointmentDto` with fields: PatientId, ProviderId, TimeSlotId, VisitReason, IsWalkin (default true)
   - Add validation attributes (Required, EmailAddress, Phone, MaxLength)

2. **Implement Patient Search Endpoint**
   - Add `SearchPatients(string query)` method to `IPatientService` interface
   - Implement search in `PatientService` using EF Core LINQ query:
     - WHERE (Name LIKE %query% OR Email LIKE %query% OR Phone LIKE %query%)
     - Include last appointment date via LEFT JOIN
     - Order by relevance (exact match first, then partial)
     - Limit to top 20 results
   - Add index on Users.Email, Users.Name, Patients.Phone for performance (< 300ms)
   - Create `GET /api/staff/patients/search?query={term}` endpoint in StaffController
   - Apply [Authorize(Roles = "Staff")] attribute
   - Return List<PatientSearchResultDto> with 200 OK

3. **Implement Minimal Patient Creation Endpoint**
   - Add `CreateMinimalPatient(CreateMinimalPatientDto dto)` method to `IPatientService`
   - Implement in `PatientService`:
     - Check if email already exists (if provided) - return existing patient if found
     - Create User entity with role "Patient", status "Active", empty password hash (patient sets later)
     - Create Patient entity linked to User
     - Save to database using transaction
     - Log audit entry for patient creation
   - Create `POST /api/staff/patients` endpoint in StaffController
   - Apply [Authorize(Roles = "Staff")] attribute
   - Return PatientSearchResultDto with 201 Created or existing patient with 200 OK if email duplicate

4. **Implement Walk-in Booking Endpoint**
   - Add `CreateWalkinAppointment(CreateWalkinAppointmentDto dto)` method to `IAppointmentService`
   - Implement in `AppointmentService`:
     - Validate PatientId exists
     - Validate ProviderId exists
     - Validate TimeSlotId exists and status is "Available"
     - Update TimeSlot status to "Booked"
     - Create Appointment with IsWalkin = true, Status = "Arrived" (default for walk-ins)
     - Calculate no-show risk score (set to 0 for walk-ins as immediate arrival expected)
     - Save to database using transaction
     - Trigger optional confirmation email/SMS if patient contact info available
     - Log audit entry for walk-in appointment creation
   - Create `POST /api/appointments/walkin` endpoint in AppointmentsController or StaffController
   - Apply [Authorize(Roles = "Staff")] attribute
   - Return AppointmentDto with 201 Created

5. **Implement Error Handling and Edge Cases**
   - Handle null/empty search query - return empty list
   - Handle duplicate patient email - return existing patient instead of creating
   - Handle slot no longer available (concurrent booking) - return 409 Conflict
   - Handle invalid patient/provider/slot IDs - return 404 Not Found with descriptive message
   - Log all errors to structured logging (Seq/Application Insights)
   - Return ProblemDetails for 4xx/5xx responses

6. **Add RBAC Enforcement**
   - Apply [Authorize(Roles = "Staff")] to all walk-in booking endpoints
   - Verify JWT token contains Staff role claim
   - Return 403 Forbidden if user is not Staff
   - Add audit logging for all walk-in booking operations

7. **Optimize Query Performance**
   - Add database index on Users(Email, Name) for fast search
   - Add database index on Patients(Phone) for phone search
   - Use compiled queries for repeated search patterns
   - Implement caching for provider availability if needed
   - Measure query execution time - ensure < 300ms for search

## Current Project State

```
src/backend/PatientAccess.Web/
├── Controllers/
│   ├── AppointmentsController.cs            # Existing appointment endpoints
│   ├── AuthController.cs                    # Authentication
│   ├── PatientController.cs                 # Patient endpoints
│   ├── ProvidersController.cs               # Provider endpoints
│   └── StaffController.cs                   # Existing staff endpoints
└── Program.cs                               # Startup configuration

src/backend/PatientAccess.Business/
├── Services/
│   ├── AppointmentService.cs                # Appointment business logic
│   ├── PatientService.cs                    # Patient business logic
│   └── UserService.cs                       # User management
├── Interfaces/
│   ├── IAppointmentService.cs               # Appointment service interface
│   ├── IPatientService.cs                   # Patient service interface
│   └── IUserService.cs                      # User service interface
└── DTOs/                                    # Data transfer objects

Database Schema (PostgreSQL):
├── Users (email, password_hash, role, is_active)
├── Patients (user_id FK, emergency_contact, insurance)
├── Providers (name, specialty, availability_schedule)
├── TimeSlots (provider_id FK, start_time, end_time, status)
└── Appointments (patient_id FK, provider_id FK, time_slot_id FK, status, is_walkin, visit_reason)
```

## Expected Changes

| Action | File Path                                                             | Description                                                                                   |
| ------ | --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| CREATE | src/backend/PatientAccess.Business/DTOs/PatientSearchResultDto.cs     | DTO for patient search results (Id, FullName, DOB, Email, Phone, LastAppointmentDate)         |
| CREATE | src/backend/PatientAccess.Business/DTOs/CreateMinimalPatientDto.cs    | DTO for minimal patient creation (FirstName, LastName, DOB, Phone, Email optional)            |
| CREATE | src/backend/PatientAccess.Business/DTOs/CreateWalkinAppointmentDto.cs | DTO for walk-in appointment creation (PatientId, ProviderId, TimeSlotId, VisitReason)         |
| CREATE | src/backend/PatientAccess.Business/Services/WalkinBookingService.cs   | Business logic service for walk-in booking orchestration                                      |
| MODIFY | src/backend/PatientAccess.Web/Controllers/StaffController.cs          | Add SearchPatients and CreateMinimalPatient endpoints with Staff RBAC                         |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs   | Add CreateWalkinAppointment endpoint with Staff RBAC                                          |
| MODIFY | src/backend/PatientAccess.Business/Interfaces/IPatientService.cs      | Add SearchPatients and CreateMinimalPatient method signatures                                 |
| MODIFY | src/backend/PatientAccess.Business/Services/PatientService.cs         | Implement patient search (< 300ms) and minimal patient creation with duplicate email handling |
| MODIFY | src/backend/PatientAccess.Business/Interfaces/IAppointmentService.cs  | Add CreateWalkinAppointment method signature                                                  |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs     | Implement walk-in appointment creation with IsWalkin flag and optional notifications          |

## External References

- .NET 8 Web API Documentation: https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0
- Entity Framework Core Query Performance: https://learn.microsoft.com/en-us/ef/core/performance/
- ASP.NET Core Authorization: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-8.0
- PostgreSQL Full-Text Search: https://www.postgresql.org/docs/16/textsearch.html
- AutoMapper Documentation: https://docs.automapper.org/en/stable/
- BCrypt.Net-Next: https://github.com/BcryptNet/bcrypt.net

## Build Commands

- `dotnet build src/backend/PatientAccess.sln` - Build solution
- `dotnet run --project src/backend/PatientAccess.Web/PatientAccess.Web.csproj` - Run API locally
- `dotnet test src/backend/PatientAccess.Tests/PatientAccess.Tests.csproj` - Run unit tests
- `dotnet ef migrations add WalkinBookingIndices --project src/backend/PatientAccess.Web` - Create migration for search indices
- `dotnet ef database update --project src/backend/PatientAccess.Web` - Apply migration

## Implementation Validation Strategy

- [ ] Unit tests pass for WalkinBookingService, PatientService search, and AppointmentService walk-in creation
- [ ] Integration tests pass for patient search endpoint (< 300ms response time)
- [ ] Integration tests pass for minimal patient creation with duplicate email handling
- [ ] Integration tests pass for walk-in appointment creation with IsWalkin flag
- [ ] RBAC enforcement verified - non-Staff users receive 403 Forbidden on all endpoints
- [ ] Database indices created for Users(Email, Name) and Patients(Phone)
- [ ] Query performance measured - patient search < 300ms under load (100 concurrent requests)
- [ ] Error handling returns ProblemDetails for all 4xx/5xx responses
- [ ] Audit logs capture all walk-in booking operations (search, creation, booking)
- [ ] Concurrent slot booking conflict handled correctly (409 Conflict returned)

## Implementation Checklist

- [ ] Create PatientSearchResultDto with validation attributes
- [ ] Create CreateMinimalPatientDto with validation attributes (Required, EmailAddress, Phone)
- [ ] Create CreateWalkinAppointmentDto with validation attributes
- [ ] Add SearchPatients method signature to IPatientService interface
- [ ] Implement SearchPatients in PatientService with EF Core LINQ:
  - [ ] Search by Name, Email, Phone (LIKE %query%)
  - [ ] Include last appointment date via LEFT JOIN
  - [ ] Order by relevance (exact match first)
  - [ ] Limit to top 20 results
- [ ] Add CreateMinimalPatient method signature to IPatientService interface
- [ ] Implement CreateMinimalPatient in PatientService:
  - [ ] Check email uniqueness (return existing if found)
  - [ ] Create User with role "Patient", status "Active"
  - [ ] Create Patient entity linked to User
  - [ ] Use transaction for atomicity
  - [ ] Log audit entry
- [ ] Add CreateWalkinAppointment method signature to IAppointmentService interface
- [ ] Implement CreateWalkinAppointment in AppointmentService:
  - [ ] Validate PatientId, ProviderId, TimeSlotId exist
  - [ ] Check TimeSlot availability (status = "Available")
  - [ ] Update TimeSlot status to "Booked"
  - [ ] Create Appointment with IsWalkin = true, Status = "Arrived"
  - [ ] Set no-show risk score to 0 for walk-ins
  - [ ] Trigger optional confirmation notification
  - [ ] Use transaction for atomicity
  - [ ] Log audit entry
- [ ] Create GET /api/staff/patients/search endpoint in StaffController with [Authorize(Roles = "Staff")]
- [ ] Create POST /api/staff/patients endpoint in StaffController with [Authorize(Roles = "Staff")]
- [ ] Create POST /api/appointments/walkin endpoint in AppointmentsController with [Authorize(Roles = "Staff")]
- [ ] Add database index on Users(Email, Name) for search performance
- [ ] Add database index on Patients(Phone) for phone search
- [ ] Implement error handling for null/empty search query (return empty list)
- [ ] Implement duplicate email handling (return existing patient with 200 OK)
- [ ] Implement slot conflict handling (return 409 Conflict)
- [ ] Implement 404 Not Found for invalid IDs with descriptive messages
- [ ] Add structured logging for all walk-in operations
- [ ] Write unit tests for PatientService.SearchPatients (mock EF context)
- [ ] Write unit tests for PatientService.CreateMinimalPatient (duplicate email scenario)
- [ ] Write unit tests for AppointmentService.CreateWalkinAppointment (slot conflict scenario)
- [ ] Write integration tests for patient search endpoint (measure response time < 300ms)
- [ ] Write integration tests for walk-in booking endpoint (verify IsWalkin flag)
- [ ] Verify RBAC enforcement with role-based test cases (Patient role should get 403)
