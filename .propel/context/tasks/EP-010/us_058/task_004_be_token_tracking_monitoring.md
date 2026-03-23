# Task - task_004_be_token_tracking_monitoring

## Requirement Reference
- User Story: US_058
- Story Location: .propel/context/tasks/EP-010/us_058/us_058.md
- Acceptance Criteria:
    - **AC4**: Given token/cost management requirements (AIR-O05), When AI services are invoked, Then per-request token usage and estimated cost are logged, daily aggregates are tracked, and a configurable daily ceiling triggers alerts at 80% threshold and halts non-critical AI calls at 100%.
    - **AC5**: Given AI operational monitoring (NFR-015), When admins view AI metrics, Then they see model version, request latency, error rates, and confidence score distributions for operational visibility.

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
| Frontend | N/A | N/A |
| Backend | ASP.NET Core | 8.0 |
| Backend | C# | 12.0 |
| Backend | Prometheus.NET | 8.0 |
| Backend | Hangfire | 1.8.x |
| Caching | StackExchange.Redis | 2.8+ |
| Database | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | Azure OpenAI | GPT-4o |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-O05, NFR-015 |
| **AI Pattern** | Token/cost tracking, Daily ceiling management, Operational metrics |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | appsettings.json (DailyCeiling, PricingTiers) |
| **Model Provider** | Azure OpenAI (GPT-4o) |

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

Implement comprehensive token tracking and AI operational monitoring to enforce cost management (AIR-O05) and provide operational visibility (NFR-015). This task creates TokenUsageTracker to log per-request token consumption and estimated costs, CostCalculator with GPT-4o pricing tiers, DailyCeilingManager to track daily aggregates against configurable limits with 80% warning alerts and 100% non-critical halt, and AIOperationalMetrics capturing model versions, latency distributions, error rates, confidence score histograms. Includes admin-only API endpoints (GET /api/admin/ai-metrics) for operational metrics, Prometheus integration with custom metrics (ai_token_usage_total, ai_request_duration_seconds, ai_confidence_score), DailyUsageReportJob for email alerts, and comprehensive monitoring dashboard. Ensures token/cost data persists to database (task_001 schema) for audit trail and historical analysis.

**Key Capabilities:**
- TokenUsageTracker logs per-request token usage + estimated cost (AC4)
- CostCalculator with GPT-4o pricing ($0.005/1K prompt, $0.015/1K completion)
- DailyCeilingManager tracks daily aggregates vs configurable limits
- 80% threshold triggers warning alert to admins (AC4)
- 100% threshold halts non-critical AI calls (AC4)
- AIOperationalMetrics captures model version, latency, error rates (AC5)
- Confidence score distribution histogram (Low/Medium/High confidence ranges) (AC5)
- Admin-only GET /api/admin/ai-metrics endpoint
- Prometheus integration (ai_token_usage_total, ai_request_duration_seconds, ai_confidence_score)
- DailyUsageReportJob (Hangfire) sends email alerts at 80%/100%

## Dependent Tasks
- EP-010: US_058: task_001 (AI metadata schema - token/cost fields in ExtractedClinicalData)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/TokenUsageTracker.cs` - Per-request token logging
- **NEW**: `src/backend/PatientAccess.Business/Services/CostCalculator.cs` - Cost estimation
- **NEW**: `src/backend/PatientAccess.Business/Services/DailyCeilingManager.cs` - Daily ceiling enforcement
- **NEW**: `src/backend/PatientAccess.Business/Services/AIOperationalMetrics.cs` - Operational metrics aggregation
- **NEW**: `src/backend/PatientAccess.Web/Controllers/AIMetricsController.cs` - Admin-only metrics API
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/DailyUsageReportJob.cs` - Email alerts for usage thresholds
- **NEW**: `src/backend/PatientAccess.Business/Services/AIMetricsService.cs` - Prometheus metrics service
- **NEW**: `docs/AI_TOKEN_COST_MANAGEMENT.md` - Token/cost management documentation
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register services, configure Prometheus metrics
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add daily ceiling configuration

## Implementation Plan

