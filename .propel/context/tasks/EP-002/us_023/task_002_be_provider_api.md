# Task - task_002_be_provider_api

## Requirement Reference
- User Story: US_023
- Story Location: .propel/context/tasks/EP-002/us_023/us_023.md
- Acceptance Criteria:
    - AC-1: Display available providers with name, specialty, ratings summary, and next available slot
    - AC-2: Filter updates complete within 300ms showing only matching providers
    - AC-3: Real-time search with 300ms debounce matching provider name and specialty
    - AC-4: Empty state handling (return empty array when no results)
- Edge Case:
    - Large provider list (100+ providers) requires pagination with 20 providers per page
    - Providers with no available slots return null/empty for NextAvailableSlot field

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
Implement RESTful API endpoints for provider and service browsing in the Patient Portal. Create GET /api/providers endpoint with support for search, filtering (specialty, availability, service type), and pagination. Implement efficient database queries with 300ms P95 response time target (NFR-001), proper indexing on Specialty and IsActive fields. Include DTOs for provider data transfer with calculated NextAvailableSlot field from TimeSlots table. Handle edge cases for large datasets (100+ providers) with page-based pagination (20 items per page) and providers with no available slots.

## Dependent Tasks
- task_003_db_provider_data_seeding.md - Database seed data for testing

## Impacted Components
- Backend (.NET): New components to be created
  - `src/backend/PatientAccess.Business/Services/ProviderService.cs` (NEW)
  - `src/backend/PatientAccess.Business/Interfaces/IProviderService.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/ProviderDto.cs` (NEW)
  - `src/backend/PatientAccess.Business/DTOs/ProviderListResponseDto.cs` (NEW)
  - `src/backend/PatientAccess.Web/Controllers/ProvidersController.cs` (NEW)
  - `src/backend/PatientAccess.Business/Models/Provider.cs` (NEW - EF Core entity model)
  - `src/backend/PatientAccess.Business/Models/TimeSlot.cs` (NEW - EF Core entity model)
  - `src/backend/PatientAccess.Business/Data/ApplicationDbContext.cs` (MODIFY - add DbSet<Provider>, DbSet<TimeSlot>)

## Implementation Plan
1. **Create EF Core entity models**:
   - Define Provider entity matching Providers table schema (ProviderId, Name, Specialty, Rating, IsActive)
   - Define TimeSlot entity matching TimeSlots table schema (ProviderId, StartTime, EndTime, IsBooked)
   - Add navigation property: Provider.TimeSlots (one-to-many)

2. **Add DbSets to ApplicationDbContext**:
   - Register DbSet<Provider> and DbSet<TimeSlot>
   - Configure entity relationships using Fluent API (if not using conventions)
   - Ensure indexes on Specialty, IsActive for query performance

3. **Create DTOs for data transfer**:
   - ProviderDto: Id, Name, Specialty, Rating, NextAvailableSlot (DateTime?), ServiceType
   - ProviderListResponseDto: Items (List<ProviderDto>), TotalCount, Page, PageSize, TotalPages

4. **Implement IProviderService interface**:
   - Task<ProviderListResponseDto> GetProvidersAsync(string searchTerm, string specialty, DateTime? availabilityDate, string serviceType, int page, int pageSize)

5. **Build ProviderService business logic**:
   - Query Providers table with filters (IsActive = true, Specialty LIKE searchTerm OR Name LIKE searchTerm)
   - Calculate NextAvailableSlot: LEFT JOIN TimeSlots WHERE IsBooked = false, ORDER BY StartTime ASC, take first
   - Implement pagination with OFFSET/LIMIT (page * pageSize, pageSize)
   - Return empty Items array when no results (AC-4)
   - Optimize query to meet 300ms P95 response time (NFR-001)

6. **Create ProvidersController API endpoint**:
   - GET /api/providers?searchTerm={}&specialty={}&availabilityDate={}&serviceType={}&page=1&pageSize=20
   - [Authorize] attribute for authenticated users only
   - Return 200 OK with ProviderListResponseDto
   - Return 400 Bad Request for invalid query params (e.g., page < 1)
   - Return 500 Internal Server Error with generic message for exceptions

7. **Add error handling and logging**:
   - Try-catch blocks in service layer
   - Log exceptions with ILogger<ProviderService>
   - Return user-friendly error messages (no stack traces in production)

8. **Performance optimization**:
   - Use AsNoTracking() for read-only queries
   - Project only required fields in LINQ query
   - Verify query execution plan uses indexes (IX_Providers_Specialty, IX_Providers_IsActive)

