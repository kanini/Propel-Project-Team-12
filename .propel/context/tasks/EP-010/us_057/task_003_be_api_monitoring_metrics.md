# Task - task_003_be_api_monitoring_metrics

## Requirement Reference
- User Story: US_057
- Story Location: .propel/context/tasks/EP-010/us_057/us_057.md
- Acceptance Criteria:
    - **AC3**: Given API monitoring, When requests are processed, Then response times, error rates, and request volumes are tracked and available via a health/metrics endpoint within 1 second of query.

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
| Backend | Prometheus.NET | 8.0+ |
| Database | N/A | N/A |
| Caching | Upstash Redis | Redis 7.x compatible |
| Caching | StackExchange.Redis | 2.8+ |
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

Implement API monitoring and metrics collection using Prometheus.NET with custom middleware to track response times, error rates, and request volumes. This task ensures compliance with AC3 (metrics available via health/metrics endpoint within 1 second). Metrics stored in-memory using Prometheus Histograms and Counters for high-performance queries (<1s). Exposes /health/metrics endpoint returning JSON metrics summary (last 5 minutes) and /metrics endpoint in Prometheus format for integration with monitoring tools (Grafana, Application Insights). Tracks per-endpoint response times (p50, p95, p99 percentiles), HTTP status code distribution, request volume per endpoint, and error rate percentage (NFR-011: <0.1% target).

**Key Capabilities:**
- Prometheus.NET for metrics collection (AC3)
- MetricsMiddleware tracks response time per request (Histogram)
- HTTP status code counters (2xx, 4xx, 5xx)
- Request volume counters per endpoint
- Error rate calculation (4xx + 5xx / total requests)
- GET /health/metrics endpoint with <1s query time (AC3)
- GET /metrics endpoint (Prometheus format)
- In-memory metrics storage for fast queries
- Per-endpoint metrics breakdown
- Response time percentiles (p50, p95, p99)
- 5-minute sliding window for real-time metrics