1. **Create CostCalculator**
   - File: `src/backend/PatientAccess.Business/Services/CostCalculator.cs`
   - GPT-4o pricing calculations:
     ```csharp
     using Microsoft.Extensions.Configuration;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Calculates AI service costs based on token usage and model pricing (AC4 - US_058).
         /// </summary>
         public class CostCalculator
         {
             private readonly IConfiguration _configuration;
             private readonly ILogger<CostCalculator> _logger;
             
             // GPT-4o default pricing (per 1K tokens) - Azure OpenAI
             private const decimal DefaultPromptTokenCost = 0.005m; // $0.005 per 1K prompt tokens
             private const decimal DefaultCompletionTokenCost = 0.015m; // $0.015 per 1K completion tokens
             
             public CostCalculator(
                 IConfiguration configuration,
                 ILogger<CostCalculator> logger)
             {
                 _configuration = configuration;
                 _logger = logger;
             }
             
             /// <summary>
             /// Calculates estimated cost for AI request (AC4 - AIR-O05).
             /// </summary>
             public decimal CalculateCost(int promptTokens, int completionTokens, string modelVersion)
             {
                 // Get pricing for model version (configurable per-model pricing)
                 var promptCost = _configuration.GetValue<decimal?>(
                     $"AIPricing:{modelVersion}:PromptTokenCost") ?? DefaultPromptTokenCost;
                 
                 var completionCost = _configuration.GetValue<decimal?>(
                     $"AIPricing:{modelVersion}:CompletionTokenCost") ?? DefaultCompletionTokenCost;
                 
                 // Calculate cost: (tokens / 1000) * cost_per_1k_tokens
                 var promptCostTotal = (promptTokens / 1000m) * promptCost;
                 var completionCostTotal = (completionTokens / 1000m) * completionCost;
                 
                 var totalCost = promptCostTotal + completionCostTotal;
                 
                 _logger.LogDebug(
                     "Cost calculated: PromptTokens={PromptTokens} ({PromptCost:C}), " +
                     "CompletionTokens={CompletionTokens} ({CompletionCost:C}), " +
                     "TotalCost={TotalCost:C} (Model: {ModelVersion})",
                     promptTokens, promptCostTotal,
                     completionTokens, completionCostTotal,
                     totalCost, modelVersion);
                 
                 return totalCost;
             }
             
             /// <summary>
             /// Calculates total cost from total tokens (for Document Intelligence which returns single token count).
             /// </summary>
             public decimal CalculateCostFromTotalTokens(int totalTokens, string modelVersion)
             {
                 // Estimate 75% prompt, 25% completion tokens (typical for document extraction)
                 var promptTokens = (int)(totalTokens * 0.75);
                 var completionTokens = totalTokens - promptTokens;
                 
                 return CalculateCost(promptTokens, completionTokens, modelVersion);
             }
         }
     }
     ```

