using Pgvector;

namespace PatientAccess.Data.Models;

/// <summary>
/// Stores chunked document text with vector embeddings for RAG retrieval (DR-010).
/// </summary>
public class DocumentChunk
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string ChunkText { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public Vector? Embedding { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ClinicalDocument Document { get; set; } = null!;
}
