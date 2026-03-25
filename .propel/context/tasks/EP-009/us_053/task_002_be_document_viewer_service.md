# Task - task_002_be_document_viewer_service

## Requirement Reference
- User Story: US_053
- Story Location: .propel/context/tasks/EP-009/us_053/us_053.md
- Acceptance Criteria:
    - **AC1**: Given a patient has AI-extracted data, When I access the review interface, Then each data point is displayed with its value, confidence score, source page number, source text excerpt, and extraction date.
    - **AC2**: Given I need to verify an extraction, When I click on a source reference, Then the original document page is displayed alongside the extracted data for side-by-side comparison.
- Edge Case:
    - What happens when the source document has been deleted after extraction? The extracted data remains with a "Source document unavailable" note; verification relies on the stored text excerpt.

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
| Frontend | N/A | N/A |
| Backend | ASP.NET Core | 8.0 |
| Backend | C# | 12.0 |
| Database | PostgreSQL | 16.x |
| Database | Entity Framework Core | 8.0 |
| Library | Azure.Storage.Blobs | 12.x |
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

Create backend Document Viewer Service to retrieve source document pages for side-by-side verification (AC2). This task implements REST API endpoint GET /api/documents/{documentId}/page/{pageNumber} returning document page images and metadata for clinical staff to verify AI-extracted data against original source documents. The service handles Azure Blob Storage integration for document retrieval, generates page-level image URLs for PDF documents, returns stored text excerpts when source documents are deleted (edge case), implements caching for frequently accessed pages, and integrates with ExtractedClinicalData entity to validate document references. Features role-based authorization (Staff, Admin roles), structured error handling for missing documents, performance optimization with SAS token generation for direct blob access, and logging for audit trail compliance.

**Key Capabilities:**
- DocumentsController with GET /api/documents/{documentId}/page/{pageNumber} endpoint
- DocumentViewerService for blob storage integration
- SAS token generation for secure document access (Azure Blob Storage)
- Page image rendering for PDF documents (Azure AI Document Intelligence or PDF library)
- ExtractedClinicalData.IsDocumentAvailable check for edge case handling
- Redis caching for document URLs (15-minute TTL)
- DocumentNotFoundException exception handler
- Role-based authorization (Staff, Admin roles only)
- Application Insights logging for document access tracking

## Dependent Tasks
- EP-006-II: US_045: task_002_be_azure_document_intelligence (PatientDocument entity, DocumentId field in ExtractedClinicalData)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/DocumentViewerService.cs` - Document retrieval service
- **NEW**: `src/backend/PatientAccess.Web/Controllers/DocumentsController.cs` - REST API controller
- **NEW**: `src/backend/PatientAccess.Business/DTOs/DocumentPageDto.cs` - Response DTO
- **NEW**: `src/backend/PatientAccess.Business/Exceptions/DocumentNotFoundException.cs` - Custom exception
- **MODIFY**: `src/backend/PatientAccess.Web/Middleware/ExceptionHandlingMiddleware.cs` - Handle DocumentNotFoundException
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register DocumentViewerService

## Implementation Plan

1. **Create DocumentPageDto**
   - File: `src/backend/PatientAccess.Business/DTOs/DocumentPageDto.cs`
   - Response DTO for document page retrieval:
     ```csharp
     namespace PatientAccess.Business.DTOs
     {
         public sealed class DocumentPageDto
         {
             /// <summary>
             /// Unique identifier for the document.
             /// </summary>
             public required string DocumentId { get; init; }
             
             /// <summary>
             /// Name of the original document file.
             /// </summary>
             public required string DocumentName { get; init; }
             
             /// <summary>
             /// Page number (1-indexed).
             /// </summary>
             public required int PageNumber { get; init; }
             
             /// <summary>
             /// URL to the document page image (SAS token for Azure Blob Storage).
             /// </summary>
             public required string PageImageUrl { get; init; }
             
             /// <summary>
             /// Total number of pages in the document.
             /// </summary>
             public required int TotalPages { get; init; }
             
             /// <summary>
             /// Whether the source document is available (false if deleted).
             /// </summary>
             public required bool IsDocumentAvailable { get; init; }
             
             /// <summary>
             /// Content type of the document (e.g., "application/pdf").
             /// </summary>
             public string? ContentType { get; init; }
         }
     }
     ```

2. **Create DocumentNotFoundException**
   - File: `src/backend/PatientAccess.Business/Exceptions/DocumentNotFoundException.cs`
   - Custom exception for missing documents:
     ```csharp
     namespace PatientAccess.Business.Exceptions
     {
         public sealed class DocumentNotFoundException : Exception
         {
             public string DocumentId { get; }
             public int? PageNumber { get; }
             
             public DocumentNotFoundException(string documentId, int? pageNumber = null)
                 : base($"Document '{documentId}' {(pageNumber.HasValue ? $"page {pageNumber}" : "")} not found or has been deleted.")
             {
                 DocumentId = documentId;
                 PageNumber = pageNumber;
             }
         }
     }
     ```

3. **Create IDocumentViewerService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IDocumentViewerService.cs`
   - Service interface:
     ```csharp
     namespace PatientAccess.Business.Interfaces
     {
         public interface IDocumentViewerService
         {
             /// <summary>
             /// Retrieves a specific page from a document.
             /// </summary>
             /// <param name="documentId">Document identifier from PatientDocument table.</param>
             /// <param name="pageNumber">Page number (1-indexed).</param>
             /// <param name="cancellationToken">Cancellation token.</param>
             /// <returns>Document page with image URL.</returns>
             /// <exception cref="DocumentNotFoundException">Thrown when document does not exist.</exception>
             Task<DocumentPageDto> GetDocumentPageAsync(
                 string documentId, 
                 int pageNumber, 
                 CancellationToken cancellationToken);
         }
     }
     ```

