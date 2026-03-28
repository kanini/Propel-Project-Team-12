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
/// Orchestrates the full RAG pipeline: Download → PDF Extract → Chunk → Embed → pgvector → Cosine Search → Gemini LLM → Clinical Data + Medical Codes.
/// </summary>
public class ClinicalDataExtractionService : IClinicalDataExtractionService
{
    private readonly PatientAccessDbContext _context;
    private readonly ISupabaseStorageService _supabaseService;
    private readonly IPdfTextExtractionService _pdfService;
    private readonly ITextChunkingService _chunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorService;
    private readonly IGeminiAiService _geminiService;
    private readonly ILogger<ClinicalDataExtractionService> _logger;

    private const decimal LowConfidenceThreshold = 50.0m;

    public ClinicalDataExtractionService(
        PatientAccessDbContext context,
        ISupabaseStorageService supabaseService,
        IPdfTextExtractionService pdfService,
        ITextChunkingService chunkingService,
        IEmbeddingService embeddingService,
        IVectorSearchService vectorService,
        IGeminiAiService geminiService,
        ILogger<ClinicalDataExtractionService> logger)
    {
        _context = context;
        _supabaseService = supabaseService;
        _pdfService = pdfService;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _vectorService = vectorService;
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<ExtractionResultDto> ExtractClinicalDataAsync(Guid documentId)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting RAG extraction pipeline for document {DocumentId}", documentId);

        var document = await _context.ClinicalDocuments.FirstOrDefaultAsync(d => d.DocumentId == documentId)
            ?? throw new InvalidOperationException($"Document {documentId} not found");

        string? localFilePath = null;

        try
        {
            // Stage 1: Download from Supabase
            _logger.LogInformation("Stage 1/6: Downloading from Supabase");
            localFilePath = await _supabaseService.DownloadDocumentAsync(documentId);

            // Stage 2: Extract text from PDF (replaces Tesseract OCR)
            _logger.LogInformation("Stage 2/6: Extracting text from PDF with PdfPig");
            var fullText = await _pdfService.ExtractTextAsync(localFilePath);

            if (string.IsNullOrWhiteSpace(fullText))
            {
                _logger.LogWarning("No text extracted from document {DocumentId}", documentId);
                return BuildEmptyResult(documentId, stopwatch.ElapsedMilliseconds);
            }

            // Stage 3: Chunk the text (512 tokens, 12.5% overlap)
            _logger.LogInformation("Stage 3/6: Chunking text ({Length} chars)", fullText.Length);
            var chunks = _chunkingService.ChunkText(fullText, maxTokens: 512, overlapRatio: 0.125);
            _logger.LogInformation("Created {ChunkCount} chunks", chunks.Count);

            // Stage 4: Generate embeddings and store in pgvector
            _logger.LogInformation("Stage 4/6: Generating embeddings via Gemini text-embedding-004");
            var chunkTexts = chunks.Select(c => c.Text).ToList();
            var embeddings = await _embeddingService.GenerateBatchEmbeddingsAsync(chunkTexts);

            await _vectorService.StoreChunksAsync(documentId, chunks, embeddings);

            // Stage 5: RAG retrieval — use cosine similarity to find most relevant chunks
            _logger.LogInformation("Stage 5/6: RAG retrieval via cosine similarity");
            var relevantChunks = await _vectorService.SearchSimilarChunksAsync(
                documentId,
                "Extract all clinical data: vitals, medications, allergies, diagnoses, lab results, procedures, encounters",
                topK: 15);

            var contextText = string.Join("\n\n", relevantChunks);

            // Stage 6: Call Gemini LLM with RAG context + system prompt
            _logger.LogInformation("Stage 6/6: Calling Gemini LLM with RAG context ({ContextLen} chars)", contextText.Length);
            var systemPrompt = GetSystemPrompt();
            var geminiResponse = await _geminiService.ExtractClinicalDataWithCodesAsync(contextText, systemPrompt);

            // Build result
            var allDataPoints = geminiResponse.DataPoints;
            foreach (var dp in allDataPoints)
            {
                if (dp.ConfidenceScore <= 0) dp.ConfidenceScore = 70m;
            }

            var result = new ExtractionResultDto
            {
                DocumentId = documentId,
                DataPoints = allDataPoints,
                MedicalCodes = geminiResponse.MedicalCodes,
                TotalDataPoints = allDataPoints.Count,
                FlaggedForReviewCount = allDataPoints.Count(dp => dp.ConfidenceScore < LowConfidenceThreshold),
                DataTypeBreakdown = allDataPoints.GroupBy(dp => dp.DataType.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                RequiresManualReview = allDataPoints.Any(dp => dp.ConfidenceScore < LowConfidenceThreshold),
                ExtractedAt = DateTime.UtcNow,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };

            if (stopwatch.ElapsedMilliseconds > 30000)
            {
                _logger.LogWarning("Document {DocumentId} extraction exceeded 30s: {Ms}ms", documentId, stopwatch.ElapsedMilliseconds);
            }

            _logger.LogInformation("RAG pipeline complete: {DataPoints} data points, {Codes} medical codes, {Flagged} flagged",
                result.TotalDataPoints, geminiResponse.MedicalCodes.Count, result.FlaggedForReviewCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG pipeline failed for document {DocumentId} after {Ms}ms", documentId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            if (localFilePath != null && File.Exists(localFilePath))
                File.Delete(localFilePath);
        }
    }

    private static string GetSystemPrompt()
    {
        return @"You are a clinical data extraction AI. Analyze the provided medical document text and extract ALL clinical data points and suggest appropriate ICD-10 and CPT medical codes.

Return a JSON object with this EXACT structure:
{
  ""clinical_data"": [
    {
      ""data_type"": ""Vital"" | ""Medication"" | ""Allergy"" | ""Diagnosis"" | ""LabResult"",
      ""data_key"": ""descriptive key (e.g., BloodPressure, CurrentMedication, DrugAllergy)"",
      ""data_value"": ""the exact extracted value"",
      ""confidence"": 0-100,
      ""source_excerpt"": ""verbatim text excerpt from the document"",
      ""structured_data"": { }
    }
  ],
  ""medical_codes"": [
    {
      ""code_system"": ""ICD10"" | ""CPT"",
      ""code_value"": ""the standard code (e.g., I10, E55.9, 99213)"",
      ""description"": ""official code description"",
      ""confidence"": 0-100,
      ""source_data_key"": ""the data_key this code relates to""
    }
  ]
}

RULES:
- Extract every distinct clinical fact: vitals (BP, HR, temp, weight, BMI), medications (name, dose, frequency), allergies (allergen, reaction, severity), diagnoses/conditions, lab results (test, value, reference range).
- For ICD-10 codes: map each diagnosis/condition to an ICD-10 code. Use the most specific code available.
- For CPT codes: suggest procedure codes based on encounters, lab panels, or procedures mentioned.
- Confidence 90-100: clearly stated facts. 70-89: inferred or partially visible. Below 70: uncertain.
- Do NOT invent data. Only extract what is present in the text.
- Return valid JSON only. No markdown fences.";
    }

    private static ExtractionResultDto BuildEmptyResult(Guid documentId, long ms)
    {
        return new ExtractionResultDto
        {
            DocumentId = documentId,
            DataPoints = new(),
            MedicalCodes = new(),
            TotalDataPoints = 0,
            FlaggedForReviewCount = 0,
            DataTypeBreakdown = new(),
            RequiresManualReview = false,
            ExtractedAt = DateTime.UtcNow,
            ProcessingTimeMs = ms
        };
    }
}
