using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for clinical document management (US_067).
/// Handles document retrieval and metadata operations.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Retrieves recent clinical documents for authenticated patient.
    /// Returns documents ordered by upload date descending.
    /// </summary>
    /// <param name="userId">Patient user ID from JWT claims</param>
    /// <param name="limit">Maximum number of documents to return (default: 3, max: 10)</param>
    /// <returns>List of recent documents with processing status</returns>
    Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(Guid userId, int limit);
}
