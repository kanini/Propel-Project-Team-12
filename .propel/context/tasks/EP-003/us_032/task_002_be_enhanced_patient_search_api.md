# Task - task_002_be_enhanced_patient_search_api

## Requirement Reference

- User Story: US_032 - Patient Search Functionality
- Story Location: .propel/context/tasks/EP-003/us_032/us_032.md
- Acceptance Criteria:
  - AC-1: Given I am on any Staff page with patient search, When I type in the search field, Then real-time filtering begins after 300ms debounce matching against patient name, email, and phone number.
  - AC-2: Given search results are returned, When I view the results, Then each result shows patient name, date of birth, email, phone, and last appointment date for quick identification.
  - AC-3: Given I select a patient from results, When I click on the record, Then I am navigated to that patient's profile context (booking, verification, or queue depending on the source page).
  - AC-4: Given no patients match, When the search returns empty, Then a "No patients found" message displays with option to create a new patient record.
- Edge Case:
  - What happens when multiple patients have similar names? Search results include DOB and email for disambiguation.
  - How does the system handle special characters in search queries? Search input is sanitized to prevent injection while still matching names with accents or hyphens.

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

Enhance the existing patient search API endpoint (created in US_029) to support more robust search capabilities including last appointment date retrieval, improved relevance sorting, input sanitization for special characters (accents, hyphens), and optimized query performance (< 300ms). This task ensures the search endpoint is generic and reusable across multiple Staff pages (Walk-in Booking, Arrival Management, Queue Management, Dashboard) while maintaining RBAC enforcement and comprehensive error handling.

## Dependent Tasks

- US_029 Task 002 (Walk-in Booking API) - created initial patient search endpoint `/api/staff/patients/search`

## Impacted Components

