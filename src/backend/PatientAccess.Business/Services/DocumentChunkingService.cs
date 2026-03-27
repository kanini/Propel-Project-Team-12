using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using SharpToken;

namespace PatientAccess.Business.Services;

/// <summary>
/// Implements document chunking for RAG knowledge base.
/// Uses Tiktoken cl100k_base encoding (matches Azure OpenAI text-embedding-3-small).
/// Implements AIR-R01: 512-token chunks with 64-token (12.5%) overlap.
/// </summary>
public class DocumentChunkingService : IDocumentChunkingService
{
    private readonly ILogger<DocumentChunkingService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly GptEncoding _encoder;

    // AIR-R01 requirements
    private const int MAX_CHUNK_SIZE = 512;
    private const int OVERLAP_SIZE = 64; // 12.5% of 512

    public DocumentChunkingService(
        ILogger<DocumentChunkingService> logger,
        PatientAccessDbContext context)
    {
        _logger = logger;
        _context = context;
        
        // Initialize cl100k_base encoder (matches text-embedding-3-small)
        _encoder = GptEncoding.GetEncoding("cl100k_base");
    }

    /// <inheritdoc/>
    public async Task<List<DocumentChunk>> ChunkICD10DocumentAsync(string documentText, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Chunking ICD-10 document ({Length} chars)", documentText.Length);

        // Preprocess to preserve ICD-10 code boundaries
        var preprocessedText = PreprocessICD10Document(documentText);
        var chunks = ChunkDocument(preprocessedText, "ICD10");

        await _context.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {ChunkCount} ICD-10 chunks (avg {AvgTokens} tokens)",
            chunks.Count,
            chunks.Average(c => c.TokenCount));

        return chunks;
    }

    /// <inheritdoc/>
    public async Task<List<DocumentChunk>> ChunkCPTDocumentAsync(string documentText, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Chunking CPT document ({Length} chars)", documentText.Length);

        // Preprocess to preserve CPT code boundaries
        var preprocessedText = PreprocessCPTDocument(documentText);
        var chunks = ChunkDocument(preprocessedText, "CPT");

        await _context.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {ChunkCount} CPT chunks (avg {AvgTokens} tokens)",
            chunks.Count,
            chunks.Average(c => c.TokenCount));

        return chunks;
    }

    /// <inheritdoc/>
    public async Task<List<DocumentChunk>> ChunkClinicalTerminologyAsync(string documentText, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Chunking clinical terminology document ({Length} chars)", documentText.Length);

        // No preprocessing needed for clinical terminology (no code boundary constraints)
        var chunks = ChunkDocument(documentText, "ClinicalTerminology");

        await _context.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {ChunkCount} clinical terminology chunks (avg {AvgTokens} tokens)",
            chunks.Count,
            chunks.Average(c => c.TokenCount));

        return chunks;
    }

    /// <inheritdoc/>
    public Task<int> GetTokenCountAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Task.FromResult(0);
        }

        var tokens = _encoder.Encode(text);
        return Task.FromResult(tokens.Count);
    }

    /// <summary>
    /// Core chunking logic with 512-token chunks and 64-token overlap.
    /// </summary>
    private List<DocumentChunk> ChunkDocument(string documentText, string codeSystem)
    {
        if (string.IsNullOrWhiteSpace(documentText))
        {
            _logger.LogWarning("Empty document text provided for {CodeSystem}", codeSystem);
            return new List<DocumentChunk>();
        }

        var tokens = _encoder.Encode(documentText);
        var chunks = new List<DocumentChunk>();

        int startToken = 0;
        int chunkIndex = 0;

        while (startToken < tokens.Count)
        {
            int endToken = Math.Min(startToken + MAX_CHUNK_SIZE, tokens.Count);
            var chunkTokens = tokens.Skip(startToken).Take(endToken - startToken).ToList();
            var chunkText = _encoder.Decode(chunkTokens);

            // Validate chunk size (MANDATORY per AIR-R01)
            if (chunkTokens.Count > MAX_CHUNK_SIZE)
            {
                _logger.LogError("Chunk {ChunkIndex} exceeds MAX_CHUNK_SIZE: {TokenCount} tokens", chunkIndex, chunkTokens.Count);
                throw new InvalidOperationException($"Chunk {chunkIndex} exceeds MAX_CHUNK_SIZE ({MAX_CHUNK_SIZE}): {chunkTokens.Count} tokens");
            }

            var chunk = new DocumentChunk
            {
                Id = Guid.NewGuid(),
                CodeSystem = codeSystem,
                SourceText = chunkText,
                TokenCount = chunkTokens.Count,
                ChunkIndex = chunkIndex,
                StartToken = startToken,
                EndToken = endToken,
                OverlapWithPrevious = chunkIndex > 0,
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };

            chunks.Add(chunk);

            // Move forward by (MAX_CHUNK_SIZE - OVERLAP_SIZE) for 12.5% overlap
            startToken += (MAX_CHUNK_SIZE - OVERLAP_SIZE);
            chunkIndex++;
        }

        _logger.LogDebug("Created {ChunkCount} chunks for {CodeSystem} document ({TotalTokens} tokens)",
            chunks.Count,
            codeSystem,
            tokens.Count);

        return chunks;
    }

    /// <summary>
    /// Preprocesses ICD-10 document to preserve code boundaries.
    /// Adds newlines between ICD-10 codes to prevent mid-code splits.
    /// </summary>
    private string PreprocessICD10Document(string documentText)
    {
        // ICD-10 format: Letter + 2 digits + . + 1-2 digits (e.g., E11.9, A01.05)
        // Add newline before each ICD-10 code to preserve boundaries
        var pattern = @"(?=([A-Z]\d{2}\.\d{1,2}))";
        var preprocessed = Regex.Replace(documentText, pattern, "\n", RegexOptions.Multiline);

        _logger.LogDebug("ICD-10 preprocessing: {OriginalLength} -> {PreprocessedLength} chars",
            documentText.Length,
            preprocessed.Length);

        return preprocessed;
    }

    /// <summary>
    /// Preprocesses CPT document to preserve code boundaries and modifiers.
    /// Adds newlines between CPT codes to prevent mid-code splits.
    /// </summary>
    private string PreprocessCPTDocument(string documentText)
    {
        // CPT format: 5 digits with optional modifiers (e.g., 99213, 99213-25, 80053)
        // Add newline before each CPT code to preserve boundaries
        var pattern = @"(?=(\d{5}(-\d{2})?))";
        var preprocessed = Regex.Replace(documentText, pattern, "\n", RegexOptions.Multiline);

        _logger.LogDebug("CPT preprocessing: {OriginalLength} -> {PreprocessedLength} chars",
            documentText.Length,
            preprocessed.Length);

        return preprocessed;
    }
}
