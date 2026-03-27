using FluentValidation;
using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Validators;

/// <summary>
/// FluentValidation validator for ModifyCodeDto (US_052 Task 2).
/// Validates ICD-10 and CPT code format compliance and rationale requirements.
/// </summary>
public class ModifyCodeDtoValidator : AbstractValidator<ModifyCodeDto>
{
    public ModifyCodeDtoValidator()
    {
        // NewCode validation
        RuleFor(dto => dto.NewCode)
            .NotEmpty()
            .WithMessage("New code is required")
            .Must(BeValidCodeFormat)
            .WithMessage("Invalid code format. ICD-10: Letter + 2 digits + . + 1-2 digits (e.g., E11.9). CPT: 5 digits (e.g., 99213)");

        // NewDescription validation
        RuleFor(dto => dto.NewDescription)
            .NotEmpty()
            .WithMessage("Code description is required")
            .MinimumLength(5)
            .WithMessage("Description must be at least 5 characters")
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");

        // Rationale validation
        RuleFor(dto => dto.Rationale)
            .NotEmpty()
            .WithMessage("Modification rationale is required for audit compliance")
            .MinimumLength(10)
            .WithMessage("Rationale must be at least 10 characters to provide meaningful context")
            .MaximumLength(2000)
            .WithMessage("Rationale cannot exceed 2000 characters");
    }

    /// <summary>
    /// Validates that the code matches either ICD-10 or CPT format.
    /// ICD-10: Letter + 2 digits + "." + 1-2 digits (e.g., E11.9, E11.65)
    /// CPT: Exactly 5 digits (e.g., 99213, 80053)
    /// </summary>
    private bool BeValidCodeFormat(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        // ICD-10 format: Letter + 2 digits + "." + 1-2 digits
        var icd10Pattern = @"^[A-Z]\d{2}\.\d{1,2}$";
        if (System.Text.RegularExpressions.Regex.IsMatch(code, icd10Pattern))
            return true;

        // CPT format: Exactly 5 digits
        var cptPattern = @"^\d{5}$";
        if (System.Text.RegularExpressions.Regex.IsMatch(code, cptPattern))
            return true;

        return false;
    }
}
