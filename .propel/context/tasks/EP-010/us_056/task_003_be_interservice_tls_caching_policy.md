# Task - task_003_be_interservice_tls_caching_policy

## Requirement Reference
- User Story: US_056
- Story Location: .propel/context/tasks/EP-010/us_056/us_056.md
- Acceptance Criteria:
    - **AC3**: Given inter-service communication (NFR-003), When the backend communicates with external services (Azure OpenAI, Twilio, SendGrid), Then all connections use TLS with certificate validation enabled.
    - **AC4**: Given the zero-PHI caching strategy (NFR-004), When data is cached in Upstash Redis, Then only non-PHI session tokens and aggregate data are stored; no patient names, medical records, or identifiers are cached.

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
| Backend | Azure.AI.OpenAI | 2.0+ |
| Backend | Twilio | 7.1.0 |
| Backend | SendGrid | 9.29.3 |
| Database | N/A | N/A |
| Caching | Upstash Redis | Redis 7.x compatible |
| Caching | StackExchange.Redis | 2.8+ |
| Vector Store | N/A | N/A |
| AI Gateway | Azure OpenAI | GPT-4o |
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

Configure TLS certificate validation for all external service clients (Azure OpenAI, Twilio, SendGrid) and implement zero-PHI caching policy for Redis. This task ensures compliance with AC3 (inter-service TLS with certificate validation) and AC4 (zero-PHI caching strategy). For inter-service communication, all HTTP clients use HttpClientFactory with default certificate validation enabled (validates certificate chain, expiry, and revocation). For caching, implement RedisKeyPrefix enum restricting cache keys to allowed types (session tokens, aggregate data only) and create CachingPolicyValidator to reject attempts to cache PHI fields (patient names, SSN, medical records).

**Key Capabilities:**
- Configure Azure OpenAI client with TLS certificate validation (AC3)
- Configure Twilio client with TLS certificate validation (AC3)
- Configure SendGrid client with TLS certificate validation (AC3)
- Create RedisKeyPrefix enum for allowed cache key types (AC4)
- Implement CachingPolicyValidator to reject PHI caching attempts (AC4)
- Create CachingExtensions helper methods with policy validation
- Document zero-PHI caching policy and prohibited fields
- Integration tests for TLS certificate validation and caching policy enforcement