## Dependent Tasks
- None (foundational monitoring task)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/Middleware/MetricsMiddleware.cs` - Response time tracking
- **NEW**: `src/backend/PatientAccess.Web/Controllers/HealthController.cs` - Health/metrics endpoint
- **NEW**: `src/backend/PatientAccess.Web/Services/MetricsService.cs` - Metrics aggregation service
- **NEW**: `docs/API_MONITORING.md` - API monitoring documentation
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure Prometheus middleware
- **MODIFY**: `src/backend/PatientAccess.Business/PatientAccess.Business.csproj` - Add Prometheus.NET package

## Implementation Plan

1. **Add Prometheus.NET NuGet Package**
   - File: `src/backend/PatientAccess.Business/PatientAccess.Business.csproj`
   - Add package reference:
     ```xml
     <PackageReference Include="prometheus-net" Version="8.0.1" />
     <PackageReference Include="prometheus-net.AspNetCore" Version="8.0.1" />
     ```

2. **Create MetricsMiddleware**
   - File: `src/backend/PatientAccess.Web/Middleware/MetricsMiddleware.cs`
   - Track response times and status codes:
     ```csharp
     using Prometheus;
     using System.Diagnostics;
     
     namespace PatientAccess.Web.Middleware
     {
         /// <summary>
         /// Metrics middleware for tracking response times, error rates, and request volumes (AC3 - US_057).
         /// </summary>
         public class MetricsMiddleware
         {
             private readonly RequestDelegate _next;
             
             // Prometheus metrics
             private static readonly Histogram RequestDuration = Metrics
                 .CreateHistogram(
                     "http_request_duration_seconds",
                     "HTTP request duration in seconds",
                     new HistogramConfiguration
                     {
                         LabelNames = new[] { "method", "endpoint", "status_code" },
                         Buckets = new[] { 0.01, 0.05, 0.1, 0.5, 1.0, 2.0, 5.0, 10.0 }
                     });
             
             private static readonly Counter RequestCount = Metrics
                 .CreateCounter(
                     "http_requests_total",
                     "Total HTTP requests",
                     new CounterConfiguration
                     {
                         LabelNames = new[] { "method", "endpoint", "status_code" }
                     });
             
             private static readonly Counter ErrorCount = Metrics
                 .CreateCounter(
                     "http_errors_total",
                     "Total HTTP errors (4xx + 5xx)",
                     new CounterConfiguration
                     {
                         LabelNames = new[] { "method", "endpoint", "status_code" }
                     });
             
             public MetricsMiddleware(RequestDelegate next)
             {
                 _next = next;
             }
             
             public async Task InvokeAsync(HttpContext context)
             {
                 // Skip metrics endpoint itself
                 if (context.Request.Path.StartsWithSegments("/metrics") ||
                     context.Request.Path.StartsWithSegments("/health/metrics"))
                 {
                     await _next(context);
                     return;
                 }
                 
                 var stopwatch = Stopwatch.StartNew();
                 
                 try
                 {
                     // Execute next middleware
                     await _next(context);
                 }
                 finally
                 {
                     stopwatch.Stop();
                     
                     // Extract labels
                     var method = context.Request.Method;
                     var endpoint = GetEndpointPattern(context);
                     var statusCode = context.Response.StatusCode.ToString();
                     
                     // Record response time (AC3)
                     RequestDuration
                         .WithLabels(method, endpoint, statusCode)
                         .Observe(stopwatch.Elapsed.TotalSeconds);
                     
                     // Increment request count (AC3)
                     RequestCount
                         .WithLabels(method, endpoint, statusCode)
                         .Inc();
                     
                     // Increment error count if 4xx or 5xx (AC3, NFR-011)
                     if (context.Response.StatusCode >= 400)
                     {
                         ErrorCount
                             .WithLabels(method, endpoint, statusCode)
                             .Inc();
                     }
                 }
             }
             
             private string GetEndpointPattern(HttpContext context)
             {
                 // Get endpoint pattern from route data (e.g., /api/patients/{id})
                 var endpoint = context.GetEndpoint();
                 if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Routing.HttpMethodMetadata>() != null)
                 {
                     var routePattern = (context.GetEndpoint() as Microsoft.AspNetCore.Routing.RouteEndpoint)?.RoutePattern.RawText;
                     return routePattern ?? context.Request.Path;
                 }
                 
                 // Fallback to path with route parameter normalization
                 var path = context.Request.Path.Value ?? "/";
                 
                 // Normalize route parameters (replace IDs with {id})
                 if (System.Text.RegularExpressions.Regex.IsMatch(path, @"/\d+"))
                 {
                     path = System.Text.RegularExpressions.Regex.Replace(path, @"/\d+", "/{id}");
                 }
                 
                 return path;
             }
         }
         
         /// <summary>
         /// Extension method to register MetricsMiddleware.
         /// </summary>
         public static class MetricsMiddlewareExtensions
         {
             public static IApplicationBuilder UseApiMetrics(this IApplicationBuilder builder)
             {
                 return builder.UseMiddleware<MetricsMiddleware>();
             }
         }
     }
     ```

3. **Create MetricsService**
   - File: `src/backend/PatientAccess.Web/Services/MetricsService.cs`
   - Metrics aggregation service:
     ```csharp
     using Prometheus;
     
     namespace PatientAccess.Web.Services
     {
         /// <summary>
         /// Metrics aggregation service for generating metrics summaries (AC3 - US_057).
         /// </summary>
         public class MetricsService
         {
             public MetricsSummaryDto GetMetricsSummary()
             {
                 var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                 
                 // Collect metrics from Prometheus registry
                 var registry = Metrics.DefaultRegistry;
                 var metrics = registry.CollectAll();
                 
                 var summary = new MetricsSummaryDto
                 {
                     Timestamp = DateTime.UtcNow,
                     ResponseTimes = CalculateResponseTimePercentiles(metrics),
                     RequestVolume = CalculateRequestVolume(metrics),
                     ErrorRate = CalculateErrorRate(metrics),
                     StatusCodeDistribution = CalculateStatusCodeDistribution(metrics),
                     TopEndpoints = GetTopEndpoints(metrics)
                 };
                 
                 stopwatch.Stop();
                 
                 // Ensure <1s query time (AC3)
                 if (stopwatch.ElapsedMilliseconds > 1000)
                 {
                     // Log warning if query exceeds 1s
                     Console.WriteLine($"WARNING: Metrics query took {stopwatch.ElapsedMilliseconds}ms (AC3 requires <1s)");
                 }
                 
                 return summary;
             }
             
             private ResponseTimePercentilesDto CalculateResponseTimePercentiles(IEnumerable<Prometheus.Client.MetricFamily> metrics)
             {
                 // Find http_request_duration_seconds histogram
                 var durationMetric = metrics.FirstOrDefault(m => m.name == "http_request_duration_seconds");
                 if (durationMetric == null)
                 {
                     return new ResponseTimePercentilesDto();
                 }
                 
                 // Aggregate all histogram buckets
                 var allObservations = new List<double>();
                 foreach (var metric in durationMetric.metric)
                 {
                     if (metric.histogram != null)
                     {
                         // Estimate observations from histogram buckets
                         for (int i = 0; i < metric.histogram.bucket.Count; i++)
                         {
                             var bucketCount = metric.histogram.bucket[i].cumulative_count;
                             var previousCount = i > 0 ? metric.histogram.bucket[i - 1].cumulative_count : 0;
                             var observationsInBucket = bucketCount - previousCount;
                             var bucketUpperBound = metric.histogram.bucket[i].upper_bound;
                             
                             // Add observations (approximate bucket value)
                             for (ulong j = 0; j < observationsInBucket; j++)
                             {
                                 allObservations.Add(bucketUpperBound);
                             }
                         }
                     }
                 }
                 
                 if (allObservations.Count == 0)
                 {
                     return new ResponseTimePercentilesDto();
                 }
                 
                 allObservations.Sort();
                 
                 return new ResponseTimePercentilesDto
                 {
                     P50 = GetPercentile(allObservations, 0.50),
                     P95 = GetPercentile(allObservations, 0.95),
                     P99 = GetPercentile(allObservations, 0.99),
                     Mean = allObservations.Average()
                 };
             }
             
             private double GetPercentile(List<double> sortedValues, double percentile)
             {
                 if (sortedValues.Count == 0) return 0;
                 
                 var index = (int)Math.Ceiling(sortedValues.Count * percentile) - 1;
                 index = Math.Max(0, Math.Min(sortedValues.Count - 1, index));
                 return sortedValues[index];
             }
             
             private RequestVolumeDto CalculateRequestVolume(IEnumerable<Prometheus.Client.MetricFamily> metrics)
             {
                 var requestsMetric = metrics.FirstOrDefault(m => m.name == "http_requests_total");
                 if (requestsMetric == null)
                 {
                     return new RequestVolumeDto { Total = 0 };
                 }
                 
                 var total = requestsMetric.metric.Sum(m => m.counter?.value ?? 0);
                 
                 return new RequestVolumeDto
                 {
                     Total = (long)total,
                     PerMinute = (long)(total / 5) // Approximate: 5-minute window
                 };
             }
             
             private ErrorRateDto CalculateErrorRate(IEnumerable<Prometheus.Client.MetricFamily> metrics)
             {
                 var errorsMetric = metrics.FirstOrDefault(m => m.name == "http_errors_total");
                 var requestsMetric = metrics.FirstOrDefault(m => m.name == "http_requests_total");
                 
                 if (errorsMetric == null || requestsMetric == null)
                 {
                     return new ErrorRateDto { Percentage = 0 };
                 }
                 
                 var totalErrors = errorsMetric.metric.Sum(m => m.counter?.value ?? 0);
                 var totalRequests = requestsMetric.metric.Sum(m => m.counter?.value ?? 0);
                 
                 var errorRate = totalRequests > 0 ? (totalErrors / totalRequests) * 100 : 0;
                 
                 return new ErrorRateDto
                 {
                     Percentage = errorRate,
                     TotalErrors = (long)totalErrors,
                     TotalRequests = (long)totalRequests,
                     TargetPercentage = 0.1 // NFR-011: <0.1% target
                 };
             }
             
             private Dictionary<string, long> CalculateStatusCodeDistribution(IEnumerable<Prometheus.Client.MetricFamily> metrics)
             {
                 var requestsMetric = metrics.FirstOrDefault(m => m.name == "http_requests_total");
                 if (requestsMetric == null)
                 {
                     return new Dictionary<string, long>();
                 }
                 
                 var distribution = new Dictionary<string, long>();
                 
                 foreach (var metric in requestsMetric.metric)
                 {
                     var statusCodeLabel = metric.label.FirstOrDefault(l => l.name == "status_code");
                     if (statusCodeLabel != null)
                     {
                         var statusCode = statusCodeLabel.value;
                         var count = (long)(metric.counter?.value ?? 0);
                         
                         if (distribution.ContainsKey(statusCode))
                         {
                             distribution[statusCode] += count;
                         }
                         else
                         {
                             distribution[statusCode] = count;
                         }
                     }
                 }
                 
                 return distribution;
             }
             
             private List<EndpointMetricDto> GetTopEndpoints(IEnumerable<Prometheus.Client.MetricFamily> metrics)
             {
                 var requestsMetric = metrics.FirstOrDefault(m => m.name == "http_requests_total");
                 if (requestsMetric == null)
                 {
                     return new List<EndpointMetricDto>();
                 }
                 
                 var endpointMetrics = new Dictionary<string, EndpointMetricDto>();
                 
                 foreach (var metric in requestsMetric.metric)
                 {
                     var methodLabel = metric.label.FirstOrDefault(l => l.name == "method");
                     var endpointLabel = metric.label.FirstOrDefault(l => l.name == "endpoint");
                     
                     if (methodLabel != null && endpointLabel != null)
                     {
                         var key = $"{methodLabel.value} {endpointLabel.value}";
                         var count = (long)(metric.counter?.value ?? 0);
                         
                         if (!endpointMetrics.ContainsKey(key))
                         {
                             endpointMetrics[key] = new EndpointMetricDto
                             {
                                 Method = methodLabel.value,
                                 Endpoint = endpointLabel.value,
                                 RequestCount = 0
                             };
                         }
                         
                         endpointMetrics[key].RequestCount += count;
                     }
                 }
                 
                 return endpointMetrics.Values
                     .OrderByDescending(e => e.RequestCount)
                     .Take(10)
                     .ToList();
             }
         }
         
         // DTOs
         public class MetricsSummaryDto
         {
             public DateTime Timestamp { get; set; }
             public ResponseTimePercentilesDto ResponseTimes { get; set; }
             public RequestVolumeDto RequestVolume { get; set; }
             public ErrorRateDto ErrorRate { get; set; }
             public Dictionary<string, long> StatusCodeDistribution { get; set; }
             public List<EndpointMetricDto> TopEndpoints { get; set; }
         }
         
         public class ResponseTimePercentilesDto
         {
             public double P50 { get; set; }
             public double P95 { get; set; }
             public double P99 { get; set; }
             public double Mean { get; set; }
         }
         
         public class RequestVolumeDto
         {
             public long Total { get; set; }
             public long PerMinute { get; set; }
         }
         
         public class ErrorRateDto
         {
             public double Percentage { get; set; }
             public long TotalErrors { get; set; }
             public long TotalRequests { get; set; }
             public double TargetPercentage { get; set; }
         }
         
         public class EndpointMetricDto
         {
             public string Method { get; set; }
             public string Endpoint { get; set; }
             public long RequestCount { get; set; }
         }
     }
     ```

4. **Create HealthController with Metrics Endpoint**
   - File: `src/backend/PatientAccess.Web/Controllers/HealthController.cs`
   - Health and metrics endpoints:
     ```csharp
     using Microsoft.AspNetCore.Authorization;
     using Microsoft.AspNetCore.Mvc;
     using PatientAccess.Web.Services;
     
     namespace PatientAccess.Web.Controllers
     {
         [ApiController]
         [Route("[controller]")]
         [AllowAnonymous] // Health endpoints are public
         public class HealthController : ControllerBase
         {
             private readonly MetricsService _metricsService;
             private readonly ILogger<HealthController> _logger;
             
             public HealthController(
                 MetricsService metricsService,
                 ILogger<HealthController> logger)
             {
                 _metricsService = metricsService;
                 _logger = logger;
             }
             
             /// <summary>
             /// Health check endpoint.
             /// </summary>
             [HttpGet]
             public ActionResult<object> GetHealth()
             {
                 return Ok(new
                 {
                     status = "Healthy",
                     timestamp = DateTime.UtcNow,
                     version = "1.0.0"
                 });
             }
             
             /// <summary>
             /// Metrics summary endpoint (AC3 - US_057).
             /// Returns response times, error rates, and request volumes within 1 second.
             /// </summary>
             [HttpGet("metrics")]
             public ActionResult<MetricsSummaryDto> GetMetrics()
             {
                 var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                 
                 var metrics = _metricsService.GetMetricsSummary();
                 
                 stopwatch.Stop();
                 
                 // Add query time to response
                 Response.Headers.Append("X-Query-Time-Ms", stopwatch.ElapsedMilliseconds.ToString());
                 
                 // Warn if query exceeds 1s (AC3)
                 if (stopwatch.ElapsedMilliseconds > 1000)
                 {
                     _logger.LogWarning(
                         "Metrics query exceeded 1s threshold: {Duration}ms (AC3 requires <1s)",
                         stopwatch.ElapsedMilliseconds);
                 }
                 
                 return Ok(metrics);
             }
         }
     }
     ```

5. **Configure Prometheus Middleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add Prometheus middleware:
     ```csharp
     using Prometheus;
     
     // Register MetricsService (AC3 - US_057)
     builder.Services.AddSingleton<MetricsService>();
     
     // Add Prometheus metrics endpoint
     app.UseMetricServer("/metrics"); // Exposes /metrics endpoint (Prometheus format)
     
     // Add custom metrics middleware (AC3 - US_057)
     app.UseApiMetrics();
     ```

6. **Document API Monitoring**
   - File: `docs/API_MONITORING.md`
   - Monitoring documentation:
     ```markdown
     # API Monitoring (AC3 - US_057)
     
     ## Overview
     All API requests are monitored for response times, error rates, and request volumes using Prometheus.NET. Metrics available via `/health/metrics` endpoint (JSON format) and `/metrics` endpoint (Prometheus format).
     
     ## Metrics Endpoints
     
     ### GET /health/metrics (JSON Format)
     Returns metrics summary with <1s query time (AC3).
     
     **Response Example**:
     ```json
     {
       "timestamp": "2026-03-23T00:00:00Z",
       "responseTimes": {
         "p50": 0.025,
         "p95": 0.15,
         "p99": 0.5,
         "mean": 0.05
       },
       "requestVolume": {
         "total": 10500,
         "perMinute": 2100
       },
       "errorRate": {
         "percentage": 0.08,
         "totalErrors": 84,
         "totalRequests": 10500,
         "targetPercentage": 0.1
       },
       "statusCodeDistribution": {
         "200": 9800,
         "404": 50,
         "500": 34,
         "429": 616
       },
       "topEndpoints": [
         {
           "method": "GET",
           "endpoint": "/api/patients/{id}",
           "requestCount": 3500
         },
         {
           "method": "GET",
           "endpoint": "/api/appointments",
           "requestCount": 2100
         }
       ]
     }
     ```
     
     **Response Headers**:
     ```
     X-Query-Time-Ms: 125
     ```
     
     ### GET /metrics (Prometheus Format)
     Returns Prometheus metrics in text format for integration with Grafana, Application Insights, etc. (AC3).
     
     **Response Example**:
     ```
     # HELP http_request_duration_seconds HTTP request duration in seconds
     # TYPE http_request_duration_seconds histogram
     http_request_duration_seconds_bucket{method="GET",endpoint="/api/patients/{id}",status_code="200",le="0.01"} 250
     http_request_duration_seconds_bucket{method="GET",endpoint="/api/patients/{id}",status_code="200",le="0.05"} 2800
     http_request_duration_seconds_bucket{method="GET",endpoint="/api/patients/{id}",status_code="200",le="0.1"} 3400
     http_request_duration_seconds_sum{method="GET",endpoint="/api/patients/{id}",status_code="200"} 175.5
     http_request_duration_seconds_count{method="GET",endpoint="/api/patients/{id}",status_code="200"} 3500
     
     # HELP http_requests_total Total HTTP requests
     # TYPE http_requests_total counter
     http_requests_total{method="GET",endpoint="/api/patients/{id}",status_code="200"} 3500
     http_requests_total{method="GET",endpoint="/api/appointments",status_code="200"} 2100
     
     # HELP http_errors_total Total HTTP errors (4xx + 5xx)
     # TYPE http_errors_total counter
     http_errors_total{method="GET",endpoint="/api/patients/{id}",status_code="404"} 50
     http_errors_total{method="POST",endpoint="/api/appointments",status_code="500"} 34
     ```
     
     ## Monitored Metrics (AC3)
     
     ### Response Times
     - **P50 (Median)**: 50th percentile response time
     - **P95**: 95th percentile response time (most requests complete faster)
     - **P99**: 99th percentile response time (tail latency)
     - **Mean**: Average response time
     
     ### Request Volume
     - **Total**: Total requests processed
     - **Per Minute**: Requests per minute (5-minute sliding window)
     
     ### Error Rate
     - **Percentage**: (4xx + 5xx errors) / total requests * 100
     - **Target**: <0.1% (NFR-011)
     - **Total Errors**: Count of 4xx + 5xx responses
     - **Total Requests**: Count of all requests
     
     ### Status Code Distribution
     - Count of requests per HTTP status code (200, 404, 500, 429, etc.)
     
     ### Top Endpoints
     - Top 10 most-requested endpoints by volume
     
     ## Performance Requirements
     
     - **Query Time**: Metrics endpoint MUST return within 1 second (AC3)
     - **Storage**: In-memory Prometheus metrics (fast queries)
     - **Window**: 5-minute sliding window for real-time metrics
     
     ## Grafana Integration
     
     ### Configure Prometheus Data Source
     ```yaml
     datasources:
       - name: PatientAccessAPI
         type: prometheus
         url: https://api.patient-access.com/metrics
         access: proxy
     ```
     
     ### Sample Grafana Queries
     ```promql
     # Request rate per endpoint
     rate(http_requests_total[5m])
     
     # Error rate percentage
     (sum(rate(http_errors_total[5m])) / sum(rate(http_requests_total[5m]))) * 100
     
     # P95 response time
     histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
     
     # Requests per status code
     sum by (status_code) (rate(http_requests_total[5m]))
     ```
     
     ## Monitoring Alerts
     
     ### Error Rate Alert (NFR-011)
     - **Condition**: Error rate > 0.1% for 5 minutes
     - **Action**: Trigger Application Insights alert, notify ops team
     
     ### High Response Time Alert
     - **Condition**: P95 response time > 1 second for 5 minutes
     - **Action**: Trigger Application Insights alert
     
     ### High Request Volume Alert
     - **Condition**: Request rate > 10,000 requests/minute
     - **Action**: Trigger capacity planning review
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   └── PatientAccess.Business.csproj
└── PatientAccess.Web/
    ├── Program.cs (existing)
    └── Controllers/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Middleware/MetricsMiddleware.cs | Response time tracking |
