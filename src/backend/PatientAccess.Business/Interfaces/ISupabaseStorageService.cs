namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for downloading documents from Supabase Storage.
/// </summary>
public interface ISupabaseStorageService
{
    /// <summary>
    /// Downloads a document from Supabase Storage to a temporary local file.
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    /// <returns>Path to downloaded temporary file</returns>
    Task<string> DownloadDocumentAsync(Guid documentId);

    /// <summary>
    /// Gets a document as a stream from Supabase Storage.
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    /// <returns>Stream containing document content</returns>
    Task<Stream> GetDocumentStreamAsync(Guid documentId);
}