## Current Project State
```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   └── (ProviderService.cs to be created)
│   ├── Interfaces/
│   │   └── (IProviderService.cs to be created)
│   ├── DTOs/
│   │   └── (ProviderDto.cs, ProviderListResponseDto.cs to be created)
│   ├── Models/
│   │   └── (Provider.cs, TimeSlot.cs to be created)
│   └── Data/
│       └── ApplicationDbContext.cs (to be modified)
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── (ProvidersController.cs to be created)
│   └── Program.cs (to be modified for DI registration)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Models/Provider.cs | EF Core entity model for Providers table |
| CREATE | src/backend/PatientAccess.Business/Models/TimeSlot.cs | EF Core entity model for TimeSlots table |
| CREATE | src/backend/PatientAccess.Business/DTOs/ProviderDto.cs | DTO for provider data with NextAvailableSlot |
| CREATE | src/backend/PatientAccess.Business/DTOs/ProviderListResponseDto.cs | Paginated response DTO with TotalCount, Page, PageSize |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IProviderService.cs | Service interface for provider retrieval logic |
| CREATE | src/backend/PatientAccess.Business/Services/ProviderService.cs | Service implementation with filtering and pagination |
| CREATE | src/backend/PatientAccess.Web/Controllers/ProvidersController.cs | REST API controller for GET /api/providers endpoint |
| MODIFY | src/backend/PatientAccess.Business/Data/ApplicationDbContext.cs | Add DbSet<Provider> and DbSet<TimeSlot> |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IProviderService -> ProviderService in DI container |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [EF Core 8 Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Web API Best Practices](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core Performance](https://learn.microsoft.com/en-us/ef/core/performance/)
- [ASP.NET Core Pagination](https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page)
- [REST API Design Guidelines](https://restfulapi.net/resource-naming/)
- [Npgsql PostgreSQL Provider](https://www.npgsql.org/efcore/)

## Build Commands
- `dotnet build` - Build solution
- `dotnet test` - Run xUnit tests
- `dotnet run --project src/backend/PatientAccess.Web` - Start API server (http://localhost:5000)
- `dotnet ef migrations add AddProviderModels --project src/backend/PatientAccess.Business` - Generate EF Core migration (if needed)

## Implementation Validation Strategy
- [ ] Unit tests pass for ProviderService filtering logic
- [ ] Integration tests pass for GET /api/providers endpoint
- [ ] API response time meets 300ms P95 target (NFR-001)
- [ ] Pagination returns 20 items per page for 100+ provider dataset
- [ ] Empty array returned when no providers match filters (AC-4)
- [ ] NextAvailableSlot calculation verified (LEFT JOIN TimeSlots, IsBooked = false, ORDER BY StartTime)
- [ ] Providers with no TimeSlots return NextAvailableSlot = null
- [ ] Search filters work on Name and Specialty fields (case-insensitive)
- [ ] Invalid query params (page < 1, pageSize < 1) return 400 Bad Request
- [ ] Authorization verified (401 Unauthorized for unauthenticated requests)

## Implementation Checklist
- [X] Create Provider.cs entity model with properties: ProviderId, Name, Specialty, Rating, IsActive, CreatedAt
- [X] Create TimeSlot.cs entity model with ProviderId foreign key, StartTime, EndTime, IsBooked
- [X] Add navigation property Provider.TimeSlots (ICollection<TimeSlot>)
- [X] Register DbSet<Provider> and DbSet<TimeSlot> in ApplicationDbContext.cs
- [X] Create ProviderDto.cs with Id, Name, Specialty, Rating, NextAvailableSlot?, ServiceType
- [X] Create ProviderListResponseDto.cs with Items, TotalCount, Page, PageSize, TotalPages
- [X] Define IProviderService interface with GetProvidersAsync signature
- [X] Implement ProviderService.GetProvidersAsync with LINQ query (Where, Select, Skip, Take)
- [X] Add LEFT JOIN logic to calculate NextAvailableSlot from TimeSlots table
- [X] Implement search filtering: WHERE (Name LIKE %searchTerm% OR Specialty LIKE %searchTerm%)
- [X] Add specialty filter: WHERE Specialty = specialty (if provided)
- [X] Add availabilityDate filter: WHERE EXISTS (TimeSlots with StartTime >= availabilityDate AND IsBooked = false)
- [X] Implement pagination with Skip((page - 1) * pageSize).Take(pageSize)
- [X] Calculate TotalPages: Math.Ceiling(TotalCount / (double)pageSize)
- [X] Create ProvidersController.cs with GET /api/providers action method
- [X] Add [Authorize] attribute to ensure authenticated access only
- [X] Add query parameter validation (page >= 1, pageSize between 1 and 100)
- [X] Implement error handling with try-catch and ILogger
- [X] Register IProviderService -> ProviderService in Program.cs DI container
- [ ] Write unit tests: ProviderService filtering logic (name, specialty, availability)
- [ ] Write unit tests: ProviderService pagination logic (page, pageSize, TotalPages)
- [ ] Write integration tests: GET /api/providers returns 200 OK with valid data
- [ ] Write integration tests: GET /api/providers returns 401 Unauthorized when not authenticated
- [X] Use AsNoTracking() for read-only query performance
- [ ] Verify database query execution plan uses indexes (IX_Providers_Specialty, IX_Providers_IsActive)
- [ ] Test with 100+ provider dataset to verify pagination and performance
- [ ] Manual API testing with Postman/Thunder Client for all query param combinations
