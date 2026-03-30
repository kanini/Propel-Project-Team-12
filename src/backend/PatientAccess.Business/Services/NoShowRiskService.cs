using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;

namespace PatientAccess.Business.Services;

/// <summary>
/// Rule-based no-show risk scoring service (US_038 - FR-023, TR-020).
/// Calculates deterministic risk scores using three configurable weighted factors:
/// appointment lead time, previous no-show history, and confirmation response rate.
/// Meets 100ms performance requirement (NFR-016) via single indexed DB query.
/// </summary>
public class NoShowRiskService : INoShowRiskService
{
    private readonly PatientAccessDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NoShowRiskService> _logger;

    // Default weights (must sum to 1.0)
    private const decimal DefaultLeadTimeWeight = 0.30m;
    private const decimal DefaultHistoryWeight = 0.45m;
    private const decimal DefaultConfirmationWeight = 0.25m;

    public NoShowRiskService(
        PatientAccessDbContext context,
        IConfiguration configuration,
        ILogger<NoShowRiskService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates no-show risk score for an appointment.
    /// Walk-in appointments return fixed score of 0 (EC-2).
    /// First-time patients use neutral history/confirmation factors (EC-1).
    /// </summary>
    public async Task<NoShowRiskScoreDto> CalculateRiskScoreAsync(
        Guid patientId,
        DateTime scheduledDateTime,
        bool isWalkIn)
    {
        try
        {
            _logger.LogDebug(
                "Calculating no-show risk score for PatientId {PatientId}, ScheduledDateTime {ScheduledDateTime}, IsWalkIn {IsWalkIn}",
                patientId, scheduledDateTime, isWalkIn);

            // EC-2: Walk-in appointments receive fixed low-risk score (already on-site)
            if (isWalkIn)
            {
                _logger.LogDebug("Walk-in appointment detected, returning fixed score of 0");
                return new NoShowRiskScoreDto
                {
                    Score = 0,
                    RiskLevel = "Low",
                    LeadTimeFactor = 0,
                    HistoryFactor = 0,
                    ConfirmationFactor = 0
                };
            }

            // Load configurable weights
            var weights = await LoadWeightsAsync();

            // Calculate individual factors
            var leadTimeFactor = CalculateLeadTimeFactor(scheduledDateTime);
            var (historyFactor, confirmationFactor) = await CalculateHistoryFactorsAsync(patientId);

            // Calculate weighted final score
            var finalScore = (weights.LeadTimeWeight * leadTimeFactor)
                           + (weights.HistoryWeight * historyFactor)
                           + (weights.ConfirmationWeight * confirmationFactor);

            // Clamp to 0-100 range
            finalScore = Math.Max(0, Math.Min(100, finalScore));

            // Derive risk level
            var riskLevel = DeriveRiskLevel(finalScore);

            _logger.LogDebug(
                "Risk score calculated: Score={Score}, RiskLevel={RiskLevel}, LeadTime={LeadTime}, History={History}, Confirmation={Confirmation}",
                finalScore, riskLevel, leadTimeFactor, historyFactor, confirmationFactor);

            return new NoShowRiskScoreDto
            {
                Score = finalScore,
                RiskLevel = riskLevel,
                LeadTimeFactor = leadTimeFactor,
                HistoryFactor = historyFactor,
                ConfirmationFactor = confirmationFactor
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating risk score for PatientId {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Loads configurable weights from SystemSettings with appsettings.json fallback.
    /// Validates weights sum to 1.0 and normalizes if necessary (TR-020).
    /// </summary>
    private async Task<(decimal LeadTimeWeight, decimal HistoryWeight, decimal ConfirmationWeight)> LoadWeightsAsync()
    {
        try
        {
            // Try to load from SystemSettings (shared key-value config store from US_037)
            var leadTimeWeight = await GetSystemSettingAsync("Risk.LeadTimeWeight", DefaultLeadTimeWeight);
            var historyWeight = await GetSystemSettingAsync("Risk.HistoryWeight", DefaultHistoryWeight);
            var confirmationWeight = await GetSystemSettingAsync("Risk.ConfirmationWeight", DefaultConfirmationWeight);

            // Validate and normalize weights
            var sum = leadTimeWeight + historyWeight + confirmationWeight;
            if (Math.Abs(sum - 1.0m) > 0.01m)
            {
                _logger.LogWarning(
                    "Risk scoring weights do not sum to 1.0 (actual: {Sum}), normalizing",
                    sum);

                leadTimeWeight /= sum;
                historyWeight /= sum;
                confirmationWeight /= sum;
            }

            return (leadTimeWeight, historyWeight, confirmationWeight);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading weights from SystemSettings, using defaults");
            return (DefaultLeadTimeWeight, DefaultHistoryWeight, DefaultConfirmationWeight);
        }
    }

    /// <summary>
    /// Retrieves a system setting value from database or appsettings fallback.
    /// </summary>
    private async Task<decimal> GetSystemSettingAsync(string key, decimal defaultValue)
    {
        try
        {
            // Try database first (SystemSettings table from US_037)
            var setting = await _context.SystemSettings
                .AsNoTracking()
                .Where(s => s.Key == key)
                .Select(s => s.Value)
                .FirstOrDefaultAsync();

            if (setting != null && decimal.TryParse(setting, out var dbValue))
            {
                return dbValue;
            }

            // Fallback to appsettings.json
            var configKey = $"RiskScoringWeights:{key.Split('.')[1]}";
            var configValue = _configuration[configKey];
            if (configValue != null && decimal.TryParse(configValue, out var configVal))
            {
                return configVal;
            }

            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Calculates lead time factor based on hours until scheduled appointment.
    /// Mapping: &lt;2h:20, 2-24h:30, 24-72h:50, 72-168h:60, &gt;168h:80
    /// </summary>
    private decimal CalculateLeadTimeFactor(DateTime scheduledDateTime)
    {
        var leadTimeHours = (scheduledDateTime - DateTime.UtcNow).TotalHours;

        return leadTimeHours switch
        {
            < 2 => 20m,        // Imminent, low forgetting risk
            < 24 => 30m,       // Same day
            < 72 => 50m,       // 1-3 days
            < 168 => 60m,      // 3-7 days (1 week)
            _ => 80m           // > 1 week, high forgetting risk
        };
    }

    /// <summary>
    /// Calculates no-show history and confirmation response factors from NoShowHistory table.
    /// Returns (historyFactor, confirmationFactor).
    /// First-time patients without history return neutral values of 50 (EC-1).
    /// </summary>
    private async Task<(decimal HistoryFactor, decimal ConfirmationFactor)> CalculateHistoryFactorsAsync(Guid patientId)
    {
        // Single indexed DB query (unique index on PatientId, O(1) lookup)
        var history = await _context.NoShowHistory
            .AsNoTracking()
            .Where(h => h.PatientId == patientId)
            .Select(h => new
            {
                h.TotalAppointments,
                h.NoShowCount,
                h.ConfirmationResponseRate
            })
            .FirstOrDefaultAsync();

        // EC-1: First-time patient with no history - return neutral factors
        if (history == null || history.TotalAppointments == 0)
        {
            _logger.LogDebug("No history found for PatientId {PatientId}, using neutral factors", patientId);
            return (50m, 50m);
        }

        // Calculate no-show rate
        var noShowRate = history.TotalAppointments > 0
            ? (decimal)history.NoShowCount / history.TotalAppointments
            : 0m;

        // Map no-show rate to factor (0% = low risk, >50% = high risk)
        var historyFactor = noShowRate switch
        {
            0m => 20m,              // No no-shows
            < 0.10m => 20m,         // < 10%
            < 0.25m => 40m,         // 10-25%
            < 0.50m => 60m,         // 25-50%
            _ => 90m                // > 50%
        };

        // Map confirmation response rate to factor
        var confirmationFactor = history.ConfirmationResponseRate switch
        {
            null => 50m,            // No confirmation data, neutral
            > 90m => 15m,           // Very responsive, low risk
            >= 70m => 35m,          // 70-90%
            >= 50m => 55m,          // 50-70%
            _ => 75m                // < 50%, unresponsive, high risk
        };

        return (historyFactor, confirmationFactor);
    }

    /// <summary>
    /// Derives risk level from final score.
    /// Low: &lt; 40, Medium: 40-70, High: &gt; 70
    /// </summary>
    private static string DeriveRiskLevel(decimal score)
    {
        return score switch
        {
            < 40m => "Low",
            <= 70m => "Medium",
            _ => "High"
        };
    }
}
