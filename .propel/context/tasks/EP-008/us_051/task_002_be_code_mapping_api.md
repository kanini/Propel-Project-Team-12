# Task - task_002_be_code_mapping_api

## Requirement Reference
- User Story: US_051
- Story Location: .propel/context/tasks/EP-008/us_051/us_051.md
- Acceptance Criteria:
    - **AC1**: Given clinical data is extracted, When the code mapping service runs, Then ICD-10 diagnosis codes are suggested for each diagnosis/condition with confidence scores (0-100%).
    - **AC2**: Given clinical data is extracted, When the code mapping service runs, Then CPT procedure codes are suggested for identified procedures with confidence scores (0-100%).
    - **AC3**: Given the RAG pipeline retrieves context, When code mapping uses Azure OpenAI with HIPAA BAA (TR-015), Then the LLM maps clinical text to codes grounded in retrieved knowledge base chunks, providing code value, description, and rationale.
- Edge Case:
    - What happens when clinical text is ambiguous and maps to multiple codes? All plausible codes are returned ranked by confidence, with the top suggestion highlighted.
    - How does the system handle clinical data not mappable to any code? System returns "No matching code found" with recommendation for manual coding.

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
| Library | Swashbuckle.AspNetCore | 6.x |
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

Create REST API endpoints for medical code mapping service to expose ICD-10 and CPT code suggestion functionality to staff workflow and downstream systems. This task implements HTTP controllers with POST endpoints for code mapping requests, GET endpoints for retrieving existing code suggestions, PATCH endpoints for staff verification workflow, and role-based authorization (Staff, Admin roles required). The API integrates with CodeMappingService (from task_001), provides Swagger/OpenAPI documentation, handles validation errors, and includes request/response logging for audit trails.

**Key Capabilities:**
- MedicalCodesController with CRUD endpoints
- POST /api/medical-codes/map-icd10 (trigger ICD-10 code mapping)
- POST /api/medical-codes/map-cpt (trigger CPT code mapping)
- GET /api/medical-codes/{extractedDataId}/suggestions (retrieve existing suggestions)
- PATCH /api/medical-codes/{codeId}/verify (staff verification workflow)
- GET /api/medical-codes/{extractedDataId}/top-suggestion (get top-ranked code)
- Role-based authorization: [Authorize(Roles = "Staff,Admin")]
- Input validation with ModelState
- Exception handling middleware integration
- Swagger/OpenAPI documentation with examples
- Request/response logging for audit trails

