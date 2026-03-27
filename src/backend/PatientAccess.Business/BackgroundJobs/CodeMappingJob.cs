using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for medical code mapping (US_051).
/// Automatically maps extracted clinical data to ICD-10 and CPT codes using RAG pipeline.
/// Triggered after clinical data extraction completes (US_043/US_045).
/// </summary>
public class CodeMappingJob
{
    private readonly ILogger<CodeMappingJob> _logger;
    private readonly ICodeMappingService _codeMappingService;

    public CodeMappingJob(
        ILogger<CodeMappingJob> logger,
        ICodeMappingService codeMappingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _codeMappingService = codeMappingService ?? throw new ArgumentNullException(nameof(codeMappingService));
    }

    /// <summary>
    /// Executes ICD-10 code mapping for extracted diagnosis data.
    /// Uses RAG retrieval + LLM (Gemini/GPT-4o) for intelligent code suggestions.
    /// </summary>
    /// <param name="extractedDataId">ExtractedClinicalData unique identifier</param>
    /// <param name="clinicalText">Clinical text to map (diagnosis description)</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })] // 1min, 5min, 15min
    [Queue("code-mapping")] // Dedicated queue for code mapping jobs
    public async Task MapToICD10Async(Guid extractedDataId, string clinicalText)
    {
        _logger.LogInformation("Starting ICD-10 code mapping for ExtractedDataId: {ExtractedDataId}", extractedDataId);

        try
        {
            var request = new CodeMappingRequestDto
            {
                ExtractedClinicalDataId = extractedDataId,
                ClinicalText = clinicalText,
                MaxSuggestions = 5 // FR-034: Return up to 5 ICD-10 suggestions
            };

            var response = await _codeMappingService.MapToICD10Async(request, CancellationToken.None);

            _logger.LogInformation(
                "ICD-10 code mapping completed for ExtractedDataId: {ExtractedDataId}. Mapped {Count} codes, IsAmbiguous: {IsAmbiguous}",
                extractedDataId, response.SuggestionCount, response.IsAmbiguous);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "ICD-10 code mapping failed for ExtractedDataId: {ExtractedDataId}. Clinical text: {ClinicalText}",
                extractedDataId, clinicalText);
            throw; // Re-throw to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Executes CPT code mapping for extracted procedure data.
    /// Uses RAG retrieval + LLM (Gemini/GPT-4o) for intelligent code suggestions.
    /// </summary>
    /// <param name="extractedDataId">ExtractedClinicalData unique identifier</param>
    /// <param name="clinicalText">Clinical text to map (procedure description)</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })] // 1min, 5min, 15min
    [Queue("code-mapping")] // Dedicated queue for code mapping jobs
    public async Task MapToCPTAsync(Guid extractedDataId, string clinicalText)
    {
        _logger.LogInformation("Starting CPT code mapping for ExtractedDataId: {ExtractedDataId}", extractedDataId);

        try
        {
            var request = new CodeMappingRequestDto
            {
                ExtractedClinicalDataId = extractedDataId,
                ClinicalText = clinicalText,
                MaxSuggestions = 3 // FR-035: Return up to 3 CPT suggestions
            };

            var response = await _codeMappingService.MapToCPTAsync(request, CancellationToken.None);

            _logger.LogInformation(
                "CPT code mapping completed for ExtractedDataId: {ExtractedDataId}. Mapped {Count} codes, IsAmbiguous: {IsAmbiguous}",
                extractedDataId, response.SuggestionCount, response.IsAmbiguous);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "CPT code mapping failed for ExtractedDataId: {ExtractedDataId}. Clinical text: {ClinicalText}",
                extractedDataId, clinicalText);
            throw; // Re-throw to trigger Hangfire retry
        }
    }
}
