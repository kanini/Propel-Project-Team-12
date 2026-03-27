using System.Text.Json;
using Azure;
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
using Polly.Retry;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for medical code mapping using RAG pattern with Azure OpenAI GPT-4o.
/// Maps extracted clinical data to ICD-10 diagnosis codes or CPT procedure codes
/// with confidence scores and LLM-generated rationale (AIR-003, AIR-004, AIR-Q01, AIR-Q03).
/// </summary>
public class CodeMappingService : ICodeMappingService
{
    private readonly ILogger<CodeMappingService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly IHybridRetrievalService _hybridRetrievalService;
    private readonly OpenAIClient _openAIClient;
    private readonly CodeMappingSettings _settings;
    private readonly IValidator<CodeMappingResponseDto> _validator;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    // JSON deserialization model for LLM response
    private class LLMCodeMappingResponse
    {
        public List<LLMCodeSuggestion> Suggestions { get; set; } = new();
        public string? Message { get; set; }
    }

    private class LLMCodeSuggestion
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Confidence_Score { get; set; }
        public string Rationale { get; set; } = string.Empty;
    }

    public CodeMappingService(
        ILogger<CodeMappingService> logger,
        PatientAccessDbContext context,
        IHybridRetrievalService hybridRetrievalService,
        OpenAIClient openAIClient,
        IOptions<CodeMappingSettings> settings,
        IValidator<CodeMappingResponseDto> validator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _hybridRetrievalService = hybridRetrievalService ?? throw new ArgumentNullException(nameof(hybridRetrievalService));
        _openAIClient = openAIClient ?? throw new ArgumentNullException(nameof(openAIClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));

        // Configure Polly retry policy: 3 retries with exponential backoff (2s, 4s, 8s)
        _retryPolicy = Policy
            .Handle<RequestFailedException>()
            .WaitAndRetryAsync(
                retryCount: _settings.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Azure OpenAI retry {RetryCount} after {Delay}s: {Message}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });

        // Configure Polly circuit breaker: 5 failures trigger 1-minute break
        _circuitBreakerPolicy = Policy
            .Handle<RequestFailedException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: _settings.CircuitBreakerThreshold,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Circuit breaker opened for {Duration}s: {Message}",
                        duration.TotalSeconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset - code mapping service operational");
                });
    }

    /// <inheritdoc />
    public async Task<CodeMappingResponseDto> MapToICD10Async(
        CodeMappingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mapping clinical text to ICD-10 codes: ExtractedDataId={Id}, TextLength={Length}",
            request.ExtractedClinicalDataId, request.ClinicalText.Length);

        // Load prompt template
        var promptTemplatePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".propel", "prompts", "ai", "code-mapping-icd10.txt");
        if (!File.Exists(promptTemplatePath))
        {
            _logger.LogError("ICD-10 prompt template not found at: {Path}", promptTemplatePath);
            throw new FileNotFoundException($"ICD-10 prompt template not found at: {promptTemplatePath}");
        }

        var promptTemplate = await File.ReadAllTextAsync(promptTemplatePath, cancellationToken);

        return await MapToCodes(request, "ICD10", promptTemplate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CodeMappingResponseDto> MapToCPTAsync(
        CodeMappingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mapping clinical text to CPT codes: ExtractedDataId={Id}, TextLength={Length}",
            request.ExtractedClinicalDataId, request.ClinicalText.Length);

        // Load prompt template
        var promptTemplatePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".propel", "prompts", "ai", "code-mapping-cpt.txt");
        if (!File.Exists(promptTemplatePath))
        {
            _logger.LogError("CPT prompt template not found at: {Path}", promptTemplatePath);
            throw new FileNotFoundException($"CPT prompt template not found at: {promptTemplatePath}");
        }

        var promptTemplate = await File.ReadAllTextAsync(promptTemplatePath, cancellationToken);

        return await MapToCodes(request, "CPT", promptTemplate, cancellationToken);
    }

    /// <summary>
    /// Core code mapping logic using RAG retrieval and GPT-4o inference.
    /// </summary>
    private async Task<CodeMappingResponseDto> MapToCodes(
        CodeMappingRequestDto request,
        string codeSystem,
        string promptTemplate,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        // 1. Retrieve relevant codes from knowledge base using hybrid retrieval (RAG)
        _logger.LogInformation("Retrieving {CodeSystem} codes from knowledge base", codeSystem);
        var retrievalRequest = new CodeSearchRequestDto
        {
            Query = request.ClinicalText,
            CodeSystem = codeSystem,
            TopK = 5, // AIR-R02: top-5 retrieval
            MinSimilarityThreshold = 0.75 // AIR-R02: cosine similarity > 0.75
        };

        var retrievalResponse = await _hybridRetrievalService.SearchAsync(retrievalRequest, cancellationToken);

        // Build retrieved context for prompt
        var retrievedContext = string.Join("\n\n", retrievalResponse.Results.Select(r =>
            $"Code: {r.Code}\nDescription: {r.Description}\nCategory: {r.Category ?? "N/A"}\nSimilarity: {r.SimilarityScore:F2}"));

        if (string.IsNullOrEmpty(retrievedContext))
        {
            _logger.LogWarning("No relevant codes retrieved for query: {Query}", request.ClinicalText);
            retrievedContext = "No relevant codes found in knowledge base.";
        }

        // 2. Construct prompt with retrieved context and clinical text
        var userPrompt = promptTemplate
            .Replace("{retrieved_context}", retrievedContext)
            .Replace("{clinical_text}", request.ClinicalText);

        // 3. Invoke Azure OpenAI GPT-4o for code mapping
        _logger.LogInformation("Invoking Azure OpenAI GPT-4o for {CodeSystem} mapping", codeSystem);

        var chatMessages = new ChatRequestMessage[]
        {
            new ChatRequestSystemMessage($"You are a medical coding expert specializing in {codeSystem} code assignment."),
            new ChatRequestUserMessage(userPrompt)
        };

        var chatOptions = new ChatCompletionsOptions
        {
            DeploymentName = _settings.Gpt4oDeploymentName,
            Messages = { chatMessages[0], chatMessages[1] },
            Temperature = _settings.Temperature, // 0.0 for deterministic medical coding
            MaxTokens = _settings.MaxTokens,
            ResponseFormat = ChatCompletionsResponseFormat.JsonObject // Force JSON output (GPT-4o feature)
        };

        ChatCompletions response;
        try
        {
            response = await _circuitBreakerPolicy.ExecuteAsync(() =>
                _retryPolicy.ExecuteAsync(() =>
                    _openAIClient.GetChatCompletionsAsync(chatOptions, cancellationToken)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI GPT-4o invocation failed for {CodeSystem} mapping", codeSystem);
            throw;
        }

        var llmOutput = response.Choices[0].Message.Content;
        _logger.LogDebug("GPT-4o response: {Output}", llmOutput);

        // 4. Parse JSON response from LLM
        LLMCodeMappingResponse? parsedResponse;
        try
        {
            parsedResponse = JsonSerializer.Deserialize<LLMCodeMappingResponse>(llmOutput, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse GPT-4o JSON response: {Output}", llmOutput);
            await TrackSchemaValidityAsync(false, cancellationToken);
            throw new InvalidOperationException("LLM returned invalid JSON format", ex);
        }

        if (parsedResponse == null)
        {
            _logger.LogError("GPT-4o returned null response");
            await TrackSchemaValidityAsync(false, cancellationToken);
            throw new InvalidOperationException("LLM returned null response");
        }

        // 5. Build response DTO
        var responseDto = new CodeMappingResponseDto
        {
            ExtractedClinicalDataId = request.ExtractedClinicalDataId,
            CodeSystem = codeSystem,
            Message = parsedResponse.Message,
            Suggestions = parsedResponse.Suggestions.Select((s, index) => new MedicalCodeSuggestionDto
            {
                Code = s.Code,
                Description = s.Description,
                ConfidenceScore = s.Confidence_Score,
                Rationale = s.Rationale,
                Rank = index + 1,
                IsTopSuggestion = index == 0
            }).ToList(),
            SuggestionCount = parsedResponse.Suggestions.Count
        };

        // Detect ambiguity: top 2 suggestions with confidence difference < threshold
        if (responseDto.SuggestionCount >= 2)
        {
            var confidenceDiff = responseDto.Suggestions[0].ConfidenceScore - responseDto.Suggestions[1].ConfidenceScore;
            responseDto.IsAmbiguous = confidenceDiff < _settings.AmbiguityThreshold;
        }

        // 6. Validate output schema (AIR-Q03: >99% schema validity)
        var isValid = await ValidateOutputSchemaAsync(responseDto);
        if (!isValid)
        {
            _logger.LogWarning("Invalid schema for ExtractedDataId: {Id}", request.ExtractedClinicalDataId);
            await TrackSchemaValidityAsync(false, cancellationToken);
            // Continue processing - log but don't fail
        }
        else
        {
            await TrackSchemaValidityAsync(true, cancellationToken);
        }

        // 7. Persist to database
        foreach (var suggestion in responseDto.Suggestions)
        {
            var medicalCode = new MedicalCode
            {
                MedicalCodeId = Guid.NewGuid(),
                ExtractedDataId = request.ExtractedClinicalDataId,
                CodeSystem = codeSystem == "ICD10" ? Data.Models.CodeSystem.ICD10 : Data.Models.CodeSystem.CPT,
                CodeValue = suggestion.Code,
                CodeDescription = suggestion.Description,
                ConfidenceScore = suggestion.ConfidenceScore,
                Rationale = suggestion.Rationale,
                Rank = suggestion.Rank,
                IsTopSuggestion = suggestion.IsTopSuggestion,
                RetrievedContext = retrievedContext.Length > 5000 ? retrievedContext[..5000] : retrievedContext,
                VerificationStatus = MedicalCodeVerificationStatus.AISuggested,
                CreatedAt = DateTime.UtcNow
            };

            await _context.MedicalCodes.AddAsync(medicalCode, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogInformation("Code mapping completed: CodeSystem={CodeSystem}, SuggestionCount={Count}, Duration={Duration}ms",
            codeSystem, responseDto.SuggestionCount, duration);

        return responseDto;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateOutputSchemaAsync(CodeMappingResponseDto response)
    {
        var validationResult = await _validator.ValidateAsync(response);
        return validationResult.IsValid;
    }

    /// <inheritdoc />
    public async Task<decimal> CalculateAgreementRateAsync(DateTime periodStart, DateTime periodEnd)
    {
        _logger.LogInformation("Calculating AI-Human Agreement Rate: Period={Start} to {End}",
            periodStart, periodEnd);

        // Get all verified codes in the period
        var verifiedCodes = await _context.MedicalCodes
            .Where(mc => mc.VerifiedAt >= periodStart && mc.VerifiedAt <= periodEnd &&
                        (mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted ||
                         mc.VerificationStatus == MedicalCodeVerificationStatus.Rejected))
            .ToListAsync();

        if (verifiedCodes.Count == 0)
        {
            _logger.LogWarning("No verified codes found in period - cannot calculate agreement rate");
            return 0;
        }

        // Agreement = top suggestion accepted by staff
        var agreementCount = verifiedCodes.Count(mc =>
            mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted && mc.IsTopSuggestion);

        var agreementRate = (decimal)agreementCount / verifiedCodes.Count * 100;

        _logger.LogInformation("Agreement rate: {Rate}% ({Agreed}/{Total})",
            agreementRate.ToString("F2"), agreementCount, verifiedCodes.Count);

        // Track metric
        var metric = new QualityMetric
        {
            QualityMetricId = Guid.NewGuid(),
            MetricType = "AIHumanAgreement",
            MetricValue = agreementRate,
            SampleSize = verifiedCodes.Count,
            MeasurementPeriod = "Custom",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 98.0m, // AIR-Q01: >98% target
            Status = agreementRate >= 98.0m ? "MeetsTarget" : "BelowTarget",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QualityMetrics.AddAsync(metric);
        await _context.SaveChangesAsync();

        return agreementRate;
    }

    /// <summary>
    /// Tracks schema validity rate for AIR-Q03 (>99% output schema validity).
    /// </summary>
    private async Task TrackSchemaValidityAsync(bool isValid, CancellationToken cancellationToken)
    {
        // Get today's schema validity metrics
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "SchemaValidity" &&
                        qm.PeriodStart >= today &&
                        qm.PeriodStart < tomorrow)
            .FirstOrDefaultAsync(cancellationToken);

        if (todayMetric == null)
        {
            // Create new metric for today
            todayMetric = new QualityMetric
            {
                QualityMetricId = Guid.NewGuid(),
                MetricType = "SchemaValidity",
                MetricValue = isValid ? 100m : 0m,
                SampleSize = 1,
                MeasurementPeriod = "Daily",
                PeriodStart = today,
                PeriodEnd = tomorrow,
                Target = 99.0m, // AIR-Q03: >99% target
                Status = isValid ? "MeetsTarget" : "BelowTarget",
                CreatedAt = DateTime.UtcNow
            };

            await _context.QualityMetrics.AddAsync(todayMetric, cancellationToken);
        }
        else
        {
            // Update existing metric
            var totalSamples = todayMetric.SampleSize + 1;
            var totalValid = (todayMetric.MetricValue * todayMetric.SampleSize / 100) + (isValid ? 1 : 0);
            todayMetric.MetricValue = totalValid / totalSamples * 100;
            todayMetric.SampleSize = totalSamples;
            todayMetric.Status = todayMetric.MetricValue >= 99.0m ? "MeetsTarget" : "BelowTarget";
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
