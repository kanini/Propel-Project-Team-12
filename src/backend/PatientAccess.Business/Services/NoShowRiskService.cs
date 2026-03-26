using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;

namespace PatientAccess.Business.Services;

/// <summary>
/// US_038 AC-1: Rule-based no-show risk scoring engine (TR-020).
/// Calculates deterministic risk score (0-100) using three configurable weighted factors:
/// appointment lead time, previous no-show history, and confirmation response rate.
/// Performance: Single indexed DB query + in-memory arithmetic &lt; 100ms (NFR-016).
/// </summary>
public class NoShowRiskService : INoShowRiskService
{
    private readonly PatientAccessDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NoShowRiskService> _logger;

    // Default weights (sum to 1.0)
    private const decimal DefaultLeadTimeWeight = 0.30m;
    private const decimal DefaultHistoryWeight = 0.45m;
    private const decimal DefaultConfirmationWeight = 0.25m;

    public NoShowRiskService(
        PatientAccessDbContext context,
        IConfiguration configuration,
        ILogger<NoShowRiskService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Calculate no-show risk score for a patient's appointment.
    /// </summary>
    public async Task<NoShowRiskScoreDto> CalculateRiskScoreAsync(
        Guid patientId,
        DateTime scheduledDateTime,
        bool isWalkIn)
    {
        // EC-2: Walk-in appointments receive fixed low-risk score (0) since already on-site
        if (isWalkIn)
        {
            _logger.LogDebug("Walk-in appointment for patient {PatientId}: returning fixed low-risk score (0)", patientId);
            return new NoShowRiskScoreDto
            {
                Score = 0,
                RiskLevel = "Low",
                LeadTimeFactor = 0,
                HistoryFactor = 0,
                ConfirmationFactor = 0
            };
        }

        // Load configurable weights from SystemSettings or appsettings.json
        var weights = await LoadWeightsAsync();

        // Calculate lead time factor
        var leadTimeFactor = CalculateLeadTimeFactor(scheduledDateTime);

        // Calculate no-show history and confirmation factors from NoShowHistory table (single indexed query)
        var (historyFactor, confirmationFactor) = await CalculateHistoryFactorsAsync(patientId);

        // Calculate final weighted score
        var score = (weights.LeadTime * leadTimeFactor) +
                    (weights.History * historyFactor) +
                    (weights.Confirmation * confirmationFactor);

        // Clamp to 0-100 range
        score = Math.Max(0, Math.Min(100, score));

        // Derive risk level classification
        var riskLevel = score < 40 ? "Low" : score <= 70 ? "Medium" : "High";

        _logger.LogDebug(
            "Risk score calculated for patient {PatientId}: Score={Score:F2}, RiskLevel={RiskLevel}, " +
            "LeadTime={LeadTimeFactor:F2}, History={HistoryFactor:F2}, Confirmation={ConfirmationFactor:F2}",
            patientId, score, riskLevel, leadTimeFactor, historyFactor, confirmationFactor);

        return new NoShowRiskScoreDto
        {
            Score = score,
            RiskLevel = riskLevel,
            LeadTimeFactor = leadTimeFactor,
            HistoryFactor = historyFactor,
            ConfirmationFactor = confirmationFactor
        };
    }

    /// <summary>
    /// Load configurable weights from SystemSettings (preferred) or appsettings.json (fallback).
    /// Normalizes weights to sum to 1.0 if necessary.
    /// </summary>
    private async Task<(decimal LeadTime, decimal History, decimal Confirmation)> LoadWeightsAsync()
    {
        // Try to read from SystemSettings first (shared key-value config store from US_037)
        decimal leadTimeWeight = DefaultLeadTimeWeight;
        decimal historyWeight = DefaultHistoryWeight;
        decimal confirmationWeight = DefaultConfirmationWeight;

        try
        {
            var leadTimeSetting = await _context.SystemSettings
                .Where(s => s.Key == "Risk.LeadTimeWeight")
                .FirstOrDefaultAsync();
            var historySetting = await _context.SystemSettings
                .Where(s => s.Key == "Risk.HistoryWeight")
                .FirstOrDefaultAsync();
            var confirmationSetting = await _context.SystemSettings
                .Where(s => s.Key == "Risk.ConfirmationWeight")
                .FirstOrDefaultAsync();

            if (leadTimeSetting != null && decimal.TryParse(leadTimeSetting.Value, out var ltw))
                leadTimeWeight = ltw;
            if (historySetting != null && decimal.TryParse(historySetting.Value, out var hw))
                historyWeight = hw;
            if (confirmationSetting != null && decimal.TryParse(confirmationSetting.Value, out var cw))
                confirmationWeight = cw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read risk weights from SystemSettings, using fallback from appsettings.json");
        }

        // Fallback to appsettings.json if SystemSettings not populated
        if (leadTimeWeight == DefaultLeadTimeWeight && historyWeight == DefaultHistoryWeight && confirmationWeight == DefaultConfirmationWeight)
        {
            if (decimal.TryParse(_configuration["RiskScoringWeights:LeadTime"], out var configLtw))
                leadTimeWeight = configLtw;
            if (decimal.TryParse(_configuration["RiskScoringWeights:History"], out var configHw))
                historyWeight = configHw;
            if (decimal.TryParse(_configuration["RiskScoringWeights:Confirmation"], out var configCw))
                confirmationWeight = configCw;
        }

        // Normalize weights to sum to 1.0
        var sum = leadTimeWeight + historyWeight + confirmationWeight;
        if (sum != 1.0m && sum > 0)
        {
            _logger.LogWarning(
                "Risk scoring weights do not sum to 1.0 (sum={Sum}). Normalizing weights.",
                sum);
            leadTimeWeight /= sum;
            historyWeight /= sum;
            confirmationWeight /= sum;
        }

        return (leadTimeWeight, historyWeight, confirmationWeight);
    }

    /// <summary>
    /// Calculate lead time factor (0-100) based on hours until appointment.
    /// Imminent appointments have lower forgetting risk; far-out appointments have higher risk.
    /// </summary>
    private decimal CalculateLeadTimeFactor(DateTime scheduledDateTime)
    {
        var leadTimeHours = (scheduledDateTime - DateTime.UtcNow).TotalHours;

        // Map lead time hours to risk factor
        return leadTimeHours switch
        {
            < 2 => 20m,        // Imminent, low forgetting risk
            < 24 => 30m,       // Same day
            < 72 => 50m,       // 1-3 days
            < 168 => 60m,      // 3-7 days (1 week)
            _ => 80m           // >1 week, high forgetting risk
        };
    }

    /// <summary>
    /// Calculate no-show history and confirmation response factors from NoShowHistory table.
    /// EC-1: First-time patients with no history return neutral factors (50).
    /// Uses single indexed query on PatientId (unique index from NoShowHistoryConfiguration).
    /// </summary>
    private async Task<(decimal HistoryFactor, decimal ConfirmationFactor)> CalculateHistoryFactorsAsync(Guid patientId)
    {
        // Single indexed query (O(1) lookup on unique PatientId)
        var history = await _context.NoShowHistory
            .Where(h => h.PatientId == patientId)
            .FirstOrDefaultAsync();

        // EC-1: First-time patient with no history — return neutral factors
        if (history == null)
        {
            _logger.LogDebug("No history found for patient {PatientId}: returning neutral factors (50)", patientId);
            return (50m, 50m);
        }

        // Calculate no-show rate
        decimal historyFactor;
        if (history.TotalAppointments == 0 || history.NoShowCount == 0)
        {
            historyFactor = 20m; // No no-shows, low risk
        }
        else
        {
            var noShowRate = (decimal)history.NoShowCount / history.TotalAppointments;
            historyFactor = noShowRate switch
            {
                < 0.10m => 20m,      // <10%
                < 0.25m => 40m,      // 10-25%
                < 0.50m => 60m,      // 25-50%
                _ => 90m             // >50%, high risk
            };
        }

        // Calculate confirmation response factor
        decimal confirmationFactor;
        if (history.ConfirmationResponseRate == null)
        {
            // EC-1: No confirmation data yet — neutral factor
            confirmationFactor = 50m;
        }
        else
        {
            var responseRate = history.ConfirmationResponseRate.Value;
            confirmationFactor = responseRate switch
            {
                > 0.90m => 15m,      // >90%, very responsive, low risk
                > 0.70m => 35m,      // 70-90%
                > 0.50m => 55m,      // 50-70%
                _ => 75m             // <50%, unresponsive, high risk
            };
        }

        return (historyFactor, confirmationFactor);
    }
}
