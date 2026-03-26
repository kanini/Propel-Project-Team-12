using Pgvector;

namespace PatientAccess.Data.Models;

/// <summary>
/// Clinical terminology knowledge base entity with vector embeddings for semantic retrieval (AIR-R04, DR-010).
/// Stores clinical terms (diagnoses, medications, procedures, symptoms) with 1536-dimensional embeddings 
/// from Azure OpenAI text-embedding-3-small, mapped to ICD-10 and CPT codes.
/// </summary>
public class ClinicalTerminology
{
    /// <summary>
    /// Unique identifier for the clinical terminology entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Clinical term (e.g., "Type 2 Diabetes Mellitus", "Hypertension", "Chest Pain").
    /// Maximum 500 characters. Indexed for fast term lookups.
    /// </summary>
    public string Term { get; set; } = string.Empty;

    /// <summary>
    /// Clinical term category (e.g., "Diagnosis", "Medication", "Procedure", "Symptom").
    /// Maximum 100 characters. Used for filtering and categorization.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// List of synonyms and alternative terms (e.g., ["T2DM", "NIDDM", "Adult-onset diabetes"]).
    /// Stored as JSONB array. Used for fuzzy matching and search expansion.
    /// </summary>
    public string Synonyms { get; set; } = "[]";

    /// <summary>
    /// 1536-dimensional vector embedding from Azure OpenAI text-embedding-3-small.
    /// Used for semantic similarity search with cosine distance.
    /// </summary>
    public Vector? Embedding { get; set; }

    /// <summary>
    /// Original text used to generate the embedding (term + category + synonyms).
    /// Maximum 2000 characters. Stored for traceability and re-indexing.
    /// </summary>
    public string ChunkText { get; set; } = string.Empty;

    /// <summary>
    /// JSONB metadata field for extensibility.
    /// Contains: source, mappedICD10Codes, mappedCPTCodes, clinicalContext.
    /// Example: {"source": "SNOMED-CT", "mappedICD10Codes": ["E11.9"], "mappedCPTCodes": ["99213"]}
    /// </summary>
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Source of the clinical terminology (e.g., "SNOMED-CT", "LOINC", "Internal").
    /// Maximum 100 characters. Used for data lineage and source tracking.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the terminology entry is currently active.
    /// Default: true. Set to false for deprecated terms to support soft delete.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the terminology entry was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the terminology entry was last updated (UTC).
    /// Nullable to distinguish never-updated entries.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