## Dependent Tasks
- None (foundational security task)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Enums/RedisKeyPrefix.cs` - Allowed cache key types
- **NEW**: `src/backend/PatientAccess.Business/Validators/CachingPolicyValidator.cs` - PHI validation
- **NEW**: `src/backend/PatientAccess.Business/Extensions/CachingExtensions.cs` - Safe caching helpers
- **NEW**: `docs/CACHING_POLICY.md` - Zero-PHI caching policy documentation
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure HttpClient TLS validation
- **MODIFY**: `docs/CACHING.md` - Document zero-PHI policy

## Implementation Plan

1. **Configure Azure OpenAI Client with TLS Validation**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Configure HttpClientFactory for Azure OpenAI:
     ```csharp
     // Configure Azure OpenAI client with TLS certificate validation (AC3 - US_056)
     builder.Services.AddHttpClient("AzureOpenAI", client =>
     {
         var endpoint = builder.Configuration["AzureOpenAI:Endpoint"] 
             ?? throw new InvalidOperationException("AzureOpenAI:Endpoint not configured");
         var apiKey = builder.Configuration["AzureOpenAI:ApiKey"] 
             ?? throw new InvalidOperationException("AzureOpenAI:ApiKey not configured");
         
         client.BaseAddress = new Uri(endpoint);
         client.DefaultRequestHeaders.Add("api-key", apiKey);
         client.Timeout = TimeSpan.FromSeconds(30);
     })
     .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
     {
         // TLS certificate validation enabled by default (AC3)
         ServerCertificateCustomValidationCallback = null, // Use default validation
         SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                        System.Security.Authentication.SslProtocols.Tls13
     });
     ```

2. **Configure Twilio Client with TLS Validation**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Configure HttpClientFactory for Twilio:
     ```csharp
     // Configure Twilio client with TLS certificate validation (AC3 - US_056)
     builder.Services.AddHttpClient("Twilio", client =>
     {
         var accountSid = builder.Configuration["Twilio:AccountSid"] 
             ?? throw new InvalidOperationException("Twilio:AccountSid not configured");
         var authToken = builder.Configuration["Twilio:AuthToken"] 
             ?? throw new InvalidOperationException("Twilio:AuthToken not configured");
         
         client.BaseAddress = new Uri("https://api.twilio.com");
         
         // Basic authentication (Twilio uses HTTP Basic Auth)
         var credentials = Convert.ToBase64String(
             System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
         client.DefaultRequestHeaders.Authorization = 
             new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
         
         client.Timeout = TimeSpan.FromSeconds(30);
     })
     .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
     {
         // TLS certificate validation enabled by default (AC3)
         ServerCertificateCustomValidationCallback = null,
         SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                        System.Security.Authentication.SslProtocols.Tls13
     });
     ```

3. **Configure SendGrid Client with TLS Validation**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Configure HttpClientFactory for SendGrid:
     ```csharp
     // Configure SendGrid client with TLS certificate validation (AC3 - US_056)
     builder.Services.AddHttpClient("SendGrid", client =>
     {
         var apiKey = builder.Configuration["SendGrid:ApiKey"] 
             ?? throw new InvalidOperationException("SendGrid:ApiKey not configured");
         
         client.BaseAddress = new Uri("https://api.sendgrid.com");
         client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
         client.Timeout = TimeSpan.FromSeconds(30);
     })
     .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
     {
         // TLS certificate validation enabled by default (AC3)
         ServerCertificateCustomValidationCallback = null,
         SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                        System.Security.Authentication.SslProtocols.Tls13
     });
     ```

4. **Create RedisKeyPrefix Enum (AC4 - Zero-PHI Caching)**
   - File: `src/backend/PatientAccess.Business/Enums/RedisKeyPrefix.cs`
   - Allowed cache key types only:
     ```csharp
     namespace PatientAccess.Business.Enums
     {
         /// <summary>
         /// Redis key prefixes for zero-PHI caching strategy (AC4 - US_056).
         /// ONLY these key types are permitted in cache.
         /// </summary>
         public enum RedisKeyPrefix
         {
             /// <summary>
             /// Session tokens (JWT, refresh tokens).
             /// Format: "session:{userId}:{sessionId}"
             /// TTL: 15 minutes
             /// </summary>
             Session,
             
             /// <summary>
             /// Aggregate appointment counts (no patient identifiers).
             /// Format: "aggregate:appointments:{providerId}:{date}"
             /// TTL: 5 minutes
             /// </summary>
             AggregateAppointments,
             
             /// <summary>
             /// Aggregate waitlist counts (no patient identifiers).
             /// Format: "aggregate:waitlist:{providerId}"
             /// TTL: 5 minutes
             /// </summary>
             AggregateWaitlist,
             
             /// <summary>
             /// Provider timeslot availability (no patient data).
             /// Format: "timeslots:{providerId}:{date}"
             /// TTL: 10 minutes
             /// </summary>
             ProviderTimeslots,
             
             /// <summary>
             /// Rate limiting counters (no PHI).
             /// Format: "ratelimit:{ipAddress}:{endpoint}"
             /// TTL: 5 minutes
             /// </summary>
             RateLimit,
             
             /// <summary>
             /// Feature flags (no PHI).
             /// Format: "feature:{flagName}"
             /// TTL: 60 minutes
             /// </summary>
             FeatureFlag
         }
     }
     ```

5. **Create CachingPolicyValidator (AC4)**
   - File: `src/backend/PatientAccess.Business/Validators/CachingPolicyValidator.cs`
   - Validate zero-PHI policy:
     ```csharp
     using System.Text.Json;
     
     namespace PatientAccess.Business.Validators
     {
         /// <summary>
         /// Validates that cached data does not contain PHI (AC4 - US_056).
         /// Prevents accidental caching of patient names, medical records, SSN, etc.
         /// </summary>
         public static class CachingPolicyValidator
         {
             /// <summary>
             /// Prohibited field names (case-insensitive).
             /// Any object containing these fields CANNOT be cached.
             /// </summary>
             private static readonly HashSet<string> ProhibitedFields = new(StringComparer.OrdinalIgnoreCase)
             {
                 // Patient identifiers
                 "patientid", "patient_id", "patientname", "patient_name",
                 "firstname", "first_name", "lastname", "last_name",
                 "email", "phone", "phonenumber", "phone_number",
                 
                 // Medical identifiers
                 "mrn", "medicicalrecordnumber", "medical_record_number",
                 "ssn", "socialsecuritynumber", "social_security_number",
                 "insuranceid", "insurance_id", "insurancenumber",
                 
                 // Clinical data
                 "diagnosis", "diagnosiscode", "medication", "allergy",
                 "vital", "vitalsign", "labresult", "lab_result",
                 "clinicalnote", "clinical_note", "chiefcomplaint", "chief_complaint",
                 
                 // Appointment details with PHI
                 "visitreason", "visit_reason", "symptoms",
                 
                 // Document references
                 "documentcontent", "document_content", "extracteddata", "extracted_data"
             };
             
             /// <summary>
             /// Validates that an object is safe to cache (contains no PHI).
             /// Throws InvalidOperationException if PHI detected.
             /// </summary>
             public static void ValidateNoPHI<T>(T data, string cacheKey)
             {
                 if (data == null)
                     return;
                 
                 // Serialize to JSON and inspect property names
                 var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                 {
                     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                 });
                 
                 var jsonDocument = JsonDocument.Parse(json);
                 ValidateJsonElement(jsonDocument.RootElement, cacheKey);
             }
             
             private static void ValidateJsonElement(JsonElement element, string cacheKey)
             {
                 if (element.ValueKind == JsonValueKind.Object)
                 {
                     foreach (var property in element.EnumerateObject())
                     {
                         // Check if property name is prohibited
                         if (ProhibitedFields.Contains(property.Name))
                         {
                             throw new InvalidOperationException(
                                 $"PHI caching violation detected: Field '{property.Name}' is prohibited " +
                                 $"in cache key '{cacheKey}'. Zero-PHI caching policy enforced (AC4 - US_056).");
                         }
                         
                         // Recursively validate nested objects/arrays
                         ValidateJsonElement(property.Value, cacheKey);
                     }
                 }
                 else if (element.ValueKind == JsonValueKind.Array)
                 {
                     foreach (var item in element.EnumerateArray())
                     {
                         ValidateJsonElement(item, cacheKey);
                     }
                 }
             }
             
             /// <summary>
             /// Validates that a cache key uses an allowed prefix.
             /// Throws InvalidOperationException if prefix invalid.
             /// </summary>
             public static void ValidateKeyPrefix(string cacheKey)
             {
                 var allowedPrefixes = Enum.GetValues<RedisKeyPrefix>()
                     .Select(p => p.ToString().ToLowerInvariant());
                 
                 var hasValidPrefix = allowedPrefixes.Any(prefix => 
                     cacheKey.StartsWith(prefix + ":", StringComparison.OrdinalIgnoreCase));
                 
                 if (!hasValidPrefix)
                 {
                     throw new InvalidOperationException(
                         $"Invalid cache key prefix: '{cacheKey}'. " +
                         $"Allowed prefixes: {string.Join(", ", allowedPrefixes)} (AC4 - US_056).");
                 }
             }
         }
     }
     ```

6. **Create CachingExtensions (AC4)**
   - File: `src/backend/PatientAccess.Business/Extensions/CachingExtensions.cs`
   - Safe caching helper methods:
     ```csharp
     using StackExchange.Redis;
     using System.Text.Json;
     using PatientAccess.Business.Enums;
     using PatientAccess.Business.Validators;
     
     namespace PatientAccess.Business.Extensions
     {
         /// <summary>
         /// Redis caching extensions with zero-PHI policy enforcement (AC4 - US_056).
         /// All methods validate against PHI caching violations before write.
         /// </summary>
         public static class CachingExtensions
         {
             /// <summary>
             /// Safely sets a cache value with PHI validation.
             /// Throws InvalidOperationException if PHI detected.
             /// </summary>
             public static async Task SetSafeAsync<T>(
                 this IDatabase redis,
                 RedisKeyPrefix prefix,
                 string keySuffix,
                 T value,
                 TimeSpan? expiry = null)
             {
                 var cacheKey = $"{prefix.ToString().ToLowerInvariant()}:{keySuffix}";
                 
                 // Validate key prefix (AC4)
                 CachingPolicyValidator.ValidateKeyPrefix(cacheKey);
                 
                 // Validate no PHI in value (AC4)
                 CachingPolicyValidator.ValidateNoPHI(value, cacheKey);
                 
                 // Serialize and set
                 var json = JsonSerializer.Serialize(value);
                 await redis.StringSetAsync(cacheKey, json, expiry);
             }
             
             /// <summary>
             /// Gets a cached value safely.
             /// </summary>
             public static async Task<T?> GetSafeAsync<T>(
                 this IDatabase redis,
                 RedisKeyPrefix prefix,
                 string keySuffix)
             {
                 var cacheKey = $"{prefix.ToString().ToLowerInvariant()}:{keySuffix}";
                 
                 // Validate key prefix (AC4)
                 CachingPolicyValidator.ValidateKeyPrefix(cacheKey);
                 
                 var json = await redis.StringGetAsync(cacheKey);
                 if (!json.HasValue)
                     return default;
                 
                 return JsonSerializer.Deserialize<T>(json!);
             }
             
             /// <summary>
             /// Deletes a cached value safely.
             /// </summary>
             public static async Task<bool> DeleteSafeAsync(
                 this IDatabase redis,
                 RedisKeyPrefix prefix,
                 string keySuffix)
             {
                 var cacheKey = $"{prefix.ToString().ToLowerInvariant()}:{keySuffix}";
                 
                 // Validate key prefix (AC4)
                 CachingPolicyValidator.ValidateKeyPrefix(cacheKey);
                 
                 return await redis.KeyDeleteAsync(cacheKey);
             }
         }
     }
     ```

7. **Document Zero-PHI Caching Policy**
   - File: `docs/CACHING_POLICY.md`
   - Comprehensive policy documentation:
     ```markdown
     # Zero-PHI Caching Policy (AC4 - US_056)
     
     ## Overview
     This document defines the zero-PHI caching strategy (AD-005 from design.md). All PHI data remains in the encrypted PostgreSQL database. Redis cache stores ONLY non-PHI session tokens and aggregate data.
     
     ## Allowed Cache Types (RedisKeyPrefix Enum)
     
     ### 1. Session Tokens (RedisKeyPrefix.Session)
     - **Data**: JWT access tokens, refresh tokens, session IDs
     - **Format**: `session:{userId}:{sessionId}`
     - **TTL**: 15 minutes
     - **Example**: `session:1234:a7b3c5d9`
     - **PHI Status**: ✅ No PHI (only authentication tokens)
     
     ### 2. Aggregate Appointment Counts (RedisKeyPrefix.AggregateAppointments)
     - **Data**: Total appointments per provider per day (no patient names/identifiers)
     - **Format**: `aggregate:appointments:{providerId}:{date}`
     - **TTL**: 5 minutes
     - **Example**: `aggregate:appointments:789:2026-03-23` → `{"total": 12, "available": 4}`
     - **PHI Status**: ✅ No PHI (aggregate counts only)
     
     ### 3. Aggregate Waitlist Counts (RedisKeyPrefix.AggregateWaitlist)
     - **Data**: Total waitlist entries per provider (no patient names/identifiers)
     - **Format**: `aggregate:waitlist:{providerId}`
     - **TTL**: 5 minutes
     - **Example**: `aggregate:waitlist:789` → `{"total": 15}`
     - **PHI Status**: ✅ No PHI (aggregate counts only)
     
     ### 4. Provider Timeslot Availability (RedisKeyPrefix.ProviderTimeslots)
     - **Data**: Available appointment timeslots (no patient bookings)
     - **Format**: `timeslots:{providerId}:{date}`
     - **TTL**: 10 minutes
     - **Example**: `timeslots:789:2026-03-23` → `["09:00", "10:00", "14:00"]`
     - **PHI Status**: ✅ No PHI (provider schedules only)
     
     ### 5. Rate Limiting Counters (RedisKeyPrefix.RateLimit)
     - **Data**: API request counts per IP/endpoint
     - **Format**: `ratelimit:{ipAddress}:{endpoint}`
     - **TTL**: 5 minutes
     - **Example**: `ratelimit:192.168.1.1:/api/appointments` → `5`
     - **PHI Status**: ✅ No PHI (request counters only)
     
     ### 6. Feature Flags (RedisKeyPrefix.FeatureFlag)
     - **Data**: Feature toggle states
     - **Format**: `feature:{flagName}`
     - **TTL**: 60 minutes
     - **Example**: `feature:enableWaitlist` → `true`
     - **PHI Status**: ✅ No PHI (configuration only)
     
     ## Prohibited Data (NEVER Cache)
     
     ### Patient Identifiers
     - ❌ Patient ID, MRN (Medical Record Number)
     - ❌ Patient name (first name, last name, full name)
     - ❌ Email address, phone number
     - ❌ SSN, Insurance ID
     
     ### Clinical Data
     - ❌ Diagnosis codes (ICD-10)
     - ❌ Medications, allergies
     - ❌ Vital signs (blood pressure, temperature, etc.)
     - ❌ Lab results
     - ❌ Clinical notes, chief complaints
     
     ### Appointment Details with PHI
     - ❌ Visit reason, symptoms
     - ❌ Individual patient appointment records
     
     ### Documents
     - ❌ Clinical document content
     - ❌ Extracted clinical data
     
     ## Policy Enforcement
     
     ### Automatic Validation
     All cache write operations use `CachingExtensions.SetSafeAsync()` which automatically:
     1. Validates key prefix against allowed types
     2. Scans value for prohibited field names
     3. Throws `InvalidOperationException` if PHI detected
     
     ### Example Usage (Safe)
     ```csharp
     // ✅ SAFE: Caching session token
     await redis.SetSafeAsync(
         RedisKeyPrefix.Session,
         $"{userId}:{sessionId}",
         new { token = jwtToken, expiresAt = DateTime.UtcNow.AddMinutes(15) },
         TimeSpan.FromMinutes(15)
     );
     
     // ✅ SAFE: Caching aggregate appointment count
     await redis.SetSafeAsync(
         RedisKeyPrefix.AggregateAppointments,
         $"{providerId}:{date:yyyy-MM-dd}",
         new { total = 12, available = 4 },
         TimeSpan.FromMinutes(5)
     );
     ```
     
     ### Example Usage (Violation)
     ```csharp
     // ❌ VIOLATION: Attempting to cache patient name
     await redis.SetSafeAsync(
         RedisKeyPrefix.Session,
         $"{userId}",
         new { patientName = "John Doe", email = "john@example.com" },
         TimeSpan.FromMinutes(15)
     );
     
     // Throws: InvalidOperationException: PHI caching violation detected: 
     // Field 'patientName' is prohibited in cache key 'session:1234'. 
     // Zero-PHI caching policy enforced (AC4 - US_056).
     ```
     
     ## Compliance Verification
     
     ### Automated Tests
     - **Test_CachingPolicyValidator_RejectsPatientName**: Validates PHI detection
     - **Test_CachingExtensions_AllowsSessionToken**: Validates allowed cache type
     - **Test_CachingExtensions_RejectsInvalidPrefix**: Validates key prefix enforcement
     
     ### Manual Audit
     Periodically review Redis keys for policy compliance:
     ```bash
     # Connect to Redis
     redis-cli -u $REDIS_URL
     
     # List all keys (should only see allowed prefixes)
     KEYS *
     
     # Expected patterns:
     # session:*
     # aggregate:appointments:*
     # aggregate:waitlist:*
     # timeslots:*
     # ratelimit:*
     # feature:*
     
     # ❌ If you see patient:* or appointment:123:* → POLICY VIOLATION
     ```
     
     ## Developer Guidelines
     
     ### DO
     - ✅ Use `CachingExtensions.SetSafeAsync()` for all cache writes
     - ✅ Use allowed `RedisKeyPrefix` values only
     - ✅ Cache aggregate data (counts, availability)
     - ✅ Cache session tokens with short TTL (15 min)
     
     ### DO NOT
     - ❌ Use raw `StringSetAsync()` (bypasses validation)
     - ❌ Cache patient identifiers or clinical data
     - ❌ Create custom key prefixes outside `RedisKeyPrefix` enum
     - ❌ Extend prohibited field list without security review
     
     ## Incident Response
     
     ### PHI Caching Violation Detected
     1. **Immediate Action**: Log exception with stack trace
     2. **Verify**: Check Redis for leaked PHI using `KEYS` command
     3. **Remediate**: Delete all keys containing PHI
        ```bash
        redis-cli -u $REDIS_URL FLUSHDB
        ```
     4. **Investigate**: Review code that triggered violation
     5. **Report**: Document incident in security log
     
     ## References
     - **AC4 (US_056)**: Zero-PHI caching strategy
     - **AD-005 (design.md)**: Redis cache stores only session tokens and non-PHI data
     - **NFR-004 (design.md)**: Data protection standards
     ```

8. **Update CACHING.md Documentation**
   - File: `docs/CACHING.md`
   - Add zero-PHI policy section:
     ```markdown
     ## Zero-PHI Caching Policy (AC4 - US_056)
     
     All PHI data remains in the encrypted PostgreSQL database. Redis cache stores ONLY non-PHI data.
     
     **Allowed Cache Types**:
     - Session tokens (JWT, refresh tokens)
     - Aggregate appointment counts (no patient identifiers)
     - Provider timeslot availability (no patient bookings)
     - Rate limiting counters
     - Feature flags
     
     **Prohibited Data**:
     - Patient names, identifiers, SSN, insurance ID
     - Clinical data (diagnoses, medications, vital signs, lab results)
     - Appointment visit reasons, symptoms
     - Clinical document content
     
     **Policy Enforcement**:
     Use `CachingExtensions.SetSafeAsync()` for all cache writes. Automatic validation rejects PHI caching attempts.
     
     **Full Policy**: See docs/CACHING_POLICY.md
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Enums/
│   ├── Validators/
│   └── Extensions/
└── PatientAccess.Web/
    └── Program.cs (existing)

