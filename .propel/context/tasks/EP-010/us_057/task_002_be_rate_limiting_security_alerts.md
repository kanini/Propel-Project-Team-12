# Task - task_002_be_rate_limiting_security_alerts

## Requirement Reference
- User Story: US_057
- Story Location: .propel/context/tasks/EP-010/us_057/us_057.md
- Acceptance Criteria:
    - **AC2**: Given rate limiting (NFR-011), When a client exceeds the rate limit threshold, Then the API returns 429 Too Many Requests with a Retry-After header, and the event is logged to the audit system.
    - **AC4**: Given suspicious activity, When repeated 401/403 errors occur from the same IP (>10 in 5 minutes), Then the system logs a security alert and optionally triggers temporary IP throttling.

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
| Backend | AspNetCoreRateLimit | 5.0+ |
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

Implement rate limiting middleware using AspNetCoreRateLimit library with Redis backend storage for distributed rate limiting across multiple server instances. This task ensures compliance with AC2 (rate limiting with 429 responses and audit logging) and AC4 (security alert detection for suspicious activity). Rate limits configured per IP address and per endpoint with different thresholds for authenticated vs. anonymous requests. When rate limit exceeded, API returns HTTP 429 Too Many Requests with Retry-After header indicating wait time. Additionally, implements SecurityAlertMiddleware to detect repeated 401/403 errors from same IP (>10 in 5 minutes) and triggers security alerts with optional IP throttling.

**Key Capabilities:**
- AspNetCoreRateLimit middleware for rate limiting (AC2)
- Redis-backed distributed rate limiting (supports multi-instance deployment)
- IP-based rate limits: 100 requests/minute for authenticated, 20 requests/minute for anonymous
- Endpoint-specific rate limits (lower for resource-intensive endpoints)
- 429 Too Many Requests response with Retry-After header (AC2)
- Audit logging of rate limit violations (AC2)
- SecurityAlertMiddleware for suspicious activity detection (AC4)
- IP-based 401/403 error tracking (Redis counter with 5-minute sliding window)
- Security alert logging when threshold exceeded (>10 errors in 5 minutes)
- Optional IP throttling (reduce rate limit to 5 requests/minute for 1 hour)
- Public endpoint protection (login, registration) with CAPTCHA threshold

