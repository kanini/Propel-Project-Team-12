using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

public class HealthDashboardService : IHealthDashboardService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<HealthDashboardService> _logger;

    public HealthDashboardService(PatientAccessDbContext context, ILogger<HealthDashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthDashboard360Dto> GetPatientHealthDashboardAsync(Guid patientId)
    {
        _logger.LogInformation("Building 360° health dashboard for patient {PatientId}", patientId);

        var patient = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == patientId)
            ?? throw new InvalidOperationException($"Patient {patientId} not found");

        // Get all extracted data for this patient across all documents
        var extractedData = await _context.ExtractedClinicalData
            .Include(e => e.MedicalCodes)
            .Include(e => e.Document)
            .Where(e => e.PatientId == patientId)
            .AsNoTracking()
            .OrderByDescending(e => e.ExtractedAt)
            .ToListAsync();

        var totalDocuments = await _context.ClinicalDocuments
            .CountAsync(d => d.PatientId == patientId && d.ProcessingStatus == ProcessingStatus.Completed);

        // Group by data type
        var conditions = extractedData.Where(e => e.DataType == ClinicalDataType.Diagnosis).ToList();
        var medications = extractedData.Where(e => e.DataType == ClinicalDataType.Medication).ToList();
        var allergies = extractedData.Where(e => e.DataType == ClinicalDataType.Allergy).ToList();
        var vitals = extractedData.Where(e => e.DataType == ClinicalDataType.Vital).ToList();
        var labResults = extractedData.Where(e => e.DataType == ClinicalDataType.LabResult).ToList();

        // Collect all medical codes
        var allCodes = extractedData
            .SelectMany(e => e.MedicalCodes)
            .Select(MapMedicalCode)
            .ToList();

        var dto = new HealthDashboard360Dto
        {
            Demographics = new PatientDemographicsDto
            {
                Name = patient.Name,
                Email = patient.Email,
                Phone = patient.Phone,
                DateOfBirth = patient.DateOfBirth?.ToString("MMM dd, yyyy")
            },
            Conditions = conditions.Select(MapToClinicalItem).ToList(),
            Medications = medications.Select(MapToClinicalItem).ToList(),
            Allergies = allergies.Select(MapToClinicalItem).ToList(),
            Vitals = vitals.Select(MapToClinicalItem).ToList(),
            LabResults = labResults.Select(MapToClinicalItem).ToList(),
            MedicalCodes = allCodes,
            Stats = new DashboardStatsOverviewDto
            {
                TotalExtractedItems = extractedData.Count,
                VerifiedItems = extractedData.Count(e => e.VerificationStatus == VerificationStatus.Verified),
                PendingItems = extractedData.Count(e => e.VerificationStatus == VerificationStatus.Pending),
                TotalDocuments = totalDocuments,
                TotalMedicalCodes = allCodes.Count
            }
        };

        return dto;
    }

    private static ClinicalItemDto MapToClinicalItem(ExtractedClinicalData e)
    {
        return new ClinicalItemDto
        {
            ExtractedDataId = e.ExtractedDataId,
            DataType = e.DataType.ToString(),
            DataKey = e.DataKey,
            DataValue = e.DataValue,
            ConfidenceScore = e.ConfidenceScore,
            VerificationStatus = e.VerificationStatus.ToString(),
            Source = e.Document?.FileName,
            SourcePageNumber = e.SourcePageNumber,
            SourceTextExcerpt = e.SourceTextExcerpt,
            StructuredData = !string.IsNullOrEmpty(e.StructuredData)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(e.StructuredData)
                : null,
            MedicalCodes = e.MedicalCodes.Select(MapMedicalCode).ToList()
        };
    }

    private static MedicalCodeDto MapMedicalCode(MedicalCode m)
    {
        return new MedicalCodeDto
        {
            MedicalCodeId = m.MedicalCodeId,
            CodeSystem = m.CodeSystem,
            CodeValue = m.CodeValue,
            CodeDescription = m.CodeDescription,
            ConfidenceScore = m.ConfidenceScore,
            VerificationStatus = m.VerificationStatus.ToString(),
            VerifiedBy = m.VerifiedBy,
            VerifiedAt = m.VerifiedAt
        };
    }
}
