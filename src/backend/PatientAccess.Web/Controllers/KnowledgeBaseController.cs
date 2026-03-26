using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAccess.Business.BackgroundJobs;
using PatientAccess.Data;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Admin-only controller for knowledge base management and re-indexing operations (AIR-R01, AIR-R04).
/// Triggers document chunking and embedding generation for ICD-10, CPT, and clinical terminology.
/// All endpoints require Admin role.
/// </summary>
[ApiController]
[Route("api/knowledge-base")]
[Authorize(Policy = "AdminOnly")] // Admin-only access (re-indexing affects entire knowledge base)
public class KnowledgeBaseController : ControllerBase
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<KnowledgeBaseController> _logger;

    public KnowledgeBaseController(
        PatientAccessDbContext context,
        ILogger<KnowledgeBaseController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Triggers re-indexing (document chunking) for a specified code system.
    /// Enqueues a Hangfire background job to chunk the source document into 512-token segments.
    /// </summary>
    /// <param name="codeSystem">Code system to re-index: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="request">Re-indexing request with source document path</param>
    /// <returns>Job ID and status</returns>
    /// <response code="200">Re-indexing job enqueued successfully</response>
    /// <response code="400">Invalid code system or request</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpPost("reindex/{codeSystem}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult TriggerReIndexing(string codeSystem, [FromBody] ReIndexRequest request)
    {
        _logger.LogInformation("Re-indexing triggered for {CodeSystem} by admin", codeSystem);

        // Validate code system
        var validCodeSystems = new[] { "ICD10", "CPT", "ClinicalTerminology" };
        if (!validCodeSystems.Contains(codeSystem, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid code system specified: {CodeSystem}", codeSystem);
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}. Must be 'ICD10', 'CPT', or 'ClinicalTerminology'." });
        }

        // Validate request
        if (string.IsNullOrWhiteSpace(request?.SourceDocumentPath))
        {
            _logger.LogWarning("Re-indexing request missing SourceDocumentPath");
            return BadRequest(new { Error = "SourceDocumentPath is required" });
        }

        // Enqueue Hangfire background job
        var jobId = BackgroundJob.Enqueue<ChunkDocumentsJob>(
            job => job.ExecuteAsync(codeSystem, request.SourceDocumentPath, CancellationToken.None));

        _logger.LogInformation("Re-indexing job {JobId} enqueued for {CodeSystem} from {Path}",
            jobId, codeSystem, request.SourceDocumentPath);

        return Ok(new
        {
            JobId = jobId,
            Status = "Enqueued",
            CodeSystem = codeSystem,
            SourceDocumentPath = request.SourceDocumentPath,
            EnqueuedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets pending document chunks for a specified code system.
    /// Returns chunks that have not yet been processed (embedding generation pending).
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <returns>List of pending chunks</returns>
    /// <response code="200">Pending chunks retrieved successfully</response>
    /// <response code="400">Invalid code system</response>
    [HttpGet("chunks/{codeSystem}/pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingChunks(string codeSystem)
    {
        _logger.LogInformation("Fetching pending chunks for {CodeSystem}", codeSystem);

        // Validate code system
        var validCodeSystems = new[] { "ICD10", "CPT", "ClinicalTerminology" };
        if (!validCodeSystems.Contains(codeSystem, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}" });
        }

        var pendingChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem && !c.IsProcessed)
            .OrderBy(c => c.ChunkIndex)
            .Select(c => new
            {
                c.Id,
                c.ChunkIndex,
                c.TokenCount,
                c.StartToken,
                c.EndToken,
                c.OverlapWithPrevious,
                c.CreatedAt,
                SourceTextPreview = c.SourceText.Length > 100 ? c.SourceText.Substring(0, 100) + "..." : c.SourceText
            })
            .ToListAsync();

        _logger.LogInformation("Found {Count} pending chunks for {CodeSystem}", pendingChunks.Count, codeSystem);

        return Ok(new
        {
            CodeSystem = codeSystem,
            Count = pendingChunks.Count,
            Chunks = pendingChunks
        });
    }

    /// <summary>
    /// Gets chunking statistics for all code systems.
    /// </summary>
    /// <returns>Chunking statistics (total, pending, processed)</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    [HttpGet("chunks/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChunkingStats()
    {
        _logger.LogInformation("Fetching chunking statistics");

        var stats = await _context.DocumentChunks
            .GroupBy(c => c.CodeSystem)
            .Select(g => new
            {
                CodeSystem = g.Key,
                TotalChunks = g.Count(),
                PendingChunks = g.Count(c => !c.IsProcessed),
                ProcessedChunks = g.Count(c => c.IsProcessed),
                AvgTokenCount = g.Average(c => c.TokenCount),
                LastCreatedAt = g.Max(c => c.CreatedAt)
            })
            .ToListAsync();

        return Ok(new
        {
            Statistics = stats,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Triggers embedding generation for a specified code system.
    /// Enqueues a Hangfire background job to generate embeddings for all pending chunks.
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <returns>Job ID and status</returns>
    /// <response code="200">Embedding generation job enqueued successfully</response>
    /// <response code="400">Invalid code system</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpPost("embeddings/generate/{codeSystem}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult TriggerEmbeddingGeneration(string codeSystem)
    {
        _logger.LogInformation("Embedding generation triggered for {CodeSystem} by admin", codeSystem);

        // Validate code system
        var validCodeSystems = new[] { "ICD10", "CPT", "ClinicalTerminology" };
        if (!validCodeSystems.Contains(codeSystem, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid code system specified: {CodeSystem}", codeSystem);
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}. Must be 'ICD10', 'CPT', or 'ClinicalTerminology'." });
        }

        // Enqueue Hangfire background job
        var jobId = BackgroundJob.Enqueue<GenerateEmbeddingsJob>(
            job => job.ExecuteAsync(codeSystem, CancellationToken.None));

        _logger.LogInformation("Embedding generation job {JobId} enqueued for {CodeSystem}",
            jobId, codeSystem);

        return Ok(new
        {
            JobId = jobId,
            Status = "Enqueued",
            CodeSystem = codeSystem,
            EnqueuedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets embedding generation progress for a specified code system.
    /// Returns percentage of chunks processed and metadata.
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <returns>Progress information (total, processed, percentage)</returns>
    /// <response code="200">Progress retrieved successfully</response>
    /// <response code="400">Invalid code system</response>
    [HttpGet("embeddings/{codeSystem}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmbeddingProgress(string codeSystem)
    {
        _logger.LogInformation("Fetching embedding progress for {CodeSystem}", codeSystem);

        // Validate code system
        var validCodeSystems = new[] { "ICD10", "CPT", "ClinicalTerminology" };
        if (!validCodeSystems.Contains(codeSystem, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}" });
        }

        var totalChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem)
            .CountAsync();

        var processedChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem && c.IsProcessed)
            .CountAsync();

        var percentage = totalChunks > 0 ? (processedChunks * 100.0 / totalChunks) : 0;

        _logger.LogInformation("Embedding progress for {CodeSystem}: {Processed}/{Total} ({Percentage}%)",
            codeSystem, processedChunks, totalChunks, percentage);

        return Ok(new
        {
            CodeSystem = codeSystem,
            TotalChunks = totalChunks,
            ProcessedChunks = processedChunks,
            PercentageComplete = Math.Round(percentage, 2),
            Timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Request model for re-indexing operation.
/// </summary>
public class ReIndexRequest
{
    /// <summary>
    /// File path or blob URI to source document.
    /// </summary>
    public string SourceDocumentPath { get; set; } = string.Empty;
}