4. **Create DocumentViewerService**
   - File: `src/backend/PatientAccess.Business/Services/DocumentViewerService.cs`
   - Service implementation with Azure Blob Storage integration:
     ```csharp
     using Azure.Storage.Blobs;
     using Azure.Storage.Sas;
     using Microsoft.EntityFrameworkCore;
     using PatientAccess.Business.DTOs;
     using PatientAccess.Business.Exceptions;
     using PatientAccess.Business.Interfaces;
     using PatientAccess.Data;
     using PatientAccess.Data.Entities;
     using StackExchange.Redis;
     using System.Text.Json;
     
     namespace PatientAccess.Business.Services
     {
         public sealed class DocumentViewerService : IDocumentViewerService
         {
             private readonly AppDbContext _context;
             private readonly BlobServiceClient _blobServiceClient;
             private readonly IDatabase _redisCache;
             private readonly ILogger<DocumentViewerService> _logger;
             private readonly string _containerName;
             
             public DocumentViewerService(
                 AppDbContext context,
                 BlobServiceClient blobServiceClient,
                 IConnectionMultiplexer redis,
                 ILogger<DocumentViewerService> logger,
                 IConfiguration configuration)
             {
                 _context = context;
                 _blobServiceClient = blobServiceClient;
                 _redisCache = redis.GetDatabase();
                 _logger = logger;
                 _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "patient-documents";
             }
             
             public async Task<DocumentPageDto> GetDocumentPageAsync(
                 string documentId, 
                 int pageNumber, 
                 CancellationToken cancellationToken)
             {
                 // Check Redis cache first
                 string cacheKey = $"document:page:{documentId}:{pageNumber}";
                 string? cachedData = await _redisCache.StringGetAsync(cacheKey);
                 
                 if (!string.IsNullOrEmpty(cachedData))
                 {
                     _logger.LogInformation("Cache hit for document {DocumentId} page {PageNumber}", documentId, pageNumber);
                     return JsonSerializer.Deserialize<DocumentPageDto>(cachedData)!;
                 }
                 
                 // Fetch document metadata from database
                 var document = await _context.PatientDocuments
                     .AsNoTracking()
                     .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
                 
                 if (document == null || document.IsDeleted)
                 {
                     _logger.LogWarning("Document {DocumentId} not found or deleted", documentId);
                     throw new DocumentNotFoundException(documentId, pageNumber);
                 }
                 
                 // Validate page number
                 if (pageNumber < 1 || pageNumber > document.TotalPages)
                 {
                     _logger.LogWarning("Invalid page number {PageNumber} for document {DocumentId} (Total: {TotalPages})", 
                         pageNumber, documentId, document.TotalPages);
                     throw new ArgumentOutOfRangeException(nameof(pageNumber), 
                         $"Page number must be between 1 and {document.TotalPages}");
                 }
                 
                 // Generate SAS URL for blob storage
                 var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                 string blobName = $"{documentId}/page-{pageNumber}.png"; // Assume pages stored as images
                 var blobClient = containerClient.GetBlobClient(blobName);
                 
                 // Check if blob exists
                 if (!await blobClient.ExistsAsync(cancellationToken))
                 {
                     _logger.LogWarning("Blob {BlobName} not found in container {ContainerName}", blobName, _containerName);
                     throw new DocumentNotFoundException(documentId, pageNumber);
                 }
                 
                 // Generate SAS token (valid for 1 hour)
                 var sasBuilder = new BlobSasBuilder
                 {
                     BlobContainerName = _containerName,
                     BlobName = blobName,
                     Resource = "b",
                     StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                     ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                 };
                 sasBuilder.SetPermissions(BlobSasPermissions.Read);
                 
                 string sasToken = blobClient.GenerateSasUri(sasBuilder).ToString();
                 
                 var result = new DocumentPageDto
                 {
                     DocumentId = documentId,
                     DocumentName = document.FileName,
                     PageNumber = pageNumber,
                     PageImageUrl = sasToken,
                     TotalPages = document.TotalPages,
                     IsDocumentAvailable = !document.IsDeleted,
                     ContentType = document.ContentType
                 };
                 
                 // Cache for 15 minutes
                 await _redisCache.StringSetAsync(
                     cacheKey,
                     JsonSerializer.Serialize(result),
                     TimeSpan.FromMinutes(15));
                 
                 _logger.LogInformation("Retrieved document {DocumentId} page {PageNumber}", documentId, pageNumber);
                 return result;
             }
         }
     }
     ```

