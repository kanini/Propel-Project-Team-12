using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace PatientAccess.Business.Services;

/// <summary>
/// Hybrid retrieval service combining semantic similarity (pgvector) and keyword matching (PostgreSQL FTS).
/// Implements AIR-R02 (top-5 retrieval, cosine >0.75) and AIR-R03 (hybrid retrieval).
/// Includes Redis caching for performance (<200ms retrieval target).
/// </summary>
public class HybridRetrievalService : IHybridRetrievalService
{
    private readonly ILogger<HybridRetrievalService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly IEmbeddingGenerationService _embeddingService;
    private readonly IDistributedCache _cache;

    // Hybrid scoring weights
    private const double SEMANTIC_WEIGHT = 0.7;
    private const double KEYWORD_WEIGHT = 0.3;

    public HybridRetrievalService(
        ILogger<HybridRetrievalService> logger,
        PatientAccessDbContext context,
        IEmbeddingGenerationService embeddingService,
        IDistributedCache cache)
    {
        _logger = logger;
        _context = context;
        _embeddingService = embeddingService;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<CodeSearchResponseDto> SearchAsync(CodeSearchRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing search for query: '{Query}' in {CodeSystem}", request.Query, request.CodeSystem);

        // Check Redis cache
        var cacheKey = $"knowledge:search:{request.CodeSystem}:{request.Query}:{request.TopK}:{request.MinSimilarityThreshold}";
        var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (!string.IsNullOrEmpty(cachedResult))
        {
            _logger.LogInformation("Cache hit for query: '{Query}'", request.Query);
            return JsonSerializer.Deserialize<CodeSearchResponseDto>(cachedResult) ?? new CodeSearchResponseDto();
        }

        // Execute hybrid search
        var results = await HybridSearchAsync(
            request.Query,
            request.CodeSystem,
            request.TopK,
            request.MinSimilarityThreshold,
            cancellationToken);

        var response = new CodeSearchResponseDto
        {
            Query = request.Query,
            CodeSystem = request.CodeSystem,
            Results = results,
            ResultCount = results.Count
        };

        // Check if we need fallback to keyword-only search
        var semanticMatches = results.Where(r => r.SimilarityScore >= request.MinSimilarityThreshold).ToList();
        
        if (!semanticMatches.Any())
        {
            _logger.LogWarning("No semantic matches above threshold {Threshold} for query: '{Query}'. Falling back to keyword search.",
                request.MinSimilarityThreshold, request.Query);

            results = await KeywordSearchAsync(request.Query, request.CodeSystem, request.TopK, cancellationToken);
            response.Results = results;
            response.ResultCount = results.Count;
            response.Message = "No confident matches found. Showing keyword-based results with reduced confidence.";
        }

        // Cache for 15 minutes
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions, cancellationToken);

        _logger.LogInformation("Search completed. Returned {Count} results for query: '{Query}'", response.ResultCount, request.Query);
        return response;
    }

    /// <inheritdoc/>
    public async Task<List<CodeRetrievalResultDto>> SemanticSearchAsync(
        string query,
        string codeSystem,
        int topK,
        double minThreshold,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing semantic search for: '{Query}' in {CodeSystem}", query, codeSystem);

        // Generate query embedding
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

        // Execute semantic search by code system
        return codeSystem switch
        {
            "ICD10" => await SemanticSearchICD10Async(queryEmbedding, topK, minThreshold, cancellationToken),
            "CPT" => await SemanticSearchCPTAsync(queryEmbedding, topK, minThreshold, cancellationToken),
            "ClinicalTerminology" => await SemanticSearchClinicalAsync(queryEmbedding, topK, minThreshold, cancellationToken),
            _ => throw new ArgumentException($"Invalid code system: {codeSystem}", nameof(codeSystem))
        };
    }

