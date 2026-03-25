namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for intake chat message (US_033, AC-2, AC-4).
/// Contains AI response, extracted data, and confidence metrics.
/// </summary>
public class IntakeMessageResponseDto
{
    /// <summary>
    /// AI assistant's response message.
    /// </summary>
    public string AiMessage { get; set; } = string.Empty;

    /// <summary>
    /// Structured data extracted from the patient's message.
    /// </summary>
    public List<ExtractedDataItemDto> ExtractedData { get; set; } = new();

    /// <summary>
    /// Confidence score for the extracted data (0-100).
    /// </summary>
    public int ConfidenceLevel { get; set; }

    /// <summary>
    /// Current intake category being addressed.
    /// </summary>
    public string CurrentCategory { get; set; } = string.Empty;

    /// <summary>
    /// Overall intake progress percentage (0-100).
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Whether the intake is complete after this message.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Whether to suggest switching to manual form mode (AC-4).
    /// True when confidence drops below 70% for 3+ consecutive messages.
    /// </summary>
    public bool SuggestManualFallback { get; set; }
}

/// <summary>
/// Individual extracted data item from AI processing.
/// </summary>
public class ExtractedDataItemDto
{
    /// <summary>
    /// Field name (e.g., "medication", "allergy", "chiefComplaint").
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Extracted value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score for this specific extraction (0-100).
    /// </summary>
    public int Confidence { get; set; }
}