2. **Create TokenUsageTracker**
   - File: `src/backend/PatientAccess.Business/Services/TokenUsageTracker.cs`
   - Per-request token/cost logging:
     ```csharp
     using PatientAccess.Data;
     using PatientAccess.Data.Entities;
     using Microsoft.EntityFrameworkCore;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Tracks AI token usage and costs per request (AC4 - US_058, AIR-O05).
         /// </summary>
         public class TokenUsageTracker
         {
             private readonly AppDbContext _context;
             private readonly CostCalculator _costCalculator;
             private readonly DailyCeilingManager _ceilingManager;
             private readonly AIMetricsService _metricsService;
             private readonly ILogger<TokenUsageTracker> _logger;
             
             public TokenUsageTracker(
                 AppDbContext context,
                 CostCalculator costCalculator,
                 DailyCeilingManager ceilingManager,
                 AIMetricsService metricsService,
                 ILogger<TokenUsageTracker> logger)
             {
                 _context = context;
                 _costCalculator = costCalculator;
                 _ceilingManager = ceilingManager;
                 _metricsService = metricsService;
                 _logger = logger;
             }
             
             /// <summary>
             /// Logs token usage and cost for AI extraction (AC4).
             /// Updates ExtractedClinicalData entity from task_001.
             /// </summary>
             public async Task LogTokenUsageAsync(
                 int extractionId,
                 int promptTokens,
                 int completionTokens,
                 string modelVersion,
                 CancellationToken cancellationToken = default)
             {
                 var extraction = await _context.ExtractedClinicalData
                     .FirstOrDefaultAsync(e => e.Id == extractionId, cancellationToken);
                 
                 if (extraction == null)
                 {
                     throw new InvalidOperationException($"Extraction {extractionId} not found.");
                 }
                 
                 var totalTokens = promptTokens + completionTokens;
                 var estimatedCost = _costCalculator.CalculateCost(promptTokens, completionTokens, modelVersion);
                 
                 // Update extraction entity (task_001 schema)
                 extraction.PromptTokens = promptTokens;
                 extraction.CompletionTokens = completionTokens;
                 extraction.TotalTokens = totalTokens;
                 extraction.EstimatedCost = estimatedCost;
                 extraction.ModelVersion = modelVersion;
                 
                 await _context.SaveChangesAsync(cancellationToken);
                 
                 // Update daily aggregate
                 await _ceilingManager.IncrementDailyUsageAsync(totalTokens, estimatedCost);
                 
                 // Update Prometheus metrics
                 _metricsService.RecordTokenUsage(promptTokens, completionTokens);
                 _metricsService.RecordCost(estimatedCost);
                 
                 _logger.LogInformation(
                     "Token usage logged: ExtractionId={ExtractionId}, PromptTokens={PromptTokens}, " +
                     "CompletionTokens={CompletionTokens}, TotalTokens={TotalTokens}, Cost={Cost:C}, " +
                     "ModelVersion={ModelVersion} (AC4 - US_058)",
                     extractionId, promptTokens, completionTokens, totalTokens, estimatedCost, modelVersion);
             }
             
             /// <summary>
             /// Logs token usage for clinical document (aggregate level).
             /// </summary>
             public async Task LogDocumentTokenUsageAsync(
                 int documentId,
                 int totalTokens,
                 string modelVersion,
                 CancellationToken cancellationToken = default)
             {
                 var document = await _context.ClinicalDocuments
                     .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);
                 
                 if (document == null)
                 {
                     throw new InvalidOperationException($"Document {documentId} not found.");
                 }
                 
                 var estimatedCost = _costCalculator.CalculateCostFromTotalTokens(totalTokens, modelVersion);
                 
                 // Update document entity (task_001 schema)
                 document.IsAIProcessed = true;
                 document.AIModelVersion = modelVersion;
                 document.TotalTokensConsumed = totalTokens;
                 document.TotalEstimatedCost = estimatedCost;
                 
                 await _context.SaveChangesAsync(cancellationToken);
                 
                 // Update daily aggregate
                 await _ceilingManager.IncrementDailyUsageAsync(totalTokens, estimatedCost);
                 
                 _logger.LogInformation(
                     "Document token usage logged: DocumentId={DocumentId}, TotalTokens={TotalTokens}, " +
                     "Cost={Cost:C}, ModelVersion={ModelVersion} (AC4 - US_058)",
                     documentId, totalTokens, estimatedCost, modelVersion);
             }
         }
     }
     ```

