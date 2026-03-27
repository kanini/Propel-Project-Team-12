using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// API endpoints for medical coding knowledge base search.
/// Implements AIR-R02 (top-5 retrieval) and AIR-R03 (hybrid retrieval).
/// Requires authentication for access.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KnowledgeController : ControllerBase
{
    private readonly ILogger<KnowledgeController> _logger;
    private readonly IHybridRetrievalService _retrievalService;

    public KnowledgeController(
        ILogger<KnowledgeController> logger,
        IHybridRetrievalService retrievalService)
    {
        _logger = logger;
        _retrievalService = retrievalService;
    }

    /// <summary>
    /// Searches medical coding knowledge base using hybrid retrieval.
    /// Combines semantic similarity (pgvector) with keyword matching (FTS).
    /// Returns top-K results with cosine similarity above threshold (default: 0.75 per AIR-R02).
    /// </summary>
    /// <param name="query">Search query (e.g., "Type 2 Diabetes Mellitus")</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of top results to return (default: 5 per AIR-R02)</param>
    /// <param name="minSimilarityThreshold">Minimum cosine similarity threshold (default: 0.75)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with similarity scores</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(CodeSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] string codeSystem,
        [FromQuery] int topK = 5,
        [FromQuery] double minSimilarityThreshold = 0.75,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { Error = "Query parameter cannot be empty" });
        }

        if (query.Length < 2 || query.Length > 500)
        {
            return BadRequest(new { Error = "Query must be between 2 and 500 characters" });
        }

        if (!new[] { "ICD10", "CPT", "ClinicalTerminology" }.Contains(codeSystem))
        {
            return BadRequest(new { Error = "Invalid code system. Must be ICD10, CPT, or ClinicalTerminology" });
        }

        if (topK < 1 || topK > 50)
        {
            return BadRequest(new { Error = "TopK must be between 1 and 50" });
        }

        if (minSimilarityThreshold < 0 || minSimilarityThreshold > 1)
        {
            return BadRequest(new { Error = "MinSimilarityThreshold must be between 0 and 1" });
        }

        _logger.LogInformation("Knowledge search requested by user {UserId}: '{Query}' in {CodeSystem}",
            User.Identity?.Name,
            query,
            codeSystem);

        try
        {
            var request = new CodeSearchRequestDto
            {
                Query = query,
                CodeSystem = codeSystem,
                TopK = topK,
                MinSimilarityThreshold = minSimilarityThreshold
            };

            var response = await _retrievalService.SearchAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing knowledge search for query: '{Query}'", query);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Error = "An error occurred while executing the search" });
        }
    }

    /// <summary>
    /// Executes semantic-only search (no keyword fallback).
    /// Useful for testing semantic similarity threshold.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of top results (default: 5)</param>
    /// <param name="minSimilarityThreshold">Minimum cosine similarity threshold (default: 0.75)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("search/semantic")]
    [ProducesResponseType(typeof(List<CodeRetrievalResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SemanticSearch(
        [FromQuery] string query,
        [FromQuery] string codeSystem,
        [FromQuery] int topK = 5,
        [FromQuery] double minSimilarityThreshold = 0.75,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { Error = "Query parameter cannot be empty" });
        }

        if (!new[] { "ICD10", "CPT", "ClinicalTerminology" }.Contains(codeSystem))
        {
            return BadRequest(new { Error = "Invalid code system" });
        }

        try
        {
            var results = await _retrievalService.SemanticSearchAsync(
                query, codeSystem, topK, minSimilarityThreshold, cancellationToken);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing semantic search for query: '{Query}'", query);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Error = "An error occurred while executing the search" });
        }
    }

    /// <summary>
    /// Executes keyword-only search (no semantic similarity).
    /// Useful for testing keyword matching fallback.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of top results (default: 5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("search/keyword")]
    [ProducesResponseType(typeof(List<CodeRetrievalResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> KeywordSearch(
        [FromQuery] string query,
        [FromQuery] string codeSystem,
        [FromQuery] int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { Error = "Query parameter cannot be empty" });
        }

        if (!new[] { "ICD10", "CPT", "ClinicalTerminology" }.Contains(codeSystem))
        {
            return BadRequest(new { Error = "Invalid code system" });
        }

        try
        {
            var results = await _retrievalService.KeywordSearchAsync(
                query, codeSystem, topK, cancellationToken);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing keyword search for query: '{Query}'", query);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Error = "An error occurred while executing the search" });
        }
    }
}
