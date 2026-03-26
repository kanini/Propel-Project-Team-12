using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for OCR text extraction from PDF documents using Tesseract.
/// </summary>
public interface ITesseractOcrService
{
    /// <summary>
    /// Extracts text from all pages of a PDF document.
    /// </summary>
    /// <param name="pdfPath">Path to PDF file</param>
    /// <returns>List of OCR results per page</returns>
    Task<List<OcrResultDto>> ExtractTextFromPdfAsync(string pdfPath);
}
