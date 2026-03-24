# Task - task_002_be_code_modification_service

## Requirement Reference
- User Story: US_052
- Story Location: .propel/context/tasks/EP-008/us_052/us_052.md
- Acceptance Criteria:
    - **AC3**: Given I want to modify a code, When I click "Modify", Then a search field allows me to look up alternative ICD-10/CPT codes, select the correct one, and save with modification rationale.
- Edge Case:
    - What happens when a Staff member verifies a code that another Staff member already rejected? The most recent verification action takes precedence with full audit trail of both actions.

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

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Entity Framework Core | 8.0 |
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

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Create code modification backend service to handle staff modifications to AI-suggested medical codes per AC3. This task implements PATCH /api/medical-codes/{codeId}/modify endpoint for updating medical codes with new code values, descriptions, and modification rationale. The service integrates with existing MedicalCode entity (from US_051 task_001), creates audit trail entries (AuditLog entity) to track modification history per edge case handling, and validates that staff can modify previously verified or rejected codes (most recent action takes precedence). The implementation includes authorization (Staff, Admin roles), input validation, and logging for compliance audits.

**Key Capabilities:**
- PATCH /api/medical-codes/{codeId}/modify endpoint
- ModifyCodeDto with newCode, newDescription, rationale validation
- AuditLog entity for modification history tracking
- MedicalCode update logic: replace code/description, set VerificationStatus = "StaffVerified"
- Audit trail creation: previous code, new code, modification rationale, timestamp, user ID
- Edge case handling: allow modification of previously rejected codes (most recent wins)
- Role-based authorization: [Authorize(Roles = "Staff,Admin")]
- Input validation: FluentValidation for ModifyCodeDto
- Logging: Application Insights for modification events

