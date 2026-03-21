namespace PatientAccess.Data.Models;

/// <summary>
/// Clinical document processing status values (DR-003).
/// Maps to: 1=Uploaded, 2=Processing, 3=Completed, 4=Failed
/// </summary>
public enum ProcessingStatus
{
    Uploaded = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
