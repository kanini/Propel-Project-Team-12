namespace PatientAccess.Business.Interfaces;

public interface IPdfTextExtractionService
{
    Task<string> ExtractTextAsync(string pdfPath);
}