3. **Create DailyCeilingManager**
   - File: `src/backend/PatientAccess.Business/Services/DailyCeilingManager.cs`
   - Daily ceiling tracking with alerts:
     ```csharp
     using StackExchange.Redis;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Manages daily AI usage ceiling with alerts and throttling (AC4 - US_058, AIR-O05).
         /// </summary>
         public class DailyCeilingManager
         {
             private readonly IConnectionMultiplexer _redis;
             private readonly IConfiguration _configuration;
             private readonly ILogger<DailyCeilingManager> _logger;
             
             private const string DailyTokensKey = "ai:usage:daily:{0}:tokens"; // {0} = date YYYY-MM-DD
             private const string DailyCostKey = "ai:usage:daily:{0}:cost";
             
             public DailyCeilingManager(
                 IConnectionMultiplexer redis,
                 IConfiguration configuration,
                 ILogger<DailyCeilingManager> logger)
             {
                 _redis = redis;
                 _configuration = configuration;
                 _logger = logger;
             }
             
             /// <summary>
             /// Increments daily token and cost usage (AC4).
             /// </summary>
             public async Task IncrementDailyUsageAsync(int tokens, decimal cost)
             {
                 var db = _redis.GetDatabase();
                 var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
                 
                 var tokensKey = string.Format(DailyTokensKey, today);
                 var costKey = string.Format(DailyCostKey, today);
                 
                 // Increment counters (atomic operations)
                 await db.StringIncrementAsync(tokensKey, tokens);
                 await db.StringIncrementAsync(costKey, (double)cost);
                 
                 // Set expiration (7 days retention)
                 await db.KeyExpireAsync(tokensKey, TimeSpan.FromDays(7));
                 await db.KeyExpireAsync(costKey, TimeSpan.FromDays(7));
                 
                 // Check thresholds
                 await CheckThresholdsAsync(today);
             }
             
             /// <summary>
             /// Gets current daily usage.
             /// </summary>
             public async Task<DailyUsage> GetDailyUsageAsync(DateTime? date = null)
             {
                 var db = _redis.GetDatabase();
                 var targetDate = (date ?? DateTime.UtcNow).ToString("yyyy-MM-dd");
                 
                 var tokensKey = string.Format(DailyTokensKey, targetDate);
                 var costKey = string.Format(DailyCostKey, targetDate);
                 
                 var tokens = (int)(await db.StringGetAsync(tokensKey));
                 var cost = Convert.ToDecimal((double)(await db.StringGetAsync(costKey)));
                 
                 var maxTokens = _configuration.GetValue<int>("AIDailyCeiling:MaxTokens", 1000000);
                 var maxCost = _configuration.GetValue<decimal>("AIDailyCeiling:MaxCostUSD", 50.0m);
                 
                 return new DailyUsage
                 {
                     Date = DateTime.Parse(targetDate),
                     TotalTokens = tokens,
                     TotalCost = cost,
                     MaxTokens = maxTokens,
                     MaxCost = maxCost,
                     TokensPercentage = (tokens / (decimal)maxTokens) * 100,
                     CostPercentage = (cost / maxCost) * 100
                 };
             }
             
             /// <summary>
             /// Checks if AI usage is within ceiling (AC4).
             /// Returns true if ceiling not exceeded, false if halted (100% threshold).
             /// </summary>
             public async Task<CeilingStatus> CheckCeilingAsync()
             {
                 var usage = await GetDailyUsageAsync();
                 
                 // Check if 100% threshold exceeded (halt non-critical AI calls)
                 if (usage.TokensPercentage >= 100 || usage.CostPercentage >= 100)
                 {
                     _logger.LogWarning(
                         "Daily AI ceiling EXCEEDED (100%). Halting non-critical AI calls (AC4 - AIR-O05). " +
                         "Tokens: {Tokens}/{MaxTokens} ({TokensPercent:F1}%), Cost: {Cost:C}/{MaxCost:C} ({CostPercent:F1}%)",
                         usage.TotalTokens, usage.MaxTokens, usage.TokensPercentage,
                         usage.TotalCost, usage.MaxCost, usage.CostPercentage);
                     
                     return CeilingStatus.Exceeded;
                 }
                 
                 // Check if 80% warning threshold reached
                 if (usage.TokensPercentage >= 80 || usage.CostPercentage >= 80)
                 {
                     _logger.LogWarning(
                         "Daily AI ceiling WARNING (80%). Nearing limit (AC4 - AIR-O05). " +
                         "Tokens: {Tokens}/{MaxTokens} ({TokensPercent:F1}%), Cost: {Cost:C}/{MaxCost:C} ({CostPercent:F1}%)",
                         usage.TotalTokens, usage.MaxTokens, usage.TokensPercentage,
                         usage.TotalCost, usage.MaxCost, usage.CostPercentage);
                     
                     return CeilingStatus.Warning;
                 }
                 
                 return CeilingStatus.Normal;
             }
             
             /// <summary>
             /// Checks thresholds and triggers alerts if necessary (AC4).
             /// </summary>
             private async Task CheckThresholdsAsync(string date)
             {
                 var status = await CheckCeilingAsync();
                 
                 // Set Redis flag for threshold state
                 var db = _redis.GetDatabase();
                 await db.StringSetAsync($"ai:usage:daily:{date}:status", status.ToString());
             }
         }
         
         public class DailyUsage
         {
             public DateTime Date { get; set; }
             public int TotalTokens { get; set; }
             public decimal TotalCost { get; set; }
             public int MaxTokens { get; set; }
             public decimal MaxCost { get; set; }
             public decimal TokensPercentage { get; set; }
             public decimal CostPercentage { get; set; }
         }
         
         public enum CeilingStatus
         {
             Normal,      // < 80%
             Warning,     // 80-99%
             Exceeded     // >= 100%
         }
     }
     ```

