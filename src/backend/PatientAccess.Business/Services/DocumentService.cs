using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;

namespace PatientAccess.Business.Services;

/// <summary>
/// Document service implementation for US_067.
/// Manages clinical document retrieval and metadata operations.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        PatientAccessDbContext context,
        ILogger<DocumentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves recent clinical documents for dashboard display (US_067, AC6).
    /// Returns documents ordered by upload date descending.
    /// </summary>
    public async Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(Guid userId, int limit)
    {
        if (limit < 1 || limit > 10)
        {
            throw new ArgumentException("Limit must be between 1 and 10", nameof(limit));
        }

        try
        {
            _logger.LogInformation("Retrieving recent documents for user {UserId}, limit: {Limit}", userId, limit);

            var documents = await _context.ClinicalDocuments
                .Where(d => d.PatientId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .Take(limit)
                .Select(d => new RecentDocumentDto
                {
                    DocumentId = d.DocumentId,
                    FileName = d.FileName,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    ProcessingStatus = d.ProcessingStatus.ToString(),
                    UploadedAt = d.UploadedAt,
                    ProcessedAt = d.ProcessedAt,
                    ErrorMessage = d.ErrorMessage
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} recent documents for user {UserId}", documents.Count, userId);

            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent documents for user {UserId}", userId);
            throw;
        }
    }
}
