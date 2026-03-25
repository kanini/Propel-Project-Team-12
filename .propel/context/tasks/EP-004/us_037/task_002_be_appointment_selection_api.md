# Task - task_002_be_appointment_selection_api

## Requirement Reference

- User Story: us_037
- Story Location: .propel/context/tasks/EP-004/us_037/us_037.md
- Acceptance Criteria:
  - AC-1: API returns list of patient's upcoming appointments requiring intake with provider name, date, time, and intake status
  - AC-2: API excludes appointments that don't require intake or have passed
  - AC-3: API includes intake session ID for appointments with in-progress or completed intake
  - AC-4: API returns empty array when patient has no appointments requiring intake
- Edge Cases:
  - Past appointments with incomplete intake: Not included in response
  - Appointments without intake requirement flag: Not included in response

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

## Applicable Technology Stack

| Layer    | Technology                                        | Version                                       |
| -------- | ------------------------------------------------- | --------------------------------------------- |
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x |
| Backend  | .NET 8 ASP.NET Core Web API                       | .NET 8.0                                      |
| Database | PostgreSQL                                        | 14+                                           |
| ORM      | Entity Framework Core                             | 8.0                                           |
| Caching  | IMemoryCache                                      | N/A                                           |

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

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

## Task Overview

Implement the Appointment Selection API endpoint for retrieving a patient's upcoming appointments that require intake. This task creates the GET `/api/intake/appointments` endpoint that returns a list of appointments with their intake status (pending/in-progress/completed). The endpoint queries the Appointment and IntakeRecord tables, filters for future appointments with `RequiresIntake = true`, joins with Provider data for provider details, and includes intake session information if an IntakeRecord exists. The endpoint is secured with JWT authentication and returns only appointments belonging to the authenticated patient. Response is cached for 2 minutes to reduce database load for frequent polling.

## Dependent Tasks

