using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientAccess.Data.Models;

/// <summary>
/// Staging entity for document chunks before embedding generation.
/// Implements AIR-R01 (512-token chunks with 64-token overlap).
/// Used by DocumentChunkingService and ChunkDocumentsJob.
/// </summary>
[Table("DocumentChunks")]
public class DocumentChunk
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Code system identifier: "ICD10", "CPT", or "ClinicalTerminology"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Original chunk text content (max 2000 chars)
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string SourceText { get; set; } = string.Empty;

    /// <summary>
    /// Token count for this chunk (should be ≤512 per AIR-R01)
    /// </summary>
    [Required]
    public int TokenCount { get; set; }

    /// <summary>
    /// Sequence number within source document
    /// </summary>
    [Required]
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Starting token offset in source document
    /// </summary>
    [Required]
    public int StartToken { get; set; }

    /// <summary>
    /// Ending token offset in source document
    /// </summary>
    [Required]
    public int EndToken { get; set; }

    /// <summary>
    /// Indicates if this chunk overlaps with previous chunk (64-token overlap per AIR-R01)
    /// </summary>
    [Required]
    public bool OverlapWithPrevious { get; set; }

    /// <summary>
    /// Foreign key to target entity (ICD10Code/CPTCode/ClinicalTerminology) after embedding generation
    /// </summary>
    public Guid? TargetEntityId { get; set; }

    /// <summary>
    /// Indicates if embedding has been generated for this chunk
    /// </summary>
    [Required]
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Timestamp when embedding was generated
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Chunk creation timestamp (UTC)
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }
}