4. **Create AIOperationalMetrics**
   - File: `src/backend/PatientAccess.Business/Services/AIOperationalMetrics.cs`
   - Operational metrics aggregation (AC5):
     ```csharp
     using PatientAccess.Data;
     using Microsoft.EntityFrameworkCore;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Aggregates AI operational metrics for admin visibility (AC5 - US_058, NFR-015).
         /// </summary>
         public class AIOperationalMetrics
         {
             private readonly AppDbContext _context;
             private readonly DailyCeilingManager _ceilingManager;
             private readonly ILogger<AIOperationalMetrics> _logger;
             
             public AIOperationalMetrics(
                 AppDbContext context,
                 DailyCeilingManager ceilingManager,
                 ILogger<AIOperationalMetrics> logger)
             {
                 _context = context;
                 _ceilingManager = ceilingManager;
                 _logger = logger;
             }
             
             /// <summary>
             /// Gets comprehensive AI operational metrics (AC5 - NFR-015).
             /// </summary>
             public async Task<AIMetricsSummary> GetMetricsSummaryAsync(
                 DateTime startDate,
                 DateTime endDate,
                 CancellationToken cancellationToken = default)
             {
                 // Model version distribution
                 var modelVersions = await _context.ExtractedClinicalData
                     .Where(e => e.IsAISuggested && e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                     .GroupBy(e => e.ModelVersion)
                     .Select(g => new ModelVersionMetric
                     {
                         ModelVersion = g.Key ?? "Unknown",
                         RequestCount = g.Count(),
                         TotalTokens = g.Sum(e => e.TotalTokens ?? 0),
                         TotalCost = g.Sum(e => e.EstimatedCost ?? 0)
                     })
                     .ToListAsync(cancellationToken);
                 
                 // Confidence score distribution (AC5)
                 var confidenceDistribution = await _context.ExtractedClinicalData
                     .Where(e => e.IsAISuggested && e.ConfidenceScore.HasValue && 
                                 e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                     .ToListAsync(cancellationToken);
                 
                 var lowConfidence = confidenceDistribution.Count(e => e.ConfidenceScore < 0.5m);
                 var mediumConfidence = confidenceDistribution.Count(e => e.ConfidenceScore >= 0.5m && e.ConfidenceScore < 0.7m);
                 var highConfidence = confidenceDistribution.Count(e => e.ConfidenceScore >= 0.7m);
                 
                 // Error rates (AC5)
                 var totalRequests = await _context.ExtractedClinicalData
                     .Where(e => e.IsAISuggested && e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                     .CountAsync(cancellationToken);
                 
                 var failedRequests = await _context.ExtractedClinicalData
                     .Where(e => e.IsAISuggested && e.RequiresManualReview && 
                                 e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                     .CountAsync(cancellationToken);
                 
                 var errorRate = totalRequests > 0 ? (failedRequests / (decimal)totalRequests) * 100 : 0;
                 
                 // Daily usage
                 var dailyUsage = await _ceilingManager.GetDailyUsageAsync();
                 
                 return new AIMetricsSummary
                 {
                     StartDate = startDate,
                     EndDate = endDate,
                     ModelVersions = modelVersions,
                     ConfidenceDistribution = new ConfidenceDistribution
                     {
                         LowConfidence = lowConfidence,
                         MediumConfidence = mediumConfidence,
                         HighConfidence = highConfidence,
                         TotalExtractions = confidenceDistribution.Count
                     },
                     TotalRequests = totalRequests,
                     FailedRequests = failedRequests,
                     ErrorRatePercentage = errorRate,
                     DailyUsage = dailyUsage
                 };
             }
         }
         
         public class AIMetricsSummary
         {
             public DateTime StartDate { get; set; }
             public DateTime EndDate { get; set; }
             public List<ModelVersionMetric> ModelVersions { get; set; }
             public ConfidenceDistribution ConfidenceDistribution { get; set; }
             public int TotalRequests { get; set; }
             public int FailedRequests { get; set; }
             public decimal ErrorRatePercentage { get; set; }
             public DailyUsage DailyUsage { get; set; }
         }
         
         public class ModelVersionMetric
         {
             public string ModelVersion { get; set; }
             public int RequestCount { get; set; }
             public int TotalTokens { get; set; }
             public decimal TotalCost { get; set; }
         }
         
         public class ConfidenceDistribution
         {
             public int LowConfidence { get; set; } // < 50%
             public int MediumConfidence { get; set; } // 50-70%
             public int HighConfidence { get; set; } // >= 70%
             public int TotalExtractions { get; set; }
         }
     }
     ```

