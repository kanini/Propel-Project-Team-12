using FuzzySharp;
using System.Text.RegularExpressions;

namespace PatientAccess.Business.Utilities;

/// <summary>
/// Utility class for fuzzy string matching using Levenshtein distance algorithm.
/// Provides normalization and similarity calculation for entity resolution (AIR-005).
/// </summary>
public static class FuzzyMatcher
{
    /// <summary>
    /// Calculates similarity between two strings using Levenshtein distance (0-100 scale).
    /// </summary>
    /// <param name="str1">First string to compare</param>
    /// <param name="str2">Second string to compare</param>
    /// <returns>Similarity score: 100 = identical, 0 = completely different</returns>
    public static int CalculateSimilarity(string? str1, string? str2)
    {
        if (string.IsNullOrWhiteSpace(str1) || string.IsNullOrWhiteSpace(str2))
            return 0;

        var normalized1 = NormalizeString(str1);
        var normalized2 = NormalizeString(str2);

        // Use FuzzySharp's Fuzz.Ratio for basic Levenshtein distance
        return Fuzz.Ratio(normalized1, normalized2);
    }

    /// <summary>
    /// Calculates partial similarity for substring matching (useful for brand name variations).
    /// </summary>
    /// <param name="str1">First string to compare</param>
    /// <param name="str2">Second string to compare</param>
    /// <returns>Partial similarity score: 100 = substring match, 0 = no match</returns>
    public static int CalculatePartialSimilarity(string? str1, string? str2)
    {
        if (string.IsNullOrWhiteSpace(str1) || string.IsNullOrWhiteSpace(str2))
            return 0;

        var normalized1 = NormalizeString(str1);
        var normalized2 = NormalizeString(str2);

        // Use FuzzySharp's PartialRatio for substring matching
        return Fuzz.PartialRatio(normalized1, normalized2);
    }

    /// <summary>
    /// Normalizes string for comparison: lowercase, trim, remove punctuation, standardize whitespace.
    /// </summary>
    /// <param name="input">String to normalize</param>
    /// <returns>Normalized string</returns>
    public static string NormalizeString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Convert to lowercase
        var normalized = input.ToLowerInvariant().Trim();

        // Remove punctuation (keep alphanumeric and spaces)
        normalized = Regex.Replace(normalized, @"[^\w\s]", "");

        // Standardize whitespace (replace multiple spaces with single space)
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized;
    }

    /// <summary>
    /// Checks if two strings are an exact match (case-insensitive, after normalization).
    /// </summary>
    /// <param name="str1">First string to compare</param>
    /// <param name="str2">Second string to compare</param>
    /// <returns>True if strings are identical after normalization</returns>
    public static bool IsExactMatch(string? str1, string? str2)
    {
        if (string.IsNullOrWhiteSpace(str1) && string.IsNullOrWhiteSpace(str2))
            return true;

        if (string.IsNullOrWhiteSpace(str1) || string.IsNullOrWhiteSpace(str2))
            return false;

        return NormalizeString(str1) == NormalizeString(str2);
    }

    /// <summary>
    /// Checks if two strings have high similarity (default threshold: 90%).
    /// </summary>
    /// <param name="str1">First string to compare</param>
    /// <param name="str2">Second string to compare</param>
    /// <param name="threshold">Minimum similarity score (0-100), default 90</param>
    /// <returns>True if similarity >= threshold</returns>
    public static bool IsHighSimilarity(string? str1, string? str2, int threshold = 90)
    {
        return CalculateSimilarity(str1, str2) >= threshold;
    }

    /// <summary>
    /// Checks if two strings are a potential match (similarity between minThreshold and maxThreshold).
    /// Used to flag ambiguous cases requiring manual review (FR-031).
    /// </summary>
    /// <param name="str1">First string to compare</param>
    /// <param name="str2">Second string to compare</param>
    /// <param name="minThreshold">Minimum similarity score (default 70)</param>
    /// <param name="maxThreshold">Maximum similarity score (default 89)</param>
    /// <returns>True if similarity is within threshold range</returns>
    public static bool IsPotentialMatch(string? str1, string? str2, int minThreshold = 70, int maxThreshold = 89)
    {
        var similarity = CalculateSimilarity(str1, str2);
        return similarity >= minThreshold && similarity <= maxThreshold;
    }

    /// <summary>
    /// Removes common medication suffixes and forms for better matching.
    /// Examples: "tablets", "mg", "extended-release", etc.
    /// </summary>
    /// <param name="drugName">Medication name</param>
    /// <returns>Cleaned drug name</returns>
    public static string NormalizeMedicationName(string? drugName)
    {
        if (string.IsNullOrWhiteSpace(drugName))
            return string.Empty;

        var normalized = NormalizeString(drugName);

        // Remove common medication forms and suffixes
        var formsToRemove = new[] {
            "tablet", "tablets", "capsule", "capsules", "mg", "mcg", "ml",
            "extended release", "er", "xr", "immediate release", "ir",
            "oral", "injection", "inhaler", "solution", "suspension"
        };

        foreach (var form in formsToRemove)
        {
            normalized = Regex.Replace(normalized, $@"\b{form}\b", "", RegexOptions.IgnoreCase);
        }

        // Clean up extra spaces
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        return normalized;
    }
}
