using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

public interface IHealthDashboardService
{
    Task<HealthDashboard360Dto> GetPatientHealthDashboardAsync(Guid patientId);
}
