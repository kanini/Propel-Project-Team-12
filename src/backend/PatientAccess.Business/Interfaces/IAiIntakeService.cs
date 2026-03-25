namespace PatientAccess.Business.Interfaces;

/// <summary>
/// AI service interface for intake NLU processing (US_033, task_003).
/// Abstracts the LLM integration for natural language understanding.
/// </summary>
public interface IAiIntakeService
{
    /// <summary>
    /// Extracts structured data from a patient's natural language message (AC-2).
    /// </summary>
    /// <param name="patientMessage">The patient's message text</param>
    /// <param name="conversationHistory">Previous messages in the conversation</param>
    /// <param name="currentCategory">Current intake category being addressed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extraction result with structured data and confidence score</returns>
    Task<AiExtractionResult> ExtractStructuredDataAsync(
        string patientMessage,
        List<ConversationMessage> conversationHistory,
        string currentCategory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the next question/prompt for the patient based on progress (AC-1).
    /// </summary>
    /// <param name="conversationHistory">Previous messages in the conversation</param>
    /// <param name="completedCategories">List of categories already completed</param>
    /// <param name="currentCategory">Current category being addressed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next AI question/prompt</returns>
    Task<string> GenerateNextQuestionAsync(
        List<ConversationMessage> conversationHistory,
        List<string> completedCategories,
        string currentCategory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a clarification prompt when understanding is low.
    /// </summary>
    /// <param name="originalMessage">The message that was not understood</param>
    /// <param name="currentCategory">Current category being addressed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Clarification prompt</returns>
    Task<string> GenerateClarificationPromptAsync(
        string originalMessage,
        string currentCategory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the welcome message for starting an intake session.
    /// </summary>
    /// <param name="patientName">Patient's name for personalization</param>
    /// <param name="appointmentType">Type of appointment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Welcome message</returns>
    Task<string> GenerateWelcomeMessageAsync(
        string patientName,
        string? appointmentType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of AI extraction from patient message.
/// </summary>
public class AiExtractionResult
{
    /// <summary>
    /// Structured data extracted from the message.
    /// </summary>
    public List<ExtractedDataItem> ExtractedData { get; set; } = new();

    /// <summary>
    /// Overall confidence score (0-100).
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Whether the AI understood the message.
    /// </summary>
    public bool Understood { get; set; }

    /// <summary>
    /// Suggested next category to address.
    /// </summary>
    public string? NextCategory { get; set; }
}

/// <summary>
/// Individual extracted data item.
/// </summary>
public class ExtractedDataItem
{
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Confidence { get; set; }
}

/// <summary>
/// Message in conversation history.
/// </summary>
public class ConversationMessage
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
