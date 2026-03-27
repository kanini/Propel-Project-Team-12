using System.Text;
using System.Text.Json;
using Azure.AI.OpenAI;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PatientAccess.Business.Configuration;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using Polly;
using Polly.CircuitBreaker;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for mapping extracted clinical text to ICD-10/CPT codes using RAG pattern with Azure OpenAI GPT-4o.
/// US_051 Task 1 - Code Mapping Service.
/// Implements FR-034 (ICD-10 suggestions), FR-035 (CPT suggestions), AIR-003/AIR-004 (RAG-based mapping),
/// AIR-Q01 (AI-Human Agreement >98%), AIR-Q03 (Schema Validity >99%).
/// </summary>
public class CodeMappingService : ICodeMappingService
{
    private readonly ILogger<CodeMappingService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly IHybridRetrievalService _retrievalService;
    private readonly OpenAIClient _openAIClient;
    private readonly IOptions<CodeMappingSettings> _settings;
    private readonly IOptions<GeminiAISettings> _geminiSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IValidator<CodeMappingResponseDto> _validator;
    private readonly AsyncPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public CodeMappingService(
        ILogger<CodeMappingService> logger,
        PatientAccessDbContext context,
        IHybridRetrievalService retrievalService,
        OpenAIClient openAIClient,
        IOptions<CodeMappingSettings> settings,
        IOptions<GeminiAISettings> geminiSettings,
        IHttpClientFactory httpClientFactory,
        IValidator<CodeMappingResponseDto> validator)
    {
        _logger = logger;
        _context = context;
        _retrievalService = retrievalService;
        _openAIClient = openAIClient;
        _settings = settings;
        _geminiSettings = geminiSettings;
        _httpClientFactory = httpClientFactory;
        _validator = validator;

        // Polly Retry Policy: 3 retries with exponential backoff (2s, 4s, 8s)
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, attempt, context) =>
                {
                    _logger.LogWarning(exception,
                        "Retry {Attempt} for LLM API call after {Delay}s: {Message}",
                        attempt, timeSpan.TotalSeconds, exception.Message);
                });

        // Polly Circuit Breaker Policy: Opens after 5 consecutive failures, 1-minute break
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError(exception,
                        "Circuit breaker opened for {Duration}s due to: {Message}",
                        duration.TotalSeconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset - allowing requests");
                });
    }

    /// <summary>
    /// Maps clinical text to ICD-10 diagnosis codes using RAG retrieval and GPT-4o inference.
    /// </summary>
    public async Task<CodeMappingResponseDto> MapToICD10Async(
        CodeMappingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Mapping clinical text to ICD-10 codes for ExtractedDataId: {Id}",
            request.ExtractedClinicalDataId);

        return await MapToCodesAsync(request, "ICD10", ".propel/prompts/ai/code-mapping-icd10.txt", cancellationToken);
    }

    /// <summary>
    /// Maps clinical text to CPT procedure codes using RAG retrieval and GPT-4o inference.
    /// </summary>
    public async Task<CodeMappingResponseDto> MapToCPTAsync(
        CodeMappingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Mapping clinical text to CPT codes for ExtractedDataId: {Id}",
            request.ExtractedClinicalDataId);

        return await MapToCodesAsync(request, "CPT", ".propel/prompts/ai/code-mapping-cpt.txt", cancellationToken);
    }

    /// <summary>
    /// Core method for code mapping logic (shared by ICD-10 and CPT).
    /// </summary>
    private async Task<CodeMappingResponseDto> MapToCodesAsync(
        CodeMappingRequestDto request,
        string codeSystem,
        string promptTemplatePath,
        CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Retrieve relevant codes from knowledge base using hybrid retrieval (RAG)
            var retrievalRequest = new CodeSearchRequestDto
            {
                Query = request.ClinicalText,
                CodeSystem = codeSystem,
                TopK = 5, // Retrieve top-5 per AIR-R02
                MinSimilarityThreshold = 0.75 // Cosine similarity >0.75 per AIR-R02
            };

            var retrievalResponse = await _retrievalService.SearchAsync(retrievalRequest, cancellationToken);

            // Build retrieved context string for prompt
            var retrievedContext = string.Join("\n\n",
                retrievalResponse.Results.Select(r =>
                    $"Code: {r.Code}\n" +
                    $"Description: {r.Description}\n" +
                    $"Category: {r.Category}\n" +
                    $"Similarity: {r.SimilarityScore:F2}"));

            _logger.LogDebug(
                "Retrieved {Count} codes from knowledge base for query: '{Query}'",
                retrievalResponse.ResultCount, request.ClinicalText);

            // Step 2: Load prompt template
            var fullPromptPath = Path.IsPathRooted(promptTemplatePath)
                ? promptTemplatePath
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", promptTemplatePath);

            // Fallback: try relative to current directory if combined path doesn't exist
            if (!File.Exists(fullPromptPath))
                fullPromptPath = promptTemplatePath;

            var promptTemplate = await File.ReadAllTextAsync(fullPromptPath, cancellationToken);
            var prompt = promptTemplate
                .Replace("{retrieved_context}", retrievedContext)
                .Replace("{clinical_text}", request.ClinicalText);

            // Step 3: Invoke LLM (Gemini or Azure OpenAI) with Polly resilience
            _logger.LogInformation(
                "Using {Provider} for code mapping (UseGemini={UseGemini})",
                _settings.Value.UseGemini ? "Google Gemini" : "Azure OpenAI",
                _settings.Value.UseGemini);

            var llmOutput = await _circuitBreakerPolicy.ExecuteAsync(() =>
                _retryPolicy.ExecuteAsync(() =>
                    _settings.Value.UseGemini
                        ? InvokeGeminiAsync(prompt, cancellationToken)
                        : InvokeAzureOpenAIAsync(prompt, cancellationToken)));

            // Step 4: Parse JSON response
            LLMCodeMappingResponse? parsedResponse;
            try
            {
                parsedResponse = JsonSerializer.Deserialize<LLMCodeMappingResponse>(llmOutput);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to parse LLM JSON response for ExtractedDataId: {Id}. Raw output: {Output}",
                    request.ExtractedClinicalDataId, llmOutput);
                throw new InvalidOperationException("LLM returned invalid JSON format", ex);
            }

            // Step 5: Build response DTO
            var responseDto = new CodeMappingResponseDto
            {
                ExtractedClinicalDataId = request.ExtractedClinicalDataId,
                CodeSystem = codeSystem,
                Suggestions = parsedResponse?.Suggestions?
                    .Take(request.MaxSuggestions)
                    .Select((s, index) => new MedicalCodeSuggestionDto
                    {
                        Code = s.Code ?? string.Empty,
                        Description = s.Description ?? string.Empty,
                        ConfidenceScore = s.ConfidenceScore,
                        Rationale = s.Rationale ?? string.Empty,
                        Rank = index + 1,
                        IsTopSuggestion = index == 0
                    }).ToList() ?? new(),
                Message = parsedResponse?.Message
            };

            responseDto.SuggestionCount = responseDto.Suggestions.Count;

            // Check for ambiguity (confidence difference <AmbiguityThreshold)
            if (responseDto.SuggestionCount > 1)
            {
                var topConfidence = responseDto.Suggestions[0].ConfidenceScore;
                var secondConfidence = responseDto.Suggestions[1].ConfidenceScore;
                responseDto.IsAmbiguous = Math.Abs(topConfidence - secondConfidence) < _settings.Value.AmbiguityThreshold;

                if (responseDto.IsAmbiguous)
                {
                    _logger.LogInformation(
                        "Ambiguous mapping detected: Top confidence = {Top}%, Second = {Second}%",
                        topConfidence, secondConfidence);
                }
            }

            // Step 6: Validate output schema (AIR-Q03: >99% validity)
            var isValid = await ValidateOutputSchemaAsync(responseDto);
            await TrackSchemaValidityAsync(isValid, cancellationToken);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Invalid schema for ExtractedDataId: {Id}. ValidationErrors: {Errors}",
                    request.ExtractedClinicalDataId,
                    string.Join(", ", (await _validator.ValidateAsync(responseDto)).Errors.Select(e => e.ErrorMessage)));
            }

            // Step 7: Persist to database
            await PersistMedicalCodesAsync(request.ExtractedClinicalDataId, codeSystem, responseDto, retrievedContext, cancellationToken);

            _logger.LogInformation(
                "Mapped {Count} {CodeSystem} codes for ExtractedDataId: {Id}",
                responseDto.SuggestionCount, codeSystem, request.ExtractedClinicalDataId);

            return responseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error mapping clinical text to {CodeSystem} codes for ExtractedDataId: {Id}",
                codeSystem, request.ExtractedClinicalDataId);
            throw;
        }
    }

    /// <summary>
    /// Invokes Azure OpenAI GPT-4o for code mapping inference.
    /// </summary>
    private async Task<string> InvokeAzureOpenAIAsync(string prompt, CancellationToken cancellationToken)
    {
        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            DeploymentName = _settings.Value.Gpt4oDeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage("You are a medical coding expert."),
                new ChatRequestUserMessage(prompt)
            },
            Temperature = _settings.Value.Temperature, // 0.0 for deterministic
            MaxTokens = _settings.Value.MaxTokens, // 1000 tokens
            ResponseFormat = ChatCompletionsResponseFormat.JsonObject // Force JSON output
        };

        _logger.LogDebug("Invoking Azure OpenAI GPT-4o with deployment: {DeploymentName}",
            _settings.Value.Gpt4oDeploymentName);

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        var llmOutput = response.Value.Choices[0].Message.Content;

        _logger.LogDebug("Received LLM response: {Length} characters", llmOutput?.Length ?? 0);

        return llmOutput ?? string.Empty;
    }

    /// <summary>
    /// Invokes Google Gemini AI for code mapping inference.
    /// Uses REST API with JSON response format (equivalent to GPT-4o JSON mode).
    /// </summary>
    private async Task<string> InvokeGeminiAsync(string prompt, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invoking Gemini AI with model: {ModelName}", _geminiSettings.Value.ModelName);

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(_geminiSettings.Value.TimeoutSeconds);

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
                            text = $"You are a medical coding expert. {prompt}\n\nIMPORTANT: Respond ONLY with valid JSON. Do not include any markdown formatting, code blocks, or explanatory text."
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = _geminiSettings.Value.Temperature, // 0.0 for deterministic
                maxOutputTokens = _geminiSettings.Value.MaxTokens, // 8000 tokens
                candidateCount = 1,
                responseMimeType = "application/json" // Force JSON response
            }
        };

        var jsonPayload = JsonSerializer.Serialize(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Build endpoint URL with API key
        var endpoint = $"{_geminiSettings.Value.ApiEndpoint}?key={_geminiSettings.Value.ApiKey}";

        _logger.LogDebug("Sending request to Gemini API: {Endpoint}", _geminiSettings.Value.ApiEndpoint);

        var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogDebug("Received Gemini API response: {Length} characters", responseBody.Length);

        // Parse Gemini response structure
        // Expected format: { "candidates": [ { "content": { "parts": [ { "text": "..." } ] } } ] }
        using var jsonDoc = JsonDocument.Parse(responseBody);
        var candidates = jsonDoc.RootElement.GetProperty("candidates");
        if (candidates.GetArrayLength() == 0)
        {
            _logger.LogError("Gemini API returned no candidates. Response: {Response}", responseBody);
            throw new InvalidOperationException("Gemini API returned no candidates");
        }

        var firstCandidate = candidates[0];
        var contentProperty = firstCandidate.GetProperty("content");
        var parts = contentProperty.GetProperty("parts");
        if (parts.GetArrayLength() == 0)
        {
            _logger.LogError("Gemini API response has no parts. Response: {Response}", responseBody);
            throw new InvalidOperationException("Gemini API response has no content parts");
        }

        var textContent = parts[0].GetProperty("text").GetString();

        _logger.LogDebug("Extracted text from Gemini response: {Length} characters", textContent?.Length ?? 0);

        return textContent ?? string.Empty;
    }

    /// <summary>
    /// Persists medical code suggestions to database.
    /// </summary>
    private async Task PersistMedicalCodesAsync(
        Guid extractedClinicalDataId,
        string codeSystem,
        CodeMappingResponseDto response,
        string retrievedContext,
        CancellationToken cancellationToken)
    {
        var codeSystemEnum = codeSystem == "ICD10" ? CodeSystem.ICD10 : CodeSystem.CPT;

        foreach (var suggestion in response.Suggestions)
        {
            var medicalCode = new MedicalCode
            {
                MedicalCodeId = Guid.NewGuid(),
                ExtractedDataId = extractedClinicalDataId,
                CodeSystem = codeSystemEnum,
                CodeValue = suggestion.Code,
                CodeDescription = suggestion.Description,
                ConfidenceScore = suggestion.ConfidenceScore,
                Rationale = suggestion.Rationale,
                Rank = suggestion.Rank,
                IsTopSuggestion = suggestion.IsTopSuggestion,
                RetrievedContext = retrievedContext,
                VerificationStatus = MedicalCodeVerificationStatus.AISuggested,
                CreatedAt = DateTime.UtcNow
            };

            await _context.MedicalCodes.AddAsync(medicalCode, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Validates CodeMappingResponseDto output schema per AIR-Q03.
    /// </summary>
    public async Task<bool> ValidateOutputSchemaAsync(CodeMappingResponseDto response)
    {
        var validationResult = await _validator.ValidateAsync(response);
        return validationResult.IsValid;
    }

    /// <summary>
    /// Tracks schema validity metric for quality reporting (AIR-Q03: >99% target).
    /// </summary>
    private async Task TrackSchemaValidityAsync(bool isValid, CancellationToken cancellationToken)
    {
        // Simplified implementation: Track individual validation result
        // In production, aggregate results over a period (e.g., daily batch)
        var metric = new QualityMetric
        {
            Id = Guid.NewGuid(),
            MetricType = "SchemaValidity",
            MetricValue = isValid ? 100.0m : 0.0m,
            SampleSize = 1,
            MeasurementPeriod = "Individual",
            PeriodStart = DateTime.UtcNow,
            PeriodEnd = DateTime.UtcNow,
            Target = 99.0m,
            Status = isValid ? "MeetsTarget" : "BelowTarget",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QualityMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Calculates AI-Human Agreement Rate for a given time period (AIR-Q01: >98% target).
    /// </summary>
    public async Task<decimal> CalculateAgreementRateAsync(DateTime periodStart, DateTime periodEnd)
    {
        var verifiedCodes = await _context.MedicalCodes
            .Where(mc => mc.VerifiedAt >= periodStart && mc.VerifiedAt <= periodEnd &&
                        (mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted ||
                         mc.VerificationStatus == MedicalCodeVerificationStatus.Rejected))
            .ToListAsync();

        if (verifiedCodes.Count == 0)
        {
            _logger.LogWarning("No verified codes found for period {Start} to {End}",
                periodStart, periodEnd);
            return 0;
        }

        // Agreement = (AcceptedTopSuggestions / TotalVerifiedTopSuggestions) * 100
        var verifiedTopSuggestions = verifiedCodes.Where(mc => mc.IsTopSuggestion).ToList();
        if (verifiedTopSuggestions.Count == 0) return 0;

        var agreementCount = verifiedTopSuggestions.Count(mc =>
            mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted);
        var agreementRate = (decimal)agreementCount / verifiedTopSuggestions.Count * 100;

        // Track metric
        var metric = new QualityMetric
        {
            Id = Guid.NewGuid(),
            MetricType = "AIHumanAgreement",
            MetricValue = agreementRate,
            SampleSize = verifiedTopSuggestions.Count,
            MeasurementPeriod = "Custom",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 98.0m,
            Status = agreementRate >= 98.0m ? "MeetsTarget" : "BelowTarget",
            Notes = $"Accepted: {agreementCount}/{verifiedTopSuggestions.Count}",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QualityMetrics.AddAsync(metric);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Calculated AI-Human Agreement Rate: {Rate}% ({Agreement}/{Total})",
            agreementRate, agreementCount, verifiedTopSuggestions.Count);

        return agreementRate;
    }

    /// <summary>
    /// Internal DTO for deserializing LLM JSON response.
    /// </summary>
    private class LLMCodeMappingResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("suggestions")]
        public List<LLMCodeSuggestion> Suggestions { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    private class LLMCodeSuggestion
    {
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public string? Code { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string? Description { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("confidence_score")]
        public decimal ConfidenceScore { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("rationale")]
        public string? Rationale { get; set; }
    }
}