5. **Create AIMetricsController (Admin-Only)**
   - File: `src/backend/PatientAccess.Web/Controllers/AIMetricsController.cs`
   - Admin API endpoints:
     ```csharp
     using Microsoft.AspNetCore.Authorization;
     using Microsoft.AspNetCore.Mvc;
     using PatientAccess.Business.Services;
     
     namespace PatientAccess.Web.Controllers
     {
         /// <summary>
         /// API endpoints for AI operational metrics (AC5 - US_058).
         /// Admin-only access.
         /// </summary>
         [ApiController]
         [Route("api/admin/ai-metrics")]
         [Authorize(Roles = "Admin")]
         public class AIMetricsController : ControllerBase
         {
             private readonly AIOperationalMetrics _operationalMetrics;
             private readonly DailyCeilingManager _ceilingManager;
             private readonly ILogger<AIMetricsController> _logger;
             
             public AIMetricsController(
                 AIOperationalMetrics operationalMetrics,
                 DailyCeilingManager ceilingManager,
                 ILogger<AIMetricsController> logger)
             {
                 _operationalMetrics = operationalMetrics;
                 _ceilingManager = ceilingManager;
                 _logger = logger;
             }
             
             /// <summary>
             /// Gets comprehensive AI operational metrics (AC5 - NFR-015).
             /// </summary>
             [HttpGet]
             public async Task<ActionResult<AIMetricsSummary>> GetMetrics(
                 [FromQuery] DateTime? startDate = null,
                 [FromQuery] DateTime? endDate = null,
                 CancellationToken cancellationToken = default)
             {
                 var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                 var end = endDate ?? DateTime.UtcNow;
                 
                 var metrics = await _operationalMetrics.GetMetricsSummaryAsync(start, end, cancellationToken);
                 
                 return Ok(metrics);
             }
             
             /// <summary>
             /// Gets daily usage and ceiling status (AC4 - AIR-O05).
             /// </summary>
             [HttpGet("daily-usage")]
             public async Task<ActionResult<DailyUsage>> GetDailyUsage(
                 [FromQuery] DateTime? date = null)
             {
                 var usage = await _ceilingManager.GetDailyUsageAsync(date);
                 return Ok(usage);
             }
             
             /// <summary>
             /// Checks current ceiling status (AC4).
             /// </summary>
             [HttpGet("ceiling-status")]
             public async Task<ActionResult<CeilingStatusResponse>> GetCeilingStatus()
             {
                 var status = await _ceilingManager.CheckCeilingAsync();
                 var usage = await _ceilingManager.GetDailyUsageAsync();
                 
                 return Ok(new CeilingStatusResponse
                 {
                     Status = status.ToString(),
                     Message = GetStatusMessage(status),
                     Usage = usage
                 });
             }
             
             private string GetStatusMessage(CeilingStatus status)
             {
                 return status switch
                 {
                     CeilingStatus.Normal => "AI usage within normal limits.",
                     CeilingStatus.Warning => "Warning: AI usage at 80% of daily ceiling. Consider monitoring closely.",
                     CeilingStatus.Exceeded => "Critical: Daily AI ceiling exceeded. Non-critical AI calls halted.",
                     _ => "Unknown status."
                 };
             }
         }
         
         public class CeilingStatusResponse
         {
             public string Status { get; set; }
             public string Message { get; set; }
             public DailyUsage Usage { get; set; }
         }
     }
     ```

