namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for uploading and downloading documents via Supabase S3-compatible storage.
/// </summary>
public interface ISupabaseStorageService
{
    /// <summary>
    /// Uploads a document to S3-compatible storage.
    /// </summary>
    /// <param name="localFilePath">Path to the local file to upload</param>
    /// <param name="patientId">Patient identifier for folder structure</param>
    /// <param name="documentId">Document unique identifier</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <returns>The S3 object key where the file was stored</returns>
    Task<string> UploadDocumentAsync(string localFilePath, Guid patientId, Guid documentId, string contentType);

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
