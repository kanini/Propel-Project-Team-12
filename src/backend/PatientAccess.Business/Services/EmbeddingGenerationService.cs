using System.Text.Json;
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
/// Service for generating vector embeddings using Azure OpenAI text-embedding-3-small.
/// Implements DR-010 (1536-dimensional vectors) and AIR-R04 (separate indices per code system).
/// Includes Polly circuit breaker and retry logic for Azure OpenAI transient failures.
/// </summary>
public class EmbeddingGenerationService : IEmbeddingGenerationService
{
    private readonly ILogger<EmbeddingGenerationService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly AzureOpenAISettings _settings;
    private readonly OpenAIClient _openAIClient;
    private readonly AsyncRetryPolicy<Response<Embeddings>> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public EmbeddingGenerationService(
        ILogger<EmbeddingGenerationService> logger,
        PatientAccessDbContext context,
        IOptions<AzureOpenAISettings> settings,
        OpenAIClient openAIClient)
    {
        _logger = logger;
        _context = context;
        _settings = settings.Value;
        _openAIClient = openAIClient;

        // Polly retry policy: 3 retries with exponential backoff
        _retryPolicy = Policy
            .HandleResult<Response<Embeddings>>(r => false) // Never retry on success
            .Or<RequestFailedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}s: {Message}",
                        retryCount, timeSpan.TotalSeconds, outcome.Exception?.Message ?? "Unknown error");
                });

        // Polly circuit breaker: opens after 5 consecutive failures, resets after 1 minute
        _circuitBreakerPolicy = Policy
            .Handle<RequestFailedException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError(exception, "Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                });
    }

    /// <inheritdoc/>
    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or whitespace", nameof(text));
        }

        try
        {
            var options = new EmbeddingsOptions(_settings.EmbeddingDeploymentName, new[] { text });

            var response = await _circuitBreakerPolicy.ExecuteAsync(async () =>
                await _retryPolicy.ExecuteAsync(async () =>
                    await _openAIClient.GetEmbeddingsAsync(options, cancellationToken)));

            var embedding = response.Value.Data[0].Embedding.ToArray();
            return new Vector(embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding for text: {TextPreview}", text.Substring(0, Math.Min(100, text.Length)));
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, Vector>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken)
    {
        if (texts == null || texts.Count == 0)
        {
            return new Dictionary<string, Vector>();
        }

        if (texts.Count > _settings.BatchSize)
        {
            throw new ArgumentException($"Batch size exceeds limit: {texts.Count} > {_settings.BatchSize}", nameof(texts));
        }

        _logger.LogInformation("Generating embeddings for batch of {Count} texts", texts.Count);

        try
        {
            var options = new EmbeddingsOptions(_settings.EmbeddingDeploymentName, texts);

            var response = await _circuitBreakerPolicy.ExecuteAsync(async () =>
                await _retryPolicy.ExecuteAsync(async () =>
                    await _openAIClient.GetEmbeddingsAsync(options, cancellationToken)));

            var embeddings = new Dictionary<string, Vector>();
            for (int i = 0; i < texts.Count; i++)
            {
                var embedding = response.Value.Data[i].Embedding.ToArray();
                embeddings[texts[i]] = new Vector(embedding);
            }

            _logger.LogInformation("Successfully generated {Count} embeddings", embeddings.Count);
            return embeddings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate batch embeddings for {Count} texts", texts.Count);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ProcessPendingChunksAsync(string codeSystem, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to process pending chunks for {CodeSystem}", codeSystem);

        var pendingChunks = await _context.DocumentChunks
            .Where(c => c.CodeSystem == codeSystem && !c.IsProcessed)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);

        if (!pendingChunks.Any())
        {
            _logger.LogInformation("No pending chunks found for {CodeSystem}", codeSystem);
            return;
        }

        _logger.LogInformation("Processing {Count} pending chunks for {CodeSystem}", pendingChunks.Count, codeSystem);

        int processedCount = 0;
        int batchNumber = 0;

        // Process in batches
        for (int i = 0; i < pendingChunks.Count; i += _settings.BatchSize)
        {
            batchNumber++;
            var batch = pendingChunks.Skip(i).Take(_settings.BatchSize).ToList();

            _logger.LogInformation("Processing batch {BatchNumber} ({Count} chunks) for {CodeSystem}",
                batchNumber, batch.Count, codeSystem);

            try
            {
                await ProcessBatchAsync(batch, codeSystem, cancellationToken);
                processedCount += batch.Count;

                _logger.LogInformation("Batch {BatchNumber} completed. Progress: {Processed}/{Total} ({Percentage:F1}%)",
                    batchNumber, processedCount, pendingChunks.Count, (processedCount * 100.0 / pendingChunks.Count));

                // Rate limiting: delay between batches to respect Azure OpenAI quotas
                if (i + _settings.BatchSize < pendingChunks.Count)
                {
                    var delayMs = (int)(60000.0 / _settings.MaxRequestsPerMinute);
                    _logger.LogDebug("Rate limiting delay: {DelayMs}ms", delayMs);
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process batch {BatchNumber} for {CodeSystem}", batchNumber, codeSystem);
                throw;
            }
        }

        _logger.LogInformation("Completed processing {Count} chunks for {CodeSystem}", processedCount, codeSystem);
    }

    private async Task ProcessBatchAsync(List<DocumentChunk> chunks, string codeSystem, CancellationToken cancellationToken)
    {
        var texts = chunks.Select(c => c.SourceText).ToList();
        var embeddings = await GenerateBatchEmbeddingsAsync(texts, cancellationToken);

        // Persist embeddings to appropriate table
        foreach (var chunk in chunks)
        {
            if (!embeddings.ContainsKey(chunk.SourceText))
            {
                _logger.LogWarning("No embedding generated for chunk {ChunkId}", chunk.Id);
                continue;
            }

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
                    throw new ArgumentException($"Invalid code system: {codeSystem}", nameof(codeSystem));
            }

            // Mark chunk as processed
            chunk.IsProcessed = true;
            chunk.ProcessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Persisted {Count} embeddings to database", chunks.Count);
    }

    private async Task PersistICD10EmbeddingAsync(DocumentChunk chunk, Vector embedding, CancellationToken cancellationToken)
    {
        // Parse ICD-10 code from chunk text (e.g., extract "E11.9" from "E11.9 Type 2 diabetes mellitus")
        var codeMatch = Regex.Match(chunk.SourceText, @"([A-Z]\d{2}\.\d{1,2})");
        var code = codeMatch.Success ? codeMatch.Value : $"CHUNK_{chunk.ChunkIndex}";

        var metadata = JsonSerializer.Serialize(new
        {
            chunkId = chunk.Id,
            chunkIndex = chunk.ChunkIndex,
            source = "chunking_pipeline"
        });

        var icd10Code = new ICD10Code
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = chunk.SourceText.Length > 500 ? chunk.SourceText.Substring(0, 500) : chunk.SourceText,
            Category = ExtractCategoryFromText(chunk.SourceText),
            Embedding = embedding,
            Metadata = metadata,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.ICD10Codes.AddAsync(icd10Code, cancellationToken);
        chunk.TargetEntityId = icd10Code.Id;

        _logger.LogDebug("Created ICD10Code entity: {Code}", code);
    }

    private async Task PersistCPTEmbeddingAsync(DocumentChunk chunk, Vector embedding, CancellationToken cancellationToken)
    {
        // Parse CPT code from chunk text (e.g., extract "99213" or "99213-25")
        var codeMatch = Regex.Match(chunk.SourceText, @"(\d{5}(?:-\d{2})?)");
        var code = codeMatch.Success ? codeMatch.Value : $"CHUNK_{chunk.ChunkIndex}";

        var metadata = JsonSerializer.Serialize(new
        {
            chunkId = chunk.Id,
            chunkIndex = chunk.ChunkIndex,
            source = "chunking_pipeline"
        });

        var cptCode = new CPTCode
        {
            Id = Guid.NewGuid(),
            Code = code.Contains("-") ? code.Split('-')[0] : code,
            Description = chunk.SourceText.Length > 500 ? chunk.SourceText.Substring(0, 500) : chunk.SourceText,
            Category = ExtractCategoryFromText(chunk.SourceText),
            Modifier = code.Contains("-") ? code.Split('-')[1] : null,
            Embedding = embedding,
            Metadata = metadata,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.CPTCodes.AddAsync(cptCode, cancellationToken);
        chunk.TargetEntityId = cptCode.Id;

        _logger.LogDebug("Created CPTCode entity: {Code}", code);
    }

    private async Task PersistClinicalTerminologyEmbeddingAsync(DocumentChunk chunk, Vector embedding, CancellationToken cancellationToken)
    {
        // Extract term (first line or first 100 chars)
        var term = chunk.SourceText.Split('\n').FirstOrDefault()?.Trim() ?? chunk.SourceText.Substring(0, Math.Min(100, chunk.SourceText.Length));

        var metadata = JsonSerializer.Serialize(new
        {
            chunkId = chunk.Id,
            chunkIndex = chunk.ChunkIndex,
            source = "chunking_pipeline"
        });

        var clinicalTerm = new ClinicalTerminology
        {
            Id = Guid.NewGuid(),
            Term = term,
            ChunkText = chunk.SourceText,
            Category = ExtractCategoryFromText(chunk.SourceText),
            Embedding = embedding,
            Synonyms = "[]", // Empty JSON array initially
            Metadata = metadata,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.ClinicalTerminology.AddAsync(clinicalTerm, cancellationToken);
        chunk.TargetEntityId = clinicalTerm.Id;

        _logger.LogDebug("Created ClinicalTerminology entity: {Term}", term);
    }

    private string ExtractCategoryFromText(string text)
    {
        // Simple category extraction - can be enhanced with more sophisticated logic
        if (text.Contains("diabetes", StringComparison.OrdinalIgnoreCase))
            return "Endocrine";
        if (text.Contains("hypertension", StringComparison.OrdinalIgnoreCase) || text.Contains("cardiac", StringComparison.OrdinalIgnoreCase))
            return "Cardiovascular";
        if (text.Contains("office visit", StringComparison.OrdinalIgnoreCase) || text.Contains("consultation", StringComparison.OrdinalIgnoreCase))
            return "Evaluation and Management";

        return "General";
    }
}