6. **Create AIMetricsService (Prometheus)**
   - File: `src/backend/PatientAccess.Business/Services/AIMetricsService.cs`
   - Prometheus custom metrics:
     ```csharp
     using Prometheus;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Prometheus metrics for AI operations (AC5 - US_058).
         /// </summary>
         public class AIMetricsService
         {
             // Token usage counter
             private static readonly Counter TokenUsageCounter = Metrics.CreateCounter(
                 "ai_token_usage_total",
                 "Total AI tokens consumed",
                 new CounterConfiguration
                 {
                     LabelNames = new[] { "token_type" } // "prompt" or "completion"
                 }
             );
             
             // Cost counter
             private static readonly Counter CostCounter = Metrics.CreateCounter(
                 "ai_cost_total_usd",
                 "Total AI cost in USD"
             );
             
             // Request duration histogram
             private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
                 "ai_request_duration_seconds",
                 "AI request processing duration in seconds",
                 new HistogramConfiguration
                 {
                     Buckets = Histogram.ExponentialBuckets(0.1, 2, 10) // 0.1s to ~100s
                 }
             );
             
             // Confidence score gauge
             private static readonly Gauge ConfidenceScore = Metrics.CreateGauge(
                 "ai_confidence_score",
                 "AI extraction confidence score",
                 new GaugeConfiguration
                 {
                     LabelNames = new[] { "confidence_range" } // "low", "medium", "high"
                 }
             );
             
             public void RecordTokenUsage(int promptTokens, int completionTokens)
             {
                 TokenUsageCounter.WithLabels("prompt").Inc(promptTokens);
                 TokenUsageCounter.WithLabels("completion").Inc(completionTokens);
             }
             
             public void RecordCost(decimal cost)
             {
                 CostCounter.Inc((double)cost);
             }
             
             public void RecordRequestDuration(double durationSeconds)
             {
                 RequestDuration.Observe(durationSeconds);
             }
             
             public void RecordConfidenceScore(decimal? confidenceScore)
             {
                 if (!confidenceScore.HasValue)
                     return;
                 
                 var range = confidenceScore.Value switch
                 {
                     < 0.5m => "low",
                     < 0.7m => "medium",
                     _ => "high"
                 };
                 
                 ConfidenceScore.WithLabels(range).Set((double)confidenceScore.Value);
             }
         }
     }
     ```

7. **Configure Services and Settings**
   - Files: `Program.cs`, `appsettings.json`
   - Service registration and configuration

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   ├── BackgroundJobs/
│   └── DTOs/
├── PatientAccess.Data/
│   └── Entities/
│       └── ExtractedClinicalData.cs (with token/cost fields from task_001)
├── PatientAccess.Web/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
└── docs/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/TokenUsageTracker.cs | Per-request token logging |
| CREATE | src/backend/PatientAccess.Business/Services/CostCalculator.cs | Cost estimation with GPT-4o pricing |
| CREATE | src/backend/PatientAccess.Business/Services/DailyCeilingManager.cs | Daily ceiling enforcement with alerts |
| CREATE | src/backend/PatientAccess.Business/Services/AIOperationalMetrics.cs | Operational metrics aggregation |
| CREATE | src/backend/PatientAccess.Business/Services/AIMetricsService.cs | Prometheus custom metrics |
| CREATE | src/backend/PatientAccess.Web/Controllers/AIMetricsController.cs | Admin-only metrics API |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/DailyUsageReportJob.cs | Email alerts for thresholds |
| CREATE | docs/AI_TOKEN_COST_MANAGEMENT.md | Token/cost management documentation |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register services, configure Prometheus |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add daily ceiling configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Azure OpenAI Pricing
- **GPT-4o Pricing**: https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/

### Prometheus Documentation
- **Custom Metrics**: https://github.com/prometheus-net/prometheus-net
- **Histogram**: https://prometheus.io/docs/practices/histograms/

### Design Requirements
- **AIR-O05**: Rate limiting for AI services (design.md)
- **NFR-015**: Graceful degradation when AI unavailable (design.md)

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build PatientAccess.sln

