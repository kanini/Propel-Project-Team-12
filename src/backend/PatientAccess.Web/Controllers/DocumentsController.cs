using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Services;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Documents controller for clinical document management (US_042, US_044, US_067).
/// Provides endpoints for document upload, status tracking, and retry functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly DocumentUploadService _uploadService;
    private readonly IDocumentService _documentService;
    private readonly IDocumentProcessingService _processingService;
    private readonly PatientAccessDbContext _context;
    private readonly IAuditLogService _auditService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        IDocumentProcessingService processingService,
        PatientAccessDbContext context,
        IAuditLogService auditService,
        DocumentUploadService uploadService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get recent clinical documents for dashboard display (US_067, AC6).
    /// Returns most recent documents ordered by upload date.
    /// </summary>
    /// <param name="limit">Maximum number of documents to return (default: 3, max: 10)</param>
    /// <returns>List of recent documents with processing status</returns>
    [HttpGet("recent")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(List<RecentDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<RecentDocumentDto>>> GetRecentAsync(
        [FromQuery] int limit = 3)
    {
        try
        {
            if (limit < 1 || limit > 10)
            {
                return BadRequest(new { error = "Limit must be between 1 and 10" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            _logger.LogInformation("Retrieving {Limit} recent documents for user {UserId}", limit, userId);

            // TODO: Implement PHI access logging when audit service supports it (NFR-007)
            // await _auditService.LogAccessAsync(userId, "ClinicalDocument", "ViewRecent");

            var documents = await _documentService.GetRecentDocumentsAsync(userId, limit);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent documents");
            return StatusCode(500, new { error = "Unable to retrieve documents" });
        }
    }

    /// <summary>
    /// Get all clinical documents for authenticated user with status tracking (US_044, AC1).
    /// Returns documents ordered by upload date descending.
    /// </summary>
    /// <returns>List of all user documents with processing status</returns>
    /// <response code="200">Documents retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DocumentStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DocumentStatusDto>>> GetAllDocumentsAsync()
    {
        try
        {
            // Extract current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            _logger.LogInformation("Retrieving all documents for user {UserId}", userId);

            // Query documents for authenticated user with ownership filtering
            var documents = await _context.ClinicalDocuments
                .AsNoTracking() // Performance optimization for read-only query
                .Where(d => d.UploadedBy == userId) // Ownership validation (NFR-008)
                .OrderByDescending(d => d.UploadedAt) // Newest first (AC1)
                .Select(d => new DocumentStatusDto
                {
                    Id = d.DocumentId,
                    FileName = d.FileName,
                    UploadedAt = d.UploadedAt,
                    FileSize = d.FileSize,
                    Status = d.ProcessingStatus.ToString(),
                    ProcessingTimeMs = d.ProcessedAt.HasValue 
                        ? (long)(d.ProcessedAt.Value - d.UploadedAt).TotalMilliseconds 
                        : null,
                    ErrorMessage = d.ErrorMessage,
                    ProcessedAt = d.ProcessedAt,
                    // Calculate IsStuckProcessing: Processing status AND >5 minutes elapsed
                    IsStuckProcessing = d.ProcessingStatus == ProcessingStatus.Processing 
                        && (DateTime.UtcNow - d.UploadedAt).TotalMinutes > 5
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} documents for user {UserId}", documents.Count, userId);

            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for user");
            return StatusCode(500, new { error = "Unable to retrieve documents. Please try again later." });
        }
    }


    /// <summary>
    /// Initializes a chunked upload session with file validation (AC1, AC4).
    /// Validates PDF format and 10MB size limit before accepting upload.
    /// </summary>
    /// <param name="request">Upload initialization request with file metadata</param>
    /// <returns>Upload session details including session ID and Pusher channel</returns>
    /// <response code="201">Upload session created successfully</response>
    /// <response code="400">Invalid file metadata (non-PDF or oversized file)</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("upload/initialize")]
    [ProducesResponseType(typeof(InitializeUploadResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InitializeUpload([FromBody] InitializeUploadRequestDto request)
    {
        try
        {
            // Get authenticated user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            var response = await _uploadService.InitializeUploadAsync(request, userId);

            _logger.LogInformation("Upload session initialized: {SessionId} by user {UserId}", response.UploadSessionId, userId);

            return CreatedAtAction(nameof(InitializeUpload), new { sessionId = response.UploadSessionId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid upload request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing upload");
            return StatusCode(500, new { message = "An error occurred while initializing upload" });
        }
    }

    /// <summary>
    /// Uploads a single chunk with progress tracking (AC2).
    /// Accepts multipart form data with chunk binary.
    /// Broadcasts real-time progress via Pusher Channels.
    /// </summary>
    /// <param name="request">Chunk upload request with session ID, chunk index, and binary data</param>
    /// <returns>Chunk upload status with progress percentage</returns>
    /// <response code="200">Chunk uploaded successfully</response>
    /// <response code="400">Invalid chunk data or session</response>
    /// <response code="404">Upload session not found or expired</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("upload/chunk")]
    [RequestSizeLimit(2 * 1024 * 1024)] // 2MB max chunk size
    [RequestFormLimits(MultipartBodyLengthLimit = 2 * 1024 * 1024)]
    [ProducesResponseType(typeof(ChunkUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadChunk([FromForm] Guid uploadSessionId, [FromForm] int chunkIndex, IFormFile chunkData)
    {
        try
        {
            if (chunkData == null || chunkData.Length == 0)
            {
                return BadRequest(new { message = "Chunk data is required" });
            }

            using var chunkStream = chunkData.OpenReadStream();
            var response = await _uploadService.UploadChunkAsync(uploadSessionId, chunkIndex, chunkStream);

            _logger.LogDebug("Chunk {ChunkIndex} uploaded for session {SessionId}. Progress: {PercentComplete}%",
                chunkIndex, uploadSessionId, response.PercentComplete);

            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found or expired"))
        {
            _logger.LogWarning("Upload session not found: {SessionId}", uploadSessionId);
            return NotFound(new { message = $"Upload session {uploadSessionId} not found or expired" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid chunk upload: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading chunk {ChunkIndex} for session {SessionId}",
                chunkIndex, uploadSessionId);
            return StatusCode(500, new { message = "An error occurred while uploading chunk" });
        }
    }

    /// <summary>
    /// Finalizes chunked upload, assembles file, and creates database record (AC3).
    /// Returns document metadata with confirmation message.
    /// </summary>
    /// <param name="request">Finalization request with session ID</param>
    /// <returns>Document upload confirmation with metadata</returns>
    /// <response code="200">Upload finalized successfully, document created</response>
    /// <response code="400">Upload incomplete or session invalid</response>
    /// <response code="404">Upload session not found</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("upload/finalize")]
    [ProducesResponseType(typeof(DocumentUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FinalizeUpload([FromBody] FinalizeUploadRequestDto request)
    {
        try
        {
            var response = await _uploadService.FinalizeUploadAsync(request);

            _logger.LogInformation("Upload finalized. Document {DocumentId} created for session {SessionId}",
                response.DocumentId, request.UploadSessionId);

            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found or expired"))
        {
            _logger.LogWarning("Upload session not found: {SessionId}", request.UploadSessionId);
            return NotFound(new { message = $"Upload session {request.UploadSessionId} not found or expired" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("incomplete"))
        {
            _logger.LogWarning("Attempted to finalize incomplete upload: {SessionId}", request.UploadSessionId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing upload session {SessionId}", request.UploadSessionId);
            return StatusCode(500, new { message = "An error occurred while finalizing upload", detail = ex.Message });
        }
    }

    /// <summary>
    /// Retry processing for a failed document (US_044, Edge Case).
    /// Re-enqueues document in Hangfire for processing pipeline.
    /// </summary>
    /// <param name="id">Document unique identifier</param>
    /// <returns>Updated document status</returns>
    /// <response code="200">Document retry enqueued successfully</response>
    /// <response code="400">Document is not in Failed status</response>
    /// <response code="404">Document not found</response>
    /// <response code="403">User does not have permission to retry this document</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("{id:guid}/retry")]
    [ProducesResponseType(typeof(DocumentStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DocumentStatusDto>> RetryProcessingAsync(Guid id)
    {
        try
        {
            // Extract current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            // Load document with ownership validation
            var document = await _context.ClinicalDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for retry", id);
                return NotFound(new { error = $"Document {id} not found" });
            }

            // Ownership validation (NFR-008): prevent users from retrying documents uploaded by others
            if (document.UploadedBy != userId)
            {
                _logger.LogWarning("User {UserId} attempted to retry document {DocumentId} uploaded by {UploadedBy}",
                    userId, id, document.UploadedBy);
                return Forbid();
            }

            // Validate document status: only Failed documents can be retried
            if (document.ProcessingStatus != ProcessingStatus.Failed)
            {
                _logger.LogWarning("Attempted to retry document {DocumentId} with status {Status}",
                    id, document.ProcessingStatus);
                return BadRequest(new { error = $"Cannot retry document with status '{document.ProcessingStatus}'. Only Failed documents can be retried." });
            }

            _logger.LogInformation("Retrying document {DocumentId} for user {UserId}", id, userId);

            // Enqueue retry job via processing service
            await _processingService.RetryProcessingAsync(id);

            // Audit logging for retry action
            // TODO: Implement comprehensive audit logging when service supports it
            _logger.LogInformation("Document {DocumentId} retry initiated by user {UserId}", id, userId);

            // Return updated document status
            var updatedDocument = await _context.ClinicalDocuments
                .AsNoTracking()
                .Where(d => d.DocumentId == id)
                .Select(d => new DocumentStatusDto
                {
                    Id = d.DocumentId,
                    FileName = d.FileName,
                    UploadedAt = d.UploadedAt,
                    FileSize = d.FileSize,
                    Status = d.ProcessingStatus.ToString(),
                    ProcessingTimeMs = d.ProcessedAt.HasValue
                        ? (long)(d.ProcessedAt.Value - d.UploadedAt).TotalMilliseconds
                        : null,
                    ErrorMessage = d.ErrorMessage,
                    ProcessedAt = d.ProcessedAt,
                    IsStuckProcessing = false // Just reset, not stuck yet
                })
                .FirstAsync();

            return Ok(updatedDocument);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not in Failed status"))
        {
            _logger.LogWarning("Retry validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying document {DocumentId}", id);
            return StatusCode(500, new { error = "Unable to retry document processing. Please try again later." });
        }
    }
}
