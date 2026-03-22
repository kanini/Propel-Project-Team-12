# Task - task_002_be_waitlist_api

## Requirement Reference
- User Story: US_025
- Story Location: .propel/context/tasks/EP-002/us_025/us_025.md
- Acceptance Criteria:
    - AC-2: Create waitlist entry with priority timestamp (FIFO ordering)
    - AC-3: Check for existing waitlist entry and return 409 Conflict with position data
    - AC-4: Retrieve waitlist entries for patient with position calculation
- Edge Case:
    - 50+ patients on waitlist - efficient queue position calculation using ROW_NUMBER() window function

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
Implement RESTful API endpoints for waitlist enrollment and management. Create POST /api/waitlist to enroll patients with duplicate detection (409 Conflict), GET /api/waitlist to retrieve patient's waitlist entries with calculated queue positions, PUT /api/waitlist/{id} to update preferences, and DELETE /api/waitlist/{id} to remove entries. Implement priority timestamp-based FIFO ordering using PostgreSQL ROW_NUMBER() window function for efficient position calculation with 50+ patient queues. Integrate with notification service placeholders for SMS/Email confirmation delivery.

## Dependent Tasks
- None (WaitlistEntries table already exists in schema)

## Impacted Components
- Backend (.NET): New components to be created
  - `src/backend/PatientAccess.Business/Services/WaitlistService.cs` (NEW)
  - `src/backend/PatientAccess.Business/Interfaces/IWaitlistService.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/JoinWaitlistRequestDto.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/WaitlistEntryDto.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/UpdateWaitlistRequestDto.cs` (NEW)
  - `src/backend/PatientAccess.Web/Controllers/WaitlistController.cs` (NEW)
  - `src/backend/PatientAccess.Business/Models/WaitlistEntry.cs` (NEW or UPDATE)

## Implementation Plan
1. **Create or update WaitlistEntry entity model**:
   - Properties: WaitlistEntryId, PatientId, ProviderId, PreferredStartDate, PreferredEndDate, NotificationChannels (enum: SMS=1, Email=2, Both=3), Status (Active/Notified/Expired), PriorityTimestamp (CreatedAt), CreatedAt
   - Navigation properties: Patient, Provider
   - Constraints: UNIQUE (PatientId, ProviderId, Status) WHERE Status = 'Active' - prevent duplicate active entries

2. **Create DTOs**:
   - JoinWaitlistRequestDto: ProviderId, PreferredStartDate, PreferredEndDate, NotificationChannels
   - WaitlistEntryDto: Id, ProviderId, ProviderName, Specialty, PreferredStartDate, PreferredEndDate, NotificationChannels, Status, QueuePosition, CreatedAt
   - UpdateWaitlistRequestDto: PreferredStartDate, PreferredEndDate, NotificationChannels

3. **Implement IWaitlistService interface**:
   - Task<WaitlistEntryDto> JoinWaitlistAsync(Guid patientId, JoinWaitlistRequestDto request)
   - Task<List<WaitlistEntryDto>> GetPatientWaitlistAsync(Guid patientId)
   - Task<WaitlistEntryDto> UpdateWaitlistAsync(Guid entryId, Guid patientId, UpdateWaitlistRequestDto request)
   - Task DeleteWaitlistAsync(Guid entryId, Guid patientId)

4. **Build JoinWaitlistAsync logic**:
   - Check for existing active entry: WHERE PatientId = {id} AND ProviderId = {providerId} AND Status = 'Active'
   - If exists, calculate position and throw ConflictException with existing entry data (409)
   - If not exists, create new WaitlistEntry with PriorityTimestamp = DateTime.UtcNow, Status = 'Active'
   - Send notification confirmation (placeholder: log notification intent)
   - Return WaitlistEntryDto with QueuePosition = 1 (newly added)

5. **Implement GetPatientWaitlistAsync with queue position calculation**:
   - Query: SELECT *, ROW_NUMBER() OVER (PARTITION BY ProviderId ORDER BY PriorityTimestamp ASC) AS QueuePosition FROM WaitlistEntries WHERE PatientId = {id} AND Status = 'Active'
   - Use EF Core FromSqlInterpolated for window function support
   - Map to WaitlistEntryDto with calculated QueuePosition
   - Include Provider.Name and Provider.Specialty via JOIN

6. **Build UpdateWaitlistAsync logic**:
   - Verify ownership: WHERE WaitlistEntryId = {id} AND PatientId = {patientId}
   - If not found or not owned, return 404 Not Found
   - Update PreferredStartDate, PreferredEndDate, NotificationChannels
   - Keep PriorityTimestamp unchanged (maintain queue position)
   - Return updated WaitlistEntryDto

