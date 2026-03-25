namespace PatientAccess.Business.DTOs;

/// <summary>
/// Insurance precheck response DTO (US_036)
/// </summary>
public class InsurancePrecheckResponseDto
{
    public required string Status { get; set; } // verified, pending, failed, not_found
    public string? ProviderName { get; set; }
    public string? MemberId { get; set; }
    public string? EffectiveDate { get; set; }
    public string? ExpirationDate { get; set; }
    public decimal? CopayAmount { get; set; }
    public decimal? DeductibleRemaining { get; set; }
    public string? Message { get; set; }
}
