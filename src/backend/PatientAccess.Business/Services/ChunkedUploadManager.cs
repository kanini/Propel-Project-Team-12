using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace PatientAccess.Business.Services;

/// <summary>
/// Manages chunked upload sessions with in-memory tracking (US_042).
/// Handles session lifecycle, chunk storage, and file assembly.
/// </summary>
public class ChunkedUploadManager
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChunkedUploadManager> _logger;
    private readonly string _tempUploadPath;
    private readonly string _permanentUploadPath;
    private const int DefaultChunkSizeMB = 1; // 1MB chunks
    private const int SessionExpirationMinutes = 60;

    public ChunkedUploadManager(
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<ChunkedUploadManager> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Read storage paths from configuration
        _tempUploadPath = _configuration["FileStorage:TempUploadPath"] ?? Path.Combine(Path.GetTempPath(), "clinical-uploads", "temp");
        _permanentUploadPath = _configuration["FileStorage:PermanentUploadPath"] ?? Path.Combine(Path.GetTempPath(), "clinical-uploads", "permanent");

        // Ensure directories exist
        Directory.CreateDirectory(_tempUploadPath);
        Directory.CreateDirectory(_permanentUploadPath);

        _logger.LogInformation("ChunkedUploadManager initialized. Temp: {TempPath}, Permanent: {PermanentPath}", _tempUploadPath, _permanentUploadPath);
    }

    /// <summary>
    /// Creates a new upload session with metadata tracking
    /// </summary>
    public Guid CreateSession(string fileName, long fileSize, string mimeType, int totalChunks, Guid patientId, Guid uploadedBy)
    {
        var sessionId = Guid.NewGuid();
        var session = new UploadSession
        {
            SessionId = sessionId,
            FileName = fileName,
            FileSize = fileSize,
            MimeType = mimeType,
            TotalChunks = totalChunks,
            PatientId = patientId,
            UploadedBy = uploadedBy,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(SessionExpirationMinutes),
            CompletedChunkIndices = new ConcurrentBag<int>()
        };

        // Store session in memory cache with sliding expiration
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(SessionExpirationMinutes))
            .SetPriority(CacheItemPriority.Normal);

        _cache.Set(GetSessionKey(sessionId), session, cacheOptions);

        // Create session-specific temp directory
        var sessionTempPath = GetSessionTempPath(sessionId);
        Directory.CreateDirectory(sessionTempPath);

        _logger.LogInformation("Upload session created: {SessionId} for file {FileName} ({FileSize} bytes, {TotalChunks} chunks)", 
            sessionId, fileName, fileSize, totalChunks);

        return sessionId;
    }

    /// <summary>
    /// Saves a chunk to temporary storage and updates session progress
    /// </summary>
    public async Task<(int ChunksReceived, int PercentComplete)> SaveChunkAsync(Guid sessionId, int chunkIndex, Stream chunkData)
    {
        var session = GetSession(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Upload session {sessionId} not found or expired");
        }

        _logger.LogDebug("Saving chunk {ChunkIndex} for session {SessionId}", chunkIndex, sessionId);

        // Validate chunk index
        if (chunkIndex < 0 || chunkIndex >= session.TotalChunks)
        {
            throw new ArgumentException($"Invalid chunk index {chunkIndex}. Expected 0-{session.TotalChunks - 1}", nameof(chunkIndex));
        }

        // Save chunk to temp file
        var chunkPath = GetChunkPath(sessionId, chunkIndex);
        using (var fileStream = new FileStream(chunkPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await chunkData.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
        }

        // Mark chunk as completed (thread-safe)
        if (!session.CompletedChunkIndices.Contains(chunkIndex))
        {
            session.CompletedChunkIndices.Add(chunkIndex);
        }

        var chunksReceived = session.CompletedChunkIndices.Count;
        var percentComplete = (int)((chunksReceived / (double)session.TotalChunks) * 100);

        _logger.LogInformation("Chunk {ChunkIndex} saved for session {SessionId}. Progress: {ChunksReceived}/{TotalChunks} ({PercentComplete}%)", 
            chunkIndex, sessionId, chunksReceived, session.TotalChunks, percentComplete);

        return (chunksReceived, percentComplete);
    }

    /// <summary>
    /// Checks if all chunks have been uploaded
    /// </summary>
    public bool IsSessionComplete(Guid sessionId)
    {
        var session = GetSession(sessionId);
        if (session == null) return false;

        return session.CompletedChunkIndices.Count == session.TotalChunks;
    }

    /// <summary>
    /// Assembles all chunks into final file and returns permanent file path and document ID
    /// </summary>
    public async Task<(string FilePath, Guid DocumentId, UploadSession Session)> FinalizeSessionAsync(Guid sessionId, Guid documentId)
    {
        var session = GetSession(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Upload session {sessionId} not found or expired");
        }

        if (!IsSessionComplete(sessionId))
        {
            throw new InvalidOperationException($"Cannot finalize incomplete upload. Received {session.CompletedChunkIndices.Count}/{session.TotalChunks} chunks");
        }

        _logger.LogInformation("Finalizing upload session {SessionId}. Assembling {TotalChunks} chunks", sessionId, session.TotalChunks);

        // Create permanent file path: {permanentPath}/{patientId}/{documentId}.pdf
        var patientDir = Path.Combine(_permanentUploadPath, session.PatientId.ToString());
        Directory.CreateDirectory(patientDir);

        var extension = Path.GetExtension(session.FileName);
        var permanentPath = Path.Combine(patientDir, $"{documentId}{extension}");

        // Assemble chunks sequentially into final file
        using (var finalStream = new FileStream(permanentPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            for (int i = 0; i < session.TotalChunks; i++)
            {
                var chunkPath = GetChunkPath(sessionId, i);
                if (!File.Exists(chunkPath))
                {
                    throw new InvalidOperationException($"Chunk {i} missing for session {sessionId}");
                }

                using (var chunkStream = File.OpenRead(chunkPath))
                {
                    await chunkStream.CopyToAsync(finalStream);
                }
            }

            await finalStream.FlushAsync();
        }

        _logger.LogInformation("Upload session {SessionId} finalized. Document {DocumentId} saved to {FilePath}", sessionId, documentId, permanentPath);

        return (permanentPath, documentId, session);
    }

    /// <summary>
    /// Cleans up temporary chunks and removes session from cache
    /// </summary>
    public void CleanupSession(Guid sessionId)
    {
        _logger.LogInformation("Cleaning up upload session {SessionId}", sessionId);

        // Delete session temp directory with all chunks
        var sessionTempPath = GetSessionTempPath(sessionId);
        if (Directory.Exists(sessionTempPath))
        {
            try
            {
                Directory.Delete(sessionTempPath, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temp directory for session {SessionId}", sessionId);
            }
        }

        // Remove session from cache
        _cache.Remove(GetSessionKey(sessionId));
    }

    /// <summary>
    /// Gets recommended chunk size in bytes
    /// </summary>
    public int GetChunkSize() => DefaultChunkSizeMB * 1024 * 1024; // 1MB

    /// <summary>
    /// Retrieves active upload session
    /// </summary>
    private UploadSession? GetSession(Guid sessionId)
    {
        return _cache.Get<UploadSession>(GetSessionKey(sessionId));
    }

    private string GetSessionKey(Guid sessionId) => $"upload-session:{sessionId}";
    private string GetSessionTempPath(Guid sessionId) => Path.Combine(_tempUploadPath, sessionId.ToString());
    private string GetChunkPath(Guid sessionId, int chunkIndex) => Path.Combine(GetSessionTempPath(sessionId), $"chunk_{chunkIndex}.tmp");

    /// <summary>
    /// Upload session state tracking
    /// </summary>
    public class UploadSession
    {
        public Guid SessionId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public int TotalChunks { get; set; }
        public Guid PatientId { get; set; }
        public Guid UploadedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public ConcurrentBag<int> CompletedChunkIndices { get; set; } = new ConcurrentBag<int>();
    }
}
