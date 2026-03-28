using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PatientAccess.Business.Services;

public class PdfTextExtractionService : IPdfTextExtractionService
{
    private readonly ILogger<PdfTextExtractionService> _logger;

    public PdfTextExtractionService(ILogger<PdfTextExtractionService> logger)
    {
        _logger = logger;
    }

    public Task<string> ExtractTextAsync(string pdfPath)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException($"PDF file not found: {pdfPath}");

        _logger.LogInformation("Extracting text from PDF: {PdfPath}", pdfPath);

        var textParts = new List<string>();

        using var document = PdfDocument.Open(pdfPath);
        foreach (Page page in document.GetPages())
        {
            var pageText = page.Text;
            if (!string.IsNullOrWhiteSpace(pageText))
            {
                textParts.Add($"--- PAGE {page.Number} ---\n{pageText}");
            }
        }

        var fullText = string.Join("\n\n", textParts);
        _logger.LogInformation("Extracted {CharCount} characters from {PageCount} pages", fullText.Length, document.NumberOfPages);

        return Task.FromResult(fullText);
    }
}
