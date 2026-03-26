using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Orchestrates clinical data extraction from documents (Supabase + OCR + AI).
/// </summary>
public interface IClinicalDataExtractionService
{
    /// <summary>
    /// Extracts clinical data from a document using multi-stage pipeline.
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    /// <returns>Extraction result with data points and metadata</returns>
    Task<ExtractionResultDto> ExtractClinicalDataAsync(Guid documentId);
}
