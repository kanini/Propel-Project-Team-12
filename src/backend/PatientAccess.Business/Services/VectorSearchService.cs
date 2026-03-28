using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

public class VectorSearchService : IVectorSearchService
{
    private readonly PatientAccessDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<VectorSearchService> _logger;

    public VectorSearchService(
        PatientAccessDbContext context,
        IEmbeddingService embeddingService,
        ILogger<VectorSearchService> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task StoreChunksAsync(Guid documentId, List<TextChunkDto> chunks, List<float[]> embeddings)
    {
        // Remove existing chunks for this document (re-processing scenario)
        var existing = await _context.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .ToListAsync();

        if (existing.Any())
        {
            _context.DocumentChunks.RemoveRange(existing);
            await _context.SaveChangesAsync();
        }

        var entities = new List<DocumentChunk>();
        for (var i = 0; i < chunks.Count; i++)
        {
            var entity = new DocumentChunk
            {
                ChunkId = Guid.NewGuid(),
                DocumentId = documentId,
                ChunkIndex = chunks[i].Index,
                ChunkText = chunks[i].Text,
                TokenCount = chunks[i].TokenCount,
                Embedding = embeddings.Count > i && embeddings[i].Length > 0
                    ? new Vector(embeddings[i])
                    : null,
                CreatedAt = DateTime.UtcNow
            };
            entities.Add(entity);
        }

        await _context.DocumentChunks.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stored {Count} chunks with embeddings for document {DocumentId}", entities.Count, documentId);
    }

    public async Task<List<string>> SearchSimilarChunksAsync(Guid documentId, string query, int topK = 10)
    {
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

        if (queryEmbedding.Length == 0)
        {
            _logger.LogWarning("Empty query embedding, falling back to retrieving all chunks");
            return await RetrieveAllChunksAsync(documentId);
        }

        var vectorParam = new Vector(queryEmbedding);
        var vectorString = $"[{string.Join(",", queryEmbedding)}]";

        // Use raw SQL for cosine similarity search via pgvector <=> operator
        var chunks = await _context.DocumentChunks
            .FromSqlRaw(
                @"SELECT ""ChunkId"", ""DocumentId"", ""ChunkIndex"", ""ChunkText"", ""TokenCount"", ""Embedding"", ""CreatedAt""
                  FROM ""DocumentChunks""
                  WHERE ""DocumentId"" = {0} AND ""Embedding"" IS NOT NULL
                  ORDER BY ""Embedding"" <=> {1}::vector
                  LIMIT {2}",
                documentId, vectorString, topK)
            .AsNoTracking()
            .ToListAsync();

        return chunks.Select(c => c.ChunkText).ToList();
    }

    public async Task<List<string>> RetrieveAllChunksAsync(Guid documentId)
    {
        var chunks = await _context.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .Select(c => c.ChunkText)
            .AsNoTracking()
            .ToListAsync();

        return chunks;
    }
}
