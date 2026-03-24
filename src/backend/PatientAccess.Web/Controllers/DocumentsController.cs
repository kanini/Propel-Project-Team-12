using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Services;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Documents API controller for clinical document upload (US_042).
/// Provides chunked upload endpoints with real-time progress tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all document endpoints
public class DocumentsController : ControllerBase
{
    private readonly DocumentUploadService _uploadService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        DocumentUploadService uploadService,
        ILogger<DocumentsController> logger)
    {
        _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            return StatusCode(500, new { message = "An error occurred while finalizing upload" });
        }
    }
}
