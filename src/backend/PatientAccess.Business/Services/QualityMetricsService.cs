using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for calculating and tracking quality metrics for medical code mapping.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Implements AIR-Q01 (AI-Human Agreement Rate >98%) and AIR-Q03 (Schema Validity >99%) monitoring.
/// </summary>
public class QualityMetricsService : IQualityMetricsService
{
    private readonly ILogger<QualityMetricsService> _logger;
    private readonly PatientAccessDbContext _context;
    private readonly IAlertingService _alertingService;

    public QualityMetricsService(
        ILogger<QualityMetricsService> logger,
        PatientAccessDbContext context,
        IAlertingService alertingService)
    {
        _logger = logger;
        _context = context;
        _alertingService = alertingService;
    }

    /// <summary>
    /// Calculates AI-Human Agreement Rate for a given period (AIR-Q01: >98% target).
    /// Agreement = (StaffVerified TopSuggestions / TotalVerified TopSuggestions) * 100.
    /// </summary>
    public async Task<QualityMetricDto> CalculateAIHumanAgreementRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calculating AI-Human Agreement Rate for period {Start} to {End}",
            periodStart, periodEnd);

        // Get all verified top suggestions in period (only top suggestions count for agreement)
        var verifiedTopSuggestions = await _context.MedicalCodes
            .Where(mc => mc.VerifiedAt >= periodStart &&
                        mc.VerifiedAt < periodEnd &&
                        mc.IsTopSuggestion &&
                        (mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted ||
                         mc.VerificationStatus == MedicalCodeVerificationStatus.Rejected))
            .ToListAsync(cancellationToken);

        if (verifiedTopSuggestions.Count == 0)
        {
            _logger.LogWarning(
                "No verified top suggestions found for period {Start} to {End}",
                periodStart, periodEnd);

            return new QualityMetricDto
            {
                MetricType = "AIHumanAgreement",
                MetricValue = 0,
                SampleSize = 0,
                MeasurementPeriod = "Daily",
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Target = 98.0m,
                Status = "BelowTarget",
                Notes = "No verified codes found in period",
                CreatedAt = DateTime.UtcNow
            };
        }

        // Agreement = accepted top suggestions
        var acceptedCount = verifiedTopSuggestions.Count(mc =>
            mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted);

        var agreementRate = (decimal)acceptedCount / verifiedTopSuggestions.Count * 100;
        var status = agreementRate >= 98.0m ? "MeetsTarget" :
                     agreementRate >= 95.0m ? "AtRisk" : "BelowTarget";

        var metricDto = new QualityMetricDto
        {
            MetricType = "AIHumanAgreement",
            MetricValue = Math.Round(agreementRate, 2),
            SampleSize = verifiedTopSuggestions.Count,
            MeasurementPeriod = "Daily",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 98.0m,
            Status = status,
            Notes = $"Accepted: {acceptedCount}/{verifiedTopSuggestions.Count}",
            CreatedAt = DateTime.UtcNow
        };

        // Persist to database
        var metric = new QualityMetric
        {
            Id = Guid.NewGuid(),
            MetricType = "AIHumanAgreement",
            MetricValue = metricDto.MetricValue,
            SampleSize = verifiedTopSuggestions.Count,
            MeasurementPeriod = "Daily",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 98.0m,
            Status = status,
            Notes = metricDto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _context.QualityMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Send alert if below target
        if (status == "BelowTarget")
        {
            await _alertingService.SendQualityAlertAsync(
                $"⚠️ AI-Human Agreement Rate Below Target: {agreementRate:F2}% (Target: 98.0%)",
                $"Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}\n" +
                $"Agreement Rate: {agreementRate:F2}%\n" +
                $"Sample Size: {verifiedTopSuggestions.Count}\n" +
                $"Accepted: {acceptedCount}\n" +
                $"Rejected: {verifiedTopSuggestions.Count - acceptedCount}\n\n" +
                $"Action Required: Review rejected code suggestions and retrain model if pattern detected.",
                cancellationToken);
        }

        _logger.LogInformation(
            "AI-Human Agreement Rate: {Rate}% (Sample: {Count}, Status: {Status})",
            agreementRate, verifiedTopSuggestions.Count, status);

        return metricDto;
    }

    /// <summary>
    /// Calculates Schema Validity Rate for a given period (AIR-Q03: >99% target).
    /// Validity = (ValidResponses / TotalResponses) * 100.
    /// </summary>
    public async Task<QualityMetricDto> CalculateSchemaValidityRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calculating Schema Validity Rate for period {Start} to {End}",
            periodStart, periodEnd);

        // Query schema validation records tracked during code mapping
        var validationRecords = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "SchemaValidity" &&
                        qm.CreatedAt >= periodStart &&
                        qm.CreatedAt < periodEnd)
            .ToListAsync(cancellationToken);

        if (validationRecords.Count == 0)
        {
            _logger.LogWarning(
                "No schema validation records found for period {Start} to {End}",
                periodStart, periodEnd);

            return new QualityMetricDto
            {
                MetricType = "SchemaValidity",
                MetricValue = 100.0m, // Assume 100% if no records (no failures)
                SampleSize = 0,
                MeasurementPeriod = "Daily",
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Target = 99.0m,
                Status = "MeetsTarget",
                Notes = "No validation records found (no failures detected)",
                CreatedAt = DateTime.UtcNow
            };
        }

        // Calculate aggregate validity rate from individual validation records
        var totalValidations = validationRecords.Sum(vr => vr.SampleSize);
        var validCount = validationRecords
            .Where(vr => vr.Status == "MeetsTarget")
            .Sum(vr => vr.SampleSize);

        var validityRate = totalValidations > 0
            ? (decimal)validCount / totalValidations * 100
            : 100.0m;

        var status = validityRate >= 99.0m ? "MeetsTarget" :
                     validityRate >= 97.0m ? "AtRisk" : "BelowTarget";

        var metricDto = new QualityMetricDto
        {
            MetricType = "SchemaValidity",
            MetricValue = Math.Round(validityRate, 2),
            SampleSize = totalValidations,
            MeasurementPeriod = "Daily",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 99.0m,
            Status = status,
            Notes = $"Valid: {validCount}/{totalValidations}",
            CreatedAt = DateTime.UtcNow
        };

        // Persist aggregate metric
        var metric = new QualityMetric
        {
            Id = Guid.NewGuid(),
            MetricType = "SchemaValidityAggregate",
            MetricValue = metricDto.MetricValue,
            SampleSize = totalValidations,
            MeasurementPeriod = "Daily",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 99.0m,
            Status = status,
            Notes = metricDto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _context.QualityMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Send alert if below target
        if (status == "BelowTarget")
        {
            await _alertingService.SendQualityAlertAsync(
                $"⚠️ Schema Validity Rate Below Target: {validityRate:F2}% (Target: 99.0%)",
                $"Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}\n" +
                $"Validity Rate: {validityRate:F2}%\n" +
                $"Sample Size: {totalValidations}\n" +
                $"Valid: {validCount}\n" +
                $"Invalid: {totalValidations - validCount}\n\n" +
                $"Action Required: Review LLM response format and adjust prompt templates.",
                cancellationToken);
        }

        _logger.LogInformation(
            "Schema Validity Rate: {Rate}% (Sample: {Count}, Status: {Status})",
            validityRate, totalValidations, status);

        return metricDto;
    }

    /// <summary>
    /// Gets daily quality metrics summary with historical trend data.
    /// </summary>
    public async Task<QualityMetricsSummaryDto> GetDailySummaryAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var periodStart = date.Date;
        var periodEnd = periodStart.AddDays(1);

        // Get current day metrics
        var agreementMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "AIHumanAgreement" &&
                        qm.PeriodStart >= periodStart &&
                        qm.PeriodStart < periodEnd)
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var schemaMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "SchemaValidityAggregate" &&
                        qm.PeriodStart >= periodStart &&
                        qm.PeriodStart < periodEnd)
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Get last 7 days history
        var sevenDaysAgo = date.AddDays(-7);
        var last7Days = await _context.QualityMetrics
            .Where(qm => (qm.MetricType == "AIHumanAgreement" || qm.MetricType == "SchemaValidityAggregate") &&
                        qm.PeriodStart >= sevenDaysAgo &&
                        qm.PeriodStart < periodEnd)
            .OrderByDescending(qm => qm.PeriodStart)
            .Take(14) // 7 days * 2 metric types
            .ToListAsync(cancellationToken);

        // Calculate rolling averages
        var sevenDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 7, cancellationToken);
        var thirtyDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 30, cancellationToken);

        // Determine overall status
        var overallStatus = "Healthy";
        if (agreementMetric?.Status == "BelowTarget" || schemaMetric?.Status == "BelowTarget")
        {
            overallStatus = "Critical";
        }
        else if (agreementMetric?.Status == "AtRisk" || schemaMetric?.Status == "AtRisk")
        {
            overallStatus = "AtRisk";
        }

        return new QualityMetricsSummaryDto
        {
            AgreementRate = agreementMetric != null ? MapToDto(agreementMetric) : null,
            SchemaValidity = schemaMetric != null ? MapToDto(schemaMetric) : null,
            Last7Days = last7Days.Select(MapToDto).ToList(),
            SevenDayRollingAverage = sevenDayAvg,
            ThirtyDayRollingAverage = thirtyDayAvg,
            OverallStatus = overallStatus
        };
    }

    /// <summary>
    /// Gets weekly quality metrics summary.
    /// </summary>
    public async Task<QualityMetricsSummaryDto> GetWeeklySummaryAsync(
        DateTime weekStart,
        CancellationToken cancellationToken = default)
    {
        var periodStart = weekStart.Date;
        var periodEnd = periodStart.AddDays(7);

        // Get weekly aggregate metrics
        var agreementMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "AIHumanAgreement" &&
                        qm.MeasurementPeriod == "Weekly" &&
                        qm.PeriodStart >= periodStart &&
                        qm.PeriodStart < periodEnd)
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var schemaMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "SchemaValidityAggregate" &&
                        qm.MeasurementPeriod == "Weekly" &&
                        qm.PeriodStart >= periodStart &&
                        qm.PeriodStart < periodEnd)
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Calculate rolling averages
        var sevenDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 7, cancellationToken);
        var thirtyDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 30, cancellationToken);

        // Determine overall status
        var overallStatus = "Healthy";
        if (agreementMetric?.Status == "BelowTarget" || schemaMetric?.Status == "BelowTarget")
        {
            overallStatus = "Critical";
        }
        else if (agreementMetric?.Status == "AtRisk" || schemaMetric?.Status == "AtRisk")
        {
            overallStatus = "AtRisk";
        }

        return new QualityMetricsSummaryDto
        {
            AgreementRate = agreementMetric != null ? MapToDto(agreementMetric) : null,
            SchemaValidity = schemaMetric != null ? MapToDto(schemaMetric) : null,
            Last7Days = new List<QualityMetricDto>(), // Weekly summary doesn't need daily breakdown
            SevenDayRollingAverage = sevenDayAvg,
            ThirtyDayRollingAverage = thirtyDayAvg,
            OverallStatus = overallStatus
        };
    }

    /// <summary>
    /// Gets metric history for specified number of days.
    /// </summary>
    public async Task<List<QualityMetricDto>> GetMetricHistoryAsync(
        string metricType,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var metrics = await _context.QualityMetrics
            .Where(qm => qm.MetricType == metricType &&
                        qm.PeriodStart >= startDate)
            .OrderByDescending(qm => qm.PeriodStart)
            .Take(days)
            .ToListAsync(cancellationToken);

        return metrics.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Calculates rolling average for metric over specified number of days.
    /// </summary>
    public async Task<decimal> CalculateRollingAverageAsync(
        string metricType,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var metrics = await _context.QualityMetrics
            .Where(qm => qm.MetricType == metricType &&
                        qm.PeriodStart >= startDate)
            .OrderByDescending(qm => qm.PeriodStart)
            .Take(days)
            .ToListAsync(cancellationToken);

        if (!metrics.Any())
        {
            _logger.LogWarning("No metrics found for rolling average calculation: {MetricType}, {Days} days",
                metricType, days);
            return 0;
        }

        var average = metrics.Average(m => m.MetricValue);
        return Math.Round(average, 2);
    }

    /// <summary>
    /// Maps QualityMetric entity to QualityMetricDto.
    /// </summary>
    private QualityMetricDto MapToDto(QualityMetric metric)
    {
        return new QualityMetricDto
        {
            MetricType = metric.MetricType,
            MetricValue = metric.MetricValue,
            SampleSize = metric.SampleSize,
            MeasurementPeriod = metric.MeasurementPeriod,
            PeriodStart = metric.PeriodStart,
            PeriodEnd = metric.PeriodEnd,
            Target = metric.Target,
            Status = metric.Status,
            Notes = metric.Notes,
            CreatedAt = metric.CreatedAt
        };
    }
}
