# Task - task_003_be_quality_metrics_tracking

## Requirement Reference
- User Story: US_051
- Story Location: .propel/context/tasks/EP-008/us_051/us_051.md
- Acceptance Criteria:
    - **AC4**: Given quality requirements, When the mapping is evaluated against staff decisions, Then AI-Human Agreement Rate exceeds 98% (AIR-Q01) and output schema validity exceeds 99% (AIR-Q03).
- Edge Case:
    - N/A (Quality metrics tracking is deterministic)

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Library | Hangfire | 1.8.x |
| Database | PostgreSQL | 16.x |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Create quality metrics tracking infrastructure to monitor AI-Human Agreement Rate (AIR-Q01: >98%) and output schema validity (AIR-Q03: >99%) for medical code mapping. This task implements Hangfire background jobs for daily/weekly quality metric calculation, QualityMetricsService for metric computation logic, REST API endpoints for metrics dashboard, alerting mechanism (email notifications when metrics fall below thresholds), and Application Insights integration for trend analysis. The system tracks staff verification decisions (StaffVerified vs StaffRejected) to calculate agreement rate, tracks schema validation failures, and generates quality reports for compliance audits.

**Key Capabilities:**
- QualityMetricsService with agreement rate and schema validity calculation
- Hangfire background jobs: DailyQualityMetricsJob, WeeklyQualityMetricsJob
- Metric calculation logic: (StaffVerified / Total Verified) * 100 for agreement rate
- Schema validity tracking: (Valid Responses / Total Responses) * 100
- QualityMetricsController with GET endpoints for dashboard
- Alerting service: email notifications when metrics <98% (agreement) or <99% (validity)
- Application Insights custom metrics: "AIHumanAgreementRate", "SchemaValidityRate"
- Quality report generation: CSV export for compliance audits
- Trend analysis: 7-day rolling average, 30-day rolling average

## Dependent Tasks
- EP-008: US_051: task_001_be_code_mapping_service (MedicalCode entity, QualityMetric entity, CalculateAgreementRateAsync method)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/QualityMetricsService.cs` - Core metrics calculation logic
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IQualityMetricsService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/DailyQualityMetricsJob.cs` - Daily metric calculation job
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/WeeklyQualityMetricsJob.cs` - Weekly metric calculation job
- **NEW**: `src/backend/PatientAccess.Business/Services/AlertingService.cs` - Email alerting service
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IAlertingService.cs` - Alerting interface
- **NEW**: `src/backend/PatientAccess.Business/DTOs/QualityMetricDto.cs` - Quality metric DTO
- **NEW**: `src/backend/PatientAccess.Web/Controllers/QualityMetricsController.cs` - REST API controller
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register IQualityMetricsService, configure Hangfire jobs

## Implementation Plan

1. **Create QualityMetricDto**
   - File: `src/backend/PatientAccess.Business/DTOs/QualityMetricDto.cs`
   - Properties:
     ```csharp
     public class QualityMetricDto
     {
         public string MetricType { get; set; } // "AIHumanAgreement", "SchemaValidity"
         public decimal MetricValue { get; set; } // 98.5
         public int SampleSize { get; set; } // 1000
         public string MeasurementPeriod { get; set; } // "Daily", "Weekly"
         public DateTime PeriodStart { get; set; }
         public DateTime PeriodEnd { get; set; }
         public decimal Target { get; set; } // 98.0 for agreement, 99.0 for validity
         public string Status { get; set; } // "MeetsTarget", "BelowTarget"
         public DateTime CreatedAt { get; set; }
     }
     
     public class QualityMetricsSummaryDto
     {
         public QualityMetricDto AgreementRate { get; set; }
         public QualityMetricDto SchemaValidity { get; set; }
         public List<QualityMetricDto> Last7Days { get; set; }
         public decimal SevenDayRollingAverage { get; set; }
     }
     ```