- **MODIFY**: `src/backend/PatientAccess.Business/Services/PatientService.cs` - Enhance SearchPatients method to include last appointment date and improved sorting
- **MODIFY**: `src/backend/PatientAccess.Business/DTOs/PatientSearchResultDto.cs` - Add LastAppointmentDate field (nullable)
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/StaffController.cs` - Enhance GET /api/staff/patients/search endpoint with better error handling

## Implementation Plan

1. **Enhance PatientSearchResultDto**
   - Add `LastAppointmentDate` field (nullable DateTime)
   - Ensure all fields are present: Id, FullName, DateOfBirth, Email, Phone, LastAppointmentDate
   - Add data annotations for validation and Swagger documentation

2. **Enhance Patient Search Query Logic**
   - Modify `PatientService.SearchPatients(string query)` to:
     - LEFT JOIN Appointments table to get most recent appointment date per patient
     - Use EF Core `.GroupBy()` and `.Max()` to get last appointment date efficiently
     - Improve search matching:
       - Exact match on email (highest priority)
       - Starts-with match on name (high priority)
       - Contains match on name, email, phone (medium priority)
     - Order results by relevance: exact email match > starts-with name > contains match
     - Add secondary sort by LastAppointmentDate DESC (most recent patients first)
   - Implement input sanitization:
     - Trim whitespace
     - Allow Unicode letters (accents: á, é, ñ, ü, etc.)
     - Allow hyphens, apostrophes, spaces, periods, @ symbol
     - Remove potentially harmful characters (semicolons, quotes - prevent injection)
     - Use parameterized queries (EF Core handles this automatically)

3. **Optimize Query Performance**
   - Ensure database indices exist on:
     - Users(Email) - unique index (already exists)
     - Users(FirstName, LastName) - composite index for name search
     - Patients(Phone) - index for phone search
     - Appointments(PatientId, ScheduledDatetime) - composite index for last appointment lookup
   - Use compiled queries for repeated search patterns
   - Limit results to top 20 (prevent excessive data transfer)
   - Measure query execution time - target < 300ms for 95th percentile

4. **Implement Comprehensive Error Handling**
   - Handle null or empty query:
     - If query.Trim().Length < 2, return empty list (avoid overly broad searches)
   - Handle database connection errors:
     - Catch DbException and return 503 Service Unavailable with retry message
   - Handle timeout errors:
     - Set query timeout to 500ms to prevent long-running queries
   - Log all errors to structured logging (Seq/Application Insights)
   - Return ProblemDetails for 4xx/5xx responses

5. **Add Input Validation and Sanitization Middleware**
   - Validate query parameter:
     - Minimum length: 2 characters
     - Maximum length: 100 characters
     - Allowed characters: `^[a-zA-Z0-9\s\-'À-ÿ@.]+$` (letters, numbers, spaces, hyphens, apostrophes, accents, @ and .)
   - Return 400 Bad Request if validation fails with descriptive message
   - Sanitize input before database query (trim, normalize Unicode)

6. **Enhance RBAC Enforcement**
   - Verify [Authorize(Roles = "Staff")] attribute is applied to search endpoint
   - Add audit logging for search operations (optional - can be high volume)
   - Consider rate limiting to prevent abuse (e.g., max 100 requests per minute per user)

7. **Add Unit and Integration Tests**
   - Write unit tests for PatientService.SearchPatients:
     - Test exact email match returns highest priority result
     - Test starts-with name match
     - Test contains match
     - Test last appointment date is correctly populated
     - Test input sanitization (accents, hyphens preserved)
     - Test query length validation (< 2 chars returns empty)
   - Write integration tests for search endpoint:
     - Test response time < 300ms
     - Test RBAC enforcement (non-Staff gets 403)
     - Test various search patterns (email, name, phone)

## Current Project State

```
src/backend/PatientAccess.Web/
├── Controllers/
│   └── StaffController.cs                   # Contains GET /api/staff/patients/search (from US_029)
└── Program.cs                               # Startup configuration

src/backend/PatientAccess.Business/
├── Services/
│   └── PatientService.cs                    # Contains SearchPatients method (from US_029)
├── Interfaces/
│   └── IPatientService.cs                   # Patient service interface
└── DTOs/
    └── PatientSearchResultDto.cs            # DTO for search results (from US_029)

Database Schema:
├── Users (email, first_name, last_name, phone)
├── Patients (user_id FK, insurance, emergency_contact)
└── Appointments (patient_id FK, scheduled_datetime, status)

Existing Indices:
├── Users_Email_idx (unique)
├── Patients_UserId_idx (FK)
└── Appointments_PatientId_idx (FK)
```

## Expected Changes

| Action | File Path                                                         | Description                                                                                        |
| ------ | ----------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| MODIFY | src/backend/PatientAccess.Business/DTOs/PatientSearchResultDto.cs | Add LastAppointmentDate field (nullable DateTime), add data annotations                            |
| MODIFY | src/backend/PatientAccess.Business/Services/PatientService.cs     | Enhance SearchPatients: add last appointment join, improve relevance sorting, input sanitization   |
| MODIFY | src/backend/PatientAccess.Web/Controllers/StaffController.cs      | Enhance GET /api/staff/patients/search: add input validation, improve error handling               |
| MODIFY | Database Migrations                                               | Add composite indices on Users(FirstName, LastName) and Appointments(PatientId, ScheduledDatetime) |

## External References

- Entity Framework Core Group By: https://learn.microsoft.com/en-us/ef/core/querying/groupby
- PostgreSQL Text Search Performance: https://www.postgresql.org/docs/16/textsearch-indexes.html
- ASP.NET Core Model Validation: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-8.0
- Unicode Normalization in .NET: https://learn.microsoft.com/en-us/dotnet/standard/base-types/normalization
- EF Core Compiled Queries: https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cwith-constant#compiled-queries

## Build Commands

- `dotnet build src/backend/PatientAccess.sln` - Build solution
- `dotnet run --project src/backend/PatientAccess.Web/PatientAccess.Web.csproj` - Run API locally
- `dotnet test src/backend/PatientAccess.Tests/PatientAccess.Tests.csproj` - Run unit tests
- `dotnet ef migrations add PatientSearchIndices --project src/backend/PatientAccess.Web` - Create migration for search indices
- `dotnet ef database update --project src/backend/PatientAccess.Web` - Apply migration

## Implementation Validation Strategy

- [ ] Unit tests pass for PatientService.SearchPatients with various search patterns
- [ ] Unit tests pass for input sanitization (accents, hyphens preserved, harmful chars removed)
- [ ] Integration tests pass for search endpoint (< 300ms response time)
- [ ] RBAC enforcement verified - non-Staff users receive 403 Forbidden
- [ ] Database indices created on Users(FirstName, LastName) and Appointments(PatientId, ScheduledDatetime)
- [ ] Query performance measured - search < 300ms for 95th percentile under load
- [ ] Error handling returns ProblemDetails for 4xx/5xx responses
- [ ] Input validation returns 400 Bad Request for invalid query (< 2 chars, > 100 chars, invalid chars)
- [ ] Last appointment date correctly populated in search results
- [ ] Relevance sorting works correctly (exact email > starts-with name > contains)

## Implementation Checklist

- [ ] Modify PatientSearchResultDto to add LastAppointmentDate field (nullable DateTime)
- [ ] Add data annotations to PatientSearchResultDto for Swagger documentation
- [ ] Enhance PatientService.SearchPatients method:
  - [ ] Add LEFT JOIN to Appointments table
  - [ ] Use GroupBy and Max to get most recent appointment date per patient
  - [ ] Implement relevance-based sorting:
    - [ ] Priority 1: Exact email match
    - [ ] Priority 2: Name starts with query
    - [ ] Priority 3: Name, email, or phone contains query
  - [ ] Secondary sort by LastAppointmentDate DESC
  - [ ] Trim and sanitize input query:
    - [ ] Remove leading/trailing whitespace
    - [ ] Normalize Unicode characters
    - [ ] Allow accents, hyphens, apostrophes, @, .
    - [ ] Remove semicolons, quotes
  - [ ] Limit results to top 20
- [ ] Add input validation to StaffController search endpoint:
  - [ ] Validate query length >= 2 characters
  - [ ] Validate query length <= 100 characters
  - [ ] Validate allowed characters regex: `^[a-zA-Z0-9\s\-'À-ÿ@.]+$`
  - [ ] Return 400 Bad Request with descriptive message if validation fails
- [ ] Add database indices via migration:
  - [ ] Composite index on Users(FirstName, LastName)
  - [ ] Composite index on Appointments(PatientId, ScheduledDatetime DESC)
- [ ] Implement comprehensive error handling:
  - [ ] Handle null/empty query (return empty list)
  - [ ] Handle DbException (return 503 Service Unavailable)
  - [ ] Set query timeout to 500ms
  - [ ] Log all errors to structured logging
  - [ ] Return ProblemDetails for errors
- [ ] Verify [Authorize(Roles = "Staff")] attribute on search endpoint
- [ ] Write unit tests for SearchPatients:
  - [ ] Test exact email match has highest priority
  - [ ] Test starts-with name match
  - [ ] Test contains match in name, email, phone
  - [ ] Test last appointment date populated correctly
  - [ ] Test input sanitization preserves accents and hyphens
  - [ ] Test query length < 2 returns empty list
- [ ] Write integration tests for search endpoint:
  - [ ] Test response time < 300ms with 100 patients in DB
  - [ ] Test RBAC enforcement (Patient role gets 403)
  - [ ] Test email search pattern
  - [ ] Test name search pattern
  - [ ] Test phone search pattern
  - [ ] Test input validation (< 2 chars returns 400)
- [ ] Measure query performance under load using ApacheBench or similar tool
- [ ] Verify search endpoint is used successfully by Walk-in Booking and Arrival Management pages
