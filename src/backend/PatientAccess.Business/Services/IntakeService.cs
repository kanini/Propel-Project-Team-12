using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service implementation for intake session operations (US_033).
/// Manages intake lifecycle, delegates AI processing, and persists data.
/// </summary>
public class IntakeService : IIntakeService
{
    private readonly PatientAccessDbContext _context;
    private readonly IAiIntakeService _aiService;
    private readonly ILogger<IntakeService> _logger;

    /// <summary>
    /// Intake categories in order of completion.
    /// </summary>
    private static readonly string[] IntakeCategories = new[]
    {
        "chiefComplaint",
        "symptoms",
        "medications",
        "allergies",
        "medicalHistory",
        "familyHistory",
        "lifestyle",
        "insurance"
    };

    public IntakeService(
        PatientAccessDbContext context,
        IAiIntakeService aiService,
        ILogger<IntakeService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<StartIntakeResponseDto> StartSessionAsync(
        Guid patientId,
        StartIntakeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting intake session for PatientId: {PatientId}, AppointmentId: {AppointmentId}",
            patientId, request.AppointmentId);

        // Validate appointment belongs to patient
        var appointment = await _context.Appointments
            .Include(a => a.Provider)
            .FirstOrDefaultAsync(a => 
                a.AppointmentId == request.AppointmentId &&
                a.PatientId == patientId,
                cancellationToken);

        if (appointment == null)
        {
            throw new NotFoundException("Appointment not found or does not belong to this patient");
        }

        // Check for existing intake record
        var existingRecord = await _context.IntakeRecords
            .FirstOrDefaultAsync(ir => ir.AppointmentId == appointment.AppointmentId,
                cancellationToken);

        IntakeRecord intakeRecord;

        if (existingRecord != null)
        {
            // Return existing session
            intakeRecord = existingRecord;
            _logger.LogInformation(
                "Resuming existing intake session: {SessionId}",
                intakeRecord.IntakeRecordId);
        }
        else
        {
            // Create new intake record
            intakeRecord = new IntakeRecord
            {
                IntakeRecordId = Guid.NewGuid(),
                AppointmentId = appointment.AppointmentId,
                PatientId = patientId,
                IntakeMode = request.Mode == "manual" ? IntakeMode.ManualForm : IntakeMode.AIConversational,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false,
                // Initialize with first category
                SymptomHistory = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    { "currentCategory", "chiefComplaint" },
                    { "conversationHistory", new List<ConversationMessage>() },
                    { "consecutiveFailures", 0 }
                })
            };

