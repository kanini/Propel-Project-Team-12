namespace PatientAccess.Business.DTOs;

/// <summary>
/// Paginated provider list response for US_023 - Provider Browser (FR-006, Edge Case).
/// Supports pagination for large datasets (100+ providers) with 20 items per page.
/// </summary>
public class ProviderListResponseDto
{
    /// <summary>
    /// List of providers for current page.
    /// Empty array when no results match filters (AC-4).
    /// </summary>
    public List<ProviderDto> Providers { get; set; } = new List<ProviderDto>();

    /// <summary>
    /// Total number of providers matching filters (before pagination).
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page (default: 20).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages calculated from Total and PageSize.
    /// </summary>
    public int TotalPages { get; set; }
}