## Dependent Tasks
- EP-008: US_051: task_001_be_code_mapping_service (ICodeMappingService implementation)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/Controllers/MedicalCodesController.cs` - REST API controller
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register Swagger examples (optional)
- **NEW**: `src/backend/PatientAccess.Web/Filters/CodeMappingExceptionFilter.cs` - Exception handling filter

## Implementation Plan

1. **Create MedicalCodesController**
   - File: `src/backend/PatientAccess.Web/Controllers/MedicalCodesController.cs`
   - Authorization: [Authorize(Roles = "Staff,Admin")]
   - Constructor dependencies:
     - ICodeMappingService
     - ILogger<MedicalCodesController>
     - ApplicationDbContext

2. **Implement POST /api/medical-codes/map-icd10 Endpoint**
   - Triggers ICD-10 code mapping for extracted clinical data
   - Request body: CodeMappingRequestDto
   - Response: CodeMappingResponseDto with ICD-10 suggestions
   - Status codes: 200 OK, 400 Bad Request, 404 Not Found (if ExtractedClinicalDataId invalid), 500 Internal Server Error
   - Implementation:
     ```csharp
     [HttpPost("map-icd10")]
     [Authorize(Roles = "Staff,Admin")]
     [ProducesResponseType(typeof(CodeMappingResponseDto), StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status400BadRequest)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> MapToICD10([FromBody] CodeMappingRequestDto request, CancellationToken cancellationToken)
     {
         if (!ModelState.IsValid)
             return BadRequest(ModelState);
         
         // Verify ExtractedClinicalData exists
         var extractedData = await _context.ExtractedClinicalData
             .FindAsync(new object[] { request.ExtractedClinicalDataId }, cancellationToken);
         
         if (extractedData == null)
             return NotFound($"Extracted clinical data with ID {request.ExtractedClinicalDataId} not found");
         
         _logger.LogInformation("Mapping ICD-10 codes for ExtractedDataId: {Id}", request.ExtractedClinicalDataId);
         
         var response = await _codeMappingService.MapToICD10Async(request, cancellationToken);
         
         return Ok(response);
     }
     ```

3. **Implement POST /api/medical-codes/map-cpt Endpoint**
   - Triggers CPT code mapping for extracted clinical data
   - Similar structure to map-icd10
   - Implementation:
     ```csharp
     [HttpPost("map-cpt")]
     [Authorize(Roles = "Staff,Admin")]
     [ProducesResponseType(typeof(CodeMappingResponseDto), StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status400BadRequest)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> MapToCPT([FromBody] CodeMappingRequestDto request, CancellationToken cancellationToken)
     {
         if (!ModelState.IsValid)
             return BadRequest(ModelState);
         
         var extractedData = await _context.ExtractedClinicalData
             .FindAsync(new object[] { request.ExtractedClinicalDataId }, cancellationToken);
         
         if (extractedData == null)
             return NotFound($"Extracted clinical data with ID {request.ExtractedClinicalDataId} not found");
         
         _logger.LogInformation("Mapping CPT codes for ExtractedDataId: {Id}", request.ExtractedClinicalDataId);
         
         var response = await _codeMappingService.MapToCPTAsync(request, cancellationToken);
         
         return Ok(response);
     }
     ```

4. **Implement GET /api/medical-codes/{extractedDataId}/suggestions Endpoint**
   - Retrieves existing code suggestions for extracted clinical data
   - Supports filtering by CodeSystem (ICD10, CPT)
   - Response: List of MedicalCodeSuggestionDto
   - Implementation:
     ```csharp
     [HttpGet("{extractedDataId}/suggestions")]
     [Authorize(Roles = "Staff,Admin")]
     [ProducesResponseType(typeof(List<MedicalCodeSuggestionDto>), StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> GetSuggestions(
         Guid extractedDataId, 
         [FromQuery] string? codeSystem = null,
         CancellationToken cancellationToken = default)
     {
         var query = _context.MedicalCodes
             .Where(mc => mc.ExtractedClinicalDataId == extractedDataId);
         
         if (!string.IsNullOrEmpty(codeSystem))
         {
             query = query.Where(mc => mc.CodeSystem == codeSystem);
         }
         
         var suggestions = await query
             .OrderBy(mc => mc.Rank)
             .Select(mc => new MedicalCodeSuggestionDto
             {
                 Code = mc.Code,
                 Description = mc.Description,
                 ConfidenceScore = mc.ConfidenceScore,
                 Rationale = mc.Rationale,
                 Rank = mc.Rank,
                 IsTopSuggestion = mc.IsTopSuggestion
             })
             .ToListAsync(cancellationToken);
         
         if (!suggestions.Any())
             return NotFound($"No code suggestions found for ExtractedDataId: {extractedDataId}");
         
         return Ok(suggestions);
     }
     ```

5. **Implement GET /api/medical-codes/{extractedDataId}/top-suggestion Endpoint**
   - Retrieves only the top-ranked code suggestion (Rank = 1)
   - Supports filtering by CodeSystem
   - Response: Single MedicalCodeSuggestionDto
   - Implementation:
     ```csharp
     [HttpGet("{extractedDataId}/top-suggestion")]
     [Authorize(Roles = "Staff,Admin")]
     [ProducesResponseType(typeof(MedicalCodeSuggestionDto), StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> GetTopSuggestion(
         Guid extractedDataId,
         [FromQuery] string codeSystem,
         CancellationToken cancellationToken = default)
     {
         if (string.IsNullOrEmpty(codeSystem))
             return BadRequest("CodeSystem query parameter is required");
         
         var topSuggestion = await _context.MedicalCodes
             .Where(mc => mc.ExtractedClinicalDataId == extractedDataId &&
                         mc.CodeSystem == codeSystem &&
                         mc.IsTopSuggestion)
             .Select(mc => new MedicalCodeSuggestionDto
             {
                 Code = mc.Code,
                 Description = mc.Description,
                 ConfidenceScore = mc.ConfidenceScore,
                 Rationale = mc.Rationale,
                 Rank = mc.Rank,
                 IsTopSuggestion = mc.IsTopSuggestion
             })
             .FirstOrDefaultAsync(cancellationToken);
         
         if (topSuggestion == null)
             return NotFound($"No top suggestion found for ExtractedDataId: {extractedDataId}, CodeSystem: {codeSystem}");
         
         return Ok(topSuggestion);
     }
     ```

6. **Implement PATCH /api/medical-codes/{codeId}/verify Endpoint**
   - Staff verification workflow: approve or reject AI-suggested code
   - Request body: { "verificationStatus": "StaffVerified" or "StaffRejected" }
   - Updates MedicalCode.VerificationStatus, VerifiedBy, VerifiedAt
   - Implementation:
     ```csharp
     [HttpPatch("{codeId}/verify")]
     [Authorize(Roles = "Staff,Admin")]
     [ProducesResponseType(StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status400BadRequest)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> VerifyCode(
         Guid codeId,
         [FromBody] VerifyCodeRequestDto request,
         CancellationToken cancellationToken)
     {
         if (request.VerificationStatus != "StaffVerified" && request.VerificationStatus != "StaffRejected")
             return BadRequest("VerificationStatus must be 'StaffVerified' or 'StaffRejected'");
         
         var medicalCode = await _context.MedicalCodes.FindAsync(new object[] { codeId }, cancellationToken);
         
         if (medicalCode == null)
             return NotFound($"Medical code with ID {codeId} not found");
         
         // Get current user ID (from JWT claims)
         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
         
         medicalCode.VerificationStatus = request.VerificationStatus;
         medicalCode.VerifiedBy = userId;
         medicalCode.VerifiedAt = DateTime.UtcNow;
         
         await _context.SaveChangesAsync(cancellationToken);
         
         _logger.LogInformation("Medical code {CodeId} verified as {Status} by user {UserId}", 
             codeId, request.VerificationStatus, userId);
         
         return Ok(new { Message = $"Medical code verified as {request.VerificationStatus}" });
     }
     ```

7. **Create VerifyCodeRequestDto**
   - File: `src/backend/PatientAccess.Business/DTOs/VerifyCodeRequestDto.cs`
   - Properties:
     ```csharp
     public class VerifyCodeRequestDto
     {
         [Required]
         public string VerificationStatus { get; set; } // "StaffVerified" or "StaffRejected"
         
         public string? Notes { get; set; } // Optional staff notes
     }
     ```

8. **Create CodeMappingExceptionFilter**
   - File: `src/backend/PatientAccess.Web/Filters/CodeMappingExceptionFilter.cs`
   - Handles exceptions from CodeMappingService (Azure OpenAI failures, schema validation errors)
   - Implementation:
     ```csharp
     public class CodeMappingExceptionFilter : IExceptionFilter
     {
         private readonly ILogger<CodeMappingExceptionFilter> _logger;
         
         public CodeMappingExceptionFilter(ILogger<CodeMappingExceptionFilter> logger)
         {
             _logger = logger;
         }
         
         public void OnException(ExceptionContext context)
         {
             if (context.Exception is RequestFailedException azureException)
             {
                 _logger.LogError(azureException, "Azure OpenAI request failed");
                 context.Result = new ObjectResult(new 
                 { 
                     Error = "AI service temporarily unavailable. Please try again later." 
                 })
                 {
                     StatusCode = StatusCodes.Status503ServiceUnavailable
                 };
                 context.ExceptionHandled = true;
             }
             else if (context.Exception is ValidationException validationException)
             {
                 _logger.LogWarning(validationException, "Code mapping response validation failed");
                 context.Result = new ObjectResult(new 
                 { 
                     Error = "AI response validation failed. Code mapping quality below threshold." 
                 })
                 {
                     StatusCode = StatusCodes.Status422UnprocessableEntity
                 };
                 context.ExceptionHandled = true;
             }
         }
     }
     ```

9. **Register Exception Filter in Program.cs**
   - Add to MVC services:
     ```csharp
     builder.Services.AddControllers(options =>
     {
         options.Filters.Add<CodeMappingExceptionFilter>();
     });
     ```

10. **Add Swagger Documentation**
    - Annotate endpoints with XML comments and ProducesResponseType attributes
    - Add example requests/responses in Swagger UI
    - Document authorization requirements

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   └── CodeMappingService.cs (from task_001)
│   ├── Interfaces/
│   │   └── ICodeMappingService.cs (from task_001)
│   └── DTOs/
│       ├── CodeMappingRequestDto.cs (from task_001)
│       └── CodeMappingResponseDto.cs (from task_001)
├── PatientAccess.Data/
│   ├── Entities/
│   │   └── MedicalCode.cs (from task_001)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    ├── Controllers/
    │   └── AppointmentsController.cs (from EP-001)
    └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Controllers/MedicalCodesController.cs | REST API controller |
| CREATE | src/backend/PatientAccess.Web/Filters/CodeMappingExceptionFilter.cs | Exception handling filter |
| CREATE | src/backend/PatientAccess.Business/DTOs/VerifyCodeRequestDto.cs | Verification request DTO |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register exception filter |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Web API Documentation
- **Controllers**: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- **Routing**: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing
- **Model Validation**: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
- **Exception Filters**: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters

### REST API Best Practices
- **HTTP Status Codes**: https://developer.mozilla.org/en-US/docs/Web/HTTP/Status
- **RESTful API Design**: https://restfulapi.net/

### Swagger/OpenAPI Documentation
- **Swashbuckle**: https://github.com/domaindrivendev/Swashbuckle.AspNetCore
- **OpenAPI Specification**: https://swagger.io/specification/

### Design Requirements
- **FR-034**: System MUST suggest ICD-10 diagnosis codes with confidence scores (spec.md)
- **FR-035**: System MUST suggest CPT procedure codes with confidence scores (spec.md)
- **FR-036**: System MUST provide staff verification workflow for AI-suggested codes (spec.md)

### Existing Codebase Patterns
- **Controller Pattern**: `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs`
- **Authorization**: [Authorize(Roles = "...")] attributes

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build

# Run tests
cd PatientAccess.Tests
dotnet test

# Run application (test endpoints locally)
cd ../PatientAccess.Web
dotnet run
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Controllers/MedicalCodesControllerTests.cs`
- Test cases:
  1. **Test_MapToICD10_Returns200OK_ValidRequest**
     - Input: Valid CodeMappingRequestDto
     - Expected: 200 OK, CodeMappingResponseDto with suggestions
     - Assert: result.StatusCode == 200, response.SuggestionCount > 0
  2. **Test_MapToICD10_Returns400BadRequest_InvalidModelState**
     - Input: CodeMappingRequestDto with missing required fields
     - Expected: 400 Bad Request
     - Assert: result.StatusCode == 400
  3. **Test_MapToICD10_Returns404NotFound_InvalidExtractedDataId**
     - Input: CodeMappingRequestDto with non-existent ExtractedClinicalDataId
     - Expected: 404 Not Found
     - Assert: result.StatusCode == 404
  4. **Test_GetSuggestions_Returns200OK_ExistingSuggestions**
     - Setup: Insert MedicalCode records for ExtractedDataId
     - Execute: GET /api/medical-codes/{extractedDataId}/suggestions
     - Assert: result.StatusCode == 200, suggestions.Count > 0
  5. **Test_GetTopSuggestion_Returns200OK_TopRankedCode**
     - Setup: Insert 3 MedicalCode records, Rank 1, 2, 3
     - Execute: GET /api/medical-codes/{extractedDataId}/top-suggestion?codeSystem=ICD10
     - Assert: result.StatusCode == 200, suggestion.Rank == 1
  6. **Test_VerifyCode_Returns200OK_StaffVerified**
     - Setup: Insert MedicalCode with VerificationStatus = "Pending"
     - Execute: PATCH /api/medical-codes/{codeId}/verify with "StaffVerified"
     - Assert: result.StatusCode == 200, medicalCode.VerificationStatus == "StaffVerified"
  7. **Test_VerifyCode_Returns400BadRequest_InvalidStatus**
     - Input: VerifyCodeRequestDto with VerificationStatus = "InvalidStatus"
     - Expected: 400 Bad Request
     - Assert: result.StatusCode == 400

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Controllers/MedicalCodesControllerIntegrationTests.cs`
- Test cases:
  1. **Test_MapToICD10_EndToEnd_PersistsToDatabase**
     - Setup: Populate ExtractedClinicalData table
     - Execute: POST /api/medical-codes/map-icd10
     - Assert: MedicalCode records exist in database
  2. **Test_VerifyCode_UpdatesDatabase_VerificationStatus**
     - Setup: Insert MedicalCode with "Pending" status
     - Execute: PATCH /api/medical-codes/{codeId}/verify
     - Assert: Database record updated with "StaffVerified", VerifiedAt timestamp

### Acceptance Criteria Validation
- **AC1**: ✅ ICD-10 codes suggested via POST /api/medical-codes/map-icd10 endpoint
- **AC2**: ✅ CPT codes suggested via POST /api/medical-codes/map-cpt endpoint
- **AC3**: ✅ API integrates CodeMappingService (RAG + Azure OpenAI GPT-4o)
- **Edge Case 1**: ✅ Ambiguous text mapped to multiple codes (returned ranked in response)
- **Edge Case 2**: ✅ Unmappable data returns "No matching code found" message

## Success Criteria Checklist
- [MANDATORY] MedicalCodesController created with [Authorize(Roles = "Staff,Admin")]
- [MANDATORY] POST /api/medical-codes/map-icd10 endpoint implemented
- [MANDATORY] POST /api/medical-codes/map-cpt endpoint implemented
- [MANDATORY] GET /api/medical-codes/{extractedDataId}/suggestions endpoint implemented
- [MANDATORY] GET /api/medical-codes/{extractedDataId}/top-suggestion endpoint implemented
- [MANDATORY] PATCH /api/medical-codes/{codeId}/verify endpoint implemented (staff verification workflow)
- [MANDATORY] ModelState validation for all POST/PATCH endpoints
- [MANDATORY] 404 Not Found returned for invalid ExtractedClinicalDataId
- [MANDATORY] CodeMappingExceptionFilter handles Azure OpenAI failures (503 Service Unavailable)
- [MANDATORY] CodeMappingExceptionFilter handles validation failures (422 Unprocessable Entity)
- [MANDATORY] Unit test: Valid request returns 200 OK
- [MANDATORY] Unit test: Invalid ModelState returns 400 Bad Request
- [MANDATORY] Unit test: Invalid ExtractedDataId returns 404 Not Found
- [MANDATORY] Integration test: POST persists MedicalCode records to database
- [MANDATORY] Integration test: PATCH updates VerificationStatus in database
- [RECOMMENDED] Swagger documentation with example requests/responses
- [RECOMMENDED] Request/response logging for audit trails

## Estimated Effort
**3 hours** (Controller implementation + exception handling + unit tests)