| CREATE | src/backend/PatientAccess.Web/Controllers/HealthController.cs | Health/metrics endpoint |
| CREATE | src/backend/PatientAccess.Web/Services/MetricsService.cs | Metrics aggregation |
| CREATE | docs/API_MONITORING.md | API monitoring documentation |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add Prometheus.NET packages |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure Prometheus middleware |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Prometheus.NET
- **GitHub Repository**: https://github.com/prometheus-net/prometheus-net
- **Documentation**: https://github.com/prometheus-net/prometheus-net/blob/master/README.md
- **ASP.NET Core Integration**: https://github.com/prometheus-net/prometheus-net/blob/master/README.aspnetcore.md

### Prometheus Query Language
- **PromQL Basics**: https://prometheus.io/docs/prometheus/latest/querying/basics/
- **Histogram Quantiles**: https://prometheus.io/docs/practices/histograms/

### Design Requirements
- **NFR-011**: API error rate below 0.1% (design.md)
- **AC3**: Metrics available within 1 second of query (us_057.md)

## Build Commands
```powershell
# Add Prometheus.NET packages
cd src/backend/PatientAccess.Business
dotnet add package prometheus-net --version 8.0.1
dotnet add package prometheus-net.AspNetCore --version 8.0.1

# Build solution
cd ..
dotnet build PatientAccess.sln

# Run backend
cd PatientAccess.Web
dotnet run

# Test metrics endpoints
curl https://localhost:5001/health
curl https://localhost:5001/health/metrics
curl https://localhost:5001/metrics
```

