using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAccess.Business.BackgroundJobs;
using PatientAccess.Data;
using PatientAccess.Business.DTOs;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// API endpoints for knowledge base management (re-indexing, chunk status).
/// Restricted to Admin role only.
/// Implements AIR-R04 (separate indices per code system).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly ILogger<KnowledgeBaseController> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public KnowledgeBaseController(
        ILogger<KnowledgeBaseController> logger,
        PatientAccessDbContext context,
        IBackgroundJobClient backgroundJobClient)
    {
        _logger = logger;
        _context = context;
        _backgroundJobClient = backgroundJobClient;
    }

    /// <summary>
    /// Triggers re-indexing for specified code system.
    /// Enqueues background job for document chunking.
    /// </summary>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="request">Re-index request with document text or file path</param>
    [HttpPost("reindex/{codeSystem}")]
    public IActionResult TriggerReIndexing(string codeSystem, [FromBody] ReIndexRequest request)
    {
        if (!IsValidCodeSystem(codeSystem))
        {
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}. Valid values: ICD10, CPT, ClinicalTerminology" });
        }

        if (string.IsNullOrWhiteSpace(request.DocumentText) && string.IsNullOrWhiteSpace(request.SourceFilePath))
        {
            return BadRequest(new { Error = "Either DocumentText or SourceFilePath must be provided" });
        }

        _logger.LogInformation("Re-indexing request received for {CodeSystem} by user {UserId}",
            codeSystem,
            User.Identity?.Name);

        string jobId;

        if (!string.IsNullOrWhiteSpace(request.SourceFilePath))
        {
            // Enqueue file-based chunking job
            jobId = _backgroundJobClient.Enqueue<ChunkDocumentsJob>(
                job => job.ExecuteFromFileAsync(codeSystem, request.SourceFilePath, CancellationToken.None));
        }
        else
        {
            // Enqueue text-based chunking job
            jobId = _backgroundJobClient.Enqueue<ChunkDocumentsJob>(
                job => job.ExecuteAsync(codeSystem, request.DocumentText!, CancellationToken.None));
        }

        return Ok(new
        {
            JobId = jobId,
            Status = "Enqueued",
            CodeSystem = codeSystem,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets pending chunks for specified code system (not yet processed for embeddings).
    /// </summary>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    [HttpGet("chunks/{codeSystem}/pending")]
    public async Task<IActionResult> GetPendingChunks(string codeSystem)
    {
        if (!IsValidCodeSystem(codeSystem))
        {
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}. Valid values: ICD10, CPT, ClinicalTerminology" });
        }

        var pendingChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem && !c.IsProcessed)
            .OrderBy(c => c.ChunkIndex)
            .Select(c => new
            {
                c.Id,
                c.ChunkIndex,
                c.TokenCount,
                c.CreatedAt,
                TextPreview = c.SourceText.Length > 100 ? c.SourceText.Substring(0, 100) + "..." : c.SourceText
            })
            .ToListAsync();

        return Ok(new
        {
            CodeSystem = codeSystem,
            Count = pendingChunks.Count,
            Chunks = pendingChunks
        });
    }

    /// <summary>
    /// Gets chunk statistics for specified code system.
    /// </summary>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    [HttpGet("chunks/{codeSystem}/stats")]
    public async Task<IActionResult> GetChunkStatistics(string codeSystem)
    {
        if (!IsValidCodeSystem(codeSystem))
        {
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}. Valid values: ICD10, CPT, ClinicalTerminology" });
        }

        var chunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem)
            .ToListAsync();

        if (!chunks.Any())
        {
            return Ok(new
            {
                CodeSystem = codeSystem,
                TotalChunks = 0,
                Message = "No chunks found for this code system"
            });
        }

        var stats = new
        {
            CodeSystem = codeSystem,
            TotalChunks = chunks.Count,
            ProcessedChunks = chunks.Count(c => c.IsProcessed),
            PendingChunks = chunks.Count(c => !c.IsProcessed),
            AvgTokenCount = chunks.Average(c => c.TokenCount),
            MinTokenCount = chunks.Min(c => c.TokenCount),
            MaxTokenCount = chunks.Max(c => c.TokenCount),
            OldestChunk = chunks.Min(c => c.CreatedAt),
            NewestChunk = chunks.Max(c => c.CreatedAt)
        };

        return Ok(stats);
    }

    /// <summary>
    /// Triggers embedding generation for pending chunks of specified code system.
    /// Enqueues background job for Azure OpenAI embedding generation.
    /// </summary>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    [HttpPost("embeddings/generate/{codeSystem}")]
    public IActionResult TriggerEmbeddingGeneration(string codeSystem)
    {
        if (!IsValidCodeSystem(codeSystem))
        {
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}. Valid values: ICD10, CPT, ClinicalTerminology" });
        }

        _logger.LogInformation("Embedding generation request received for {CodeSystem} by user {UserId}",
            codeSystem,
            User.Identity?.Name);

        var jobId = _backgroundJobClient.Enqueue<GenerateEmbeddingsJob>(
            job => job.ExecuteAsync(codeSystem, CancellationToken.None));

        return Ok(new
        {
            JobId = jobId,
            Status = "Enqueued",
            CodeSystem = codeSystem,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets embedding generation progress for specified code system.
    /// </summary>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    [HttpGet("embeddings/{codeSystem}/progress")]
    public async Task<IActionResult> GetEmbeddingProgress(string codeSystem)
    {
        if (!IsValidCodeSystem(codeSystem))
        {
            return BadRequest(new { Error = $"Invalid code system: {codeSystem}. Valid values: ICD10, CPT, ClinicalTerminology" });
        }

        var totalChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem)
            .CountAsync();

        var processedChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem && c.IsProcessed)
            .CountAsync();

        var percentage = totalChunks > 0 ? (processedChunks * 100.0 / totalChunks) : 0;

        return Ok(new
        {
            CodeSystem = codeSystem,
            TotalChunks = totalChunks,
            ProcessedChunks = processedChunks,
            PendingChunks = totalChunks - processedChunks,
            PercentageComplete = Math.Round(percentage, 2)
        });
    }

    /// <summary>
    /// Validates code system parameter.
    /// </summary>
    private bool IsValidCodeSystem(string codeSystem)
    {
        return codeSystem is "ICD10" or "CPT" or "ClinicalTerminology";
    }
}

/// <summary>
/// Request DTO for re-indexing endpoint.
/// </summary>
public class ReIndexRequest
{
    /// <summary>
    /// Document text to chunk (alternative to SourceFilePath)
    /// </summary>
    public string? DocumentText { get; set; }

    /// <summary>
    /// File path to source document (alternative to DocumentText)
    /// </summary>
    public string? SourceFilePath { get; set; }
}
