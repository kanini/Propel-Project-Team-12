namespace PatientAccess.Business.DTOs;

/// <summary>
/// Mode switch request DTO (US_035)
/// </summary>
public class SwitchModeRequestDto
{
    public required string NewMode { get; set; } // "ai" or "manual"
}