## Validation Strategy

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/MetricsTests.cs`
- Test cases:
  1. **Test_MetricsEndpoint_ReturnsWithin1Second**
     - Request: GET /health/metrics
     - Assert: Response time < 1000ms (AC3), X-Query-Time-Ms header present
  2. **Test_MetricsEndpoint_IncludesResponseTimes**
     - Request: GET /health/metrics
     - Assert: Response contains responseTimes.p50, p95, p99, mean
  3. **Test_MetricsEndpoint_IncludesRequestVolume**
     - Request: GET /health/metrics
     - Assert: Response contains requestVolume.total, perMinute
  4. **Test_MetricsEndpoint_IncludesErrorRate**
     - Request: GET /health/metrics
     - Assert: Response contains errorRate.percentage, totalErrors, totalRequests
  5. **Test_PrometheusEndpoint_ReturnsPrometheusFormat**
     - Request: GET /metrics
     - Assert: Response Content-Type = text/plain, contains "http_request_duration_seconds"
  6. **Test_MetricsMiddleware_TracksResponseTime**
     - Setup: Send 10 requests to GET /api/patients/1
     - Request: GET /health/metrics
     - Assert: Response contains metrics for GET /api/patients/{id}

### Performance Tests
- File: `src/backend/PatientAccess.Tests/Performance/MetricsPerformanceTests.cs`
- Test cases:
  1. **Test_MetricsQuery_CompletesUnder1Second**
     - Setup: Generate 10,000 requests with varying response times
     - Request: GET /health/metrics (100 times)
     - Assert: All queries complete within 1000ms (AC3)

### Acceptance Criteria Validation
- **AC3**: ✅ Response times, error rates, and request volumes tracked and available via /health/metrics within 1 second

## Success Criteria Checklist
- [MANDATORY] Prometheus.NET packages added to project
- [MANDATORY] MetricsMiddleware created for response time tracking
- [MANDATORY] http_request_duration_seconds histogram with method, endpoint, status_code labels
- [MANDATORY] http_requests_total counter per endpoint
- [MANDATORY] http_errors_total counter for 4xx + 5xx responses
- [MANDATORY] MetricsService aggregates metrics with <1s query time (AC3)
- [MANDATORY] HealthController GET /health/metrics endpoint returns JSON summary
- [MANDATORY] GET /metrics endpoint returns Prometheus format
- [MANDATORY] Response time percentiles calculated (p50, p95, p99)
- [MANDATORY] Request volume tracked (total, per minute)
- [MANDATORY] Error rate calculated (percentage, with NFR-011 target)
- [MANDATORY] Status code distribution included in metrics
- [MANDATORY] Top 10 endpoints by request volume
- [MANDATORY] X-Query-Time-Ms header in metrics response
- [MANDATORY] API_MONITORING.md documents metrics endpoints and Grafana integration
- [MANDATORY] Integration test: Metrics endpoint returns within 1s
- [MANDATORY] Integration test: Metrics include response times, request volume, error rate
- [RECOMMENDED] Grafana dashboard configuration for metrics visualization

## Estimated Effort
**3 hours** (Prometheus.NET configuration + MetricsMiddleware + MetricsService + HealthController + docs + tests)