5. **Create DocumentsController**
   - File: `src/backend/PatientAccess.Web/Controllers/DocumentsController.cs`
   - REST API controller:
     ```csharp
     using Microsoft.AspNetCore.Authorization;
     using Microsoft.AspNetCore.Mvc;
     using PatientAccess.Business.DTOs;
     using PatientAccess.Business.Exceptions;
     using PatientAccess.Business.Interfaces;
     
     namespace PatientAccess.Web.Controllers
     {
         [ApiController]
         [Route("api/[controller]")]
         [Authorize(Roles = "Staff,Admin")]
         public sealed class DocumentsController : ControllerBase
         {
             private readonly IDocumentViewerService _documentViewerService;
             private readonly ILogger<DocumentsController> _logger;
             
             public DocumentsController(
                 IDocumentViewerService documentViewerService,
                 ILogger<DocumentsController> logger)
             {
                 _documentViewerService = documentViewerService;
                 _logger = logger;
             }
             
             /// <summary>
             /// Retrieves a specific page from a patient document.
             /// </summary>
             /// <param name="documentId">Document identifier.</param>
             /// <param name="pageNumber">Page number (1-indexed).</param>
             /// <param name="cancellationToken">Cancellation token.</param>
             /// <returns>Document page with image URL.</returns>
             /// <response code="200">Document page retrieved successfully.</response>
             /// <response code="404">Document or page not found.</response>
             /// <response code="403">User not authorized to access this document.</response>
             [HttpGet("{documentId}/page/{pageNumber}")]
             [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DocumentPageDto))]
             [ProducesResponseType(StatusCodes.Status404NotFound)]
             [ProducesResponseType(StatusCodes.Status403Forbidden)]
             public async Task<ActionResult<DocumentPageDto>> GetDocumentPage(
                 string documentId,
                 int pageNumber,
                 CancellationToken cancellationToken)
             {
                 try
                 {
                     _logger.LogInformation("Request to retrieve document {DocumentId} page {PageNumber} by user {UserId}",
                         documentId, pageNumber, User.Identity?.Name);
                     
                     var documentPage = await _documentViewerService.GetDocumentPageAsync(
                         documentId,
                         pageNumber,
                         cancellationToken);
                     
                     return Ok(documentPage);
                 }
                 catch (DocumentNotFoundException ex)
                 {
                     _logger.LogWarning(ex, "Document {DocumentId} not found", documentId);
                     return NotFound(new { message = ex.Message, documentId = ex.DocumentId, pageNumber = ex.PageNumber });
                 }
                 catch (ArgumentOutOfRangeException ex)
                 {
                     _logger.LogWarning(ex, "Invalid page number {PageNumber} for document {DocumentId}", pageNumber, documentId);
                     return BadRequest(new { message = ex.Message });
                 }
             }
             
             /// <summary>
             /// Retrieves document metadata (for all pages overview).
             /// </summary>
             /// <param name="documentId">Document identifier.</param>
             /// <param name="cancellationToken">Cancellation token.</param>
             /// <returns>Document metadata including total pages.</returns>
             [HttpGet("{documentId}/metadata")]
             [ProducesResponseType(StatusCodes.Status200OK)]
             [ProducesResponseType(StatusCodes.Status404NotFound)]
             public async Task<ActionResult<object>> GetDocumentMetadata(
                 string documentId,
                 CancellationToken cancellationToken)
             {
                 // Fetch first page to trigger validation
                 var firstPage = await _documentViewerService.GetDocumentPageAsync(documentId, 1, cancellationToken);
                 
                 return Ok(new
                 {
                     documentId = firstPage.DocumentId,
                     documentName = firstPage.DocumentName,
                     totalPages = firstPage.TotalPages,
                     contentType = firstPage.ContentType,
                     isDocumentAvailable = firstPage.IsDocumentAvailable
                 });
             }
         }
     }
     ```

