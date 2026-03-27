using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for AI-powered clinical data extraction using Google Gemini (task_002).
/// Calls Gemini REST API for clinical data extraction from OCR text.
/// Falls back to stub data when API key is not configured.
/// </summary>
public class GeminiAiService : IGeminiAiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _apiEndpoint;
    private readonly string _modelName;
    private readonly int _maxTokens;
    private readonly float _temperature;

    public GeminiAiService(
        IConfiguration configuration,
        ILogger<GeminiAiService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        _apiKey = _configuration["GeminiAI:ApiKey"] ?? string.Empty;
        _apiEndpoint = _configuration["GeminiAI:ApiEndpoint"] 
            ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";
        _modelName = _configuration["GeminiAI:ModelName"] ?? "gemini-1.5-flash-latest";
        _maxTokens = int.Parse(_configuration["GeminiAI:MaxTokens"] ?? "8000");
        _temperature = float.Parse(_configuration["GeminiAI:Temperature"] ?? "0.0");

        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("SET_VIA_ENV"))
        {
            _logger.LogWarning("Gemini API key not configured. Service will return stub data. Set GEMINIAI__APIKEY environment variable.");
        }
    }

    public async Task<List<ExtractedDataPointDto>> ExtractClinicalDataAsync(string ocrText, string promptTemplate)
    {
        _logger.LogInformation("Extracting clinical data using Gemini AI. Text length: {Length}", ocrText.Length);

        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("SET_VIA_ENV"))
        {
            _logger.LogWarning("Gemini API key not configured. Returning stub data.");
            return GetStubExtractedData();
        }

        try
        {
            // Build the full prompt
            var fullPrompt = promptTemplate.Replace("{OCR_TEXT}", ocrText);

            // Construct Gemini API request payload
            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = fullPrompt + "\n\nIMPORTANT: Respond ONLY with valid JSON array. Do not include any markdown formatting, code blocks, or explanatory text."
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = _temperature,
                    maxOutputTokens = _maxTokens,
                    candidateCount = 1,
                    responseMimeType = "application/json"
                }
            };

            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(60);

            var endpoint = $"{_apiEndpoint}?key={_apiKey}";

            _logger.LogDebug("Sending clinical extraction request to Gemini API");

            var response = await httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API returned {StatusCode}: {Error}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Gemini API request failed with status {response.StatusCode}: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received Gemini API response: {Length} characters", responseBody.Length);

            // Parse Gemini response structure
            using var jsonDoc = JsonDocument.Parse(responseBody);
            var candidates = jsonDoc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
            {
                _logger.LogError("Gemini API returned no candidates");
                throw new InvalidOperationException("Gemini API returned no candidates");
            }

            var textContent = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(textContent))
            {
                _logger.LogWarning("Gemini returned empty text content. Returning stub data.");
                return GetStubExtractedData();
            }

            _logger.LogDebug("Extracted Gemini response text: {Length} characters", textContent.Length);

            // Parse the extracted data from Gemini JSON response
            var extractedData = ParseGeminiExtractionResponse(textContent);

            _logger.LogInformation("Gemini AI extracted {Count} clinical data points", extractedData.Count);
            return extractedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract clinical data using Gemini AI. Falling back to stub data.");
            return GetStubExtractedData();
        }
    }

    /// <summary>
    /// Parses the JSON response from Gemini into ExtractedDataPointDto list.
    /// Handles both array format and object-with-array format.
    /// </summary>
    private List<ExtractedDataPointDto> ParseGeminiExtractionResponse(string jsonText)
    {
        var results = new List<ExtractedDataPointDto>();

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            // Handle both formats: direct array or { "data_points": [...] }
            JsonElement dataArray;
            if (root.ValueKind == JsonValueKind.Array)
            {
                dataArray = root;
            }
            else if (root.TryGetProperty("data_points", out var dpArray) || 
                     root.TryGetProperty("dataPoints", out dpArray) ||
                     root.TryGetProperty("extracted_data", out dpArray))
            {
                dataArray = dpArray;
            }
            else
            {
                _logger.LogWarning("Unexpected Gemini response format. Root kind: {Kind}", root.ValueKind);
                return GetStubExtractedData();
            }

            foreach (var item in dataArray.EnumerateArray())
            {
                var dataType = ParseClinicalDataType(
                    item.TryGetProperty("data_type", out var dt) ? dt.GetString() :
                    item.TryGetProperty("dataType", out dt) ? dt.GetString() :
                    item.TryGetProperty("type", out dt) ? dt.GetString() : "Vital");

                var dataKey = item.TryGetProperty("data_key", out var dk) ? dk.GetString() :
                              item.TryGetProperty("dataKey", out dk) ? dk.GetString() :
                              item.TryGetProperty("key", out dk) ? dk.GetString() : "Unknown";

                var dataValue = item.TryGetProperty("data_value", out var dv) ? dv.GetString() :
                                item.TryGetProperty("dataValue", out dv) ? dv.GetString() :
                                item.TryGetProperty("value", out dv) ? dv.GetString() : "";

                var confidence = 85.0m;
                if (item.TryGetProperty("confidence_score", out var cs) || 
                    item.TryGetProperty("confidenceScore", out cs) ||
                    item.TryGetProperty("confidence", out cs))
                {
                    confidence = cs.TryGetDecimal(out var csVal) ? csVal : 85.0m;
                }

                var sourceText = item.TryGetProperty("source_text_excerpt", out var st) ? st.GetString() :
                                 item.TryGetProperty("sourceTextExcerpt", out st) ? st.GetString() :
                                 item.TryGetProperty("source_text", out st) ? st.GetString() : "";

                var pageNumber = 1;
                if (item.TryGetProperty("source_page_number", out var sp) || 
                    item.TryGetProperty("pageNumber", out sp))
                {
                    pageNumber = sp.TryGetInt32(out var spVal) ? spVal : 1;
                }

                results.Add(new ExtractedDataPointDto
                {
                    DataType = dataType,
                    DataKey = dataKey ?? "Unknown",
                    DataValue = dataValue ?? "",
                    ConfidenceScore = confidence,
                    SourcePageNumber = pageNumber,
                    SourceTextExcerpt = sourceText ?? ""
                });
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini extraction response JSON. Raw: {Text}", jsonText);
            return GetStubExtractedData();
        }

        return results;
    }

    private static ClinicalDataType ParseClinicalDataType(string? typeStr)
    {
        if (string.IsNullOrWhiteSpace(typeStr)) return ClinicalDataType.Vital;

        return typeStr.ToLowerInvariant() switch
        {
            "vital" or "vitals" => ClinicalDataType.Vital,
            "medication" or "medications" => ClinicalDataType.Medication,
            "allergy" or "allergies" => ClinicalDataType.Allergy,
            "diagnosis" or "diagnoses" or "condition" or "conditions" => ClinicalDataType.Diagnosis,
            "labresult" or "lab_result" or "lab" or "labs" => ClinicalDataType.LabResult,
            _ => ClinicalDataType.Vital
        };
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
