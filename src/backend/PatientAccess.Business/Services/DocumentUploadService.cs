using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Document upload service for chunked uploads with real-time progress tracking (US_042).
/// Orchestrates upload lifecycle: initialization → chunk upload → finalization → database persistence.
/// </summary>
public class DocumentUploadService
{
    private readonly ChunkedUploadManager _uploadManager;
    private readonly IPusherService _pusherService;
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<DocumentUploadService> _logger;

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private const string AllowedMimeType = "application/pdf";

    public DocumentUploadService(
        ChunkedUploadManager uploadManager,
        IPusherService pusherService,
        PatientAccessDbContext context,
        ILogger<DocumentUploadService> logger)
    {
        _uploadManager = uploadManager ?? throw new ArgumentNullException(nameof(uploadManager));
        _pusherService = pusherService ?? throw new ArgumentNullException(nameof(pusherService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes a chunked upload session with validation (AC1)
    /// </summary>
    public async Task<InitializeUploadResponseDto> InitializeUploadAsync(InitializeUploadRequestDto request, Guid uploadedByUserId)
    {
        _logger.LogInformation("Initializing upload for file: {FileName} ({FileSize} bytes)", request.FileName, request.FileSize);

        // Validate file metadata (AC1, AC4)
        ValidateFileMetadata(request.FileName, request.FileSize, request.MimeType);

        // Verify patient exists
        var patientExists = await _context.Users.AnyAsync(u => u.UserId == request.PatientId && u.Role == UserRole.Patient);
        if (!patientExists)
        {
            throw new ArgumentException($"Patient {request.PatientId} not found", nameof(request.PatientId));
        }

        // Create upload session
        var sessionId = _uploadManager.CreateSession(
            request.FileName,
            request.FileSize,
            request.MimeType,
            request.TotalChunks,
            request.PatientId,
            uploadedByUserId);

        var response = new InitializeUploadResponseDto
        {
            UploadSessionId = sessionId,
            ChunkSize = _uploadManager.GetChunkSize(),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            PusherChannel = $"document-upload-{sessionId}"
        };

        _logger.LogInformation("Upload session initialized: {SessionId} for patient {PatientId}", sessionId, request.PatientId);

        return response;
    }

    /// <summary>
    /// Uploads a single chunk with real-time progress tracking (AC2)
    /// </summary>
    public async Task<ChunkUploadResponseDto> UploadChunkAsync(Guid sessionId, int chunkIndex, Stream chunkStream)
    {
        if (chunkStream == null || chunkStream.Length == 0)
        {
            throw new ArgumentException("Chunk data cannot be empty", nameof(chunkStream));
        }

        _logger.LogDebug("Uploading chunk {ChunkIndex} for session {SessionId}", chunkIndex, sessionId);

        // Save chunk to temporary storage
        var (chunksReceived, percentComplete) = await _uploadManager.SaveChunkAsync(
            sessionId,
            chunkIndex,
            chunkStream);

        var totalChunks = chunksReceived; // Need to query session for actual total
        var status = percentComplete >= 100 ? "Complete" : "Uploading";

        var response = new ChunkUploadResponseDto
        {
            ChunksReceived = chunksReceived,
            TotalChunks = totalChunks,
            PercentComplete = percentComplete,
            Status = status
        };

        // Trigger Pusher event for real-time progress (AC2)
        await TriggerChunkUploadedEventAsync(sessionId, chunksReceived, totalChunks, percentComplete);

        return response;
    }

    /// <summary>
    /// Finalizes upload, assembles file, and creates database record (AC3)
    /// </summary>
    public async Task<DocumentUploadResponseDto> FinalizeUploadAsync(FinalizeUploadRequestDto request)
    {
        _logger.LogInformation("Finalizing upload session {SessionId}", request.UploadSessionId);

        try
        {
            // Validate session is complete
            if (!_uploadManager.IsSessionComplete(request.UploadSessionId))
            {
                throw new InvalidOperationException("Cannot finalize incomplete upload. Upload all chunks first.");
            }

            // Assemble chunks into final file
            var (filePath, session) = await _uploadManager.FinalizeSessionAsync(request.UploadSessionId);

            // Create database record
            var document = new ClinicalDocument
            {
                DocumentId = Guid.NewGuid(),
                PatientId = session.PatientId,
                UploadedBy = session.UploadedBy,
                FileName = session.FileName,
                FileSize = session.FileSize,
                FileType = session.MimeType,
                StoragePath = filePath,
                ProcessingStatus = ProcessingStatus.Uploaded,
                UploadedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ClinicalDocuments.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} created for patient {PatientId}. File: {FileName} ({FileSize} bytes)",
                document.DocumentId, document.PatientId, document.FileName, document.FileSize);

            // Cleanup temporary chunks
            _uploadManager.CleanupSession(request.UploadSessionId);

            // Trigger completion event (AC3)
            await TriggerUploadCompleteEventAsync(request.UploadSessionId, document);

            var response = new DocumentUploadResponseDto
            {
                DocumentId = document.DocumentId,
                FileName = document.FileName,
                FileSize = document.FileSize,
                Status = "Uploaded — Processing pending",
                UploadedAt = document.UploadedAt,
                PatientId = document.PatientId
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finalize upload session {SessionId}", request.UploadSessionId);

            // Trigger failure event
            await TriggerUploadFailedEventAsync(request.UploadSessionId, ex.Message);

            // Cleanup on failure
            _uploadManager.CleanupSession(request.UploadSessionId);

            throw;
        }
    }

    /// <summary>
    /// Validates file metadata against requirements (AC1, AC4)
    /// </summary>
    private void ValidateFileMetadata(string fileName, long fileSize, string mimeType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required", nameof(fileName));
        }

        if (fileSize <= 0 || fileSize > MaxFileSizeBytes)
        {
            throw new ArgumentException($"Only PDF files up to 10MB are supported. File size: {fileSize / 1024.0 / 1024.0:F2} MB", nameof(fileSize));
        }

        if (!string.Equals(mimeType, AllowedMimeType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Only PDF files are supported. Provided MIME type: {mimeType}", nameof(mimeType));
        }
    }

    /// <summary>
    /// Triggers Pusher event for chunk upload progress (AC2)
    /// </summary>
    private async Task TriggerChunkUploadedEventAsync(Guid sessionId, int chunksReceived, int totalChunks, int percentComplete)
    {
        var channel = $"document-upload-{sessionId}";
        var eventData = new
        {
            chunksReceived,
            totalChunks,
            percentComplete,
            status = "Uploading"
        };

        await _pusherService.TriggerEventAsync(channel, "chunk-uploaded", eventData);
    }

    /// <summary>
    /// Triggers Pusher event for upload completion (AC3)
    /// </summary>
    private async Task TriggerUploadCompleteEventAsync(Guid sessionId, ClinicalDocument document)
    {
        var channel = $"document-upload-{sessionId}";
        var eventData = new
        {
            documentId = document.DocumentId,
            fileName = document.FileName,
            fileSize = document.FileSize,
            status = "Uploaded — Processing pending",
            uploadedAt = document.UploadedAt
        };

        await _pusherService.TriggerEventAsync(channel, "upload-complete", eventData);
    }

    /// <summary>
    /// Triggers Pusher event for upload failure
    /// </summary>
    private async Task TriggerUploadFailedEventAsync(Guid sessionId, string errorMessage)
    {
        var channel = $"document-upload-{sessionId}";
        var eventData = new
        {
            error = errorMessage,
            status = "Failed"
        };

        await _pusherService.TriggerEventAsync(channel, "upload-failed", eventData);
    }
}
