using Pgvector;

namespace PatientAccess.Data.Models;

/// <summary>
/// CPT (Current Procedural Terminology) medical coding knowledge base entity with vector embeddings 
/// for semantic retrieval (AIR-R04, DR-010).
/// Stores CPT procedure/service codes with 1536-dimensional embeddings from Azure OpenAI text-embedding-3-small.
/// </summary>
public class CPTCode
{
    /// <summary>
    /// Unique identifier for the CPT code entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// CPT code (e.g., "99213", "80053").
    /// Maximum 10 characters. Unique indexed for fast exact code lookups.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full CPT code description (e.g., "Office or other outpatient visit, established patient, 20-29 minutes").
    /// Maximum 1000 characters.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// CPT category (e.g., "Evaluation and Management", "Laboratory Procedures").
    /// Maximum 200 characters. Used for filtering and categorization.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional CPT modifier code (e.g., "25", "59").
    /// Maximum 50 characters. Nullable for codes without modifiers.
    /// </summary>
    public string? Modifier { get; set; }

    /// <summary>
    /// 1536-dimensional vector embedding from Azure OpenAI text-embedding-3-small.
    /// Used for semantic similarity search with cosine distance.
    /// </summary>
    public Vector? Embedding { get; set; }

    /// <summary>
    /// Original text used to generate the embedding (code + description + category).
    /// Maximum 2000 characters. Stored for traceability and re-indexing.
    /// </summary>
    public string ChunkText { get; set; } = string.Empty;

    /// <summary>
    /// JSONB metadata field for extensibility.
    /// Contains: version, effectiveDate, status, rvuValue, billingGuidelines.
    /// Example: {"version": "CPT-2024", "effectiveDate": "2024-01-01", "status": "active", "rvuValue": "1.5"}
    /// </summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// CPT version identifier (e.g., "CPT-2024").
    /// Maximum 20 characters. Used for version control and updates.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the code is currently active (not deprecated).
    /// Default: true. Set to false for deprecated codes to support soft delete.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the code entry was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the code entry was last updated (UTC).
    /// Nullable to distinguish never-updated entries.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