- None (this is an entry point; it depends on existing Appointment and Provider tables)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/DTOs/IntakeAppointmentDto.cs` — Response DTO for appointment with intake status
- **NEW** `src/backend/PatientAccess.Business/Interfaces/IIntakeAppointmentService.cs` — Service interface for intake appointment operations
- **NEW** `src/backend/PatientAccess.Business/Services/IntakeAppointmentService.cs` — Business logic for fetching intake appointments
- **NEW** `src/backend/PatientAccess.Web/Controllers/IntakeAppointmentController.cs` — API controller for intake appointment endpoints
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register IIntakeAppointmentService in DI container
- **NEW** `src/backend/PatientAccess.Tests/Services/IntakeAppointmentServiceTests.cs` — Unit tests for IntakeAppointmentService

## Implementation Plan

1. **Create IntakeAppointmentDto** (`DTOs/IntakeAppointmentDto.cs`): Define DTO with properties: `int AppointmentId`, `int ProviderId`, `string ProviderName`, `string ProviderSpecialty`, `DateTime AppointmentDate`, `TimeSpan AppointmentTime`, `string IntakeStatus` (Pending/InProgress/Completed), `int? IntakeSessionId`. Add XML documentation comments for each property.

2. **Create IIntakeAppointmentService interface** (`Interfaces/IIntakeAppointmentService.cs`): Define method signature `Task<List<IntakeAppointmentDto>> GetPatientIntakeAppointmentsAsync(int patientId, CancellationToken cancellationToken = default)`. Add XML documentation describing the method's purpose, parameters, return value, and exceptions (throws `UnauthorizedException` if patientId mismatch).

3. **Implement IntakeAppointmentService** (`Services/IntakeAppointmentService.cs`): Inject `ApplicationDbContext` and `IMemoryCache`. Implement `GetPatientIntakeAppointmentsAsync`:
   - Check cache for key `intake_appointments_{patientId}` with 2-minute sliding expiration. Return cached data if available.
   - Query appointments: `context.Appointments.Include(a => a.Provider).Where(a => a.PatientId == patientId && a.AppointmentDate >= DateTime.UtcNow && a.RequiresIntake == true)`.
   - Query intake records: `context.IntakeRecords.Where(ir => ir.PatientId == patientId)`.
   - Join appointments with intake records on `AppointmentId`.
   - Map to `IntakeAppointmentDto[]`. For each appointment, determine `IntakeStatus`:
     - If no IntakeRecord exists: "Pending"
     - If IntakeRecord.Status == "InProgress": "InProgress"
     - If IntakeRecord.Status == "Completed": "Completed"
   - Order by `AppointmentDate` ascending.
   - Cache result with sliding expiration of 2 minutes.
   - Return list.
   - Add logging for cache hits/misses and query execution time.

4. **Create IntakeAppointmentController** (`Controllers/IntakeAppointmentController.cs`):
   - Add `[ApiController]`, `[Route("api/intake")]`, `[Authorize]` attributes.
   - Inject `IIntakeAppointmentService`, `ILogger<IntakeAppointmentController>`.
   - Implement `[HttpGet("appointments")]` endpoint:
     - Extract `patientId` from JWT claims (`User.FindFirst("PatientId")?.Value`).
     - Call `await _intakeAppointmentService.GetPatientIntakeAppointmentsAsync(patientId, cancellationToken)`.
     - Return `Ok(appointments)`.
     - Add `[ProducesResponseType(200, Type = typeof(List<IntakeAppointmentDto>))]` and `[ProducesResponseType(401)]` attributes.
     - Wrap in try-catch to handle exceptions — return `500 InternalServerError` with generic message on unhandled exceptions.
     - Log request details (patientId, response count) at Information level.

5. **Register service in DI** (`Program.cs`): Add `builder.Services.AddScoped<IIntakeAppointmentService, IntakeAppointmentService>();` in the service registration section before `builder.Build()`.

6. **Add RequiresIntake column to Appointment table** (if not exists): Create migration to add `RequiresIntake` boolean column to Appointment table with default value `true`. Update `Appointment` entity model in `PatientAccess.Business/Models/Appointment.cs` to include `public bool RequiresIntake { get; set; } = true;`. Run migration script.

7. **Update IntakeRecord model** (if needed): Ensure `IntakeRecord` entity has `Status` property with possible values ("Pending", "InProgress", "Completed"). If not, add property and create migration.

8. **Write unit tests** (`Tests/Services/IntakeAppointmentServiceTests.cs`):
   - Test `GetPatientIntakeAppointmentsAsync_ReturnsAppointments_WhenPatientHasUpcomingAppointments`: Mock appointments with RequiresIntake=true and AppointmentDate > now. Assert list returned with correct count and mapped data.
   - Test `GetPatientIntakeAppointmentsAsync_ExcludesPastAppointments`: Mock appointments with past dates. Assert empty list returned.
   - Test `GetPatientIntakeAppointmentsAsync_ExcludesAppointmentsNotRequiringIntake`: Mock appointments with RequiresIntake=false. Assert empty list returned.
   - Test `GetPatientIntakeAppointmentsAsync_MapsIntakeStatus_Correctly`: Mock appointments with associated IntakeRecords having different statuses. Assert IntakeStatus mapped correctly (Pending/InProgress/Completed).
   - Test `GetPatientIntakeAppointmentsAsync_UsesCache_OnSubsequentCalls`: Call method twice. Assert database query executed only once (cache hit on second call).
   - Test `GetPatientIntakeAppointmentsAsync_OrdersByAppointmentDate_Ascending`: Mock multiple appointments with different dates. Assert returned list ordered by AppointmentDate.
   - Use in-memory database with EF Core for testing. Follow existing test patterns in `PatientAccess.Tests/Services/`.

9. **Add API integration test** (Optional): Create integration test in `PatientAccess.Tests/Controllers/IntakeAppointmentControllerTests.cs` using `WebApplicationFactory<Program>`. Test authenticated GET request to `/api/intake/appointments` returns 200 with correct JSON structure. Test unauthenticated request returns 401.

## Acceptance Verification

- [ ] GET `/api/intake/appointments` returns 200 with list of appointments requiring intake for authenticated patient
- [ ] Response includes correct provider name, specialty, appointment date/time, and intake status
- [ ] Appointments without RequiresIntake flag are excluded from response
- [ ] Past appointments are excluded even if intake is incomplete
- [ ] IntakeStatus correctly reflects "Pending", "InProgress", or "Completed" based on IntakeRecord
- [ ] IntakeSessionId is included when IntakeRecord exists
- [ ] Response is cached with 2-minute sliding expiration
- [ ] Unauthorized requests (no JWT) return 401
- [ ] Requests with mismatched patientId return 403 (if patient tries to access another patient's data)
- [ ] Empty array returned when patient has no appointments requiring intake (not 404)
- [ ] Unit tests pass with >80% coverage
- [ ] API documented in Swagger with correct response types and descriptions
- [ ] No linting errors or build warnings
- [ ] Logging captures request info, cache hits, and errors

## Additional Notes

- **Security**: Ensure patientId is extracted from authenticated JWT token only. Never accept patientId from query parameters or request body to prevent unauthorized access to other patients' appointments.
- **Performance**: Index `Appointment.AppointmentDate` and `Appointment.RequiresIntake` columns for faster query execution. Cache appointments for 2 minutes to reduce database hits during frequent polling by frontend.
- **Data Integrity**: If RequiresIntake column does not exist, coordinate with DBA to add it with a migration. Default existing appointments to `RequiresIntake = true` unless specified otherwise by business rules.
- **Error Handling**: Return generic error messages to client (e.g., "An error occurred") while logging detailed exception information server-side to prevent information leakage.
- **Caching Strategy**: Use patient-specific cache keys (`intake_appointments_{patientId}`) to prevent cache collision. Use sliding expiration (2 minutes) instead of absolute to extend cache lifetime with frequent access. Invalidate cache when a new IntakeRecord is created or updated for the patient.
