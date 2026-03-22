using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for provider data retrieval (US_023 - Provider Browser).
/// Supports filtering, search, and pagination with 300ms P95 response time target (NFR-001).
/// </summary>
public interface IProviderService
{
    /// <summary>
    /// Retrieves paginated list of providers with filtering and search (FR-006, AC1, AC2, AC3).
    /// </summary>
    /// <param name="search">Search term for provider name or specialty (optional, AC3)</param>
    /// <param name="specialty">Specialty filter (optional, AC2)</param>
    /// <param name="availability">Availability filter: "today", "this-week", "this-month", "any-time" (optional, AC2)</param>
    /// <param name="gender">Gender filter (optional, AC2)</param>
    /// <param name="page">Page number (1-indexed, default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, Edge Case requirement)</param>
    /// <returns>Paginated provider list with total count and page metadata</returns>
    Task<ProviderListResponseDto> GetProvidersAsync(
        string? search = null,
        string? specialty = null,
        string? availability = null,
        string? gender = null,
        int page = 1,
        int pageSize = 20);
}