2. **Create IQualityMetricsService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IQualityMetricsService.cs`
   - Methods:
     ```csharp
     Task<QualityMetricDto> CalculateAIHumanAgreementRateAsync(DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken);
     Task<QualityMetricDto> CalculateSchemaValidityRateAsync(DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken);
     Task<QualityMetricsSummaryDto> GetDailySummaryAsync(DateTime date, CancellationToken cancellationToken);
     Task<QualityMetricsSummaryDto> GetWeeklySummaryAsync(DateTime weekStart, CancellationToken cancellationToken);
     Task<List<QualityMetricDto>> GetMetricHistoryAsync(string metricType, int days, CancellationToken cancellationToken);
     Task<decimal> CalculateRollingAverageAsync(string metricType, int days, CancellationToken cancellationToken);
     ```

3. **Implement QualityMetricsService**
   - File: `src/backend/PatientAccess.Business/Services/QualityMetricsService.cs`
   - Constructor dependencies:
     - ILogger<QualityMetricsService>
     - ApplicationDbContext
     - IAlertingService
   - Implement CalculateAIHumanAgreementRateAsync (AIR-Q01: >98%):
     ```csharp
     public async Task<QualityMetricDto> CalculateAIHumanAgreementRateAsync(
         DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken)
     {
         _logger.LogInformation("Calculating AI-Human Agreement Rate for period {Start} to {End}", 
             periodStart, periodEnd);
         
         // Get all verified medical codes in period
         var verifiedCodes = await _context.MedicalCodes
             .Where(mc => mc.VerifiedAt >= periodStart && 
                         mc.VerifiedAt < periodEnd &&
                         (mc.VerificationStatus == "StaffVerified" || mc.VerificationStatus == "StaffRejected"))
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
         
         // Agreement = StaffVerified top suggestions
         var agreementCount = verifiedCodes.Count(mc => 
             mc.VerificationStatus == "StaffVerified" && mc.IsTopSuggestion);
         
         var agreementRate = (decimal)agreementCount / verifiedCodes.Count * 100;
         var status = agreementRate >= 98.0m ? "MeetsTarget" : "BelowTarget";
         
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
             Id = Guid.NewGuid(),
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
         
         // Alert if below target
         if (status == "BelowTarget")
         {
             await _alertingService.SendQualityAlertAsync(
                 $"AI-Human Agreement Rate Below Target: {agreementRate:F2}% (Target: 98.0%)",
                 $"Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}\nSample Size: {verifiedCodes.Count}",
                 cancellationToken);
         }
         
         _logger.LogInformation("AI-Human Agreement Rate: {Rate}% (Sample: {Count})", 
             agreementRate, verifiedCodes.Count);
         
         return metricDto;
     }
     ```
   - Implement CalculateSchemaValidityRateAsync (AIR-Q03: >99%):
     ```csharp
     public async Task<QualityMetricDto> CalculateSchemaValidityRateAsync(
         DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken)
     {
         _logger.LogInformation("Calculating Schema Validity Rate for period {Start} to {End}", 
             periodStart, periodEnd);
         
         // Schema validity data is tracked in-memory during code mapping (task_001)
         // Here we query a hypothetical SchemaValidationLog table (or derive from QualityMetric)
         
         var validationRecords = await _context.QualityMetrics
             .Where(qm => qm.MetricType == "SchemaValidation" &&
                         qm.CreatedAt >= periodStart &&
                         qm.CreatedAt < periodEnd)
             .ToListAsync(cancellationToken);
         
         if (validationRecords.Count == 0)
         {
             return new QualityMetricDto
             {
                 MetricType = "SchemaValidity",
                 MetricValue = 0,
                 SampleSize = 0,
                 MeasurementPeriod = "Daily",
                 PeriodStart = periodStart,
                 PeriodEnd = periodEnd,
                 Target = 99.0m,
                 Status = "BelowTarget",
                 CreatedAt = DateTime.UtcNow
             };
         }
         
         // Calculate validity rate from individual validation records
         var totalValidations = validationRecords.Sum(vr => vr.SampleSize);
         var validCount = validationRecords.Sum(vr => 
             vr.Status == "MeetsTarget" ? vr.SampleSize : 0);
         
         var validityRate = (decimal)validCount / totalValidations * 100;
         var status = validityRate >= 99.0m ? "MeetsTarget" : "BelowTarget";
         
         var metricDto = new QualityMetricDto
         {
             MetricType = "SchemaValidity",
             MetricValue = validityRate,
             SampleSize = totalValidations,
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
             Id = Guid.NewGuid(),
             MetricType = "SchemaValidity",
             MetricValue = validityRate,
             SampleSize = totalValidations,
             MeasurementPeriod = "Daily",
             PeriodStart = periodStart,
             PeriodEnd = periodEnd,
             Target = 99.0m,
             Status = status,
             CreatedAt = DateTime.UtcNow
         };
         
         await _context.QualityMetrics.AddAsync(metric, cancellationToken);
         await _context.SaveChangesAsync(cancellationToken);
         
         // Alert if below target
         if (status == "BelowTarget")
         {
             await _alertingService.SendQualityAlertAsync(
                 $"Schema Validity Rate Below Target: {validityRate:F2}% (Target: 99.0%)",
                 $"Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}\nSample Size: {totalValidations}",
                 cancellationToken);
         }
         
         return metricDto;
     }
     ```
   - Implement CalculateRollingAverageAsync (7-day rolling average for trend analysis):
     ```csharp
     public async Task<decimal> CalculateRollingAverageAsync(
         string metricType, int days, CancellationToken cancellationToken)
     {
         var startDate = DateTime.UtcNow.Date.AddDays(-days);
         
         var metrics = await _context.QualityMetrics
             .Where(qm => qm.MetricType == metricType &&
                         qm.PeriodStart >= startDate)
             .OrderByDescending(qm => qm.PeriodStart)
             .Take(days)
             .ToListAsync(cancellationToken);
         
         if (!metrics.Any()) return 0;
         
         return metrics.Average(m => m.MetricValue);
     }
     ```

4. **Create IAlertingService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IAlertingService.cs`
   - Methods:
     ```csharp
     Task SendQualityAlertAsync(string subject, string message, CancellationToken cancellationToken);
     ```