## Dependent Tasks
- EP-008: US_051: task_001_be_code_mapping_service (MedicalCode entity)
- EP-008: US_050: task_004_be_hybrid_retrieval_service (Code search reuses /api/knowledge/search endpoint)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ModifyCodeDto.cs` - Modification request DTO
- **NEW**: `src/backend/PatientAccess.Data/Entities/AuditLog.cs` - Audit trail entity
- **NEW**: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddAuditLog.cs` - EF migration
- **NEW**: `src/backend/PatientAccess.Business/Validators/ModifyCodeDtoValidator.cs` - FluentValidation validator
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/MedicalCodesController.cs` - Add PATCH /modify endpoint
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add AuditLog DbSet

## Implementation Plan

1. **Create AuditLog Entity**
   - File: `src/backend/PatientAccess.Data/Entities/AuditLog.cs`
   - Properties:
     - Id (Guid, PK)
     - EntityType (string, 50 chars: "MedicalCode", "ExtractedClinicalData")
     - EntityId (Guid: FK to MedicalCode or ExtractedClinicalData)
     - Action (string, 50 chars: "Accepted", "Modified", "Rejected")
     - UserId (int, FK to Users)
     - UserName (string, 100 chars)
     - PreviousValue (string, 2000 chars, nullable: JSON of previous state)
     - NewValue (string, 2000 chars, nullable: JSON of new state)
     - Rationale (string, 2000 chars, nullable)
     - CreatedAt (DateTime, UTC)
   - Indexes:
     - EntityType, EntityId (composite for entity-specific audit trail queries)
     - UserId (for user audit history)
     - CreatedAt (for time-series queries)

2. **Create ModifyCodeDto**
   - File: `src/backend/PatientAccess.Business/DTOs/ModifyCodeDto.cs`
   - Properties:
     ```csharp
     public class ModifyCodeDto
     {
         [Required]
         public string NewCode { get; set; } // "E11.65" instead of "E11.9"
         
         [Required]
         [StringLength(1000, MinimumLength = 5)]
         public string NewDescription { get; set; }
         
         [Required]
         [StringLength(2000, MinimumLength = 10)]
         public string Rationale { get; set; } // Why staff is modifying the code
     }
     ```

3. **Create ModifyCodeDtoValidator**
   - File: `src/backend/PatientAccess.Business/Validators/ModifyCodeDtoValidator.cs`
   - Validation rules:
     ```csharp
     public class ModifyCodeDtoValidator : AbstractValidator<ModifyCodeDto>
     {
         public ModifyCodeDtoValidator()
         {
             RuleFor(dto => dto.NewCode)
                 .NotEmpty()
                 .WithMessage("New code is required")
                 .Matches(@"^[A-Z]\d{2}\.\d{1,2}$") // ICD-10 format
                 .When(dto => IsICD10Code(dto.NewCode))
                 .WithMessage("Invalid ICD-10 code format (expected: Letter + 2 digits + . + 1-2 digits)");
             
             RuleFor(dto => dto.NewCode)
                 .NotEmpty()
                 .WithMessage("New code is required")
                 .Matches(@"^\d{5}$") // CPT format (5 digits)
                 .When(dto => IsCPTCode(dto.NewCode))
                 .WithMessage("Invalid CPT code format (expected: 5 digits)");
             
             RuleFor(dto => dto.NewDescription)
                 .NotEmpty()
                 .WithMessage("Description is required")
                 .MinimumLength(5)
                 .MaximumLength(1000);
             
             RuleFor(dto => dto.Rationale)
                 .NotEmpty()
                 .WithMessage("Modification rationale is required")
                 .MinimumLength(10)
                 .WithMessage("Rationale must be at least 10 characters")
                 .MaximumLength(2000);
         }
         
         private bool IsICD10Code(string code) => code?.Length >= 4 && char.IsLetter(code[0]);
         private bool IsCPTCode(string code) => code?.Length == 5 && code.All(char.IsDigit);
     }
     ```

4. **Add PATCH /api/medical-codes/{codeId}/modify Endpoint**
   - File: `src/backend/PatientAccess.Web/Controllers/MedicalCodesController.cs`
   - Implementation:
     ```csharp
     [HttpPatch("{codeId}/modify")]
     [Authorize(Roles = "Staff,Admin")]
     [ProducesResponseType(StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status400BadRequest)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> ModifyCode(
         Guid codeId,
         [FromBody] ModifyCodeDto dto,
         CancellationToken cancellationToken)
     {
         // 1. Validate DTO
         var validator = new ModifyCodeDtoValidator();
         var validationResult = await validator.ValidateAsync(dto, cancellationToken);
         if (!validationResult.IsValid)
         {
             return BadRequest(validationResult.Errors);
         }
         
         // 2. Find existing medical code
         var medicalCode = await _context.MedicalCodes
             .FindAsync(new object[] { codeId }, cancellationToken);
         
         if (medicalCode == null)
             return NotFound($"Medical code with ID {codeId} not found");
         
         // 3. Get current user
         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
         var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
         
         // 4. Create audit log entry (before modification)
         var auditLog = new AuditLog
         {
             Id = Guid.NewGuid(),
             EntityType = "MedicalCode",
             EntityId = codeId,
             Action = "Modified",
             UserId = userId,
             UserName = userName,
             PreviousValue = JsonSerializer.Serialize(new 
             {
                 Code = medicalCode.Code,
                 Description = medicalCode.Description,
                 VerificationStatus = medicalCode.VerificationStatus
             }),
             NewValue = JsonSerializer.Serialize(new 
             {
                 Code = dto.NewCode,
                 Description = dto.NewDescription,
                 VerificationStatus = "StaffVerified"
             }),
             Rationale = dto.Rationale,
             CreatedAt = DateTime.UtcNow
         };
         
         // 5. Update medical code
         medicalCode.Code = dto.NewCode;
         medicalCode.Description = dto.NewDescription;
         medicalCode.VerificationStatus = "StaffVerified"; // Staff modification = implicit verification
         medicalCode.VerifiedBy = userId;
         medicalCode.VerifiedAt = DateTime.UtcNow;
         
         // 6. Save changes
         await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
         await _context.SaveChangesAsync(cancellationToken);
         
         _logger.LogInformation("Medical code {CodeId} modified by user {UserId}: {OldCode} → {NewCode}",
             codeId, userId, auditLog.PreviousValue, dto.NewCode);
         
         return Ok(new 
         { 
             Message = "Medical code modified successfully",
             Code = medicalCode.Code,
             Description = medicalCode.Description,
             VerificationStatus = medicalCode.VerificationStatus
         });
     }
     ```

5. **Add GET /api/medical-codes/{codeId}/audit-trail Endpoint**
   - Retrieves audit trail for a specific medical code (edge case: showing previous actions)
   - Implementation:
     ```csharp
     [HttpGet("{codeId}/audit-trail")]
     [Authorize(Roles = "Staff,Admin")]
     [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> GetAuditTrail(
         Guid codeId,
         CancellationToken cancellationToken)
     {
         var auditLogs = await _context.AuditLogs
             .Where(al => al.EntityType == "MedicalCode" && al.EntityId == codeId)
             .OrderByDescending(al => al.CreatedAt)
             .Select(al => new AuditLogDto
             {
                 Action = al.Action,
                 UserName = al.UserName,
                 PreviousValue = al.PreviousValue,
                 NewValue = al.NewValue,
                 Rationale = al.Rationale,
                 CreatedAt = al.CreatedAt
             })
             .ToListAsync(cancellationToken);
         
         if (!auditLogs.Any())
             return NotFound($"No audit trail found for medical code {codeId}");
         
         return Ok(auditLogs);
     }
     ```

6. **Create EF Core Migration**
   - Command: `dotnet ef migrations add AddAuditLog --startup-project ../PatientAccess.Web`
   - Migration adds AuditLog table with indexes

7. **Update ApplicationDbContext**
   - File: `src/backend/PatientAccess.Data/ApplicationDbContext.cs`
   - Add DbSet:
     ```csharp
     public DbSet<AuditLog> AuditLogs { get; set; }
     ```
   - Configure in OnModelCreating:
     ```csharp
     modelBuilder.Entity<AuditLog>(entity =>
     {
         entity.HasIndex(e => new { e.EntityType, e.EntityId });
         entity.HasIndex(e => e.UserId);
         entity.HasIndex(e => e.CreatedAt);
     });
     ```

8. **Register Validator in Program.cs**
   - Add to DI container:
     ```csharp
     builder.Services.AddScoped<IValidator<ModifyCodeDto>, ModifyCodeDtoValidator>();
     ```

9. **Add Logging and Telemetry**
   - Log modification events to Application Insights
   - Track metrics: modifications per day, most common modification rationales
   - Implementation:
     ```csharp
     _logger.LogInformation("Code modification: {CodeId} by {UserId}", codeId, userId);
     
     // Application Insights custom event
     var telemetry = new TelemetryClient();
     telemetry.TrackEvent("CodeModified", new Dictionary<string, string>
     {
         { "CodeId", codeId.ToString() },
         { "OldCode", medicalCode.Code },
         { "NewCode", dto.NewCode },
         { "UserId", userId.ToString() }
     });
     ```

10. **Implement Edge Case Handling: Most Recent Action Takes Precedence**
    - Logic already implemented: modification updates VerificationStatus and VerifiedAt
    - Audit trail preserves history of all actions (including previous rejections)
    - Most recent VerifiedAt timestamp determines current status
    - Frontend can display audit trail to show full history

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── DTOs/
│   │   ├── CodeMappingRequestDto.cs (from US_051 task_001)
│   │   └── VerifyCodeRequestDto.cs (from US_051 task_002)
│   └── Validators/
│       └── CodeMappingResponseValidator.cs (from US_051 task_001)
├── PatientAccess.Data/
│   ├── Entities/
│   │   └── MedicalCode.cs (from US_051 task_001)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    └── Controllers/
        └── MedicalCodesController.cs (from US_051 task_002)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/DTOs/ModifyCodeDto.cs | Modification request DTO |
| CREATE | src/backend/PatientAccess.Data/Entities/AuditLog.cs | Audit trail entity |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddAuditLog.cs | EF migration |
| CREATE | src/backend/PatientAccess.Business/Validators/ModifyCodeDtoValidator.cs | FluentValidation validator |
| MODIFY | src/backend/PatientAccess.Web/Controllers/MedicalCodesController.cs | Add PATCH /modify, GET /audit-trail |
| MODIFY | src/backend/PatientAccess.Data/ApplicationDbContext.cs | Add AuditLog DbSet |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register ModifyCodeDtoValidator |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Web API Documentation
- **PATCH Method**: https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types
- **Model Validation**: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation

### FluentValidation Documentation
- **FluentValidation GitHub**: https://github.com/FluentValidation/FluentValidation
- **Regex Validation**: https://docs.fluentvalidation.net/en/latest/built-in-validators.html#regular-expression-validator

### Audit Logging Best Practices
- **Audit Trail Design**: https://en.wikipedia.org/wiki/Audit_trail
- **HIPAA Audit Requirements**: https://www.hhs.gov/hipaa/for-professionals/security/laws-regulations/index.html

### ICD-10 and CPT Code Formats
- **ICD-10-CM Format**: Letter + 2 digits + "." + 1-2 digits (e.g., E11.9, E11.65)
- **CPT Format**: 5 digits (e.g., 99213, 80053)

### Design Requirements
- **FR-036**: Staff can accept, modify, or reject AI-suggested codes (spec.md)

### Existing Codebase Patterns
- **Controller Pattern**: `src/backend/PatientAccess.Web/Controllers/MedicalCodesController.cs`
- **Entity Pattern**: `src/backend/PatientAccess.Data/Entities/MedicalCode.cs`

## Build Commands
```powershell
# Create migration
cd src/backend/PatientAccess.Data
dotnet ef migrations add AddAuditLog --startup-project ../PatientAccess.Web

# Update database
dotnet ef database update --startup-project ../PatientAccess.Web

# Build solution
cd ..
dotnet build

# Run tests
cd PatientAccess.Tests
dotnet test
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Controllers/MedicalCodesControllerTests.cs`
- Test cases:
  1. **Test_ModifyCode_Returns200OK_ValidRequest**
     - Input: Valid ModifyCodeDto with valid ICD-10 code format
     - Expected: 200 OK, MedicalCode updated in database
     - Assert: result.StatusCode == 200, medicalCode.Code == dto.NewCode
  2. **Test_ModifyCode_Returns400BadRequest_InvalidCodeFormat**
     - Input: ModifyCodeDto with invalid ICD-10 format ("Invalid")
     - Expected: 400 Bad Request, validation error
     - Assert: result.StatusCode == 400, error message contains "Invalid ICD-10 code format"
  3. **Test_ModifyCode_Returns400BadRequest_MissingRationale**
     - Input: ModifyCodeDto with empty rationale
     - Expected: 400 Bad Request
     - Assert: error message contains "Modification rationale is required"
  4. **Test_ModifyCode_CreatesAuditLogEntry**
     - Setup: Insert MedicalCode with Code = "E11.9"
     - Execute: PATCH /api/medical-codes/{id}/modify with NewCode = "E11.65"
     - Assert: AuditLog table contains entry with PreviousValue = "E11.9", NewValue = "E11.65"
  5. **Test_GetAuditTrail_ReturnsOrderedHistory**
     - Setup: Insert 3 AuditLog entries for same codeId with different timestamps
     - Execute: GET /api/medical-codes/{codeId}/audit-trail
     - Assert: Entries ordered by CreatedAt descending (most recent first)
  6. **Test_ModifyCode_MostRecentActionTakesPrecedence**
     - Setup: Insert MedicalCode with VerificationStatus = "StaffRejected"
     - Execute: PATCH /modify
     - Assert: VerificationStatus changed to "StaffVerified" (most recent wins)

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Controllers/MedicalCodesControllerIntegrationTests.cs`
- Test cases:
  1. **Test_ModifyCodeEndpoint_UpdatesDatabaseAndCreatesAudit**
     - Execute: POST /api/medical-codes/map-icd10 to create code
     - Execute: PATCH /api/medical-codes/{id}/modify
     - Assert: MedicalCode updated, AuditLog entry exists
  2. **Test_AuditTrailEndpoint_ShowsModificationHistory**
     - Execute: PATCH /modify twice for same code
     - Execute: GET /audit-trail
     - Assert: 2 audit entries returned

### Acceptance Criteria Validation
- **AC3**: ✅ PATCH /modify endpoint allows code update with rationale
- **Edge Case**: ✅ Most recent action takes precedence, full audit trail preserved

## Success Criteria Checklist
- [MANDATORY] AuditLog entity created with EntityType, EntityId, Action, UserId, PreviousValue, NewValue, Rationale
- [MANDATORY] ModifyCodeDto created with NewCode, NewDescription, Rationale
- [MANDATORY] ModifyCodeDtoValidator validates ICD-10 format (Letter + 2 digits + . + 1-2 digits)
- [MANDATORY] ModifyCodeDtoValidator validates CPT format (5 digits)
- [MANDATORY] PATCH /api/medical-codes/{codeId}/modify endpoint implemented
- [MANDATORY] PATCH /modify updates MedicalCode.Code, Description, VerificationStatus = "StaffVerified"
- [MANDATORY] PATCH /modify creates AuditLog entry with PreviousValue, NewValue, Rationale
- [MANDATORY] GET /api/medical-codes/{codeId}/audit-trail endpoint returns ordered audit history
- [MANDATORY] Edge case: Modifying previously rejected code changes status to "StaffVerified"
- [MANDATORY] Authorization: [Authorize(Roles = "Staff,Admin")]
- [MANDATORY] Unit test: Valid request returns 200 OK
- [MANDATORY] Unit test: Invalid code format returns 400 Bad Request
- [MANDATORY] Integration test: Modification creates AuditLog entry
- [MANDATORY] Integration test: Audit trail endpoint returns modification history
- [RECOMMENDED] Application Insights telemetry: "CodeModified" event
- [RECOMMENDED] Logging: Modification actions logged with user ID and code changes

## Estimated Effort
**3 hours** (Endpoint + AuditLog entity + validation + unit tests)