    /// <inheritdoc/>
    public async Task<List<CodeRetrievalResultDto>> KeywordSearchAsync(
        string query,
        string codeSystem,
        int topK,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing keyword search for: '{Query}' in {CodeSystem}", query, codeSystem);

        // Execute keyword search by code system
        return codeSystem switch
        {
            "ICD10" => await KeywordSearchICD10Async(query, topK, cancellationToken),
            "CPT" => await KeywordSearchCPTAsync(query, topK, cancellationToken),
            "ClinicalTerminology" => await KeywordSearchClinicalAsync(query, topK, cancellationToken),
            _ => throw new ArgumentException($"Invalid code system: {codeSystem}", nameof(codeSystem))
        };
    }

    /// <inheritdoc/>
    public async Task<List<CodeRetrievalResultDto>> HybridSearchAsync(
        string query,
        string codeSystem,
        int topK,
        double minThreshold,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing hybrid search for: '{Query}' in {CodeSystem}", query, codeSystem);

        // Execute both searches in parallel (expand results for merging)
        var expandedTopK = topK * 2;
        var semanticTask = SemanticSearchAsync(query, codeSystem, expandedTopK, 0, cancellationToken); // No threshold for merging
        var keywordTask = KeywordSearchAsync(query, codeSystem, expandedTopK, cancellationToken);

        await Task.WhenAll(semanticTask, keywordTask);

        var semanticResults = semanticTask.Result.ToDictionary(r => r.Code);
        var keywordResults = keywordTask.Result.ToDictionary(r => r.Code);

        // Merge results with hybrid scoring
        var allCodes = semanticResults.Keys.Union(keywordResults.Keys).ToList();
        var hybridResults = allCodes.Select(code =>
        {
            var semanticScore = semanticResults.ContainsKey(code) ? semanticResults[code].SimilarityScore : 0;
            var keywordScore = keywordResults.ContainsKey(code) ? keywordResults[code].KeywordScore : 0;
            var finalScore = SEMANTIC_WEIGHT * semanticScore + KEYWORD_WEIGHT * keywordScore;

            var result = semanticResults.ContainsKey(code) ? semanticResults[code] : keywordResults[code];
            result.SimilarityScore = semanticScore;
            result.KeywordScore = keywordScore;
            result.FinalScore = finalScore;
            result.MatchType = semanticScore > 0 && keywordScore > 0 ? "Hybrid"
                             : semanticScore > 0 ? "Semantic"
                             : "Keyword";

            return result;
        })
        .OrderByDescending(r => r.FinalScore)
        .Take(topK)
        .ToList();

        _logger.LogDebug("Hybrid search returned {Count} results", hybridResults.Count);
        return hybridResults;
    }

    // Semantic search implementations per code system