5. **Implement AlertingService**
   - File: `src/backend/PatientAccess.Business/Services/AlertingService.cs`
   - Uses SMTP or SendGrid for email notifications
   - Sends alerts to admin/quality assurance team when metrics fall below thresholds
   - Implementation:
     ```csharp
     public async Task SendQualityAlertAsync(string subject, string message, CancellationToken cancellationToken)
     {
         _logger.LogWarning("Quality alert triggered: {Subject}", subject);
         
         // Send email to admin/QA team (using SMTP or SendGrid)
         // Implementation: Use MailKit or SendGrid SDK
         
         // For now, log to Application Insights as high-severity event
         _logger.LogError("QUALITY ALERT: {Subject}\n{Message}", subject, message);
         
         // TODO: Implement actual email sending in production
     }
     ```

6. **Create DailyQualityMetricsJob**
   - File: `src/backend/PatientAccess.Business/BackgroundJobs/DailyQualityMetricsJob.cs`
   - Hangfire job that runs daily at 2:00 AM UTC
   - Calculates AI-Human Agreement Rate and Schema Validity for previous day
   - Implementation:
     ```csharp
     public class DailyQualityMetricsJob
     {
         private readonly IQualityMetricsService _qualityMetricsService;
         private readonly ILogger<DailyQualityMetricsJob> _logger;
         
         public DailyQualityMetricsJob(
             IQualityMetricsService qualityMetricsService,
             ILogger<DailyQualityMetricsJob> logger)
         {
             _qualityMetricsService = qualityMetricsService;
             _logger = logger;
         }
         
         [AutomaticRetry(Attempts = 3)]
         public async Task ExecuteAsync(CancellationToken cancellationToken)
         {
             var yesterday = DateTime.UtcNow.Date.AddDays(-1);
             var periodStart = yesterday;
             var periodEnd = yesterday.AddDays(1);
             
             _logger.LogInformation("Running Daily Quality Metrics Job for {Date}", yesterday);
             
             // Calculate AI-Human Agreement Rate
             var agreementMetric = await _qualityMetricsService
                 .CalculateAIHumanAgreementRateAsync(periodStart, periodEnd, cancellationToken);
             
             // Calculate Schema Validity Rate
             var schemaMetric = await _qualityMetricsService
                 .CalculateSchemaValidityRateAsync(periodStart, periodEnd, cancellationToken);
             
             _logger.LogInformation("Daily metrics calculated: Agreement={Agreement}%, Validity={Validity}%", 
                 agreementMetric.MetricValue, schemaMetric.MetricValue);
         }
     }
     ```

7. **Create WeeklyQualityMetricsJob**
   - File: `src/backend/PatientAccess.Business/BackgroundJobs/WeeklyQualityMetricsJob.cs`
   - Runs weekly on Mondays at 3:00 AM UTC
   - Calculates metrics for previous week (Monday to Sunday)
   - Similar structure to DailyQualityMetricsJob

