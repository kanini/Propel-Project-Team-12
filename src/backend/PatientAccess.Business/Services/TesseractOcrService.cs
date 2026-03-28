using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models;
using Tesseract;
using Docnet.Core;
using Docnet.Core.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for OCR text extraction using Tesseract.
/// Uses Docnet.Core for PDF to image conversion and Tesseract for OCR.
/// Requires tessdata files (eng.traineddata) in the configured DataPath.
/// Windows-only service due to System.Drawing dependency.
/// </summary>
[SupportedOSPlatform("windows")]
public class TesseractOcrService : ITesseractOcrService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TesseractOcrService> _logger;
    private readonly string _tessdataPath;
    private readonly string _language;

    public TesseractOcrService(
        IConfiguration configuration,
        ILogger<TesseractOcrService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _tessdataPath = _configuration["Tesseract:DataPath"] ?? "./tessdata";
        _language = _configuration["Tesseract:Language"] ?? "eng";

        // Validate tessdata path exists
        if (!Directory.Exists(_tessdataPath))
        {
            _logger.LogWarning("Tessdata directory not found at {Path}. Creating directory. Download tessdata files from: https://github.com/tesseract-ocr/tessdata", _tessdataPath);
            Directory.CreateDirectory(_tessdataPath);
        }

        var langFile = Path.Combine(_tessdataPath, $"{_language}.traineddata");
        if (!File.Exists(langFile))
        {
            _logger.LogWarning("Tesseract language file not found: {LangFile}. OCR may fail. Download from: https://github.com/tesseract-ocr/tessdata", langFile);
        }
    }

    public async Task<List<OcrResultDto>> ExtractTextFromPdfAsync(string pdfPath)
    {
        _logger.LogInformation("Extracting text from PDF: {PdfPath}", pdfPath);

        if (!File.Exists(pdfPath))
        {
            throw new FileNotFoundException($"PDF file not found: {pdfPath}");
        }

        var ocrResults = new List<OcrResultDto>();
        var tempImageFiles = new List<string>();

        try
        {
            // Initialize Docnet library for PDF processing
            using var docReader = DocLib.Instance.GetDocReader(pdfPath, new PageDimensions(1920, 2560));
            var pageCount = docReader.GetPageCount();
            
            _logger.LogInformation("PDF loaded successfully. Page count: {PageCount}", pageCount);

            // Initialize Tesseract engine
            using var ocrEngine = new TesseractEngine(_tessdataPath, _language, EngineMode.Default);
            
            // Process each page
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var pageNumber = pageIndex + 1;
                _logger.LogInformation("Processing page {PageNumber} of {TotalPages}", pageNumber, pageCount);

                try
                {
                    // Render PDF page as image
                    using var pageReader = docReader.GetPageReader(pageIndex);
                    var rawBytes = pageReader.GetImage();
                    var width = pageReader.GetPageWidth();
                    var height = pageReader.GetPageHeight();

                    // Save to temporary file for Tesseract
                    var tempImagePath = Path.Combine(Path.GetTempPath(), $"ocr_page_{pageNumber}_{Guid.NewGuid()}.png");
                    tempImageFiles.Add(tempImagePath);

                    // Convert byte array to bitmap and save
                    using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                    {
                        var bitmapData = bitmap.LockBits(
                            new Rectangle(0, 0, width, height),
                            ImageLockMode.WriteOnly,
                            bitmap.PixelFormat);

                        System.Runtime.InteropServices.Marshal.Copy(rawBytes, 0, bitmapData.Scan0, rawBytes.Length);
                        bitmap.UnlockBits(bitmapData);
                        
                        bitmap.Save(tempImagePath, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    // Perform OCR on the image
                    using var img = Pix.LoadFromFile(tempImagePath);
                    using var page = ocrEngine.Process(img);
                    
                    var extractedText = page.GetText();
                    var confidence = page.GetMeanConfidence() * 100;

                    ocrResults.Add(new OcrResultDto
                    {
                        PageNumber = pageNumber,
                        ExtractedText = extractedText ?? string.Empty,
                        ConfidenceScore = (decimal)confidence,
                        Language = _language
                    });

                    _logger.LogInformation("Page {PageNumber} OCR completed. Confidence: {Confidence:F2}%, Text length: {Length}", 
                        pageNumber, confidence, extractedText?.Length ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process page {PageNumber}", pageNumber);
                    
                    // Add error result for this page
                    ocrResults.Add(new OcrResultDto
                    {
                        PageNumber = pageNumber,
                        ExtractedText = $"[OCR Error on page {pageNumber}: {ex.Message}]",
                        ConfidenceScore = 0,
                        Language = _language
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from PDF: {PdfPath}", pdfPath);
            throw;
        }
        finally
        {
            // Clean up temporary image files
            foreach (var tempFile in tempImageFiles)
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary file: {TempFile}", tempFile);
                }
            }
        }

        _logger.LogInformation("OCR extraction completed. Successfully extracted {PageCount} pages", ocrResults.Count);
        
        return await Task.FromResult(ocrResults);
    }
}