docs/
└── CACHING.md (existing)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Enums/RedisKeyPrefix.cs | Allowed cache key types |
| CREATE | src/backend/PatientAccess.Business/Validators/CachingPolicyValidator.cs | PHI validation |
| CREATE | src/backend/PatientAccess.Business/Extensions/CachingExtensions.cs | Safe caching helpers |
| CREATE | docs/CACHING_POLICY.md | Zero-PHI policy documentation |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure HttpClient TLS validation |
| MODIFY | docs/CACHING.md | Document zero-PHI policy |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core HttpClient
- **HttpClient Best Practices**: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines
- **HttpClientFactory**: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory
- **Certificate Validation**: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler.servercertificatecustomvalidationcallback

### Redis Caching
- **StackExchange.Redis**: https://stackexchange.github.io/StackExchange.Redis/
- **Redis Best Practices**: https://redis.io/docs/manual/patterns/

### Design Requirements
- **FR-042**: TLS 1.2+ encryption in transit (spec.md)
- **NFR-003**: Data protection standards (design.md)
- **NFR-004**: Encrypt all data in transit (design.md)
- **AD-005**: Zero-PHI caching strategy (design.md)

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build PatientAccess.sln

# Run tests
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj

# Test inter-service TLS connections (requires API keys in environment)
$env:AZUREOPENAI__ENDPOINT="https://your-endpoint.openai.azure.com/"
$env:AZUREOPENAI__APIKEY="your-api-key"
dotnet run --project PatientAccess.Web

