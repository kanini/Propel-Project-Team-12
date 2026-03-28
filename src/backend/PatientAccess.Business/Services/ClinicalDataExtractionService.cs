using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Orchestrates clinical data extraction pipeline: Supabase → OCR → Gemini (task_002).
/// </summary>
public class ClinicalDataExtractionService : IClinicalDataExtractionService
{
    private readonly PatientAccessDbContext _context;
    private readonly ISupabaseStorageService _supabaseService;
    private readonly ITesseractOcrService _ocrService;
    private readonly IGeminiAiService _geminiService;
    private readonly ILogger<ClinicalDataExtractionService> _logger;
    private readonly string _promptTemplatePath;

    private const decimal LowConfidenceThreshold = 50.0m;

    public ClinicalDataExtractionService(
        PatientAccessDbContext context,
        ISupabaseStorageService supabaseService,
        ITesseractOcrService ocrService,
        IGeminiAiService geminiService,
        ILogger<ClinicalDataExtractionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _supabaseService = supabaseService ?? throw new ArgumentNullException(nameof(supabaseService));
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _promptTemplatePath = ".propel/prompts/clinical-data-extraction.md";
    }

    public async Task<ExtractionResultDto> ExtractClinicalDataAsync(Guid documentId)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Starting clinical data extraction for document {DocumentId}", documentId);

        // Load document
        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        try
        {
            // Stage 1: Download from Supabase
            _logger.LogInformation("Stage 1: Downloading document from Supabase Storage");
            var localFilePath = await _supabaseService.DownloadDocumentAsync(documentId);

            // Stage 2: OCR Extraction
            _logger.LogInformation("Stage 2: Performing OCR extraction");
            var ocrResults = await _ocrService.ExtractTextFromPdfAsync(localFilePath);

            // Stage 3: Gemini AI Extraction
            _logger.LogInformation("Stage 3: Extracting clinical data with Gemini AI");
            var promptTemplate = LoadPromptTemplate();
            var allDataPoints = new List<ExtractedDataPointDto>();

            foreach (var ocrResult in ocrResults)
            {
                var dataPoints = await _geminiService.ExtractClinicalDataAsync(ocrResult.ExtractedText, promptTemplate);
                
                // Combine OCR and Gemini confidence scores
                foreach (var point in dataPoints)
                {
                    point.ConfidenceScore = CalculateCombinedConfidence(ocrResult.ConfidenceScore, point.ConfidenceScore);
                    point.SourcePageNumber = ocrResult.PageNumber;
                }

                allDataPoints.AddRange(dataPoints);
            }

            // Build extraction result
            var result = new ExtractionResultDto
            {
                DocumentId = documentId,
                DataPoints = allDataPoints,
                TotalDataPoints = allDataPoints.Count,
                FlaggedForReviewCount = allDataPoints.Count(dp => dp.ConfidenceScore < LowConfidenceThreshold),
                DataTypeBreakdown = BuildDataTypeBreakdown(allDataPoints),
                RequiresManualReview = allDataPoints.Any(dp => dp.ConfidenceScore < LowConfidenceThreshold),
                ExtractedAt = DateTime.UtcNow,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };

            // Log performance warning if >30s
            if (stopwatch.ElapsedMilliseconds > 30000)
            {
                _logger.LogWarning("Document {DocumentId} extraction exceeded 30s target: {ProcessingTimeMs}ms",
                    documentId, stopwatch.ElapsedMilliseconds);
            }

            // Cleanup temporary file
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }

            _logger.LogInformation("Clinical data extraction completed. Extracted {TotalDataPoints} data points, {FlaggedCount} flagged for review",
                result.TotalDataPoints, result.FlaggedForReviewCount);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to extract clinical data from document {DocumentId} after {ProcessingTimeMs}ms",
                documentId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private string LoadPromptTemplate()
    {
        if (File.Exists(_promptTemplatePath))
        {
            return File.ReadAllText(_promptTemplatePath);
        }

        _logger.LogWarning("Prompt template not found at {Path}. Using default template.", _promptTemplatePath);

        // Default template
        return @"Extract clinical data from the following medical document text.
Return a JSON array of data points with the following structure:
[
  {
    ""dataType"": ""Vital"" | ""Medication"" | ""Allergy"" | ""Diagnosis"" | ""LabResult"",
    ""dataKey"": ""descriptive key"",
    ""dataValue"": ""extracted value"",
    ""confidenceScore"": 0-100,
    ""sourceTextExcerpt"": ""relevant excerpt"",
    ""structuredData"": { additional structured fields }
  }
]

Medical Document Text:
{OCR_TEXT}";
    }

    private decimal CalculateCombinedConfidence(decimal ocrConfidence, decimal geminiConfidence)
    {
        // Combined confidence: 30% OCR + 70% Gemini
        return (ocrConfidence * 0.3m) + (geminiConfidence * 0.7m);
    }

    private Dictionary<string, int> BuildDataTypeBreakdown(List<ExtractedDataPointDto> dataPoints)
    {
        return dataPoints
            .GroupBy(dp => dp.DataType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
