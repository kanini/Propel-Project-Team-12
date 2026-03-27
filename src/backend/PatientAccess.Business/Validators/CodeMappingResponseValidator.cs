using FluentValidation;
using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Validators;

/// <summary>
/// FluentValidation validator for CodeMappingResponseDto to ensure schema validity >99% (AIR-Q03).
/// US_051 Task 1 - Code Mapping Service.
/// Validates confidence scores, code formats, and required fields per output schema contract.
/// </summary>
public class CodeMappingResponseValidator : AbstractValidator<CodeMappingResponseDto>
{
    public CodeMappingResponseValidator()
    {
        // ExtractedClinicalDataId must be a valid GUID
        RuleFor(r => r.ExtractedClinicalDataId)
            .NotEmpty()
            .WithMessage("ExtractedClinicalDataId is required");

        // CodeSystem must be "ICD10" or "CPT"
        RuleFor(r => r.CodeSystem)
            .Must(cs => cs == "ICD10" || cs == "CPT")
            .WithMessage("CodeSystem must be 'ICD10' or 'CPT'");

        // Suggestions list must not be null (can be empty for "No matching code found" case)
        RuleFor(r => r.Suggestions)
            .NotNull()
            .WithMessage("Suggestions list cannot be null");

        // Validate each suggestion in the list
        RuleForEach(r => r.Suggestions).ChildRules(suggestion =>
        {
            // Code must not be empty and must be within 20 characters
            suggestion.RuleFor(s => s.Code)
                .NotEmpty()
                .WithMessage("Code is required")
                .MaximumLength(20)
                .WithMessage("Code must not exceed 20 characters");

            // Description must not be empty and must be within 1000 characters
            suggestion.RuleFor(s => s.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MaximumLength(1000)
                .WithMessage("Description must not exceed 1000 characters");

            // ConfidenceScore must be between 0 and 100 (inclusive)
            suggestion.RuleFor(s => s.ConfidenceScore)
                .InclusiveBetween(0, 100)
                .WithMessage("Confidence score must be between 0 and 100");

            // Rationale must not be empty and must be within 2000 characters
            suggestion.RuleFor(s => s.Rationale)
                .NotEmpty()
                .WithMessage("Rationale is required")
                .MaximumLength(2000)
                .WithMessage("Rationale must not exceed 2000 characters");

            // Rank must be greater than 0 (1 = top suggestion, 2-5 = alternatives)
            suggestion.RuleFor(s => s.Rank)
                .GreaterThan(0)
                .WithMessage("Rank must be greater than 0");
        });

        // SuggestionCount must match actual count of Suggestions list
        RuleFor(r => r.SuggestionCount)
            .Must((response, count) => count == response.Suggestions.Count)
            .WithMessage("SuggestionCount must match the number of items in Suggestions list");

        // If Message is not empty, it should be within 500 characters
        RuleFor(r => r.Message)
            .MaximumLength(500)
            .When(r => !string.IsNullOrEmpty(r.Message))
            .WithMessage("Message must not exceed 500 characters");
    }
}