# Test caching policy validator (unit tests)
dotnet test --filter "FullyQualifiedName~CachingPolicyValidatorTests"
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Validators/CachingPolicyValidatorTests.cs`
- Test cases:
  1. **Test_ValidateNoPHI_AllowsSessionToken**
     - Input: `{ token: "jwt-token", expiresAt: "2026-03-23T00:00:00Z" }`
     - Call: `CachingPolicyValidator.ValidateNoPHI(data, "session:1234")`
     - Assert: No exception thrown
  2. **Test_ValidateNoPHI_RejectsPatientName**
     - Input: `{ patientName: "John Doe", email: "john@example.com" }`
     - Call: `CachingPolicyValidator.ValidateNoPHI(data, "session:1234")`
     - Assert: InvalidOperationException with message "Field 'patientName' is prohibited"
  3. **Test_ValidateKeyPrefix_AllowsSessionPrefix**
     - Input: `"session:1234:abc"`
     - Call: `CachingPolicyValidator.ValidateKeyPrefix(cacheKey)`
     - Assert: No exception thrown
  4. **Test_ValidateKeyPrefix_RejectsInvalidPrefix**
     - Input: `"patient:1234"`
     - Call: `CachingPolicyValidator.ValidateKeyPrefix(cacheKey)`
     - Assert: InvalidOperationException with message "Invalid cache key prefix"

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/InterServiceTlsTests.cs`
- Test cases:
  1. **Test_AzureOpenAIClient_ValidatesCertificate**
     - Setup: Mock HTTPS endpoint with self-signed certificate
     - Call: HttpClient request to mock endpoint
     - Assert: Request fails with certificate validation error (expected)
  2. **Test_TwilioClient_UsesTls12Plus**
     - Setup: HttpClient for Twilio
     - Call: Inspect HttpClientHandler.SslProtocols
     - Assert: SslProtocols includes Tls12 and Tls13 only
  3. **Test_SendGridClient_UsesTls12Plus**
     - Setup: HttpClient for SendGrid
     - Call: Inspect HttpClientHandler.SslProtocols
     - Assert: SslProtocols includes Tls12 and Tls13 only

