using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.Services;

/// <summary>
/// Stub implementation of IAiIntakeService for development/testing (US_033).
/// The real LLM integration is implemented in task_003 (AI LLM Integration).
/// This stub provides simple rule-based responses.
/// </summary>
public class StubAiIntakeService : IAiIntakeService
{
    private static readonly Dictionary<string, string> CategoryQuestions = new()
    {
        { "chiefComplaint", "What brings you in today? Please describe your main health concern." },
        { "symptoms", "Can you describe any symptoms you're experiencing? Include when they started and how severe they are." },
        { "medications", "Are you currently taking any medications? Please list them along with dosages if you know them." },
        { "allergies", "Do you have any known allergies to medications, foods, or other substances?" },
        { "medicalHistory", "Do you have any medical conditions or past surgeries we should know about?" },
        { "familyHistory", "Is there any significant medical history in your family, such as heart disease, diabetes, or cancer?" },
        { "lifestyle", "Can you tell me about your lifestyle? This includes smoking, alcohol use, and exercise habits." },
        { "insurance", "Finally, can you confirm your insurance information?" },
        { "complete", "Thank you for providing all that information. I've gathered everything we need. Please review the summary before your visit." }
    };

    /// <inheritdoc />
    public Task<AiExtractionResult> ExtractStructuredDataAsync(
        string patientMessage,
        List<ConversationMessage> conversationHistory,
        string currentCategory,
        CancellationToken cancellationToken = default)
    {
        // Simple keyword-based extraction for stub
        var extractedData = new List<ExtractedDataItem>();
        var confidence = 85; // Default high confidence for stub
        var messageLower = patientMessage.ToLower().Trim();

        // Check for skip/next requests
        if (messageLower.Contains("skip") || 
            messageLower.Contains("next question") || 
            messageLower.Contains("go to") ||
            messageLower == "no" && conversationHistory.Count > 2) // Multiple "no" responses
        {
            // User wants to skip - still capture the response but with lower value
            extractedData.Add(new ExtractedDataItem
            {
                Field = currentCategory,
                Value = "Not provided",
                Confidence = 80
            });
            confidence = 80;
        }
        else if (currentCategory == "complete")
        {
            // Intake is already complete - acknowledge but don't extract
            extractedData.Add(new ExtractedDataItem
            {
                Field = "acknowledgment",
                Value = patientMessage,
                Confidence = 100
            });
            confidence = 100;
        }
        else if (patientMessage.Length < 3)
        {
            // Too short - low confidence
            confidence = 50;
        }
        else if (currentCategory == "chiefComplaint")
        {
            extractedData.Add(new ExtractedDataItem
            {
                Field = "chiefComplaint",
                Value = patientMessage,
                Confidence = 90
            });
            confidence = 90;
        }
        else if (currentCategory == "symptoms")
        {
            extractedData.Add(new ExtractedDataItem
            {
                Field = "symptoms",
                Value = patientMessage,
                Confidence = 85
            });
            confidence = 85;
        }
        else if (currentCategory == "medications")
        {
            if (messageLower.Contains("no") || messageLower.Contains("none") || messageLower.Contains("not taking"))
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "medications",
                    Value = "None reported",
                    Confidence = 95
                });
                confidence = 95;
            }
            else
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "medications",
                    Value = patientMessage,
                    Confidence = 85
                });
                confidence = 85;
            }
        }
        else if (currentCategory == "allergies")
        {
            if (messageLower.Contains("no") || messageLower.Contains("none") || messageLower.Contains("don't have"))
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "allergies",
                    Value = "No known allergies",
                    Confidence = 95
                });
                confidence = 95;
            }
            else
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "allergies",
                    Value = patientMessage,
                    Confidence = 85
                });
                confidence = 85;
            }
        }
        else if (currentCategory == "medicalHistory")
        {
            if (messageLower.Contains("no") || messageLower.Contains("none") || 
                messageLower.Contains("don't have") || messageLower.Contains("not provided"))
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "medicalHistory",
                    Value = "No significant medical history reported",
                    Confidence = 95
                });
                confidence = 95;
            }
            else
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "medicalHistory",
                    Value = patientMessage,
                    Confidence = 85
                });
                confidence = 85;
            }
        }
        else if (currentCategory == "familyHistory")
        {
            if (messageLower.Contains("no") || messageLower.Contains("none") || 
                messageLower.Contains("don't have") || messageLower.Contains("not provided"))
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "familyHistory",
                    Value = "No significant family history reported",
                    Confidence = 95
                });
                confidence = 95;
            }
            else
            {
                extractedData.Add(new ExtractedDataItem
                {
                    Field = "familyHistory",
                    Value = patientMessage,
                    Confidence = 85
                });
                confidence = 85;
            }
        }
        else if (currentCategory == "lifestyle")
        {
            extractedData.Add(new ExtractedDataItem
            {
                Field = "lifestyle",
                Value = patientMessage,
                Confidence = 80
            });
            confidence = 80;
        }
        else if (currentCategory == "insurance")
        {
            extractedData.Add(new ExtractedDataItem
            {
                Field = "insurance",
                Value = patientMessage,
                Confidence = 80
            });
            confidence = 80;
        }
        else
        {
            // Generic extraction
            extractedData.Add(new ExtractedDataItem
            {
                Field = currentCategory,
                Value = patientMessage,
                Confidence = confidence
            });
        }

        // Determine next category
        var categories = new[] { "chiefComplaint", "symptoms", "medications", "allergies", 
                                 "medicalHistory", "familyHistory", "lifestyle", "insurance" };
        
        string nextCategory;
        if (currentCategory == "complete")
        {
            // Already complete - stay at complete
            nextCategory = "complete";
        }
        else
        {
            var currentIndex = Array.IndexOf(categories, currentCategory);
            nextCategory = currentIndex < categories.Length - 1 
                ? categories[currentIndex + 1] 
                : "complete";
        }

        return Task.FromResult(new AiExtractionResult
        {
            ExtractedData = extractedData,
            ConfidenceScore = confidence,
            Understood = confidence >= 70,
            NextCategory = nextCategory
        });
    }

    /// <inheritdoc />
    public Task<string> GenerateNextQuestionAsync(
        List<ConversationMessage> conversationHistory,
        List<string> completedCategories,
        string currentCategory,
        CancellationToken cancellationToken = default)
    {
        var question = CategoryQuestions.TryGetValue(currentCategory, out var q) 
            ? q 
            : "Please continue with your health information.";

        return Task.FromResult(question);
    }

    /// <inheritdoc />
    public Task<string> GenerateClarificationPromptAsync(
        string originalMessage,
        string currentCategory,
        CancellationToken cancellationToken = default)
    {
        var prompts = new Dictionary<string, string>
        {
            { "chiefComplaint", "I'd like to understand your concern better. Could you provide more details about what's bothering you?" },
            { "symptoms", "Could you be more specific about your symptoms? For example, when did they start and how would you rate their severity?" },
            { "medications", "I want to make sure I capture your medications correctly. Could you list them one at a time?" },
            { "allergies", "To ensure your safety, could you clarify your allergies? Even seasonal allergies are important to note." },
            { "medicalHistory", "Could you provide more details about your medical history? Any past surgeries or ongoing conditions?" }
        };

        var prompt = prompts.TryGetValue(currentCategory, out var p) 
            ? p 
            : "I didn't quite understand that. Could you please rephrase?";

        return Task.FromResult(prompt);
    }

    /// <inheritdoc />
    public Task<string> GenerateWelcomeMessageAsync(
        string patientName,
        string? appointmentType,
        CancellationToken cancellationToken = default)
    {
        var message = $"Hello {patientName}! I'm here to help you complete your pre-visit intake form. " +
                     "I'll ask you a few questions about your health to help your provider prepare for your appointment. " +
                     "Let's start - what brings you in today?";

        return Task.FromResult(message);
    }
}
