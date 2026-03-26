using System.Text.RegularExpressions;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PatientAccess.Business.Configuration;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using Pgvector;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for generating embeddings using Azure OpenAI text-embedding-3-small (AIR-R04, DR-010).
/// Produces 1536-dimensional vectors with batch processing, rate limiting, and Polly resilience patterns.
/// </summary>
public class EmbeddingGenerationService : IEmbeddingGenerationService
{
    private readonly ILogger<EmbeddingGenerationService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly AzureOpenAISettings _settings;
    private readonly OpenAIClient _openAIClient;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public EmbeddingGenerationService(
        ILogger<EmbeddingGenerationService> logger,
        PatientAccessDbContext context,
        IOptions<AzureOpenAISettings> settings,
        OpenAIClient openAIClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _openAIClient = openAIClient ?? throw new ArgumentNullException(nameof(openAIClient));

        // Configure Polly retry policy: 3 retries with exponential backoff
        _retryPolicy = Policy
            .Handle<RequestFailedException>() // Azure SDK exception for API failures
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}s due to: {Message}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });

        // Configure Polly circuit breaker: 5 failures trigger 1-minute break
        _circuitBreakerPolicy = Policy
            .Handle<RequestFailedException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Circuit breaker opened for {Duration}s due to: {Message}",
                        duration.TotalSeconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset - resuming normal operations");
                });
    }

    /// <inheritdoc />
    public async Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        _logger.LogDebug("Generating embedding for text ({Length} chars)", text.Length);

        return await _circuitBreakerPolicy.ExecuteAsync(() =>
            _retryPolicy.ExecuteAsync(async () =>
            {
                var options = new EmbeddingsOptions(_settings.EmbeddingDeploymentName, new[] { text });
                var response = await _openAIClient.GetEmbeddingsAsync(options, cancellationToken);

                var embedding = response.Value.Data[0].Embedding.ToArray().ToList();

                _logger.LogDebug("Embedding generated: {Dimensions} dimensions", embedding.Count);

                return embedding;
            }));
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, List<float>>> GenerateBatchEmbeddingsAsync(
        List<string> texts, CancellationToken cancellationToken = default)
    {
        if (texts == null || texts.Count == 0)
        {
            throw new ArgumentException("Texts list cannot be null or empty", nameof(texts));
        }

        if (texts.Count > _settings.BatchSize)
        {
            throw new ArgumentException(
                $"Batch size exceeds limit: {texts.Count} > {_settings.BatchSize}",
                nameof(texts));
        }

        _logger.LogInformation("Generating batch embeddings for {Count} texts", texts.Count);

        var startTime = DateTime.UtcNow;

        var embeddings = await _circuitBreakerPolicy.ExecuteAsync(() =>
            _retryPolicy.ExecuteAsync(async () =>
            {
                var options = new EmbeddingsOptions(_settings.EmbeddingDeploymentName, texts);
                var response = await _openAIClient.GetEmbeddingsAsync(options, cancellationToken);

                var result = new Dictionary<string, List<float>>();
                for (int i = 0; i < texts.Count; i++)
                {
                    result[texts[i]] = response.Value.Data[i].Embedding.ToArray().ToList();
                }

                return result;
            }));

        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogInformation("Batch embeddings generated in {Duration}ms: {Count} embeddings",
            duration, embeddings.Count);

        return embeddings;
    }

    /// <inheritdoc />
    public async Task ProcessPendingChunksAsync(string codeSystem, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing pending chunks for {CodeSystem}", codeSystem);

        var startTime = DateTime.UtcNow;

        // Fetch all pending chunks for this code system
        var pendingChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem && !c.IsProcessed)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);

        if (pendingChunks.Count == 0)
        {
            _logger.LogInformation("No pending chunks found for {CodeSystem}", codeSystem);
            return;
        }

        _logger.LogInformation("Found {Count} pending chunks for {CodeSystem}",
            pendingChunks.Count, codeSystem);

        int processedCount = 0;

        // Process in batches
        for (int i = 0; i < pendingChunks.Count; i += _settings.BatchSize)
        {
            var batch = pendingChunks.Skip(i).Take(_settings.BatchSize).ToList();

            _logger.LogDebug("Processing batch {BatchNum}/{TotalBatches} ({ChunkCount} chunks)",
                (i / _settings.BatchSize) + 1,
                (int)Math.Ceiling((double)pendingChunks.Count / _settings.BatchSize),
                batch.Count);

            await ProcessBatchAsync(batch, codeSystem, cancellationToken);
            processedCount += batch.Count;

            // Rate limiting: delay between batches to respect Azure OpenAI quotas
            if (i + _settings.BatchSize < pendingChunks.Count)
            {
                var delaySeconds = 60.0 / _settings.MaxRequestsPerMinute;
                _logger.LogDebug("Rate limiting: Delaying {Delay}s before next batch", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        var totalDuration = (DateTime.UtcNow - startTime).TotalSeconds;
        _logger.LogInformation("Completed processing {Count} chunks for {CodeSystem} in {Duration}s",
            processedCount, codeSystem, totalDuration);
    }

    /// <summary>
    /// Processes a batch of document chunks: generates embeddings and persists to appropriate table.
    /// </summary>
    private async Task ProcessBatchAsync(
        List<DocumentChunk> chunks,
        string codeSystem,
        CancellationToken cancellationToken)
    {
        var texts = chunks.Select(c => c.SourceText).ToList();
        var embeddings = await GenerateBatchEmbeddingsAsync(texts, cancellationToken);

        // Persist embeddings to appropriate table based on code system
        foreach (var chunk in chunks)
        {
            var embedding = embeddings[chunk.SourceText];

            switch (codeSystem)
            {
                case "ICD10":
                    await PersistICD10EmbeddingAsync(chunk, embedding, cancellationToken);
                    break;
                case "CPT":
                    await PersistCPTEmbeddingAsync(chunk, embedding, cancellationToken);
                    break;
                case "ClinicalTerminology":
                    await PersistClinicalTerminologyEmbeddingAsync(chunk, embedding, cancellationToken);
                    break;
                default:
                    throw new ArgumentException($"Invalid code system: {codeSystem}");
            }

            // Mark chunk as processed
            chunk.IsProcessed = true;
            chunk.ProcessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Batch persisted: {Count} embeddings saved to {CodeSystem} table",
            chunks.Count, codeSystem);
    }

    /// <summary>
    /// Persists ICD-10 embedding to ICD10Codes table.
    /// Extracts ICD-10 code from chunk text using regex pattern.
    /// </summary>
    private async Task PersistICD10EmbeddingAsync(
        DocumentChunk chunk,
        List<float> embedding,
        CancellationToken cancellationToken)
    {
        // Extract ICD-10 code from chunk text (format: [A-Z]\d{2}\.\d{1,2})
        var codeMatch = Regex.Match(chunk.SourceText, @"\b([A-Z]\d{2}\.\d{1,2})\b");
        var code = codeMatch.Success ? codeMatch.Value : "UNKNOWN";

        // Extract category from chunk text (after code and description)
        var category = ExtractCategoryFromChunk(chunk.SourceText, "ICD10");

        var icd10Code = new ICD10Code
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = chunk.SourceText.Length > 1000
                ? chunk.SourceText.Substring(0, 1000)
                : chunk.SourceText,
            Category = category,
            ChapterCode = code.Length >= 3 ? $"{code.Substring(0, 1)}00-{code.Substring(0, 1)}99" : "UNKNOWN",
            Embedding = new Vector(embedding.ToArray()),
            ChunkText = chunk.SourceText,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                chunkId = chunk.Id,
                chunkIndex = chunk.ChunkIndex,
                version = "ICD-10-CM-2024",
                status = "active"
            }),
            Version = "ICD-10-CM-2024",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ICD10Codes.AddAsync(icd10Code, cancellationToken);
        chunk.TargetEntityId = icd10Code.Id;

        _logger.LogDebug("ICD-10 embedding persisted: {Code}", code);
    }

    /// <summary>
    /// Persists CPT embedding to CPTCodes table.
    /// Extracts CPT code from chunk text using regex pattern.
    /// </summary>
    private async Task PersistCPTEmbeddingAsync(
        DocumentChunk chunk,
        List<float> embedding,
        CancellationToken cancellationToken)
    {
        // Extract CPT code from chunk text (format: \d{5}(?:-\d{2})?)
        var codeMatch = Regex.Match(chunk.SourceText, @"\b(\d{5}(?:-\d{2})?)\b");
        var code = codeMatch.Success ? codeMatch.Value : "UNKNOWN";

        // Extract modifier if present (format: 99213-25)
        string? modifier = null;
        if (code.Contains('-') && code.Length > 5)
        {
            modifier = code.Substring(6);
            code = code.Substring(0, 5);
        }

        var category = ExtractCategoryFromChunk(chunk.SourceText, "CPT");

        var cptCode = new CPTCode
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = chunk.SourceText.Length > 1000
                ? chunk.SourceText.Substring(0, 1000)
                : chunk.SourceText,
            Category = category,
            Modifier = modifier,
            Embedding = new Vector(embedding.ToArray()),
            ChunkText = chunk.SourceText,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                chunkId = chunk.Id,
                chunkIndex = chunk.ChunkIndex,
                version = "CPT-2024",
                status = "active"
            }),
            Version = "CPT-2024",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.CPTCodes.AddAsync(cptCode, cancellationToken);
        chunk.TargetEntityId = cptCode.Id;

        _logger.LogDebug("CPT embedding persisted: {Code}", code);
    }

    /// <summary>
    /// Persists clinical terminology embedding to ClinicalTerminology table.
    /// Extracts term from chunk text (first line or sentence).
    /// </summary>
    private async Task PersistClinicalTerminologyEmbeddingAsync(
        DocumentChunk chunk,
        List<float> embedding,
        CancellationToken cancellationToken)
    {
        // Extract term from chunk text (first line or up to first period)
        var lines = chunk.SourceText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var term = lines.Length > 0 ? lines[0].Trim() : chunk.SourceText;

        // Limit term length to 500 characters
        if (term.Length > 500)
        {
            term = term.Substring(0, 500);
        }

        var category = ExtractCategoryFromChunk(chunk.SourceText, "ClinicalTerminology");

        var clinicalTerm = new ClinicalTerminology
        {
            Id = Guid.NewGuid(),
            Term = term,
            Category = category,
            Synonyms = "[]", // Empty array - can be populated later
            Embedding = new Vector(embedding.ToArray()),
            ChunkText = chunk.SourceText,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                chunkId = chunk.Id,
                chunkIndex = chunk.ChunkIndex,
                source = "Internal"
            }),
            Source = "Internal",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ClinicalTerminology.AddAsync(clinicalTerm, cancellationToken);
        chunk.TargetEntityId = clinicalTerm.Id;

        _logger.LogDebug("Clinical terminology embedding persisted: {Term}", term);
    }

    /// <summary>
    /// Extracts category from chunk text based on code system.
    /// Uses simple heuristics - can be enhanced with ML classification.
    /// </summary>
    private string ExtractCategoryFromChunk(string chunkText, string codeSystem)
    {
        // Default categories by code system
        return codeSystem switch
        {
            "ICD10" => "General Diagnosis",
            "CPT" => "General Procedure",
            "ClinicalTerminology" => "General Term",
            _ => "Unknown"
        };
    }
}
