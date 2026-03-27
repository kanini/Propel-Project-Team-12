using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for quality metrics calculation and tracking (AIR-Q01, AIR-Q03).
/// Monitors AI-Human Agreement Rate and output schema validity.
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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _alertingService = alertingService ?? throw new ArgumentNullException(nameof(alertingService));
    }

    /// <inheritdoc />
    public async Task<QualityMetricDto> CalculateAIHumanAgreementRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating AI-Human Agreement Rate for period {Start} to {End}",
            periodStart, periodEnd);

        // Get all verified medical codes in period (StaffVerified or StaffRejected)
        var verifiedCodes = await _context.MedicalCodes
            .Where(mc => mc.VerifiedAt >= periodStart &&
                        mc.VerifiedAt < periodEnd &&
                        (mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted ||
                         mc.VerificationStatus == MedicalCodeVerificationStatus.Rejected))
            .ToListAsync(cancellationToken);

        if (verifiedCodes.Count == 0)
        {
            _logger.LogWarning("No verified codes found for period {Start} to {End}", periodStart, periodEnd);
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
                CreatedAt = DateTime.UtcNow
            };
        }

        // Agreement = StaffVerified (Accepted) top suggestions / Total verified
        var agreementCount = verifiedCodes.Count(mc =>
            mc.VerificationStatus == MedicalCodeVerificationStatus.Accepted && mc.IsTopSuggestion);

        var agreementRate = (decimal)agreementCount / verifiedCodes.Count * 100;
        var status = agreementRate >= 98.0m ? "MeetsTarget" : (agreementRate > 100.0m ? "ExceedsTarget" : "BelowTarget");

        var metricDto = new QualityMetricDto
        {
            MetricType = "AIHumanAgreement",
            MetricValue = agreementRate,
            SampleSize = verifiedCodes.Count,
            MeasurementPeriod = "Daily",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 98.0m,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        // Persist to database
        var metric = new QualityMetric
        {
            QualityMetricId = Guid.NewGuid(),
            MetricType = "AIHumanAgreement",
            MetricValue = agreementRate,
            SampleSize = verifiedCodes.Count,
            MeasurementPeriod = "Daily",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 98.0m,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _context.QualityMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AI-Human Agreement Rate: {Rate:F2}% (Sample: {Count}, Status: {Status})",
            agreementRate, verifiedCodes.Count, status);

        // Alert if below target
        if (status == "BelowTarget")
        {
            await _alertingService.SendQualityAlertAsync(
                $"AI-Human Agreement Rate Below Target: {agreementRate:F2}% (Target: 98.0%)",
                $"Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}\n" +
                $"Sample Size: {verifiedCodes.Count}\n" +
                $"Agreed: {agreementCount}\n" +
                $"Rate: {agreementRate:F2}%",
                cancellationToken);
        }

        return metricDto;
    }

    /// <inheritdoc />
    public async Task<QualityMetricDto> CalculateSchemaValidityRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating Schema Validity Rate for period {Start} to {End}",
            periodStart, periodEnd);

        // Query daily schema validation metrics (tracked by CodeMappingService in task_001)
        var dailyValidationMetrics = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "SchemaValidity" &&
                        qm.PeriodStart >= periodStart &&
                        qm.PeriodStart < periodEnd &&
                        qm.MeasurementPeriod == "Daily")
            .ToListAsync(cancellationToken);

        if (dailyValidationMetrics.Count == 0)
        {
            _logger.LogWarning("No schema validation metrics found for period {Start} to {End}", periodStart, periodEnd);
            return new QualityMetricDto
            {
                MetricType = "SchemaValidity",
                MetricValue = 100.0m, // Default to 100% if no data (optimistic)
                SampleSize = 0,
                MeasurementPeriod = "Daily",
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Target = 99.0m,
                Status = "MeetsTarget",
                CreatedAt = DateTime.UtcNow
            };
        }

        // Calculate aggregate validity rate from daily metrics
        // Each daily metric already contains: (valid samples / total samples) * 100
        // We need weighted average: sum(metricValue * sampleSize) / sum(sampleSize)
        var totalWeightedValue = dailyValidationMetrics.Sum(m => m.MetricValue * m.SampleSize);
        var totalSamples = dailyValidationMetrics.Sum(m => m.SampleSize);

        var validityRate = totalSamples > 0 ? totalWeightedValue / totalSamples : 100.0m;
        var status = validityRate >= 99.0m ? "MeetsTarget" : "BelowTarget";

        var metricDto = new QualityMetricDto
        {
            MetricType = "SchemaValidity",
            MetricValue = validityRate,
            SampleSize = totalSamples,
            MeasurementPeriod = "Daily",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 99.0m,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        // Persist aggregate metric
        var metric = new QualityMetric
        {
            QualityMetricId = Guid.NewGuid(),
            MetricType = "SchemaValidity",
            MetricValue = validityRate,
            SampleSize = totalSamples,
            MeasurementPeriod = "Aggregate",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Target = 99.0m,
            Status = status,
            Notes = $"Aggregated from {dailyValidationMetrics.Count} daily metrics",
            CreatedAt = DateTime.UtcNow
        };

        await _context.QualityMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Schema Validity Rate: {Rate:F2}% (Sample: {Count}, Status: {Status})",
            validityRate, totalSamples, status);

        // Alert if below target
        if (status == "BelowTarget")
        {
            await _alertingService.SendQualityAlertAsync(
                $"Schema Validity Rate Below Target: {validityRate:F2}% (Target: 99.0%)",
                $"Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}\n" +
                $"Sample Size: {totalSamples}\n" +
                $"Rate: {validityRate:F2}%",
                cancellationToken);
        }

        return metricDto;
    }

    /// <inheritdoc />
    public async Task<QualityMetricsSummaryDto> GetDailySummaryAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving daily quality metrics summary for {Date}", date.Date);

        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        // Get current day metrics
        var agreementMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "AIHumanAgreement" &&
                        qm.PeriodStart >= dayStart &&
                        qm.PeriodStart < dayEnd)
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var schemaMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "SchemaValidity" &&
                        qm.PeriodStart >= dayStart &&
                        qm.PeriodStart < dayEnd)
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Get last 7 days for trend
        var sevenDaysAgo = dayStart.AddDays(-7);
        var last7Days = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "AIHumanAgreement" &&
                        qm.PeriodStart >= sevenDaysAgo &&
                        qm.PeriodStart < dayEnd)
            .OrderByDescending(qm => qm.PeriodStart)
            .Take(7)
            .Select(qm => new QualityMetricDto
            {
                MetricType = qm.MetricType,
                MetricValue = qm.MetricValue,
                SampleSize = qm.SampleSize,
                MeasurementPeriod = qm.MeasurementPeriod,
                PeriodStart = qm.PeriodStart,
                PeriodEnd = qm.PeriodEnd,
                Target = qm.Target,
                Status = qm.Status,
                CreatedAt = qm.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Calculate rolling averages
        var sevenDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 7, cancellationToken);
        var thirtyDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 30, cancellationToken);

        return new QualityMetricsSummaryDto
        {
            AgreementRate = agreementMetric != null ? new QualityMetricDto
            {
                MetricType = agreementMetric.MetricType,
                MetricValue = agreementMetric.MetricValue,
                SampleSize = agreementMetric.SampleSize,
                MeasurementPeriod = agreementMetric.MeasurementPeriod,
                PeriodStart = agreementMetric.PeriodStart,
                PeriodEnd = agreementMetric.PeriodEnd,
                Target = agreementMetric.Target,
                Status = agreementMetric.Status,
                CreatedAt = agreementMetric.CreatedAt
            } : null,
            SchemaValidity = schemaMetric != null ? new QualityMetricDto
            {
                MetricType = schemaMetric.MetricType,
                MetricValue = schemaMetric.MetricValue,
                SampleSize = schemaMetric.SampleSize,
                MeasurementPeriod = schemaMetric.MeasurementPeriod,
                PeriodStart = schemaMetric.PeriodStart,
                PeriodEnd = schemaMetric.PeriodEnd,
                Target = schemaMetric.Target,
                Status = schemaMetric.Status,
                CreatedAt = schemaMetric.CreatedAt
            } : null,
            Last7Days = last7Days,
            SevenDayRollingAverage = sevenDayAvg,
            ThirtyDayRollingAverage = thirtyDayAvg
        };
    }

    /// <inheritdoc />
    public async Task<QualityMetricsSummaryDto> GetWeeklySummaryAsync(
        DateTime weekStart,
        CancellationToken cancellationToken = default)
    {
        // For weekly summary, use the week start date as the reference
        var weekEnd = weekStart.AddDays(7);

        _logger.LogInformation("Retrieving weekly quality metrics summary for week starting {WeekStart}", weekStart);

        var agreementMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "AIHumanAgreement" &&
                        qm.PeriodStart >= weekStart &&
                        qm.PeriodStart < weekEnd &&
                        qm.MeasurementPeriod == "Weekly")
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var schemaMetric = await _context.QualityMetrics
            .Where(qm => qm.MetricType == "SchemaValidity" &&
                        qm.PeriodStart >= weekStart &&
                        qm.PeriodStart < weekEnd &&
                        qm.MeasurementPeriod == "Weekly")
            .OrderByDescending(qm => qm.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var sevenDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 7, cancellationToken);
        var thirtyDayAvg = await CalculateRollingAverageAsync("AIHumanAgreement", 30, cancellationToken);

        return new QualityMetricsSummaryDto
        {
            AgreementRate = agreementMetric != null ? MapToDto(agreementMetric) : null,
            SchemaValidity = schemaMetric != null ? MapToDto(schemaMetric) : null,
            Last7Days = new List<QualityMetricDto>(),
            SevenDayRollingAverage = sevenDayAvg,
            ThirtyDayRollingAverage = thirtyDayAvg
        };
    }

    /// <inheritdoc />
    public async Task<List<QualityMetricDto>> GetMetricHistoryAsync(
        string metricType,
        int days,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving {MetricType} history for last {Days} days", metricType, days);

        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var metrics = await _context.QualityMetrics
            .Where(qm => qm.MetricType == metricType &&
                        qm.PeriodStart >= startDate)
            .OrderByDescending(qm => qm.PeriodStart)
            .Select(qm => new QualityMetricDto
            {
                MetricType = qm.MetricType,
                MetricValue = qm.MetricValue,
                SampleSize = qm.SampleSize,
                MeasurementPeriod = qm.MeasurementPeriod,
                PeriodStart = qm.PeriodStart,
                PeriodEnd = qm.PeriodEnd,
                Target = qm.Target,
                Status = qm.Status,
                CreatedAt = qm.CreatedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} {MetricType} metrics", metrics.Count, metricType);

        return metrics;
    }

    /// <inheritdoc />
    public async Task<decimal> CalculateRollingAverageAsync(
        string metricType,
        int days,
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
            _logger.LogWarning("No metrics found for {MetricType} in last {Days} days", metricType, days);
            return 0;
        }

        var average = metrics.Average(m => m.MetricValue);

        _logger.LogDebug("{Days}-day rolling average for {MetricType}: {Average:F2}%",
            days, metricType, average);

        return average;
    }

    /// <summary>
    /// Helper method to map QualityMetric entity to DTO.
    /// </summary>
    private static QualityMetricDto MapToDto(QualityMetric metric)
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
            CreatedAt = metric.CreatedAt
        };
    }
}