    private async Task<List<CodeRetrievalResultDto>> SemanticSearchICD10Async(
        Vector queryEmbedding,
        int topK,
        double minThreshold,
        CancellationToken cancellationToken)
    {
        var results = await _context.ICD10Codes
            .Where(c => c.IsActive && c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(c => new
            {
                c.Code,
                c.Description,
                c.Category,
                c.Metadata,
                Distance = c.Embedding!.CosineDistance(queryEmbedding)
            })
            .ToListAsync(cancellationToken);

        return results
            .Select(r => new CodeRetrievalResultDto
            {
                Code = r.Code,
                Description = r.Description ?? string.Empty,
                Category = r.Category ?? string.Empty,
                SimilarityScore = 1 - r.Distance, // Convert distance to similarity
                KeywordScore = 0,
                FinalScore = 1 - r.Distance,
                MatchType = "Semantic",
                Metadata = r.Metadata ?? "{}"
            })
            .Where(r => r.SimilarityScore >= minThreshold)
            .ToList();
    }

    private async Task<List<CodeRetrievalResultDto>> SemanticSearchCPTAsync(
        Vector queryEmbedding,
        int topK,
        double minThreshold,
        CancellationToken cancellationToken)
    {
        var results = await _context.CPTCodes
            .Where(c => c.IsActive && c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(c => new
            {
                c.Code,
                c.Description,
                c.Category,
                c.Metadata,
                Distance = c.Embedding!.CosineDistance(queryEmbedding)
            })
            .ToListAsync(cancellationToken);

        return results
            .Select(r => new CodeRetrievalResultDto
            {
                Code = r.Code,
                Description = r.Description ?? string.Empty,
                Category = r.Category ?? string.Empty,
                SimilarityScore = 1 - r.Distance,
                KeywordScore = 0,
                FinalScore = 1 - r.Distance,
                MatchType = "Semantic",
                Metadata = r.Metadata ?? "{}"
            })
            .Where(r => r.SimilarityScore >= minThreshold)
            .ToList();
    }

    private async Task<List<CodeRetrievalResultDto>> SemanticSearchClinicalAsync(
        Vector queryEmbedding,
        int topK,
        double minThreshold,
        CancellationToken cancellationToken)
    {
        var results = await _context.ClinicalTerminology
            .Where(c => c.IsActive && c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(topK)
            .Select(c => new
            {
                c.Term,
                c.ChunkText,
                c.Category,
                c.Metadata,
                Distance = c.Embedding!.CosineDistance(queryEmbedding)
            })
            .ToListAsync(cancellationToken);

        return results
            .Select(r => new CodeRetrievalResultDto
            {
                Code = r.Term,
                Description = r.ChunkText ?? string.Empty,
                Category = r.Category ?? string.Empty,
                SimilarityScore = 1 - r.Distance,
                KeywordScore = 0,
                FinalScore = 1 - r.Distance,
                MatchType = "Semantic",
                Metadata = r.Metadata ?? "{}"
            })
            .Where(r => r.SimilarityScore >= minThreshold)
            .ToList();
    }

    // Keyword search implementations per code system (using simple LIKE for now - can be enhanced with PostgreSQL FTS)

    private async Task<List<CodeRetrievalResultDto>> KeywordSearchICD10Async(
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var searchTerm = $"%{query}%";
        
        var results = await _context.ICD10Codes
            .Where(c => c.IsActive && 
                   (EF.Functions.ILike(c.Description!, searchTerm) || 
                    EF.Functions.ILike(c.Code, searchTerm)))
            .Take(topK)
            .Select(c => new CodeRetrievalResultDto
            {
                Code = c.Code,
                Description = c.Description ?? string.Empty,
                Category = c.Category ?? string.Empty,
                SimilarityScore = 0,
                KeywordScore = 1.0, // Simple binary match - can be enhanced with ts_rank
                FinalScore = 1.0,
                MatchType = "Keyword",
                Metadata = c.Metadata ?? "{}"
            })
            .ToListAsync(cancellationToken);

        return results;
    }

    private async Task<List<CodeRetrievalResultDto>> KeywordSearchCPTAsync(
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var searchTerm = $"%{query}%";
        
        var results = await _context.CPTCodes
            .Where(c => c.IsActive && 
                   (EF.Functions.ILike(c.Description!, searchTerm) || 
                    EF.Functions.ILike(c.Code, searchTerm)))
            .Take(topK)
            .Select(c => new CodeRetrievalResultDto
            {
                Code = c.Code,
                Description = c.Description ?? string.Empty,
                Category = c.Category ?? string.Empty,
                SimilarityScore = 0,
                KeywordScore = 1.0,
                FinalScore = 1.0,
                MatchType = "Keyword",
                Metadata = c.Metadata ?? "{}"
            })
            .ToListAsync(cancellationToken);

        return results;
    }

    private async Task<List<CodeRetrievalResultDto>> KeywordSearchClinicalAsync(
        string query,
        int topK,
        CancellationToken cancellationToken)
    {
        var searchTerm = $"%{query}%";
        
        var results = await _context.ClinicalTerminology
            .Where(c => c.IsActive && 
                   (EF.Functions.ILike(c.ChunkText, searchTerm) || 
                    EF.Functions.ILike(c.Term, searchTerm)))
            .Take(topK)
            .Select(c => new CodeRetrievalResultDto
            {
                Code = c.Term,
                Description = c.ChunkText ?? string.Empty,
                Category = c.Category ?? string.Empty,
                SimilarityScore = 0,
                KeywordScore = 1.0,
                FinalScore = 1.0,
                MatchType = "Keyword",
                Metadata = c.Metadata ?? "{}"
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