### Acceptance Criteria Validation
- **AC3**: ✅ All external service clients use TLS with certificate validation enabled
- **AC4**: ✅ Zero-PHI caching policy enforced with automatic validation

## Success Criteria Checklist
- [MANDATORY] Azure OpenAI client configured with TLS certificate validation
- [MANDATORY] Twilio client configured with TLS certificate validation
- [MANDATORY] SendGrid client configured with TLS certificate validation
- [MANDATORY] All HttpClients use TLS 1.2+ minimum protocol
- [MANDATORY] RedisKeyPrefix enum created with allowed cache types
- [MANDATORY] CachingPolicyValidator created with prohibited field validation
- [MANDATORY] CachingExtensions.SetSafeAsync() enforces zero-PHI policy
- [MANDATORY] CachingExtensions.GetSafeAsync() validates key prefix
- [MANDATORY] CachingExtensions.DeleteSafeAsync() validates key prefix
- [MANDATORY] CACHING_POLICY.md documents zero-PHI policy comprehensively
- [MANDATORY] CACHING.md updated with zero-PHI policy summary
- [MANDATORY] Unit test: CachingPolicyValidator allows session tokens
- [MANDATORY] Unit test: CachingPolicyValidator rejects patient names
- [MANDATORY] Unit test: Invalid key prefix rejected
- [MANDATORY] Integration test: HttpClient validates TLS certificates
- [RECOMMENDED] Redis key audit script for manual compliance check
- [RECOMMENDED] Incident response procedure for PHI caching violations

## Estimated Effort
**4 hours** (HttpClient TLS configuration + RedisKeyPrefix enum + CachingPolicyValidator + CachingExtensions + policy docs + tests)
