using PatientAccess.Business.Models;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for entity resolution and de-duplication of clinical data (AIR-005, FR-030).
/// Detects duplicate medications, conditions, allergies, and encounters using fuzzy matching.
/// </summary>
public interface IEntityResolutionService
{
    /// <summary>
    /// Resolves medication duplicates, detecting conflicts when same drug has different doses.
    /// </summary>
    /// <param name="medications">List of extracted medication data points</param>
    /// <returns>Dictionary mapping source data IDs to match results and grouped duplicates</returns>
    Task<Dictionary<Guid, EntityMatchResult>> ResolveMedicationDuplicatesAsync(List<ExtractedClinicalData> medications);

    /// <summary>
    /// Resolves condition/diagnosis duplicates using name matching and ICD-10 code comparison.
    /// </summary>
    /// <param name="conditions">List of extracted condition data points</param>
    /// <returns>Dictionary mapping source data IDs to match results and grouped duplicates</returns>
    Task<Dictionary<Guid, EntityMatchResult>> ResolveConditionDuplicatesAsync(List<ExtractedClinicalData> conditions);

    /// <summary>
    /// Resolves allergy duplicates, detecting conflicts when same allergen has different severity.
    /// </summary>
    /// <param name="allergies">List of extracted allergy data points</param>
    /// <returns>Dictionary mapping source data IDs to match results and grouped duplicates</returns>
    Task<Dictionary<Guid, EntityMatchResult>> ResolveAllergyDuplicatesAsync(List<ExtractedClinicalData> allergies);

    /// <summary>
    /// Resolves encounter/visit duplicates using date, type, provider, and facility matching.
    /// </summary>
    /// <param name="encounters">List of extracted encounter data points</param>
    /// <returns>Dictionary mapping source data IDs to match results and grouped duplicates</returns>
    Task<Dictionary<Guid, EntityMatchResult>> ResolveEncounterDuplicatesAsync(List<ExtractedClinicalData> encounters);

    /// <summary>
    /// Compares two clinical data points for similarity using field-specific matching logic.
    /// </summary>
    /// <param name="entity1">First extracted clinical data point</param>
    /// <param name="entity2">Second extracted clinical data point</param>
    /// <returns>Detailed similarity analysis with field-level scores</returns>
    Task<EntitySimilarity> CalculateEntitySimilarityAsync(ExtractedClinicalData entity1, ExtractedClinicalData entity2);
}
