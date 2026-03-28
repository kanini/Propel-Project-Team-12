using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

public class ClinicalVerificationService : IClinicalVerificationService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<ClinicalVerificationService> _logger;

    public ClinicalVerificationService(PatientAccessDbContext context, ILogger<ClinicalVerificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VerificationQueueResponseDto> GetVerificationQueueAsync(int limit = 10, string? searchTerm = null)
    {
        // Query patients who have at least one extracted clinical data point
        var query = _context.ExtractedClinicalData
            .Include(e => e.Patient)
            .Include(e => e.Document)
            .Include(e => e.MedicalCodes)
            .AsNoTracking();

        // Group by patient
        var patientGroups = await query
            .GroupBy(e => new { e.PatientId, e.Patient.Name })
            .Select(g => new
            {
                g.Key.PatientId,
                PatientName = g.Key.Name,
                PendingClinicalData = g.Count(e => e.VerificationStatus == VerificationStatus.AISuggested),
                PendingMedicalCodes = g.SelectMany(e => e.MedicalCodes)
                    .Count(m => m.VerificationStatus == MedicalCodeVerificationStatus.AISuggested),
                TotalClinicalData = g.Count(),
                LastUploadDate = g.Max(e => e.Document.UploadedAt),
            })
            .ToListAsync();

        // Apply search term filter in memory (EF can't translate complex string ops in GroupBy)
        var filtered = patientGroups.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.PatientName.ToLowerInvariant().Contains(term) ||
                p.PatientId.ToString().ToLowerInvariant().Contains(term));
        }

        var items = filtered
            .OrderByDescending(p => p.PendingClinicalData + p.PendingMedicalCodes)
            .ThenByDescending(p => p.LastUploadDate)
            .Take(limit)
            .Select(p =>
            {
                var totalPending = p.PendingClinicalData + p.PendingMedicalCodes;
                var priority = totalPending >= 8 ? "High" : totalPending >= 4 ? "Medium" : "Low";
                return new VerificationQueueItemDto
                {
                    PatientId = p.PatientId,
                    PatientName = p.PatientName,
                    PendingClinicalDataCount = p.PendingClinicalData,
                    PendingMedicalCodesCount = p.PendingMedicalCodes,
                    ConflictCount = 0, // Future enhancement
                    Priority = priority,
                    LastUploadDate = p.LastUploadDate,
                };
            })
            .ToList();

        return new VerificationQueueResponseDto
        {
            Items = items,
            TotalCount = items.Count,
        };
    }

    public async Task<ClinicalVerificationDashboardDto> GetVerificationDashboardAsync(Guid patientId)
    {
        var extractedData = await _context.ExtractedClinicalData
            .Include(e => e.Document)
            .Where(e => e.PatientId == patientId)
            .AsNoTracking()
            .OrderByDescending(e => e.ExtractedAt)
            .ToListAsync();

        var medicalCodes = await _context.MedicalCodes
            .Include(m => m.ExtractedData)
            .Include(m => m.Verifier)
            .Where(m => m.ExtractedData.PatientId == patientId)
            .AsNoTracking()
            .ToListAsync();

        return new ClinicalVerificationDashboardDto
        {
            PendingCount = extractedData.Count(e => e.VerificationStatus == VerificationStatus.AISuggested),
            VerifiedCount = extractedData.Count(e => e.VerificationStatus == VerificationStatus.StaffVerified),
            RejectedCount = extractedData.Count(e => e.VerificationStatus == VerificationStatus.Rejected),
            ConflictCount = 0, // Conflict detection is a future enhancement
            ClinicalData = extractedData.Select(e => new VerificationItemDto
            {
                ExtractedDataId = e.ExtractedDataId,
                DataType = e.DataType.ToString(),
                DataValue = e.DataValue,
                ConfidenceScore = e.ConfidenceScore,
                VerificationStatus = e.VerificationStatus.ToString(),
                SourceDocument = e.Document?.FileName,
                SourcePageNumber = e.SourcePageNumber,
                SourceTextExcerpt = e.SourceTextExcerpt
            }).ToList(),
            MedicalCodes = medicalCodes.Select(m => new VerificationMedicalCodeDto
            {
                MedicalCodeId = m.MedicalCodeId,
                CodeSystem = m.CodeSystem.ToString(),
                CodeValue = m.CodeValue,
                CodeDescription = m.CodeDescription,
                ConfidenceScore = m.ConfidenceScore,
                VerificationStatus = m.VerificationStatus.ToString(),
                VerifiedByName = m.Verifier?.Name,
                VerifiedAt = m.VerifiedAt
            }).ToList()
        };
    }

    public async Task VerifyDataPointAsync(Guid extractedDataId, Guid staffUserId)
    {
        var entity = await _context.ExtractedClinicalData.FindAsync(extractedDataId)
            ?? throw new InvalidOperationException($"Extracted data {extractedDataId} not found");

        entity.VerificationStatus = VerificationStatus.StaffVerified;
        entity.VerifiedBy = staffUserId;
        entity.VerifiedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Data point {Id} verified by staff {StaffId}", extractedDataId, staffUserId);
    }

    public async Task RejectDataPointAsync(Guid extractedDataId, Guid staffUserId)
    {
        var entity = await _context.ExtractedClinicalData.FindAsync(extractedDataId)
            ?? throw new InvalidOperationException($"Extracted data {extractedDataId} not found");

        entity.VerificationStatus = VerificationStatus.Rejected;
        entity.VerifiedBy = staffUserId;
        entity.VerifiedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Data point {Id} rejected by staff {StaffId}", extractedDataId, staffUserId);
    }

    public async Task VerifyMedicalCodeAsync(Guid medicalCodeId, Guid staffUserId)
    {
        var code = await _context.MedicalCodes.FindAsync(medicalCodeId)
            ?? throw new InvalidOperationException($"Medical code {medicalCodeId} not found");

        code.VerificationStatus = MedicalCodeVerificationStatus.Accepted;
        code.VerifiedBy = staffUserId;
        code.VerifiedAt = DateTime.UtcNow;
        code.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Medical code {Id} ({Code}) accepted by staff {StaffId}", medicalCodeId, code.CodeValue, staffUserId);
    }

    public async Task RejectMedicalCodeAsync(Guid medicalCodeId, Guid staffUserId, string reason)
    {
        var code = await _context.MedicalCodes.FindAsync(medicalCodeId)
            ?? throw new InvalidOperationException($"Medical code {medicalCodeId} not found");

        code.VerificationStatus = MedicalCodeVerificationStatus.Rejected;
        code.VerifiedBy = staffUserId;
        code.VerifiedAt = DateTime.UtcNow;
        code.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Medical code {Id} ({Code}) rejected by staff {StaffId}: {Reason}", medicalCodeId, code.CodeValue, staffUserId, reason);
    }

    public async Task ModifyMedicalCodeAsync(Guid medicalCodeId, string newCodeValue, string newDescription, Guid staffUserId)
    {
        var code = await _context.MedicalCodes.FindAsync(medicalCodeId)
            ?? throw new InvalidOperationException($"Medical code {medicalCodeId} not found");

        code.CodeValue = newCodeValue;
        code.CodeDescription = newDescription;
        code.VerificationStatus = MedicalCodeVerificationStatus.Modified;
        code.VerifiedBy = staffUserId;
        code.VerifiedAt = DateTime.UtcNow;
        code.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Medical code {Id} modified to {Code} by staff {StaffId}", medicalCodeId, newCodeValue, staffUserId);
    }
}
