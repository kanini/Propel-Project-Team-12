using FluentValidation;
using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Validators;

/// <summary>
/// Validator for ModifyCodeDto (EP-008-US-052).
/// Validates ICD-10 and CPT code formats per industry standards.
/// </summary>
public class ModifyCodeDtoValidator : AbstractValidator<ModifyCodeDto>
{
    public ModifyCodeDtoValidator()
    {
        RuleFor(dto => dto.NewCode)
            .NotEmpty()
            .WithMessage("New code is required");

        // ICD-10 format validation: Letter + 2 digits + "." + 1-2 digits
        // Examples: E11.9, A01.05, Z23
        RuleFor(dto => dto.NewCode)
            .Matches(@"^[A-Z]\d{2}\.?\d{0,2}$")
            .When(dto => IsICD10Format(dto.NewCode))
            .WithMessage("Invalid ICD-10 code format. Expected format: Letter + 2 digits + optional '.' + 0-2 digits (e.g., E11.9, A01.05)");

        // CPT format validation: 5 digits
        // Examples: 99213, 80053, 12345
        RuleFor(dto => dto.NewCode)
            .Matches(@"^\d{5}$")
            .When(dto => IsCPTFormat(dto.NewCode))
            .WithMessage("Invalid CPT code format. Expected format: 5 digits (e.g., 99213, 80053)");

        RuleFor(dto => dto.NewDescription)
            .NotEmpty()
            .WithMessage("Code description is required")
            .MinimumLength(5)
            .WithMessage("Description must be at least 5 characters")
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");

        RuleFor(dto => dto.Rationale)
            .NotEmpty()
            .WithMessage("Modification rationale is required")
            .MinimumLength(10)
            .WithMessage("Rationale must be at least 10 characters to provide meaningful context")
            .MaximumLength(2000)
            .WithMessage("Rationale cannot exceed 2000 characters");
    }

    /// <summary>
    /// Determines if the code follows ICD-10 format pattern.
    /// ICD-10 codes start with a letter.
    /// </summary>
    private static bool IsICD10Format(string code)
    {
        return !string.IsNullOrEmpty(code) && code.Length >= 3 && char.IsLetter(code[0]);
    }

    /// <summary>
    /// Determines if the code follows CPT format pattern.
    /// CPT codes are exactly 5 digits.
    /// </summary>
    private static bool IsCPTFormat(string code)
    {
        return !string.IsNullOrEmpty(code) && code.Length == 5 && code.All(char.IsDigit);
    }
}
