using FluentValidation;
using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Validators;

/// <summary>
/// FluentValidation validator for CodeMappingResponseDto.
/// Ensures output schema validity > 99% per AIR-Q03.
/// </summary>
public class CodeMappingResponseValidator : AbstractValidator<CodeMappingResponseDto>
{
    public CodeMappingResponseValidator()
    {
        RuleFor(r => r.ExtractedClinicalDataId)
            .NotEmpty()
            .WithMessage("ExtractedClinicalDataId is required");

        RuleFor(r => r.CodeSystem)
            .NotEmpty()
            .Must(cs => cs == "ICD10" || cs == "CPT")
            .WithMessage("CodeSystem must be 'ICD10' or 'CPT'");

        // Suggestions can be null/empty if Message is set (edge case: no matching code found)
        RuleFor(r => r.Suggestions)
            .NotNull()
            .When(r => string.IsNullOrEmpty(r.Message))
            .WithMessage("Suggestions cannot be null when no error message is present");

        // Validate each suggestion in the list
        RuleForEach(r => r.Suggestions).ChildRules(suggestion =>
        {
            suggestion.RuleFor(s => s.Code)
                .NotEmpty()
                .WithMessage("Suggestion code cannot be empty")
                .MaximumLength(20)
                .WithMessage("Suggestion code cannot exceed 20 characters");

            suggestion.RuleFor(s => s.Description)
                .NotEmpty()
                .WithMessage("Suggestion description cannot be empty")
                .MaximumLength(1000)
                .WithMessage("Suggestion description cannot exceed 1000 characters");

            suggestion.RuleFor(s => s.ConfidenceScore)
                .InclusiveBetween(0, 100)
                .WithMessage("Confidence score must be between 0 and 100");

            suggestion.RuleFor(s => s.Rationale)
                .NotEmpty()
                .WithMessage("Suggestion rationale cannot be empty")
                .MaximumLength(2000)
                .WithMessage("Suggestion rationale cannot exceed 2000 characters");

            suggestion.RuleFor(s => s.Rank)
                .GreaterThan(0)
                .WithMessage("Suggestion rank must be greater than 0");
        });

        // Validate SuggestionCount matches actual count
        RuleFor(r => r.SuggestionCount)
            .Equal(r => r.Suggestions != null ? r.Suggestions.Count : 0)
            .WithMessage("SuggestionCount must match the actual number of suggestions");
    }
}
