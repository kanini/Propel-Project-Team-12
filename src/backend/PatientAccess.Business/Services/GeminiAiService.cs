using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

public class GeminiAiService : IGeminiAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiAiService> _logger;
    private readonly string _apiKey;
    private readonly string _apiEndpoint;
    private readonly int _maxTokens;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GeminiAiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeminiAiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiKey = configuration["GeminiAI:ApiKey"] ?? string.Empty;
        _apiEndpoint = configuration["GeminiAI:ApiEndpoint"]
            ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent";
        _maxTokens = int.Parse(configuration["GeminiAI:MaxTokens"] ?? "8000");
    }

    public async Task<GeminiExtractionResponseDto> ExtractClinicalDataWithCodesAsync(string contextChunks, string systemPrompt)
    {
        _logger.LogInformation("Calling Gemini LLM for clinical extraction. Context length: {Length}", contextChunks.Length);

        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("SET_VIA_ENV", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Gemini API key not configured. Returning stub data.");
            return GetStubResponse();
        }   

        var client = _httpClientFactory.CreateClient();
        var url = $"{_apiEndpoint}?key={_apiKey}";

        var fullPrompt = $"{systemPrompt}\n\n--- DOCUMENT TEXT ---\n{contextChunks}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = fullPrompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                maxOutputTokens = _maxTokens,
                temperature = 0.1 // Low temperature for factual extraction
            }
        };

        // Retry up to 3 times with exponential backoff
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var response = await client.PostAsJsonAsync(url, payload);

                if ((int)response.StatusCode == 429)
                {
                    var delay = attempt * 2000;
                    _logger.LogWarning("Gemini rate limited. Retrying in {Delay}ms (attempt {Attempt}/3)", delay, attempt);
                    await Task.Delay(delay);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                var textContent = json
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "{}";

                _logger.LogInformation("Gemini response received. Parsing JSON.");

                return ParseGeminiResponse(textContent);
            }
            catch (HttpRequestException ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "Gemini API call failed (attempt {Attempt}/3). Retrying.", attempt);
                await Task.Delay(attempt * 1000);
            }
        }

        _logger.LogError("All Gemini API attempts failed. Returning empty result.");
        return new GeminiExtractionResponseDto();
    }

    private GeminiExtractionResponseDto ParseGeminiResponse(string jsonText)
    {
        try
        {
            // Gemini returns JSON with clinical_data and medical_codes arrays
            var doc = JsonDocument.Parse(jsonText);
            var result = new GeminiExtractionResponseDto();

            if (doc.RootElement.TryGetProperty("clinical_data", out var clinicalArr))
            {
                foreach (var item in clinicalArr.EnumerateArray())
                {
                    var dataType = MapDataType(item.GetProperty("data_type").GetString() ?? "");
                    result.DataPoints.Add(new ExtractedDataPointDto
                    {
                        DataType = dataType,
                        DataKey = item.GetProperty("data_key").GetString() ?? "",
                        DataValue = item.GetProperty("data_value").GetString() ?? "",
                        ConfidenceScore = item.TryGetProperty("confidence", out var conf) ? conf.GetDecimal() : 70m,
                        SourceTextExcerpt = item.TryGetProperty("source_excerpt", out var src) ? src.GetString() : null,
                        StructuredData = item.TryGetProperty("structured_data", out var sd)
                            ? JsonSerializer.Deserialize<Dictionary<string, object>>(sd.GetRawText(), JsonOptions)
                            : null
                    });
                }
            }

            if (doc.RootElement.TryGetProperty("medical_codes", out var codesArr))
            {
                foreach (var code in codesArr.EnumerateArray())
                {
                    result.MedicalCodes.Add(new MedicalCodeSuggestionDto
                    {
                        CodeSystem = code.GetProperty("code_system").GetString() ?? "ICD10",
                        CodeValue = code.GetProperty("code_value").GetString() ?? "",
                        CodeDescription = code.GetProperty("description").GetString() ?? "",
                        ConfidenceScore = code.TryGetProperty("confidence", out var conf) ? conf.GetDecimal() : 70m,
                        SourceDataKey = code.TryGetProperty("source_data_key", out var sdk) ? sdk.GetString() ?? "" : ""
                    });
                }
            }

            _logger.LogInformation("Parsed {DataPoints} clinical data points and {Codes} medical codes",
                result.DataPoints.Count, result.MedicalCodes.Count);

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini JSON response: {Response}", jsonText[..Math.Min(500, jsonText.Length)]);
            return new GeminiExtractionResponseDto();
        }
    }

    private static ClinicalDataType MapDataType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "vital" or "vitals" => ClinicalDataType.Vital,
            "medication" or "medications" => ClinicalDataType.Medication,
            "allergy" or "allergies" => ClinicalDataType.Allergy,
            "diagnosis" or "condition" or "conditions" => ClinicalDataType.Diagnosis,
            "labresult" or "lab_result" or "lab" => ClinicalDataType.LabResult,
            _ => ClinicalDataType.Diagnosis
        };
    }

    private static GeminiExtractionResponseDto GetStubResponse()
    {
        return new GeminiExtractionResponseDto
        {
            DataPoints = new List<ExtractedDataPointDto>
            {
                new() { DataType = ClinicalDataType.Vital, DataKey = "BloodPressure", DataValue = "120/80 mmHg", ConfidenceScore = 95.5m, SourceTextExcerpt = "Blood Pressure: 120/80 mmHg" },
                new() { DataType = ClinicalDataType.Medication, DataKey = "CurrentMedication", DataValue = "Lisinopril 10mg daily", ConfidenceScore = 88.0m, SourceTextExcerpt = "Current Medications: Lisinopril 10mg daily" },
                new() { DataType = ClinicalDataType.Allergy, DataKey = "DrugAllergy", DataValue = "Penicillin - Hives", ConfidenceScore = 92.0m, SourceTextExcerpt = "Allergies: Penicillin (Hives)" },
                new() { DataType = ClinicalDataType.Diagnosis, DataKey = "Condition", DataValue = "Hypertension", ConfidenceScore = 94.0m, SourceTextExcerpt = "Diagnosis: Essential Hypertension" }
            },
            MedicalCodes = new List<MedicalCodeSuggestionDto>
            {
                new() { CodeSystem = "ICD10", CodeValue = "I10", CodeDescription = "Essential (primary) hypertension", ConfidenceScore = 92m, SourceDataKey = "Condition" },
                new() { CodeSystem = "CPT", CodeValue = "99213", CodeDescription = "Office/outpatient visit, estab patient, 20-29 min", ConfidenceScore = 85m, SourceDataKey = "Encounter" }
            }
        };
    }
}
