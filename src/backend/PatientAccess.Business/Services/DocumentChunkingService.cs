using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using SharpToken;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for chunking medical coding documents into 512-token segments with 64-token overlap (AIR-R01).
/// Uses SharpToken (Tiktoken C# port) with cl100k_base encoding (matches Azure OpenAI text-embedding-3-small).
/// Processes documents separately by code system (ICD-10, CPT, clinical terminology) per AIR-R04.
/// </summary>
public class DocumentChunkingService : IDocumentChunkingService
{
    private readonly ILogger<DocumentChunkingService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly GptEncoding _encoder;

    // AIR-R01 constants
    private const int MAX_CHUNK_SIZE = 512;
    private const int OVERLAP_SIZE = 64; // 12.5% of 512

    public DocumentChunkingService(
        ILogger<DocumentChunkingService> logger,
        PatientAccessDbContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));

        // Initialize cl100k_base encoder (used by text-embedding-3-small, GPT-4, GPT-3.5-turbo)
        _encoder = GptEncoding.GetEncoding("cl100k_base");
    }

    /// <inheritdoc />
    public async Task<List<DocumentChunk>> ChunkICD10DocumentAsync(string documentText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentText))
        {
            _logger.LogWarning("ChunkICD10DocumentAsync called with empty document text");
            return new List<DocumentChunk>();
        }

        _logger.LogInformation("Chunking ICD-10 document ({Length} chars)", documentText.Length);

        // Preprocess ICD-10 document to preserve code boundaries
        var preprocessedText = PreprocessICD10Document(documentText);
        var chunks = await ChunkDocumentAsync(preprocessedText, "ICD10", cancellationToken);

        _logger.LogInformation("Created {ChunkCount} ICD-10 chunks (avg {AvgTokens} tokens/chunk)",
            chunks.Count, chunks.Any() ? chunks.Average(c => c.TokenCount) : 0);

        return chunks;
    }

    /// <inheritdoc />
    public async Task<List<DocumentChunk>> ChunkCPTDocumentAsync(string documentText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentText))
        {
            _logger.LogWarning("ChunkCPTDocumentAsync called with empty document text");
            return new List<DocumentChunk>();
        }

        _logger.LogInformation("Chunking CPT document ({Length} chars)", documentText.Length);

        // Preprocess CPT document to preserve code and modifier boundaries
        var preprocessedText = PreprocessCPTDocument(documentText);
        var chunks = await ChunkDocumentAsync(preprocessedText, "CPT", cancellationToken);

        _logger.LogInformation("Created {ChunkCount} CPT chunks (avg {AvgTokens} tokens/chunk)",
            chunks.Count, chunks.Any() ? chunks.Average(c => c.TokenCount) : 0);

        return chunks;
    }

    /// <inheritdoc />
    public async Task<List<DocumentChunk>> ChunkClinicalTerminologyAsync(string documentText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentText))
        {
            _logger.LogWarning("ChunkClinicalTerminologyAsync called with empty document text");
            return new List<DocumentChunk>();
        }

        _logger.LogInformation("Chunking clinical terminology document ({Length} chars)", documentText.Length);

        // No special preprocessing needed for clinical terminology
        var chunks = await ChunkDocumentAsync(documentText, "ClinicalTerminology", cancellationToken);

        _logger.LogInformation("Created {ChunkCount} clinical terminology chunks (avg {AvgTokens} tokens/chunk)",
            chunks.Count, chunks.Any() ? chunks.Average(c => c.TokenCount) : 0);

        return chunks;
    }

    /// <inheritdoc />
    public Task<int> GetTokenCountAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult(0);
        }

        var tokens = _encoder.Encode(text);
        return Task.FromResult(tokens.Count);
    }

    /// <summary>
    /// Core chunking logic: tokenizes document and creates overlapping chunks.
    /// Persists chunks to staging table (DocumentChunks) for subsequent embedding generation.
    /// </summary>
    private async Task<List<DocumentChunk>> ChunkDocumentAsync(string documentText, string codeSystem, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // Tokenize entire document using cl100k_base encoding
        var tokens = _encoder.Encode(documentText);

        _logger.LogDebug("Tokenized document: {TokenCount} tokens", tokens.Count);

        var chunks = new List<DocumentChunk>();
        int startToken = 0;
        int chunkIndex = 0;

        // Create sliding window chunks with overlap
        while (startToken < tokens.Count)
        {
            // Calculate chunk boundaries
            int endToken = Math.Min(startToken + MAX_CHUNK_SIZE, tokens.Count);
            var chunkTokens = tokens.Skip(startToken).Take(endToken - startToken).ToList();

            // Decode tokens back to text
            var chunkText = _encoder.Decode(chunkTokens);

            // Validate chunk size (should never exceed MAX_CHUNK_SIZE per AIR-R01)
            if (chunkTokens.Count > MAX_CHUNK_SIZE)
            {
                _logger.LogWarning("Chunk {ChunkIndex} exceeds MAX_CHUNK_SIZE: {ActualSize} > {MaxSize}",
                    chunkIndex, chunkTokens.Count, MAX_CHUNK_SIZE);
            }

            // Create chunk entity
            var chunk = new DocumentChunk
            {
                Id = Guid.NewGuid(),
                CodeSystem = codeSystem,
                SourceText = chunkText,
                TokenCount = chunkTokens.Count,
                ChunkIndex = chunkIndex,
                StartToken = startToken,
                EndToken = endToken,
                OverlapWithPrevious = chunkIndex > 0, // First chunk has no overlap
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };

            chunks.Add(chunk);

            // Move forward by (MAX_CHUNK_SIZE - OVERLAP_SIZE) to create 64-token overlap
            // Example: Chunk 0: [0-512], Chunk 1: [448-960] (overlap: 448-512 = 64 tokens)
            startToken += (MAX_CHUNK_SIZE - OVERLAP_SIZE);
            chunkIndex++;
        }

        // Persist chunks to staging table
        await _context.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogInformation("Chunking completed in {Duration}ms: {ChunkCount} chunks created for {CodeSystem}",
            duration, chunks.Count, codeSystem);

        return chunks;
    }

    /// <summary>
    /// Preprocesses ICD-10 document to preserve code boundaries.
    /// Adds newlines before ICD-10 codes to reduce likelihood of mid-code splits.
    /// ICD-10 format: Letter + 2 digits + "." + 1-2 digits (e.g., "E11.9", "E11.65")
    /// </summary>
    private string PreprocessICD10Document(string documentText)
    {
        // Regex pattern for ICD-10 codes: [A-Z]\d{2}\.\d{1,2}
        // Matches: E11.9, E11.65, J44.0, etc.
        var pattern = @"\b([A-Z]\d{2}\.\d{1,2})\b";

        // Add newline before each ICD-10 code
        var preprocessed = Regex.Replace(documentText, pattern, "\n$1", RegexOptions.Multiline);

        _logger.LogDebug("ICD-10 preprocessing: Added newlines before code entries");

        return preprocessed;
    }

    /// <summary>
    /// Preprocesses CPT document to preserve code and modifier boundaries.
    /// Adds newlines before CPT codes to reduce likelihood of mid-code splits.
    /// CPT format: 5 digits, optionally followed by "-" and 2-digit modifier (e.g., "99213", "99213-25")
    /// </summary>
    private string PreprocessCPTDocument(string documentText)
    {
        // Regex pattern for CPT codes with optional modifiers: \d{5}(-\d{2})?
        // Matches: 99213, 99213-25, 80053, etc.
        var pattern = @"\b(\d{5}(?:-\d{2})?)\b";

        // Add newline before each CPT code
        var preprocessed = Regex.Replace(documentText, pattern, "\n$1", RegexOptions.Multiline);

        _logger.LogDebug("CPT preprocessing: Added newlines before code entries");

        return preprocessed;
    }
}