            _context.IntakeRecords.Add(intakeRecord);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created new intake session: {SessionId}",
                intakeRecord.IntakeRecordId);
        }

        // Generate welcome message
        var patient = await _context.Users.FindAsync(new object[] { patientId }, cancellationToken);
        var welcomeMessage = await _aiService.GenerateWelcomeMessageAsync(
            patient?.Name ?? "there",
            appointment.VisitReason,
            cancellationToken);

        return new StartIntakeResponseDto
        {
            SessionId = intakeRecord.IntakeRecordId.ToString(),
            WelcomeMessage = welcomeMessage,
            Status = intakeRecord.IsCompleted ? "completed" : "active"
        };
    }

    /// <inheritdoc />
    public async Task<IntakeMessageResponseDto> ProcessMessageAsync(
        Guid patientId,
        IntakeMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.SessionId, out var sessionGuid))
        {
            throw new ValidationException("Invalid session ID format");
        }

        // Load intake record with authorization
        var intakeRecord = await GetIntakeRecordWithAuthAsync(patientId, sessionGuid, cancellationToken);

        _logger.LogInformation(
            "Processing message for SessionId: {SessionId}, MessageLength: {Length}",
            request.SessionId, request.Message.Length);

        // Get conversation history
        var conversationHistory = DeserializeConversationHistory(intakeRecord);
        
        // Get stored current category from previous turn, or calculate from completed categories
        var symptomsData = DeserializeSymptomHistory(intakeRecord);
        string currentCategory;
        
        if (symptomsData.TryGetValue("currentCategory", out var storedCategory) && storedCategory is string categoryStr)
        {
            currentCategory = categoryStr;
            _logger.LogInformation("Using stored currentCategory: {Category}", currentCategory);
        }
        else
        {
            var completedCategories = GetCompletedCategories(intakeRecord);
            currentCategory = GetNextCategory(completedCategories);
            _logger.LogInformation("Calculated currentCategory from completed data: {Category}", currentCategory);
        }

        // Check if already complete - if so, acknowledge message but indicate completion
        if (currentCategory == "complete")
        {
            _logger.LogInformation("Intake already complete, acknowledging final message");
            
            conversationHistory.Add(new ConversationMessage
            {
                Role = "user",
                Content = request.Message,
                Timestamp = DateTime.UtcNow
            });

            var completionMessage = "Thank you! I've gathered all the information we need. " +
                                  "Please review the summary and confirm when you're ready to submit.";
            
            conversationHistory.Add(new ConversationMessage
            {
                Role = "assistant",
                Content = completionMessage,
                Timestamp = DateTime.UtcNow
            });

            symptomsData["conversationHistory"] = conversationHistory;
            intakeRecord.SymptomHistory = JsonSerializer.Serialize(symptomsData);
            await _context.SaveChangesAsync(cancellationToken);

            return new IntakeMessageResponseDto
            {
                AiMessage = completionMessage,
                ExtractedData = new List<ExtractedDataItemDto>(),
                ConfidenceLevel = 100,
                CurrentCategory = "complete",
                Progress = 100,
                IsComplete = true,
                SuggestManualFallback = false
            };
        }

        // Extract data using AI service
        var extractionResult = await _aiService.ExtractStructuredDataAsync(
            request.Message,
            conversationHistory,
            currentCategory,
            cancellationToken);

        // Update conversation history
        conversationHistory.Add(new ConversationMessage
        {
            Role = "user",
            Content = request.Message,
            Timestamp = DateTime.UtcNow
        });

        // Check confidence and track failures
        var consecutiveFailures = GetConsecutiveFailures(intakeRecord);
        bool suggestFallback = false;

        if (extractionResult.ConfidenceScore < 70)
        {
            consecutiveFailures++;
            if (consecutiveFailures >= 3)
            {
                suggestFallback = true;
            }
        }
        else
        {
            consecutiveFailures = 0;
            // Persist extracted data if confidence is high enough
            await UpdateIntakeDataFromExtraction(intakeRecord, extractionResult, cancellationToken);
        }

        // Generate AI response
        string aiMessage;
        if (extractionResult.ConfidenceScore < 70)
        {
            aiMessage = await _aiService.GenerateClarificationPromptAsync(
                request.Message,
                currentCategory,
                cancellationToken);
        }
        else
        {
            var newCompletedCategories = GetCompletedCategories(intakeRecord);
            aiMessage = await _aiService.GenerateNextQuestionAsync(
                conversationHistory,
                newCompletedCategories,
                extractionResult.NextCategory ?? currentCategory,
                cancellationToken);
        }

        // Add AI response to history
        conversationHistory.Add(new ConversationMessage
        {
            Role = "assistant",
            Content = aiMessage,
            Timestamp = DateTime.UtcNow
        });

        // Determine the next category to track for the next turn
        var nextCategoryForTracking = extractionResult.ConfidenceScore >= 70 
            ? (extractionResult.NextCategory ?? currentCategory)
            : currentCategory; // Stay on same category if confidence was low

        // Persist conversation history, failure count, and current category
        symptomsData = DeserializeSymptomHistory(intakeRecord);
        symptomsData["conversationHistory"] = conversationHistory;
        symptomsData["consecutiveFailures"] = consecutiveFailures;
        symptomsData["currentCategory"] = nextCategoryForTracking; // Store for next request
        
        intakeRecord.SymptomHistory = JsonSerializer.Serialize(symptomsData);
        intakeRecord.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Saved state - currentCategory for next turn: {Category}, Progress: {Progress}%", 
            nextCategoryForTracking, 
            CalculateProgress(intakeRecord));

        await _context.SaveChangesAsync(cancellationToken);

        // Calculate progress
        var progress = CalculateProgress(intakeRecord);
        
        // Mark as complete if nextCategory is "complete" OR progress is 100%
        var isComplete = nextCategoryForTracking == "complete" || progress >= 100;

        _logger.LogInformation(
            "Response - NextCategory: {NextCategory}, Progress: {Progress}%, IsComplete: {IsComplete}",
            nextCategoryForTracking, progress, isComplete);

        return new IntakeMessageResponseDto
        {
            AiMessage = aiMessage,
            ExtractedData = extractionResult.ExtractedData
                .Select(e => new ExtractedDataItemDto
                {
                    Field = e.Field,
                    Value = e.Value,
                    Confidence = e.Confidence
                }).ToList(),
            ConfidenceLevel = extractionResult.ConfidenceScore,
            CurrentCategory = extractionResult.NextCategory ?? currentCategory,
            Progress = Math.Min(progress, 100), // Cap at 100
            IsComplete = isComplete,
            SuggestManualFallback = suggestFallback
        };
    }

    /// <inheritdoc />
    public async Task<IntakeSummaryDto> GetSummaryAsync(
        Guid patientId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            throw new ValidationException("Invalid session ID format");
        }

        var intakeRecord = await GetIntakeRecordWithAuthAsync(patientId, sessionGuid, cancellationToken);

        return DeserializeIntakeSummary(intakeRecord);
    }

    /// <inheritdoc />
    public async Task UpdateIntakeAsync(
        Guid patientId,
        string sessionId,
        UpdateIntakeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            throw new ValidationException("Invalid session ID format");
        }

        var intakeRecord = await GetIntakeRecordWithAuthAsync(patientId, sessionGuid, cancellationToken);

        // Apply updates
        if (request.ChiefComplaint != null)
            intakeRecord.ChiefComplaint = request.ChiefComplaint;
        if (request.SymptomHistory != null)
            intakeRecord.SymptomHistory = request.SymptomHistory;
        if (request.CurrentMedications != null)
            intakeRecord.CurrentMedications = request.CurrentMedications;
        if (request.KnownAllergies != null)
            intakeRecord.KnownAllergies = request.KnownAllergies;
        if (request.MedicalHistory != null)
            intakeRecord.MedicalHistory = request.MedicalHistory;

        intakeRecord.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated intake record: {SessionId}", sessionId);
    }

    /// <inheritdoc />
    public async Task<CompleteIntakeResponseDto> CompleteIntakeAsync(
        Guid patientId,
        string sessionId,
        CompleteIntakeRequestDto? request,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            throw new ValidationException("Invalid session ID format");
        }

        var intakeRecord = await GetIntakeRecordWithAuthAsync(patientId, sessionGuid, cancellationToken);

        intakeRecord.IsCompleted = true;
        intakeRecord.CompletedAt = DateTime.UtcNow;
        intakeRecord.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed intake session: {SessionId}", sessionId);

        return new CompleteIntakeResponseDto
        {
            Success = true,
            IntakeRecordId = intakeRecord.IntakeRecordId.ToString(),
            Message = "Intake completed successfully"
        };
    }

    /// <inheritdoc />
    public async Task<IntakeSessionDto?> GetSessionAsync(
        Guid patientId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            return null;
        }

        var intakeRecord = await _context.IntakeRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(ir => ir.IntakeRecordId == sessionGuid && ir.PatientId == patientId,
                cancellationToken);

        if (intakeRecord == null)
            return null;

        return new IntakeSessionDto
        {
            SessionId = intakeRecord.IntakeRecordId.ToString(),
            AppointmentId = intakeRecord.AppointmentId,
            Mode = intakeRecord.IntakeMode == IntakeMode.ManualForm ? "manual" : "ai",
            Status = intakeRecord.IsCompleted ? "completed" : "active",
            Progress = CalculateProgress(intakeRecord)
        };
    }

    /// <inheritdoc />
    public async Task<SwitchModeResponseDto> SwitchModeAsync(
        Guid patientId,
        string sessionId,
        string newMode,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sessionId, out var sessionGuid))
        {
            throw new ValidationException("Invalid session ID format");
        }

        var intakeRecord = await GetIntakeRecordWithAuthAsync(patientId, sessionGuid, cancellationToken);

        intakeRecord.IntakeMode = newMode == "manual" ? IntakeMode.ManualForm : IntakeMode.AIConversational;
        intakeRecord.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Switched intake mode to {Mode} for SessionId: {SessionId}",
            newMode, sessionId);

        return new SwitchModeResponseDto
        {
            SessionId = sessionId,
            Mode = newMode,
            DataPreserved = true
        };
    }

    #region Private Helper Methods

    private async Task<IntakeRecord> GetIntakeRecordWithAuthAsync(
        Guid patientId,
        Guid sessionGuid,
        CancellationToken cancellationToken)
    {
        var intakeRecord = await _context.IntakeRecords
            .FirstOrDefaultAsync(ir => ir.IntakeRecordId == sessionGuid, cancellationToken);

        if (intakeRecord == null)
        {
            throw new NotFoundException("Intake session not found");
        }

        if (intakeRecord.PatientId != patientId)
        {
            throw new UnauthorizedException("Access denied to this intake session");
        }

        return intakeRecord;
    }

    private static List<ConversationMessage> DeserializeConversationHistory(IntakeRecord record)
    {
        if (string.IsNullOrEmpty(record.SymptomHistory))
            return new List<ConversationMessage>();

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(record.SymptomHistory);
            if (data.TryGetProperty("conversationHistory", out var historyElement))
            {
                return JsonSerializer.Deserialize<List<ConversationMessage>>(historyElement.GetRawText())
                    ?? new List<ConversationMessage>();
            }
        }
        catch
        {
            // Legacy format or corrupted data
        }

        return new List<ConversationMessage>();
    }

    private static int GetConsecutiveFailures(IntakeRecord record)
    {
        if (string.IsNullOrEmpty(record.SymptomHistory))
            return 0;

        try
        {
            var data = JsonSerializer.Deserialize<JsonElement>(record.SymptomHistory);
            if (data.TryGetProperty("consecutiveFailures", out var failuresElement))
            {
                return failuresElement.GetInt32();
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return 0;
    }

    private static List<string> GetCompletedCategories(IntakeRecord record)
    {
        var completed = new List<string>();

        if (!string.IsNullOrEmpty(record.ChiefComplaint))
            completed.Add("chiefComplaint");
        
        // Check if SymptomHistory contains actual symptom data (not just conversation history)
        if (!string.IsNullOrEmpty(record.SymptomHistory))
        {
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(record.SymptomHistory);
                if (data.TryGetProperty("symptoms", out var symptomsElement) && 
                    symptomsElement.ValueKind != JsonValueKind.Null &&
                    symptomsElement.GetArrayLength() > 0)
                {
                    completed.Add("symptoms");
                }
            }
            catch
            {
                // If parsing fails, don't mark as completed
            }
        }
        
        if (!string.IsNullOrEmpty(record.CurrentMedications))
        {
            var meds = TryDeserializeList(record.CurrentMedications);
            if (meds.Count > 0)
                completed.Add("medications");
        }
        
        if (!string.IsNullOrEmpty(record.KnownAllergies))
        {
            var allergies = TryDeserializeList(record.KnownAllergies);
            if (allergies.Count > 0)
                completed.Add("allergies");
        }
        
        if (!string.IsNullOrEmpty(record.MedicalHistory))
        {
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(record.MedicalHistory);
                
                // Check for medical history data
                if (data.TryGetProperty("medicalHistory", out var medHistElement) && 
                    medHistElement.ValueKind != JsonValueKind.Null)
                {
                    completed.Add("medicalHistory");
                }
                
                // Check for family history data
                if (data.TryGetProperty("familyHistory", out var famHistElement) && 
                    famHistElement.ValueKind != JsonValueKind.Null)
                {
                    completed.Add("familyHistory");
                }
                
                // Check for lifestyle data
                if (data.TryGetProperty("lifestyle", out var lifestyleElement) && 
                    lifestyleElement.ValueKind != JsonValueKind.Null)
                {
                    completed.Add("lifestyle");
                }
            }
            catch
            {
                // If parsing fails, don't mark as completed
            }
        }
        
        // Check insurance validation
        if (record.ValidatedInsuranceRecordId.HasValue)
            completed.Add("insurance");

        return completed;
    }

    private static string GetNextCategory(List<string> completedCategories)
    {
        foreach (var category in IntakeCategories)
        {
            if (!completedCategories.Contains(category))
                return category;
        }
        return "complete";
    }

    private static int CalculateProgress(IntakeRecord record)
    {
        var completed = GetCompletedCategories(record);
        var progress = (int)((double)completed.Count / IntakeCategories.Length * 100);
        
        // If we're at "complete" category, ensure we show 100%
        var symptomsData = DeserializeSymptomHistory(record);
        if (symptomsData.TryGetValue("currentCategory", out var category) && 
            category is string catStr && catStr == "complete")
        {
            return 100;
        }
        
        return progress;
    }

    private async Task UpdateIntakeDataFromExtraction(
        IntakeRecord record,
        AiExtractionResult extraction,
        CancellationToken cancellationToken)
    {
        foreach (var item in extraction.ExtractedData)
        {
            switch (item.Field.ToLowerInvariant())
            {
                case "chiefcomplaint":
                    record.ChiefComplaint = item.Value;
                    break;
                    
                case "symptom":
                case "symptoms":
                    // Store symptoms separately from conversation history
                    var symptomsData = DeserializeSymptomHistory(record);
                    if (!symptomsData.ContainsKey("symptoms"))
                        symptomsData["symptoms"] = new List<string>();
                    
                    var symptomsList = symptomsData["symptoms"] as List<string> ?? new List<string>();
                    symptomsList.Add(item.Value);
                    symptomsData["symptoms"] = symptomsList;
                    
                    // Preserve conversation history
                    record.SymptomHistory = JsonSerializer.Serialize(symptomsData);
                    break;
                    
                case "medication":
                case "medications":
                    var meds = TryDeserializeList(record.CurrentMedications);
                    if (!string.IsNullOrWhiteSpace(item.Value) && 
                        !item.Value.ToLower().Contains("none") &&
                        !item.Value.ToLower().Contains("no medication"))
                    {
                        meds.Add(item.Value);
                    }
                    record.CurrentMedications = JsonSerializer.Serialize(meds);
                    break;
                    
                case "allergy":
                case "allergies":
                    var allergies = TryDeserializeList(record.KnownAllergies);
                    if (!string.IsNullOrWhiteSpace(item.Value) && 
                        !item.Value.ToLower().Contains("none") &&
                        !item.Value.ToLower().Contains("no allerg"))
                    {
                        allergies.Add(item.Value);
                    }
                    record.KnownAllergies = JsonSerializer.Serialize(allergies);
                    break;
                    
                case "medicalhistory":
                    var medHistoryData = DeserializeMedicalHistory(record);
                    medHistoryData["medicalHistory"] = item.Value;
                    record.MedicalHistory = JsonSerializer.Serialize(medHistoryData);
                    break;
                    
                case "familyhistory":
                    var famHistoryData = DeserializeMedicalHistory(record);
                    famHistoryData["familyHistory"] = item.Value;
                    record.MedicalHistory = JsonSerializer.Serialize(famHistoryData);
                    break;
                    
                case "lifestyle":
                    var lifestyleData = DeserializeMedicalHistory(record);
                    lifestyleData["lifestyle"] = item.Value;
                    record.MedicalHistory = JsonSerializer.Serialize(lifestyleData);
                    break;
                    
                case "insurance":
                    // Insurance handling would require additional logic
                    // For now, just log it
                    _logger.LogInformation("Insurance information captured: {Value}", item.Value);
                    break;
            }
        }

        record.UpdatedAt = DateTime.UtcNow;
    }

    private static List<string> TryDeserializeList(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static IntakeSummaryDto DeserializeIntakeSummary(IntakeRecord record)
    {
        return new IntakeSummaryDto
        {
            ChiefComplaint = record.ChiefComplaint ?? string.Empty,
            Symptoms = TryDeserializeList(record.SymptomHistory),
            Medications = TryDeserialize<List<MedicationDto>>(record.CurrentMedications) ?? new List<MedicationDto>(),
            Allergies = TryDeserialize<List<AllergyDto>>(record.KnownAllergies) ?? new List<AllergyDto>(),
            MedicalHistory = TryDeserialize<List<MedicalHistoryItemDto>>(record.MedicalHistory) ?? new List<MedicalHistoryItemDto>(),
            FamilyHistory = new List<string>(),
            Lifestyle = null,
            AdditionalConcerns = null
        };
    }

    private static T? TryDeserialize<T>(string? json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }
    
    private static Dictionary<string, object> DeserializeSymptomHistory(IntakeRecord record)
    {
        if (string.IsNullOrEmpty(record.SymptomHistory))
            return new Dictionary<string, object>();

        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(record.SymptomHistory);
            var result = new Dictionary<string, object>();
            
            foreach (var property in jsonElement.EnumerateObject())
            {
                if (property.Name == "symptoms" && property.Value.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                            list.Add(item.GetString() ?? "");
                    }
                    result[property.Name] = list;
                }
                else if (property.Name == "conversationHistory")
                {
                    var convHistory = JsonSerializer.Deserialize<List<ConversationMessage>>(property.Value.GetRawText());
                    if (convHistory != null)
                        result[property.Name] = convHistory;
                }
                else if (property.Name == "consecutiveFailures")
                {
                    result[property.Name] = property.Value.GetInt32();
                }
                else if (property.Name == "currentCategory" && property.Value.ValueKind == JsonValueKind.String)
                {
                    result[property.Name] = property.Value.GetString() ?? "";
                }
            }
            
            return result;
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }
    
    private static Dictionary<string, object> DeserializeMedicalHistory(IntakeRecord record)
    {
        if (string.IsNullOrEmpty(record.MedicalHistory))
            return new Dictionary<string, object>();

        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(record.MedicalHistory);
            var result = new Dictionary<string, object>();
            
            foreach (var property in jsonElement.EnumerateObject())
            {
                result[property.Name] = property.Value.GetRawText();
            }
            
            return result;
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    #endregion
}
