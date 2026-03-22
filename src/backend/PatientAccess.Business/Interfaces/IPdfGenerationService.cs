using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for PDF generation (US_028 - FR-012).
/// Generates appointment confirmation PDFs using QuestPDF (TR-019).
/// </summary>
public interface IPdfGenerationService
{
    /// <summary>
    /// Generates appointment confirmation PDF with QuestPDF (AC-1).
    /// Includes appointment date/time, provider name, specialty, location,
    /// visit reason, and unique confirmation number.
    /// </summary>
    /// <param name="appointment">Appointment entity with navigation properties loaded</param>
    /// <returns>PDF content as byte array</returns>
    Task<byte[]> GenerateConfirmationPdfAsync(Appointment appointment);
}
