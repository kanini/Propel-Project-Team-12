using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for medical code mapping using RAG pattern.
/// Maps extracted clinical data to ICD-10 diagnosis codes or CPT procedure codes
/// with confidence scores and LLM-generated rationale (AIR-003, AIR-004).
/// </summary>
public interface ICodeMappingService
{
    /// <summary>
    /// Maps clinical text to ICD-10 diagnosis codes using RAG retrieval and Azure OpenAI GPT-4o.
    /// Returns top-N suggestions ranked by confidence score (AIR-003, AIR-Q01).
    /// </summary>
    /// <param name="request">Code mapping request with clinical text and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with ICD-10 code suggestions and metadata</returns>
    Task<CodeMappingResponseDto> MapToICD10Async(
        CodeMappingRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Maps clinical text to CPT procedure codes using RAG retrieval and Azure OpenAI GPT-4o.
    /// Returns top-N suggestions ranked by confidence score (AIR-004, AIR-Q01).
    /// </summary>
    /// <param name="request">Code mapping request with clinical text and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response with CPT code suggestions and metadata</returns>
    Task<CodeMappingResponseDto> MapToCPTAsync(
        CodeMappingRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates code mapping response against schema requirements.
    /// Used for AIR-Q03 (>99% output schema validity) tracking.
    /// </summary>
    /// <param name="response">Code mapping response to validate</param>
    /// <returns>True if response passes schema validation</returns>
    Task<bool> ValidateOutputSchemaAsync(CodeMappingResponseDto response);

    /// <summary>
    /// Calculates AI-Human Agreement Rate for specified time period.
    /// Measures percentage of AI suggestions verified/accepted by staff (AIR-Q01: >98%).
    /// </summary>
    /// <param name="periodStart">Start of measurement period (UTC)</param>
    /// <param name="periodEnd">End of measurement period (UTC)</param>
    /// <returns>Agreement rate as percentage (0-100)</returns>
    Task<decimal> CalculateAgreementRateAsync(DateTime periodStart, DateTime periodEnd);
}