# Run tests
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj

# Run backend
cd PatientAccess.Web
dotnet run
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/TokenUsageTrackerTests.cs`
- Test cases:
  1. **Test_LogTokenUsage_UpdatesExtraction**
     - Create extraction
     - Call LogTokenUsageAsync(extractionId, 100, 50, "gpt-4o")
     - Assert: PromptTokens=100, CompletionTokens=50, TotalTokens=150, EstimatedCost calculated
  2. **Test_CostCalculator_GPT4oPricing**
     - Call CalculateCost(1000, 500, "gpt-4o")
     - Assert: Cost = (1 * $0.005) + (0.5 * $0.015) = $0.0125
  3. **Test_DailyCeiling_WarningAt80Percent**
     - Set MaxTokens=100, increment to 80 tokens
     - Call CheckCeilingAsync()
     - Assert: Status = Warning
  4. **Test_DailyCeiling_ExceededAt100Percent**
     - Set MaxTokens=100, increment to 100 tokens
     - Call CheckCeilingAsync()
     - Assert: Status = Exceeded
  5. **Test_AIOperationalMetrics_ConfidenceDistribution**
     - Create extractions with confidence scores: 0.3, 0.6, 0.85
     - Call GetMetricsSummaryAsync()
     - Assert: LowConfidence=1, MediumConfidence=1, HighConfidence=1

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/TokenTrackingIntegrationTests.cs`
- Test cases:
  1. **Test_TokenUsage_EndToEnd**
     - Create extraction → Log token usage → Query database
     - Assert: Token/cost fields populated, daily aggregate incremented
  2. **Test_DailyCeiling_AlertTriggered**
     - Increment usage to 85%
     - Assert: CeilingStatus = Warning, Redis flag set

### Acceptance Criteria Validation
- **AC4**: ✅ Per-request token/cost logged, daily aggregates tracked, 80% alert + 100% halt implemented
- **AC5**: ✅ Model version, latency, error rates, confidence distributions visible in API

## Success Criteria Checklist
- [MANDATORY] CostCalculator with GPT-4o pricing ($0.005/1K prompt, $0.015/1K completion)
- [MANDATORY] TokenUsageTracker LogTokenUsageAsync updates ExtractedClinicalData (task_001 schema)
- [MANDATORY] LogTokenUsageAsync calculates EstimatedCost using CostCalculator
- [MANDATORY] DailyCeilingManager tracks daily aggregates in Redis (tokens + cost)
- [MANDATORY] DailyCeilingManager CheckCeilingAsync returns Normal/Warning/Exceeded (AC4)
- [MANDATORY] 80% threshold triggers Warning status (AC4)
- [MANDATORY] 100% threshold triggers Exceeded status (halts non-critical AI calls) (AC4)
- [MANDATORY] AIOperationalMetrics GetMetricsSummaryAsync aggregates model version, error rates, confidence distribution (AC5)
- [MANDATORY] ConfidenceDistribution histogram: Low (<50%), Medium (50-70%), High (>=70%)
- [MANDATORY] AIMetricsController GET /api/admin/ai-metrics (Admin-only) returns AIMetricsSummary
- [MANDATORY] AIMetricsController GET /api/admin/ai-metrics/daily-usage returns DailyUsage
- [MANDATORY] AIMetricsController GET /api/admin/ai-metrics/ceiling-status returns CeilingStatusResponse
- [MANDATORY] AIMetricsService Prometheus metrics: ai_token_usage_total, ai_cost_total_usd, ai_request_duration_seconds, ai_confidence_score
- [MANDATORY] Services registered in Program.cs
- [MANDATORY] Daily ceiling configuration in appsettings.json (MaxTokens, MaxCostUSD)
- [MANDATORY] AI pricing configuration in appsettings.json (per-model pricing)
- [RECOMMENDED] DailyUsageReportJob sends email alerts at 80%/100% thresholds
- [RECOMMENDED] AI_TOKEN_COST_MANAGEMENT.md documentation with cost optimization strategies

## Estimated Effort
**3 hours** (TokenUsageTracker + CostCalculator + DailyCeilingManager + AIOperationalMetrics + AIMetricsController + Prometheus integration + docs + tests)