8. **Create QualityMetricsController**
   - File: `src/backend/PatientAccess.Web/Controllers/QualityMetricsController.cs`
   - Authorization: [Authorize(Roles = "Admin")]
   - Endpoints:
     ```csharp
     [ApiController]
     [Route("api/quality-metrics")]
     [Authorize(Roles = "Admin")]
     public class QualityMetricsController : ControllerBase
     {
         [HttpGet("summary")]
         public async Task<IActionResult> GetSummary(
             [FromQuery] DateTime? date = null,
             CancellationToken cancellationToken = default)
         {
             var targetDate = date ?? DateTime.UtcNow.Date;
             var summary = await _qualityMetricsService.GetDailySummaryAsync(targetDate, cancellationToken);
             return Ok(summary);
         }
         
         [HttpGet("history")]
         public async Task<IActionResult> GetHistory(
             [FromQuery] string metricType,
             [FromQuery] int days = 30,
             CancellationToken cancellationToken = default)
         {
             var history = await _qualityMetricsService.GetMetricHistoryAsync(metricType, days, cancellationToken);
             return Ok(history);
         }
         
         [HttpGet("rolling-average")]
         public async Task<IActionResult> GetRollingAverage(
             [FromQuery] string metricType,
             [FromQuery] int days = 7,
             CancellationToken cancellationToken = default)
         {
             var average = await _qualityMetricsService.CalculateRollingAverageAsync(metricType, days, cancellationToken);
             return Ok(new { MetricType = metricType, Days = days, RollingAverage = average });
         }
     }
     ```

9. **Configure Hangfire Jobs in Program.cs**
   - Register recurring jobs:
     ```csharp
     // After app.UseHangfireDashboard()
     RecurringJob.AddOrUpdate<DailyQualityMetricsJob>(
         "daily-quality-metrics",
         job => job.ExecuteAsync(CancellationToken.None),
         "0 2 * * *"); // Daily at 2:00 AM UTC
     
     RecurringJob.AddOrUpdate<WeeklyQualityMetricsJob>(
         "weekly-quality-metrics",
         job => job.ExecuteAsync(CancellationToken.None),
         "0 3 * * 1"); // Monday at 3:00 AM UTC
     ```

10. **Add Application Insights Custom Metrics**
    - Track agreement rate and schema validity as custom metrics
    - Implementation in QualityMetricsService:
      ```csharp
      // After calculating metric
      var telemetry = new TelemetryClient();
      telemetry.TrackMetric("AIHumanAgreementRate", (double)agreementRate);
      telemetry.TrackMetric("SchemaValidityRate", (double)validityRate);
      ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   └── CodeMappingService.cs (from task_001)
│   ├── BackgroundJobs/
│   │   └── ConfirmationEmailJob.cs (from EP-002)
│   └── Interfaces/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── MedicalCode.cs (from task_001)
│   │   └── QualityMetric.cs (from task_001)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    ├── Controllers/
    └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/QualityMetricsService.cs | Core metrics calculation |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IQualityMetricsService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/DailyQualityMetricsJob.cs | Daily metric job |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/WeeklyQualityMetricsJob.cs | Weekly metric job |
| CREATE | src/backend/PatientAccess.Business/Services/AlertingService.cs | Email alerting service |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IAlertingService.cs | Alerting interface |
| CREATE | src/backend/PatientAccess.Business/DTOs/QualityMetricDto.cs | Quality metric DTO |
| CREATE | src/backend/PatientAccess.Web/Controllers/QualityMetricsController.cs | REST API controller |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register services, configure jobs |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Hangfire Documentation
- **Recurring Jobs**: https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html
- **Cron Expressions**: https://crontab.guru/

### Application Insights Telemetry
- **Custom Metrics**: https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics
- **TrackMetric**: https://learn.microsoft.com/en-us/dotnet/api/microsoft.applicationinsights.telemetryclient.trackmetric

### Email Notifications
- **MailKit**: https://github.com/jstedfast/MailKit
- **SendGrid**: https://sendgrid.com/docs/for-developers/

### Design Requirements
- **AIR-Q01**: System MUST maintain AI-Human Agreement Rate above 98% (design.md)
- **AIR-Q03**: System MUST achieve output schema validity rate above 99% (design.md)
- **AG-004**: Maintain AI-Human Agreement Rate above 98% via Trust-First verification (design.md)

### Existing Codebase Patterns
- **Hangfire Jobs**: `src/backend/PatientAccess.Business/BackgroundJobs/ConfirmationEmailJob.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/CodeMappingService.cs`

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build

# Run tests
cd PatientAccess.Tests
dotnet test
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/QualityMetricsServiceTests.cs`
- Test cases:
  1. **Test_CalculateAIHumanAgreementRateAsync_Returns98Percent**
     - Setup: Insert 100 MedicalCode records, 98 StaffVerified, 2 StaffRejected
     - Execute: CalculateAIHumanAgreementRateAsync(yesterday, today)
     - Assert: metricDto.MetricValue == 98.0m, Status == "MeetsTarget"
  2. **Test_CalculateAIHumanAgreementRateAsync_BelowTarget_TriggersAlert**
     - Setup: Insert 100 records, 95 verified, 5 rejected
     - Execute: CalculateAIHumanAgreementRateAsync
     - Assert: AlertingService.SendQualityAlertAsync called, Status == "BelowTarget"
  3. **Test_CalculateSchemaValidityRateAsync_Returns99Percent**
     - Setup: Insert 100 SchemaValidation records, 99 valid, 1 invalid
     - Execute: CalculateSchemaValidityRateAsync
     - Assert: metricDto.MetricValue == 99.0m, Status == "MeetsTarget"
  4. **Test_CalculateRollingAverageAsync_Returns7DayAverage**
     - Setup: Insert 7 QualityMetric records with values: 98, 97, 99, 98, 98, 97, 99
     - Execute: CalculateRollingAverageAsync("AIHumanAgreement", 7)
     - Assert: average == 98.0m

