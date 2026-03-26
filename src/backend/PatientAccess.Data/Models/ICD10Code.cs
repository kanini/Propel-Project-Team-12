using Pgvector;

namespace PatientAccess.Data.Models;

/// <summary>
/// ICD-10 medical coding knowledge base entity with vector embeddings for semantic retrieval (AIR-R04, DR-010).
/// Stores ICD-10-CM diagnosis codes with 1536-dimensional embeddings from Azure OpenAI text-embedding-3-small.
/// </summary>
public class ICD10Code
{
    /// <summary>
    /// Unique identifier for the ICD-10 code entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ICD-10-CM code (e.g., "E11.9", "E11.65").
    /// Maximum 20 characters. Unique indexed for fast exact code lookups.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full ICD-10 code description (e.g., "Type 2 diabetes mellitus without complications").
    /// Maximum 1000 characters.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ICD-10 category (e.g., "Endocrine, nutritional and metabolic diseases").
    /// Maximum 200 characters. Used for filtering and categorization.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// ICD-10 chapter code range (e.g., "E00-E89").
    /// Maximum 10 characters.
    /// </summary>
    public string ChapterCode { get; set; } = string.Empty;

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
    /// Contains: version, effectiveDate, status, subcategories, relatedCodes.
    /// Example: {"version": "ICD-10-CM-2024", "effectiveDate": "2024-10-01", "status": "active"}
    /// </summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// ICD-10-CM version identifier (e.g., "ICD-10-CM-2024").
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
