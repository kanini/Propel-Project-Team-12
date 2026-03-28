using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models;
using Mscc.GenerativeAI;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for AI-powered clinical data extraction using Google Gemini.
/// Uses Mscc.GenerativeAI SDK to interact with Gemini API.
/// Requires API key from Google AI Studio (https://aistudio.google.com/apikey).
/// </summary>
public class GeminiAiService : IGeminiAiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiService> _logger;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly int _maxTokens;
    private readonly GenerativeModel? _model;

    public GeminiAiService(
        IConfiguration configuration,
        ILogger<GeminiAiService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _apiKey = _configuration["GeminiAI:ApiKey"] ?? string.Empty;
        _modelName = _configuration["GeminiAI:ModelName"] ?? "gemini-1.5-flash-latest";
        _maxTokens = int.Parse(_configuration["GeminiAI:MaxTokens"] ?? "8000");

        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "SET_VIA_ENV_GEMINIAI__APIKEY")
        {
            _logger.LogWarning("Gemini API key not configured. Service will return stub data. Set GEMINIAI__APIKEY environment variable.");
        }
        else
        {
            try
            {
                var googleAi = new GoogleAI(_apiKey);
                _model = googleAi.GenerativeModel(model: _modelName);
                _logger.LogInformation("Gemini AI service initialized successfully with model: {ModelName}", _modelName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Gemini model. Will use stub data.");
            }
        }
    }

    public async Task<List<ExtractedDataPointDto>> ExtractClinicalDataAsync(string ocrText, string promptTemplate)
    {
        _logger.LogInformation("Extracting clinical data using Gemini AI. Text length: {Length}", ocrText.Length);

        if (_model == null)
        {
            _logger.LogWarning("Gemini model not initialized. Returning stub data.");
            return GetStubExtractedData();
        }

        try
        {
            // Build the full prompt
            var fullPrompt = promptTemplate.Replace("{OCR_TEXT}", ocrText);

            // Truncate if text is too long
            if (fullPrompt.Length > _maxTokens * 4) // Rough estimate: 1 token ~= 4 chars
            {
                _logger.LogWarning("Prompt too long ({Length} chars). Truncating to fit within token limit.", fullPrompt.Length);
                var maxChars = _maxTokens * 3; // Leave some room for response
                fullPrompt = promptTemplate.Replace("{OCR_TEXT}", ocrText.Substring(0, Math.Min(ocrText.Length, maxChars)));
            }

            _logger.LogDebug("Sending request to Gemini API...");

            // Generate content
            var response = await _model.GenerateContent(fullPrompt);
            
            if (string.IsNullOrWhiteSpace(response?.Text))
            {
                _logger.LogWarning("Gemini API returned empty response. Using stub data.");
                return GetStubExtractedData();
            }

            _logger.LogInformation("Received response from Gemini API. Response length: {Length}", response.Text.Length);
            _logger.LogDebug("Gemini response: {Response}", response.Text);

            // Parse JSON response
            var extractedData = ParseGeminiResponse(response.Text);

            if (extractedData.Count == 0)
            {
                _logger.LogWarning("No data extracted from Gemini response. This may indicate low-quality OCR or no clinical data in document.");
            }

            _logger.LogInformation("Successfully extracted {Count} data points from Gemini AI", extractedData.Count);
            return extractedData;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response as JSON. Response may not be in expected format.");
            return GetStubExtractedData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract clinical data using Gemini AI");
            throw;
        }
    }

    public async Task<string> GenerateContentAsync(
        string prompt,
        string model = "gemini-1.5-flash-latest",
        float temperature = 0.7f,
        int maxOutputTokens = 8000,
        string systemInstruction = "",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating content using Gemini AI. Model: {Model}", model);

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured. Returning stub response.");
            return "Stub response: This is a placeholder for Gemini-generated content.";
        }

        try
        {
            // TODO: Implement actual Gemini API call for content generation
            // 1. Construct request with model, prompt, temperature, max tokens
            // 2. Include system instruction if provided
            // 3. Send POST request to Gemini API endpoint
            // 4. Parse and return generated text

            _logger.LogInformation("Gemini content generation would be made here (stub mode)");

            await Task.Delay(100, cancellationToken); // Simulate async operation
            return "Stub response: This is a placeholder for Gemini-generated content.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate content using Gemini AI");
            throw;
        }
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating embedding using Gemini AI. Text length: {Length}", text.Length);

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured. Returning stub embedding.");
            return GetStubEmbedding();
        }

        try
        {
            // TODO: Implement actual Gemini API call for embedding generation
            // 1. Use embedding model (e.g., text-embedding-004)
            // 2. Send POST request to Gemini embedding endpoint
            // 3. Parse and return embedding vector

            _logger.LogInformation("Gemini embedding generation would be made here (stub mode)");

            await Task.Delay(50, cancellationToken); // Simulate async operation
            return GetStubEmbedding();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding using Gemini AI");
            throw;
        }
    }

    public async Task<Dictionary<string, List<float>>> GenerateBatchEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating batch embeddings using Gemini AI. Batch size: {Count}", texts.Count);

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured. Returning stub embeddings.");
            return texts.ToDictionary(text => text, _ => GetStubEmbedding());
        }

        try
        {
            // TODO: Implement actual Gemini API batch embedding call
            // 1. Use embedding model with batch support
            // 2. Send batch request to Gemini embedding endpoint
            // 3. Parse and return list of embedding vectors

            _logger.LogInformation("Gemini batch embedding generation would be made here (stub mode)");

            await Task.Delay(100, cancellationToken); // Simulate async operation
            return texts.ToDictionary(text => text, _ => GetStubEmbedding());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate batch embeddings using Gemini AI");
            throw;
        }
    }

    private List<ExtractedDataPointDto> GetStubExtractedData()
    {
        return new List<ExtractedDataPointDto>
        {
            new ExtractedDataPointDto
            {
                DataType = ClinicalDataType.Vital,
                DataKey = "BloodPressure",
                DataValue = "120/80 mmHg",
                ConfidenceScore = 95.5m,
                SourcePageNumber = 1,
                SourceTextExcerpt = "Blood Pressure: 120/80 mmHg",
                StructuredData = new Dictionary<string, object>
                {
                    { "systolic", 120 },
                    { "diastolic", 80 },
                    { "unit", "mmHg" }
                }
            },
            new ExtractedDataPointDto
            {
                DataType = ClinicalDataType.Medication,
                DataKey = "CurrentMedication",
                DataValue = "Lisinopril 10mg daily",
                ConfidenceScore = 88.0m,
                SourcePageNumber = 1,
                SourceTextExcerpt = "Current Medications: Lisinopril 10mg daily",
                StructuredData = new Dictionary<string, object>
                {
                    { "name", "Lisinopril" },
                    { "dosage", "10mg" },
                    { "frequency", "daily" }
                }
            }
        };
    }

    private List<float> GetStubEmbedding()
    {
        // Return a stub 768-dimensional embedding vector
        var random = new Random(42); // Fixed seed for consistency
        return Enumerable.Range(0, 768).Select(_ => (float)random.NextDouble()).ToList();
    }
}
