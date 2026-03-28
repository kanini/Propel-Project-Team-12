using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for AI-powered clinical data extraction using Google Gemini.
/// </summary>
public interface IGeminiAiService
{
    /// <summary>
    /// Extracts structured clinical data from OCR text using Gemini LLM.
    /// </summary>
    /// <param name="ocrText">Raw OCR-extracted text</param>
    /// <param name="promptTemplate">Prompt template for extraction</param>
    /// <returns>List of extracted data points</returns>
    Task<List<ExtractedDataPointDto>> ExtractClinicalDataAsync(string ocrText, string promptTemplate);
    Task<Dictionary<string, List<float>>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken);
    Task<string> GenerateContentAsync(string systemPrompt, string userPrompt, float temperature, int maxTokens, string responseFormat, CancellationToken cancellationToken);
    Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);
}