## Dependent Tasks
- US_055: task_002 (AuditLoggingService for audit logging)
- US_056: task_003 (Redis caching infrastructure)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/Middleware/SecurityAlertMiddleware.cs` - Security alert detection
- **NEW**: `src/backend/PatientAccess.Web/Configuration/RateLimitConfiguration.cs` - Rate limit settings
- **NEW**: `docs/RATE_LIMITING.md` - Rate limiting documentation
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure rate limiting middleware
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add rate limit configuration
- **MODIFY**: `src/backend/PatientAccess.Business/PatientAccess.Business.csproj` - Add AspNetCoreRateLimit package

## Implementation Plan

1. **Add AspNetCoreRateLimit NuGet Package**
   - File: `src/backend/PatientAccess.Business/PatientAccess.Business.csproj`
   - Add package reference:
     ```xml
     <PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
     ```

2. **Configure Rate Limiting Settings**
   - File: `src/backend/PatientAccess.Web/appsettings.json`
   - Rate limit configuration:
     ```json
     {
       "IpRateLimiting": {
         "EnableEndpointRateLimiting": true,
         "StackBlockedRequests": false,
         "RealIpHeader": "X-Forwarded-For",
         "ClientIdHeader": "X-ClientId",
         "HttpStatusCode": 429,
         "GeneralRules": [
           {
             "Endpoint": "*",
             "Period": "1m",
             "Limit": 100
           },
           {
             "Endpoint": "*",
             "Period": "1h",
             "Limit": 1000
           },
           {
             "Endpoint": "POST:/api/auth/login",
             "Period": "5m",
             "Limit": 5
           },
           {
             "Endpoint": "POST:/api/auth/register",
             "Period": "1h",
             "Limit": 3
           },
           {
             "Endpoint": "GET:/api/patients/*",
             "Period": "1m",
             "Limit": 60
           },
           {
             "Endpoint": "POST:/api/appointments",
             "Period": "1m",
             "Limit": 10
           }
         ],
         "ClientWhitelist": []
       },
       "IpRateLimitPolicies": {
         "IpRules": [
           {
             "Ip": "127.0.0.1",
             "Rules": [
               {
                 "Endpoint": "*",
                 "Period": "1s",
                 "Limit": 10000
               }
             ]
           }
         ]
       }
     }
     ```

3. **Configure Rate Limiting Middleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add rate limiting services before `builder.Build()`:
     ```csharp
     using AspNetCoreRateLimit;
     
     // Configure Rate Limiting with Redis (AC2 - US_057)
     builder.Services.AddMemoryCache();
     
     // Load rate limit configuration
     builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
     builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
     
     // Redis-backed distributed rate limiting (for multi-instance deployment)
     builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
     builder.Services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
     
     // Rate limit configuration
     builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
     
     // Processing strategy
     builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
     ```

4. **Add Rate Limiting Middleware to Pipeline**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add after forwarded headers, before authentication:
     ```csharp
     // Add IP rate limiting middleware (AC2 - US_057)
     app.UseIpRateLimiting();
     ```

5. **Create Custom RateLimitConfiguration**
   - File: `src/backend/PatientAccess.Web/Configuration/RateLimitConfiguration.cs`
   - Custom configuration with audit logging:
     ```csharp
     using AspNetCoreRateLimit;
     using PatientAccess.Business.Interfaces;
     
     namespace PatientAccess.Web.Configuration
     {
         /// <summary>
         /// Custom rate limit configuration with audit logging (AC2 - US_057).
         /// </summary>
         public class RateLimitConfiguration : RateLimitConfiguration
         {
             private readonly IAuditLoggingService _auditLoggingService;
             private readonly ILogger<RateLimitConfiguration> _logger;
             
             public RateLimitConfiguration(
                 IHttpContextAccessor httpContextAccessor,
                 IOptions<IpRateLimitOptions> ipOptions,
                 IAuditLoggingService auditLoggingService,
                 ILogger<RateLimitConfiguration> logger)
                 : base(httpContextAccessor, ipOptions)
             {
                 _auditLoggingService = auditLoggingService;
                 _logger = logger;
                 
                 // Register rate limit event handler
                 RegisterRateLimitEventHandler();
             }
             
             private void RegisterRateLimitEventHandler()
             {
                 // Hook into rate limit middleware events
                 // Note: AspNetCoreRateLimit doesn't have built-in events, 
                 // so we'll handle this in SecurityAlertMiddleware instead
             }
         }
     }
     ```

6. **Create SecurityAlertMiddleware (AC4)**
   - File: `src/backend/PatientAccess.Web/Middleware/SecurityAlertMiddleware.cs`
   - Suspicious activity detection:
     ```csharp
     using StackExchange.Redis;
     using PatientAccess.Business.Interfaces;
     
     namespace PatientAccess.Web.Middleware
     {
         /// <summary>
         /// Security alert middleware for detecting suspicious activity (AC4 - US_057).
         /// Tracks repeated 401/403 errors from same IP and triggers alerts when threshold exceeded.
         /// </summary>
         public class SecurityAlertMiddleware
         {
             private readonly RequestDelegate _next;
             private readonly IDatabase _redis;
             private readonly IAuditLoggingService _auditLoggingService;
             private readonly ILogger<SecurityAlertMiddleware> _logger;
             
             // Security alert thresholds (AC4)
             private const int MaxAuthFailuresInWindow = 10;
             private static readonly TimeSpan AlertWindow = TimeSpan.FromMinutes(5);
             private static readonly TimeSpan ThrottleDuration = TimeSpan.FromHours(1);
             
             public SecurityAlertMiddleware(
                 RequestDelegate next,
                 IConnectionMultiplexer redis,
                 IAuditLoggingService auditLoggingService,
                 ILogger<SecurityAlertMiddleware> logger)
             {
                 _next = next;
                 _redis = redis.GetDatabase();
                 _auditLoggingService = auditLoggingService;
                 _logger = logger;
             }
             
             public async Task InvokeAsync(HttpContext context)
             {
                 // Capture original response stream
                 var originalBodyStream = context.Response.Body;
                 
                 try
                 {
                     // Execute next middleware
                     await _next(context);
                     
                     // Check for 401/403 responses (AC4)
                     if (context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                         context.Response.StatusCode == StatusCodes.Status403Forbidden)
                     {
                         await HandleAuthorizationFailure(context);
                     }
                     
                     // Check for 429 responses (rate limit) and log to audit (AC2)
                     if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                     {
                         await LogRateLimitViolation(context);
                     }
                 }
                 finally
                 {
                     context.Response.Body = originalBodyStream;
                 }
             }
             
             private async Task HandleAuthorizationFailure(HttpContext context)
             {
                 var ipAddress = GetClientIpAddress(context);
                 var redisKey = $"auth:failures:{ipAddress}";
                 
                 // Increment failure counter with 5-minute expiry (sliding window)
                 var failureCount = await _redis.StringIncrementAsync(redisKey);
                 
                 // Set expiry if this is the first failure
                 if (failureCount == 1)
                 {
                     await _redis.KeyExpireAsync(redisKey, AlertWindow);
                 }
                 
                 // Check if threshold exceeded (AC4: >10 in 5 minutes)
                 if (failureCount > MaxAuthFailuresInWindow)
                 {
                     await TriggerSecurityAlert(context, ipAddress, (int)failureCount);
                 }
             }
             
             private async Task TriggerSecurityAlert(HttpContext context, string ipAddress, int failureCount)
             {
                 // Log security alert (AC4)
                 _logger.LogWarning(
                     "SECURITY ALERT: Suspicious activity detected from IP {IpAddress}. " +
                     "{FailureCount} authorization failures in 5 minutes. Threshold: {Threshold}.",
                     ipAddress, failureCount, MaxAuthFailuresInWindow);
                 
                 // Log to audit system (AC4)
                 await _auditLoggingService.LogActionAsync(
                     userId: null,
                     actionType: "SecurityAlert",
                     resourceType: "IP",
                     resourceId: ipAddress,
                     result: "Suspicious",
                     actionDetails: $"{{\"failureCount\": {failureCount}, \"window\": \"5m\", \"threshold\": {MaxAuthFailuresInWindow}}}",
                     ipAddress: ipAddress,
                     userAgent: context.Request.Headers["User-Agent"].ToString(),
                     sessionId: null
                 );
                 
                 // Optional: Trigger IP throttling (AC4)
                 await ThrottleIpAddress(ipAddress);
             }
             
             private async Task ThrottleIpAddress(string ipAddress)
             {
                 // Store throttled IP in Redis with 1-hour expiry
                 var throttleKey = $"throttle:ip:{ipAddress}";
                 await _redis.StringSetAsync(throttleKey, "true", ThrottleDuration);
                 
                 _logger.LogWarning(
                     "IP throttling activated for {IpAddress}. Duration: {Duration} hour(s).",
                     ipAddress, ThrottleDuration.TotalHours);
             }
             
             private async Task LogRateLimitViolation(HttpContext context)
             {
                 var ipAddress = GetClientIpAddress(context);
                 var endpoint = $"{context.Request.Method}:{context.Request.Path}";
                 
                 // Log rate limit violation to audit system (AC2)
                 await _auditLoggingService.LogActionAsync(
                     userId: null,
                     actionType: "RateLimitExceeded",
                     resourceType: "API",
                     resourceId: endpoint,
                     result: "Denied",
                     actionDetails: $"{{\"endpoint\": \"{endpoint}\"}}",
                     ipAddress: ipAddress,
                     userAgent: context.Request.Headers["User-Agent"].ToString(),
                     sessionId: null
                 );
                 
                 _logger.LogWarning(
                     "Rate limit exceeded for IP {IpAddress} on endpoint {Endpoint}.",
                     ipAddress, endpoint);
             }
             
             private string GetClientIpAddress(HttpContext context)
             {
                 // Check X-Forwarded-For header first (Railway/Render proxy)
                 var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                 if (!string.IsNullOrEmpty(forwardedFor))
                 {
                     return forwardedFor.Split(',')[0].Trim();
                 }
                 
                 // Fallback to RemoteIpAddress
                 return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
             }
         }
         
         /// <summary>
         /// Extension method to register SecurityAlertMiddleware.
         /// </summary>
         public static class SecurityAlertMiddlewareExtensions
         {
             public static IApplicationBuilder UseSecurityAlerts(this IApplicationBuilder builder)
             {
                 return builder.UseMiddleware<SecurityAlertMiddleware>();
             }
         }
     }
     ```

7. **Register SecurityAlertMiddleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add after rate limiting, before authentication:
     ```csharp
     // Add security alert detection middleware (AC4 - US_057)
     app.UseSecurityAlerts();
     ```

8. **Add Retry-After Header for 429 Responses**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Configure rate limit response:
     ```csharp
     // Configure rate limit response with Retry-After header (AC2 - US_057)
     app.Use(async (context, next) =>
     {
         await next();
         
         // Add Retry-After header to 429 responses
         if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
         {
             // Calculate retry-after based on rate limit window (1 minute default)
             var retryAfter = 60; // seconds
             context.Response.Headers.Append("Retry-After", retryAfter.ToString());
             
             // Add custom response body
             if (!context.Response.HasStarted)
             {
                 context.Response.ContentType = "application/json";
                 var response = new
                 {
                     error = "TooManyRequests",
                     message = $"Rate limit exceeded. Please retry after {retryAfter} seconds.",
                     retryAfter = retryAfter,
                     timestamp = DateTime.UtcNow
                 };
                 await context.Response.WriteAsJsonAsync(response);
             }
         }
     });
     ```

9. **Document Rate Limiting Policy**
   - File: `docs/RATE_LIMITING.md`
   - Rate limiting documentation:
     ```markdown
     # Rate Limiting Policy (AC2 - US_057)
     
     ## Overview
     All API endpoints are protected by IP-based rate limiting to prevent abuse and ensure fair resource allocation. Rate limits enforced using AspNetCoreRateLimit middleware with Redis backend for distributed rate limiting.
     
     ## Rate Limit Thresholds
     
     ### General Limits (All Endpoints)
     - **100 requests per minute** per IP address
     - **1,000 requests per hour** per IP address
     
     ### Endpoint-Specific Limits
     
     #### Authentication Endpoints
     - **POST /api/auth/login**: 5 requests per 5 minutes per IP (Edge Case: CAPTCHA after 3 failures)
     - **POST /api/auth/register**: 3 requests per hour per IP
     
     #### Patient Endpoints
     - **GET /api/patients/***: 60 requests per minute per IP
     
     #### Appointment Endpoints
     - **POST /api/appointments**: 10 requests per minute per IP
     
     ### Whitelisted IPs
     - **127.0.0.1** (localhost): 10,000 requests per second (development/testing)
     
     ## HTTP 429 Response (AC2)
     
     When rate limit exceeded, API returns HTTP 429 Too Many Requests:
     
     **Response Headers**:
     ```
     HTTP/1.1 429 Too Many Requests
     Retry-After: 60
     Content-Type: application/json
     ```
     
     **Response Body**:
     ```json
     {
       "error": "TooManyRequests",
       "message": "Rate limit exceeded. Please retry after 60 seconds.",
       "retryAfter": 60,
       "timestamp": "2026-03-23T00:00:00Z"
     }
     ```
     
     ## Audit Logging (AC2)
     
     All rate limit violations are logged to the audit system with:
     - IP address
     - Endpoint
     - Timestamp
     - User agent
     
     **Audit Log Entry Example**:
     ```json
     {
       "actionType": "RateLimitExceeded",
       "resourceType": "API",
       "resourceId": "POST:/api/appointments",
       "result": "Denied",
       "ipAddress": "192.168.1.1",
       "timestamp": "2026-03-23T00:00:00Z"
     }
     ```
     
     ## Security Alert Detection (AC4)
     
     ### Suspicious Activity Threshold
     - **>10 authorization failures (401/403) from same IP in 5 minutes**
     
     ### Alert Actions
     1. **Log security alert** to audit system
     2. **Trigger Application Insights alert** (monitoring)
     3. **Optional IP throttling** (reduce rate limit to 5 requests/minute for 1 hour)
     
     **Security Alert Example**:
     ```
     SECURITY ALERT: Suspicious activity detected from IP 192.168.1.1.
     15 authorization failures in 5 minutes. Threshold: 10.
     ```
     
     ## IP Throttling (AC4)
     
     When suspicious activity detected, IP address is temporarily throttled:
     - **Rate limit reduced to 5 requests per minute**
     - **Duration: 1 hour**
     - **Automatic expiry after duration**
     
     ## Configuration
     
     Rate limits configured in `appsettings.json`:
     ```json
     {
       "IpRateLimiting": {
         "GeneralRules": [
           {
             "Endpoint": "*",
             "Period": "1m",
             "Limit": 100
           }
         ]
       }
     }
     ```
     
     ## Testing Rate Limits
     ```bash
     # Test general rate limit (100 requests/minute)
     for i in {1..101}; do
       curl https://api.patient-access.com/api/patients/1
     done
     # Expected: 101st request returns 429
     
     # Test login rate limit (5 requests/5 minutes)
     for i in {1..6}; do
       curl -X POST https://api.patient-access.com/api/auth/login \
         -H "Content-Type: application/json" \
         -d '{"email":"test@example.com","password":"wrong"}'
     done
     # Expected: 6th request returns 429 with Retry-After header
     ```
     
     ## Monitoring
     
     Rate limit metrics available via Application Insights:
     - Total 429 responses per endpoint
     - Top throttled IP addresses
     - Security alert count
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   └── PatientAccess.Business.csproj
└── PatientAccess.Web/
    ├── Program.cs (existing)
    └── appsettings.json
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Middleware/SecurityAlertMiddleware.cs | Security alert detection |
| CREATE | src/backend/PatientAccess.Web/Configuration/RateLimitConfiguration.cs | Custom rate limit config |
| CREATE | docs/RATE_LIMITING.md | Rate limiting documentation |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add AspNetCoreRateLimit package |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure rate limiting + security alerts |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add rate limit configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### AspNetCoreRateLimit
- **GitHub Repository**: https://github.com/stefanprodan/AspNetCoreRateLimit
- **Documentation**: https://github.com/stefanprodan/AspNetCoreRateLimit/wiki
- **Redis Configuration**: https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/Distributed-Caching

### HTTP Status Codes
- **429 Too Many Requests**: https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429
- **Retry-After Header**: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Retry-After

### Design Requirements
- **NFR-011**: API error rate below 0.1% (design.md)
- **AC2**: Rate limiting with 429 response and audit logging (us_057.md)
- **AC4**: Security alert detection for suspicious activity (us_057.md)

## Build Commands
```powershell
# Add AspNetCoreRateLimit package
cd src/backend/PatientAccess.Business
dotnet add package AspNetCoreRateLimit --version 5.0.0

# Build solution
cd ..
dotnet build PatientAccess.sln

# Run backend
cd PatientAccess.Web
dotnet run

# Test rate limiting
for ($i=1; $i -le 101; $i++) {
  curl https://localhost:5001/api/patients/1
}
# Expected: 101st request returns 429
```

## Validation Strategy

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/RateLimitingTests.cs`
- Test cases:
  1. **Test_RateLimit_Returns429AfterThresholdExceeded**
     - Setup: Send 101 requests to GET /api/patients/1 from same IP
     - Assert: Requests 1-100 return 200, request 101 returns 429
  2. **Test_RateLimit_IncludesRetryAfterHeader**
     - Setup: Trigger rate limit violation
     - Assert: Response headers contain "Retry-After: 60"
  3. **Test_RateLimit_LogsToAuditSystem**
     - Setup: Trigger rate limit violation
     - Assert: Audit log contains "RateLimitExceeded" entry with IP and endpoint
  4. **Test_SecurityAlert_TriggeredAfter10AuthFailures**
     - Setup: Send 11 failed login requests from same IP within 5 minutes
     - Assert: Audit log contains "SecurityAlert" entry after 11th failure
  5. **Test_SecurityAlert_ThrottlesIpAddress**
     - Setup: Trigger security alert (11 auth failures)
     - Assert: Redis contains throttle key for IP with 1-hour expiry
  6. **Test_PublicEndpoint_RateLimitStricter**
     - Setup: Send 6 login requests within 5 minutes
     - Assert: 6th request returns 429 (stricter limit for public endpoints)

### Manual Testing
- File: `docs/RATE_LIMITING.md` (Testing section)
- Test procedures:
  1. Test general rate limit (100 requests/minute)
  2. Test login rate limit (5 requests/5 minutes)
  3. Test Retry-After header presence
  4. Test security alert trigger (11 auth failures)
  5. Test IP throttling (verify reduced rate limit)

### Acceptance Criteria Validation
- **AC2**: ✅ Rate limiting returns 429 with Retry-After header, logged to audit system
- **AC4**: ✅ Security alert detection (>10 401/403 in 5 min) logs alert and triggers IP throttling

## Success Criteria Checklist
- [MANDATORY] AspNetCoreRateLimit package added to project
- [MANDATORY] Rate limiting configuration in appsettings.json
- [MANDATORY] General rate limit: 100 requests/minute per IP
- [MANDATORY] Login rate limit: 5 requests/5 minutes per IP (stricter for public endpoints)
- [MANDATORY] Redis-backed distributed rate limiting configured
- [MANDATORY] UseIpRateLimiting() middleware registered
- [MANDATORY] 429 Too Many Requests response for rate limit violations (AC2)
- [MANDATORY] Retry-After header included in 429 responses (AC2)
- [MANDATORY] SecurityAlertMiddleware created for suspicious activity detection (AC4)
- [MANDATORY] 401/403 error tracking with 5-minute sliding window (AC4)
- [MANDATORY] Security alert triggered when >10 auth failures in 5 minutes (AC4)
- [MANDATORY] IP throttling activated for suspicious IPs (reduces rate limit to 5 req/min for 1 hour)
- [MANDATORY] Audit logging for all rate limit violations (AC2)
- [MANDATORY] Audit logging for security alerts (AC4)
- [MANDATORY] RATE_LIMITING.md documents rate limits and security alerts
- [MANDATORY] Integration test: Rate limit returns 429 after threshold
- [MANDATORY] Integration test: Security alert triggered after 10 auth failures
- [RECOMMENDED] Application Insights integration for rate limit metrics

## Estimated Effort
**4 hours** (AspNetCoreRateLimit configuration + SecurityAlertMiddleware + Redis counters + audit logging + IP throttling + docs + tests)