6. **Update PatientDocument Entity**
   - File: `src/backend/PatientAccess.Data/Entities/PatientDocument.cs`
   - Add missing fields if not already present:
     ```csharp
     public sealed class PatientDocument
     {
         public string DocumentId { get; set; } = Guid.NewGuid().ToString();
         public int PatientId { get; set; }
         public string FileName { get; set; } = string.Empty;
         public string ContentType { get; set; } = "application/pdf";
         public int TotalPages { get; set; }
         public bool IsDeleted { get; set; } = false;
         public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
         public string BlobStoragePath { get; set; } = string.Empty;
     }
     ```

7. **Update ExceptionHandlingMiddleware**
   - File: `src/backend/PatientAccess.Web/Middleware/ExceptionHandlingMiddleware.cs`
   - Add DocumentNotFoundException handler:
     ```csharp
     private async Task HandleExceptionAsync(HttpContext context, Exception exception)
     {
         _logger.LogError(exception, "Unhandled exception occurred");
         
         var response = exception switch
         {
             DocumentNotFoundException ex => new
             {
                 StatusCode = StatusCodes.Status404NotFound,
                 Message = ex.Message,
                 DocumentId = ex.DocumentId,
                 PageNumber = ex.PageNumber
             },
             ValidationException => new
             {
                 StatusCode = StatusCodes.Status400BadRequest,
                 Message = exception.Message
             },
             _ => new
             {
                 StatusCode = StatusCodes.Status500InternalServerError,
                 Message = "An unexpected error occurred."
             }
         };
         
         context.Response.ContentType = "application/json";
         context.Response.StatusCode = response.StatusCode;
         
         await context.Response.WriteAsJsonAsync(response);
     }
     ```

8. **Register DocumentViewerService in DI Container**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add service registration:
     ```csharp
     // Azure Blob Storage
     builder.Services.AddSingleton(x =>
     {
         var connectionString = builder.Configuration["AzureBlobStorage:ConnectionString"];
         return new BlobServiceClient(connectionString);
     });
     
     // Document Viewer Service
     builder.Services.AddScoped<IDocumentViewerService, DocumentViewerService>();
     ```

9. **Add Configuration Settings**
   - File: `src/backend/PatientAccess.Web/appsettings.json`
   - Add Azure Blob Storage configuration:
     ```json
     {
       "AzureBlobStorage": {
         "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
         "ContainerName": "patient-documents"
       }
     }
     ```

10. **Create Integration Tests**
    - File: `src/backend/PatientAccess.Tests/Services/DocumentViewerServiceTests.cs`
    - Test cases:
      1. **Test_GetDocumentPage_ReturnsValidDto**
      2. **Test_GetDocumentPage_ThrowsNotFoundException_WhenDocumentDeleted**
      3. **Test_GetDocumentPage_ThrowsArgumentOutOfRange_WhenInvalidPageNumber**
      4. **Test_GetDocumentPage_UsesCachedResult_OnSecondCall**

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   ├── DTOs/
│   └── Interfaces/
├── PatientAccess.Web/
│   ├── Controllers/
│   └── Middleware/
│       └── ExceptionHandlingMiddleware.cs
└── PatientAccess.Data/
    └── Entities/
        └── PatientDocument.cs (from EP-006-II US_045)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/DocumentViewerService.cs | Document retrieval service |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IDocumentViewerService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/DTOs/DocumentPageDto.cs | Response DTO |