### Integration Tests
- File: `src/backend/PatientAccess.Tests/BackgroundJobs/DailyQualityMetricsJobTests.cs`
- Test cases:
  1. **Test_DailyQualityMetricsJob_CreatesQualityMetricRecords**
     - Setup: Insert verified MedicalCode records for yesterday
     - Execute: DailyQualityMetricsJob.ExecuteAsync()
     - Assert: QualityMetric table contains 2 records (AIHumanAgreement, SchemaValidity)
  2. **Test_QualityMetricsController_GetSummary_Returns200OK**
     - Setup: Insert QualityMetric records
     - Execute: GET /api/quality-metrics/summary
     - Assert: Response 200 OK, summary contains agreement and validity metrics

### Acceptance Criteria Validation
- **AC4**: ✅ AI-Human Agreement Rate tracked daily (target >98%)
- **AC4**: ✅ Output schema validity tracked daily (target >99%)
- **AC4**: ✅ Alerts sent when metrics fall below targets

## Success Criteria Checklist
- [MANDATORY] QualityMetricsService implements IQualityMetricsService interface
- [MANDATORY] CalculateAIHumanAgreementRateAsync calculates (StaffVerified / Total) * 100
- [MANDATORY] CalculateSchemaValidityRateAsync calculates (Valid / Total) * 100
- [MANDATORY] DailyQualityMetricsJob runs daily at 2:00 AM UTC via Hangfire
- [MANDATORY] WeeklyQualityMetricsJob runs weekly on Mondays at 3:00 AM UTC
- [MANDATORY] AlertingService sends email when Agreement Rate <98%
- [MANDATORY] AlertingService sends email when Schema Validity <99%
- [MANDATORY] QualityMetricsController GET /summary endpoint (Admin role)
- [MANDATORY] QualityMetricsController GET /history endpoint (Admin role)
- [MANDATORY] CalculateRollingAverageAsync supports 7-day and 30-day rolling averages
- [MANDATORY] Unit test: Agreement rate calculation returns 98% for 98/100 verified
- [MANDATORY] Unit test: Alert triggered when metric <target threshold
- [MANDATORY] Integration test: DailyQualityMetricsJob persists QualityMetric records
- [MANDATORY] Integration test: QualityMetricsController returns summary with metrics
- [RECOMMENDED] Application Insights custom metrics: "AIHumanAgreementRate", "SchemaValidityRate"
- [RECOMMENDED] CSV export for compliance audit reports

## Estimated Effort
**4 hours** (Service implementation + Hangfire jobs + alerting + unit tests)