7. **Implement DeleteWaitlistAsync logic**:
   - Verify ownership: WHERE WaitlistEntryId = {id} AND PatientId = {patientId}
   - If not found or not owned, return 404 Not Found
   - Soft delete: Set Status = 'Expired' (or hard delete if preferred)
   - Return 204 No Content

8. **Create WaitlistController API endpoints**:
   - POST /api/waitlist -> JoinWaitlistAsync
   - GET /api/waitlist -> GetPatientWaitlistAsync
   - PUT /api/waitlist/{id} -> UpdateWaitlistAsync
   - DELETE /api/waitlist/{id} -> DeleteWaitlistAsync
   - [Authorize] attribute for authenticated users only
   - Extract patientId from User.Claims
   - Return 201 Created for join, 200 OK for get/update, 204 No Content for delete
   - Return 409 Conflict for duplicate entries with existing entry data

9. **Add notification service integration (placeholder)**:
   - Create INotificationService interface with SendConfirmationAsync(string channel, string recipient, string message)
   - Mock implementation: Log notification intent with ILogger
   - Call after successful waitlist join: SendConfirmationAsync(NotificationChannels, patient.Email/Phone, "Waitlist joined for {Provider}")

## Current Project State
```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   └── (WaitlistService.cs to be created)
│   ├── Interfaces/
│   │   └── (IWaitlistService.cs, INotificationService.cs to be created)
│   ├── DTOs/
│   │   └── (JoinWaitlistRequestDto.cs, WaitlistEntryDto.cs, UpdateWaitlistRequestDto.cs to be created)
│   └── Models/
│       └── (WaitlistEntry.cs to be created or updated)
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── (WaitlistController.cs to be created)
│   └── Program.cs (to be modified for DI registration)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Models/WaitlistEntry.cs | EF Core entity for WaitlistEntries table |
| CREATE | src/backend/PatientAccess.Business/DTOs/JoinWaitlistRequestDto.cs | DTO for creating waitlist entry |
| CREATE | src/backend/PatientAccess.Business/DTOs/WaitlistEntryDto.cs | DTO for waitlist entry response with queue position |
| CREATE | src/backend/PatientAccess.Business/DTOs/UpdateWaitlistRequestDto.cs | DTO for updating waitlist preferences |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IWaitlistService.cs | Service interface for waitlist logic |
| CREATE | src/backend/PatientAccess.Business/Services/WaitlistService.cs | Service implementation with FIFO position calculation |
| CREATE | src/backend/PatientAccess.Business/Interfaces/INotificationService.cs | Notification service interface (placeholder) |
| CREATE | src/backend/PatientAccess.Business/Services/NotificationService.cs | Mock notification service logging confirmations |
| CREATE | src/backend/PatientAccess.Web/Controllers/WaitlistController.cs | REST API controller for waitlist endpoints |
| MODIFY | src/backend/PatientAccess.Business/Data/ApplicationDbContext.cs | Add DbSet<WaitlistEntry> |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IWaitlistService and INotificationService in DI |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [PostgreSQL Window Functions ROW_NUMBER](https://www.postgresql.org/docs/16/functions-window.html)
- [EF Core FromSqlInterpolated](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries#passing-parameters)
- [ASP.NET Core Conflict Handling](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors)
- [PostgreSQL PARTITION BY](https://www.postgresql.org/docs/16/tutorial-window.html)
- [EF Core Unique Constraints](https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=fluent-api#index-uniqueness)

## Build Commands
- `dotnet build` - Build solution
- `dotnet test` - Run xUnit tests
- `dotnet run --project src/backend/PatientAccess.Web` - Start API server
- `dotnet ef migrations add AddWaitlistEntry --project src/backend/PatientAccess.Business` - Generate EF Core migration (if needed)

## Implementation Validation Strategy
- [ ] Unit tests pass for WaitlistService.JoinWaitlistAsync duplicate detection
- [ ] Integration tests pass for POST /api/waitlist (201 Created)
- [ ] Integration tests pass for duplicate entry (409 Conflict with existing data)
- [ ] Integration tests pass for GET /api/waitlist with queue position calculation
- [ ] Integration tests pass for PUT /api/waitlist/{id} preference updates
- [ ] Integration tests pass for DELETE /api/waitlist/{id} (204 No Content)
- [ ] Queue position calculation verified with ROW_NUMBER() window function (50+ entries)
- [ ] Ownership verification prevents unauthorized updates/deletes (404 Not Found)
- [ ] Notification service placeholder logs confirmation intent
- [ ] UNIQUE constraint prevents duplicate active waitlist entries

## Implementation Checklist
- [X] Create or update WaitlistEntry.cs entity with properties: WaitlistEntryId, PatientId, ProviderId, PreferredStartDate, PreferredEndDate, NotificationChannels, Status, PriorityTimestamp, CreatedAt
- [X] Add navigation properties: Patient, Provider
- [ ] Add UNIQUE constraint on (PatientId, ProviderId, Status) WHERE Status = 'Active' using Fluent API
- [X] Create JoinWaitlistRequestDto.cs with ProviderId, PreferredStartDate, PreferredEndDate, NotificationChannels
- [X] Create WaitlistEntryDto.cs with Id, ProviderId, ProviderName, Specialty, PreferredStartDate, PreferredEndDate, NotificationChannels, Status, QueuePosition, CreatedAt
- [X] Create UpdateWaitlistRequestDto.cs with PreferredStartDate, PreferredEndDate, NotificationChannels
- [X] Define IWaitlistService interface with JoinWaitlistAsync, GetPatientWaitlistAsync, UpdateWaitlistAsync, DeleteWaitlistAsync
- [X] Implement JoinWaitlistAsync: Check for existing active entry WHERE PatientId = {id} AND ProviderId = {providerId} AND Status = 'Active'
- [X] If exists, calculate QueuePosition and throw ConflictException with existing entry data
- [X] If not exists, create WaitlistEntry with PriorityTimestamp = DateTime.UtcNow, Status = 'Active'
- [ ] Call INotificationService.SendConfirmationAsync (placeholder)
- [X] Return WaitlistEntryDto
- [X] Implement GetPatientWaitlistAsync using efficient position calculation
- [X] Include Provider.Name and Provider.Specialty via INNER JOIN
- [X] Map results to List<WaitlistEntryDto>
- [X] Implement UpdateWaitlistAsync: Verify ownership WHERE WaitlistEntryId = {id} AND PatientId = {patientId}
- [X] If not found, return 404 Not Found
- [X] Update PreferredStartDate, PreferredEndDate, NotificationChannels (keep PriorityTimestamp unchanged)
- [X] Return updated WaitlistEntryDto
- [X] Implement DeleteWaitlistAsync: Verify ownership
- [X] Soft delete: Set Status = 'Expired' OR hard delete
- [X] Return 204 No Content
- [ ] Create INotificationService interface with SendConfirmationAsync(string channel, string recipient, string message)
- [ ] Create NotificationService.cs with mock implementation: ILogger log only (placeholder)
- [X] Create WaitlistController.cs with POST /api/waitlist endpoint
- [X] Extract patientId from User.Claims (ClaimTypes.NameIdentifier)
- [X] Call IWaitlistService.JoinWaitlistAsync
- [X] Handle ConflictException: Return 409 Conflict with existing entry data in response body
- [X] Return 201 Created with Location header
- [X] Add GET /api/waitlist endpoint calling GetPatientWaitlistAsync
- [X] Return 200 OK with List<WaitlistEntryDto>
- [X] Add PUT /api/waitlist/{id} endpoint calling UpdateWaitlistAsync
- [X] Return 200 OK with updated WaitlistEntryDto
- [X] Handle 404 Not Found for non-existent or unauthorized access
- [X] Add DELETE /api/waitlist/{id} endpoint calling DeleteWaitlistAsync
- [X] Return 204 No Content
- [X] Add [Authorize] attribute to all endpoints
- [X] Register DbSet<WaitlistEntry> in ApplicationDbContext.cs (already exists)
- [X] Register IWaitlistService -> WaitlistService in Program.cs DI
- [ ] Register INotificationService -> NotificationService (Singleton or Scoped)
- [ ] Write unit tests: JoinWaitlistAsync throws ConflictException for duplicate active entry
- [ ] Write unit tests: JoinWaitlistAsync creates new entry successfully
- [ ] Write integration tests: POST /api/waitlist returns 201 Created
- [ ] Write integration tests: POST /api/waitlist returns 409 Conflict for duplicate
- [ ] Write integration tests: GET /api/waitlist returns entries with QueuePosition
- [ ] Write integration tests: PUT /api/waitlist/{id} updates preferences, maintains position
- [ ] Write integration tests: DELETE /api/waitlist/{id} removes entry (204 No Content)
- [ ] Write integration tests: Unauthorized access to PUT/DELETE returns 404
- [ ] Test with 50+ waitlist entries to verify performance
- [ ] Verify UNIQUE constraint prevents duplicate inserts at database level
