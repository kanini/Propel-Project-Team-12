using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for medical code mapping (ICD-10, CPT) using RAG pattern with Azure OpenAI.
/// US_051 Task 1 - Code Mapping Service.
/// Implements FR-034 (ICD-10 code suggestions), FR-035 (CPT code suggestions),
/// AIR-003 (ICD-10 mapping via RAG), AIR-004 (CPT mapping via RAG).
/// </summary>
public interface ICodeMappingService
{
    /// <summary>
    /// Maps extracted clinical text to ICD-10 diagnosis codes using RAG retrieval and GPT-4o inference.
    /// Returns top-N code suggestions ranked by confidence score (0-100%).
    /// </summary>
    /// <param name="request">Request containing clinical text and ExtractedClinicalDataId</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with ICD-10 code suggestions, confidence scores, and rationale</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when Azure OpenAI call fails after retries</exception>
    Task<CodeMappingResponseDto> MapToICD10Async(
        CodeMappingRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps extracted clinical text to CPT procedure codes using RAG retrieval and GPT-4o inference.
    /// Returns top-N code suggestions ranked by confidence score (0-100%).
    /// </summary>
    /// <param name="request">Request containing clinical text and ExtractedClinicalDataId</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with CPT code suggestions, confidence scores, and rationale</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when Azure OpenAI call fails after retries</exception>
    Task<CodeMappingResponseDto> MapToCPTAsync(
        CodeMappingRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates CodeMappingResponseDto output schema per AIR-Q03 (>99% validity target).
    /// Uses FluentValidation to check confidence scores, code formats, and required fields.
    /// </summary>
    /// <param name="response">Response DTO to validate</param>
    /// <returns>True if validation passes, false otherwise</returns>
    Task<bool> ValidateOutputSchemaAsync(CodeMappingResponseDto response);

    /// <summary>
    /// Calculates AI-Human Agreement Rate for a given time period per AIR-Q01 (>98% target).
    /// Agreement = (VerifiedTopSuggestions / TotalVerifiedCodes) * 100.
    /// Tracks metric in QualityMetrics table for reporting.
    /// </summary>
    /// <param name="periodStart">Start of measurement period (UTC)</param>
    /// <param name="periodEnd">End of measurement period (UTC)</param>
    /// <returns>Agreement rate as percentage (0-100)</returns>
    Task<decimal> CalculateAgreementRateAsync(DateTime periodStart, DateTime periodEnd);
}
