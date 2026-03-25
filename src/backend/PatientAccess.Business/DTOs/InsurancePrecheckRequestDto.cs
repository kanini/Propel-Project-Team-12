namespace PatientAccess.Business.DTOs;

/// <summary>
/// Insurance precheck request DTO (US_036)
/// </summary>
public class InsurancePrecheckRequestDto
{
    public required string ProviderId { get; set; }
    public required string MemberId { get; set; }
    public string? GroupNumber { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public int AppointmentId { get; set; }
}
