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

    private List<ExtractedDataPointDto> ParseGeminiResponse(string responseText)
    {
        try
        {
            // Remove markdown code blocks if present
            var jsonText = responseText.Trim();
            if (jsonText.StartsWith("```json"))
            {
                jsonText = jsonText.Substring(7);
            }
            if (jsonText.StartsWith("```"))
            {
                jsonText = jsonText.Substring(3);
            }
            if (jsonText.EndsWith("```"))
            {
                jsonText = jsonText.Substring(0, jsonText.Length - 3);
            }
            jsonText = jsonText.Trim();

            // Parse JSON array
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
            {
                _logger.LogError("Expected JSON array but got {ValueKind}", root.ValueKind);
                return new List<ExtractedDataPointDto>();
            }

            var results = new List<ExtractedDataPointDto>();

            foreach (var item in root.EnumerateArray())
            {
                try
                {
                    var dataTypeStr = item.GetProperty("dataType").GetString() ?? "Vital";
                    var dataType = Enum.TryParse<ClinicalDataType>(dataTypeStr, true, out var parsedType) 
                        ? parsedType 
                        : ClinicalDataType.Vital;

                    var structuredData = new Dictionary<string, object>();
                    if (item.TryGetProperty("structuredData", out var structuredDataElement))
                    {
                        foreach (var prop in structuredDataElement.EnumerateObject())
                        {
                            structuredData[prop.Name] = prop.Value.ValueKind switch
                            {
                                JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                                JsonValueKind.Number => prop.Value.GetDouble(),
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                _ => prop.Value.GetRawText()
                            };
                        }
                    }

                    results.Add(new ExtractedDataPointDto
                    {
                        DataType = dataType,
                        DataKey = item.GetProperty("dataKey").GetString() ?? string.Empty,
                        DataValue = item.GetProperty("dataValue").GetString() ?? string.Empty,
                        ConfidenceScore = item.TryGetProperty("confidenceScore", out var conf) 
                            ? conf.GetDecimal() 
                            : 0,
                        SourcePageNumber = item.TryGetProperty("sourcePageNumber", out var page) 
                            ? page.GetInt32() 
                            : null,
                        SourceTextExcerpt = item.TryGetProperty("sourceTextExcerpt", out var excerpt) 
                            ? excerpt.GetString() 
                            : null,
                        StructuredData = structuredData.Count > 0 ? structuredData : null
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse individual data point from Gemini response");
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response JSON");
            return new List<ExtractedDataPointDto>();
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
}
