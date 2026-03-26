namespace PatientAccess.Data.Models;

/// <summary>
/// Staging entity for document chunks before embedding generation (AIR-R01, AIR-R04).
/// Stores 512-token chunks with 64-token overlap from medical coding documents.
/// </summary>
public class DocumentChunk
{
    /// <summary>
    /// Unique identifier for the document chunk.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Code system type: "ICD10", "CPT", or "ClinicalTerminology".
    /// Used to route chunks to appropriate vector index per AIR-R04.
    /// Maximum 50 characters.
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Original chunk text content.
    /// Maximum 2000 characters. Contains the actual text to be embedded.
    /// </summary>
    public string SourceText { get; set; } = string.Empty;

    /// <summary>
    /// Token count for this chunk (should be ≤512 per AIR-R01).
    /// Calculated using cl100k_base encoding (matches text-embedding-3-small).
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// Sequence number within the source document.
    /// Zero-indexed. Used to maintain document order.
    /// </summary>
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Starting token offset in the source document.
    /// Used for traceability and re-chunking validation.
    /// </summary>
    public int StartToken { get; set; }

    /// <summary>
    /// Ending token offset in the source document.
    /// Used fo traceability and re-chunking validation.
    /// </summary>
    public int EndToken { get; set; }

    /// <summary>
    /// Indicates if this chunk has 64-token overlap with the previous chunk.
    /// False for the first chunk (ChunkIndex = 0), true for subsequent chunks per AIR-R01.
    /// </summary>
    public bool OverlapWithPrevious { get; set; }

    /// <summary>
    /// Foreign key to target entity (ICD10Code, CPTCode, or ClinicalTerminology).
    /// Null until embedding is generated and stored. Set after embedding process completes.
    /// </summary>
    public Guid? TargetEntityId { get; set; }

    /// <summary>
    /// Indicates if this chunk has been processed (embedding generated and stored).
    /// Default: false. Set to true after embedding pipeline completes.
    /// </summary>
    public bool IsProcessed { get; set; } = false;

    /// <summary>
    /// Timestamp when the chunk was processed (embedding generated).
    /// Nullable. Set when IsProcessed = true.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Timestamp when the chunk was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