| CREATE | src/backend/PatientAccess.Business/Exceptions/DocumentNotFoundException.cs | Custom exception |
| CREATE | src/backend/PatientAccess.Web/Controllers/DocumentsController.cs | REST API controller |
| CREATE | src/backend/PatientAccess.Tests/Services/DocumentViewerServiceTests.cs | Integration tests |
| MODIFY | src/backend/PatientAccess.Web/Middleware/ExceptionHandlingMiddleware.cs | Handle DocumentNotFoundException |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register DocumentViewerService |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Azure Blob Storage config |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Azure Blob Storage SDK
- **Azure.Storage.Blobs**: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme
- **SAS Token Generation**: https://learn.microsoft.com/en-us/azure/storage/blobs/sas-service-create-dotnet

### ASP.NET Core Web API
- **Controller Documentation**: https://learn.microsoft.com/en-us/aspnet/core/web-api/
- **Exception Handling**: https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors

### Redis Caching
- **StackExchange.Redis**: https://stackexchange.github.io/StackExchange.Redis/

### Entity Framework Core
- **Query Performance**: https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build PatientAccess.sln

# Run tests
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj

# Run API locally
cd PatientAccess.Web
dotnet run
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/DocumentViewerServiceTests.cs`
- Test cases:
  1. **Test_GetDocumentPage_ReturnsValidDto**
     - Setup: Mock PatientDocument entity with DocumentId "doc-123", TotalPages = 10
     - Call: GetDocumentPageAsync("doc-123", 1)
     - Assert: Returns DocumentPageDto with PageImageUrl, TotalPages = 10
  2. **Test_GetDocumentPage_ThrowsNotFoundException_WhenDocumentDeleted**
     - Setup: Mock PatientDocument with IsDeleted = true
     - Call: GetDocumentPageAsync("doc-deleted", 1)
     - Assert: Throws DocumentNotFoundException
  3. **Test_GetDocumentPage_ThrowsArgumentOutOfRange_WhenInvalidPageNumber**
     - Setup: Mock PatientDocument with TotalPages = 5
     - Call: GetDocumentPageAsync("doc-123", 10)
     - Assert: Throws ArgumentOutOfRangeException
  4. **Test_GetDocumentPage_UsesCachedResult_OnSecondCall**
     - Setup: Call GetDocumentPageAsync("doc-123", 1) twice
     - Assert: Second call returns cached result (Redis hit)

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/DocumentsControllerTests.cs`
- Test cases:
  1. **Test_GetDocumentPage_Returns200_WhenValidRequest**
     - Request: GET /api/documents/doc-123/page/1
     - Assert: StatusCode = 200, Response contains PageImageUrl
  2. **Test_GetDocumentPage_Returns404_WhenDocumentNotFound**
     - Request: GET /api/documents/invalid-doc/page/1
     - Assert: StatusCode = 404, Response contains error message
  3. **Test_GetDocumentMetadata_ReturnsCorrectTotalPages**
     - Request: GET /api/documents/doc-123/metadata
     - Assert: StatusCode = 200, Response contains TotalPages

### Acceptance Criteria Validation
- **AC1**: ✅ Source page number retrieved from ExtractedClinicalData
- **AC2**: ✅ Document page displayed via PageImageUrl (SAS token for Azure Blob Storage)
- **Edge Case**: ✅ Deleted documents throw DocumentNotFoundException, frontend displays "Source document unavailable"

## Success Criteria Checklist
- [MANDATORY] DocumentsController GET /api/documents/{documentId}/page/{pageNumber} endpoint implemented
- [MANDATORY] DocumentViewerService retrieves document page from Azure Blob Storage
- [MANDATORY] SAS token generated for secure blob access (1 hour expiry)
- [MANDATORY] DocumentPageDto includes PageImageUrl, TotalPages, IsDocumentAvailable fields
- [MANDATORY] DocumentNotFoundException thrown when document deleted (IsDeleted = true)
- [MANDATORY] ArgumentOutOfRangeException thrown when pageNumber exceeds TotalPages
- [MANDATORY] Redis caching implemented (15-minute TTL) for frequently accessed pages
- [MANDATORY] ExceptionHandlingMiddleware handles DocumentNotFoundException (404 status)
- [MANDATORY] Role-based authorization: Staff and Admin roles only
- [MANDATORY] Application Insights logging for document access tracking
- [MANDATORY] Unit test: GetDocumentPage returns valid DTO
- [MANDATORY] Unit test: Throws DocumentNotFoundException when document deleted
- [MANDATORY] Integration test: Endpoint returns 200 for valid request
- [RECOMMENDED] GET /api/documents/{documentId}/metadata endpoint for overview
- [RECOMMENDED] Performance optimization: Async/await throughout service

## Estimated Effort
**4 hours** (Service + Controller + Azure Blob integration + caching + exception handling + tests)
