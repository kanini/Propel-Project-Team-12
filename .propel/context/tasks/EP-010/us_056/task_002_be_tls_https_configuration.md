# Task - task_002_be_tls_https_configuration

## Requirement Reference
- User Story: US_056
- Story Location: .propel/context/tasks/EP-010/us_056/us_056.md
- Acceptance Criteria:
    - **AC2**: Given any API communication (FR-042), When data is transmitted between client and server, Then TLS 1.2+ is enforced with HSTS headers and no downgrade to HTTP is permitted.

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
| Database | N/A | N/A |
| Caching | N/A | N/A |
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

Configure ASP.NET Core Web API to enforce TLS 1.2+ encryption for all HTTP traffic with HSTS (HTTP Strict Transport Security) headers and HTTPS redirection middleware. This task ensures compliance with FR-042 (TLS 1.2+ encryption in transit) and NFR-004 (encrypt all data in transit). Railway and Render hosting platforms automatically provision TLS certificates and terminate TLS at the edge, so the backend application must trust forwarded HTTPS headers from the reverse proxy and enforce HTTPS redirection for any direct HTTP requests. Additionally, configure HSTS headers to instruct browsers to always use HTTPS for future requests (max-age: 1 year, includeSubDomains).

**Key Capabilities:**
- Configure Kestrel to enforce TLS 1.2+ minimum protocol version (AC2)
- Add HTTPS redirection middleware with permanent redirect (307)
- Configure HSTS middleware with 1-year max-age and includeSubDomains
- Configure forwarded headers middleware for Railway/Render proxy support
- Add SecurityHeadersMiddleware for additional headers (X-Content-Type-Options, X-Frame-Options)
- Update appsettings.json with HTTPS/HSTS configuration
- Document TLS certificate renewal monitoring procedure
- Health check endpoint for TLS configuration validation

## Dependent Tasks
- None (foundational security task)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Web/Middleware/SecurityHeadersMiddleware.cs` - Security headers middleware
- **NEW**: `docs/TLS_CERTIFICATE_MONITORING.md` - Certificate renewal monitoring procedure
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure HTTPS/HSTS/forwarded headers
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add HTTPS configuration
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.Production.json` - Production HSTS settings
- **MODIFY**: `docs/AUTHENTICATION.md` - Document HTTPS-only authentication

## Implementation Plan

1. **Configure Kestrel for TLS 1.2+ Minimum**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add Kestrel configuration before `builder.Build()`:
     ```csharp
     // Configure Kestrel for TLS 1.2+ minimum protocol (AC2 - US_056)
     builder.WebHost.ConfigureKestrel(serverOptions =>
     {
         serverOptions.ConfigureHttpsDefaults(httpsOptions =>
         {
             // Enforce TLS 1.2 minimum (blocks TLS 1.0, TLS 1.1)
             httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                                          System.Security.Authentication.SslProtocols.Tls13;
         });
     });
     ```

2. **Configure Forwarded Headers Middleware (Railway/Render Proxy Support)**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add before authentication middleware:
     ```csharp
     using Microsoft.AspNetCore.HttpOverrides;
     
     // Configure forwarded headers for Railway/Render reverse proxy (AC2 - US_056)
     app.UseForwardedHeaders(new ForwardedHeadersOptions
     {
         ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                            ForwardedHeaders.XForwardedProto,
         // Railway/Render use trusted proxies, accept all forwarded headers
         KnownNetworks = { },
         KnownProxies = { }
     });
     ```

3. **Configure HTTPS Redirection Middleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add after forwarded headers:
     ```csharp
     // Configure HTTPS redirection with permanent redirect (AC2 - US_056)
     app.UseHttpsRedirection();
     ```
   - File: `src/backend/PatientAccess.Web/appsettings.json`
   - Configure redirection status code:
     ```json
     {
       "HttpsRedirection": {
         "HttpsPort": 443,
         "RedirectStatusCode": 307
       }
     }
     ```

4. **Configure HSTS Middleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add after HTTPS redirection:
     ```csharp
     // Configure HSTS (HTTP Strict Transport Security) (AC2 - US_056)
     if (!app.Environment.IsDevelopment())
     {
         app.UseHsts();
     }
     ```
   - File: `src/backend/PatientAccess.Web/appsettings.Production.json`
   - Configure HSTS settings:
     ```json
     {
       "Hsts": {
         "MaxAge": 31536000,
         "IncludeSubDomains": true,
         "Preload": false
       }
     }
     ```
   - Update `builder.Services` configuration in `Program.cs`:
     ```csharp
     // Add HSTS configuration (AC2 - US_056)
     builder.Services.AddHsts(options =>
     {
         options.MaxAge = TimeSpan.FromDays(365); // 1 year
         options.IncludeSubDomains = true;
         options.Preload = false; // Set to true only after domain added to HSTS preload list
     });
     ```

5. **Create Security Headers Middleware**
   - File: `src/backend/PatientAccess.Web/Middleware/SecurityHeadersMiddleware.cs`
   - Additional security headers (defense-in-depth):
     ```csharp
     namespace PatientAccess.Web.Middleware
     {
         /// <summary>
         /// Middleware to add security headers to all HTTP responses (AC2 - US_056).
         /// Complements HSTS with additional browser security protections.
         /// </summary>
         public class SecurityHeadersMiddleware
         {
             private readonly RequestDelegate _next;
     
             public SecurityHeadersMiddleware(RequestDelegate next)
             {
                 _next = next;
             }
     
             public async Task InvokeAsync(HttpContext context)
             {
                 // X-Content-Type-Options: Prevent MIME-sniffing attacks
                 context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                 
                 // X-Frame-Options: Prevent clickjacking attacks
                 context.Response.Headers.Append("X-Frame-Options", "DENY");
                 
                 // X-XSS-Protection: Enable browser XSS filter (legacy browsers)
                 context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                 
                 // Referrer-Policy: Control referrer information leakage
                 context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                 
                 // Content-Security-Policy: Restrict resource loading (basic policy)
                 context.Response.Headers.Append(
                     "Content-Security-Policy",
                     "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " + // Allow frontend JS frameworks
                     "style-src 'self' 'unsafe-inline'; " + // Allow inline styles for Tailwind
                     "img-src 'self' data: https:; " +
                     "font-src 'self' data:; " +
                     "connect-src 'self' https://api.openai.azure.com https://api.twilio.com https://api.sendgrid.com; " +
                     "frame-ancestors 'none';"
                 );
                 
                 // Permissions-Policy: Restrict browser features
                 context.Response.Headers.Append(
                     "Permissions-Policy",
                     "geolocation=(), microphone=(), camera=()"
                 );
                 
                 await _next(context);
             }
         }
     
         /// <summary>
         /// Extension method to register SecurityHeadersMiddleware.
         /// </summary>
         public static class SecurityHeadersMiddlewareExtensions
         {
             public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
             {
                 return builder.UseMiddleware<SecurityHeadersMiddleware>();
             }
         }
     }
     ```

6. **Register Security Headers Middleware**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Add after HSTS:
     ```csharp
     // Add security headers (X-Content-Type-Options, X-Frame-Options, CSP) (AC2 - US_056)
     app.UseSecurityHeaders();
     ```

7. **Update CORS Configuration for HTTPS-Only**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Modify CORS policy to enforce HTTPS origins:
     ```csharp
     // Configure CORS for HTTPS-only origins (AC2 - US_056)
     builder.Services.AddCors(options =>
     {
         options.AddPolicy("AllowFrontend", policy =>
         {
             if (builder.Environment.IsDevelopment())
             {
                 // Development: Allow localhost (HTTP/HTTPS)
                 policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
             }
             else
             {
                 // Production: HTTPS-only (Railway/Render/Vercel)
                 var frontendUrl = builder.Configuration["Frontend:Url"] 
                     ?? throw new InvalidOperationException("Frontend:Url not configured");
                 
                 // Validate HTTPS-only origin
                 if (!frontendUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                 {
                     throw new InvalidOperationException(
                         "Frontend:Url must use HTTPS in production (FR-042, AC2)");
                 }
                 
                 policy.WithOrigins(frontendUrl)
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
             }
         });
     });
     ```
   - File: `src/backend/PatientAccess.Web/appsettings.Production.json`
   - Add frontend URL configuration:
     ```json
     {
       "Frontend": {
         "Url": "https://patient-access.vercel.app"
       }
     }
     ```

8. **Document TLS Certificate Renewal Monitoring**
   - File: `docs/TLS_CERTIFICATE_MONITORING.md`
   - Certificate renewal procedure:
     ```markdown
     # TLS Certificate Renewal Monitoring Procedure
     
     ## Overview
     Railway and Render automatically provision and renew TLS certificates using Let's Encrypt. This document describes monitoring procedures to detect certificate renewal failures (Edge Case - US_056).
     
     ## Automatic Certificate Management
     
     ### Railway
     - **Certificate Provider**: Let's Encrypt (90-day certificates)
     - **Auto-Renewal**: Certificates automatically renewed 30 days before expiration
     - **Custom Domains**: Certificates provisioned automatically when custom domain added
     - **Monitoring**: Railway sends email notifications if renewal fails
     
     ### Render
     - **Certificate Provider**: Let's Encrypt (90-day certificates)
     - **Auto-Renewal**: Certificates automatically renewed 30 days before expiration
     - **Custom Domains**: Certificates provisioned automatically when custom domain added
     - **Monitoring**: Render dashboard shows certificate expiration dates
     
     ## Monitoring Strategy
     
     ### 1. Platform Notifications
     - **Railway**: Email notifications enabled for certificate renewal failures
     - **Render**: Dashboard alerts for expiring certificates (< 7 days)
     
     ### 2. External Certificate Monitoring
     Use SSL certificate monitoring service (free tier):
     - **SSL Labs**: https://www.ssllabs.com/ssltest/
     - **SSL Checker**: https://www.sslshopper.com/ssl-checker.html
     - **UptimeRobot**: HTTP(S) monitoring with SSL expiration alerts
     
     **Configuration (UptimeRobot)**:
     1. Create HTTP(S) monitor for backend API endpoint
     2. Enable SSL certificate expiration monitoring
     3. Set alert threshold: 7 days before expiration
     4. Configure email/SMS alerts to ops team
     
     ### 3. Health Check Integration
     The `/health` endpoint validates TLS configuration and certificates:
     
     ```bash
     # Check health endpoint (HTTPS-only)
     curl https://api.patient-access.com/health
     
     # Expected response:
     {
       "status": "Healthy",
       "tls": {
         "protocolVersion": "TLS 1.3",
         "certificateExpiry": "2026-06-20T00:00:00Z",
         "daysUntilExpiry": 89,
         "hstsEnabled": true
       }
     }
     ```
     
     ## Certificate Renewal Failure Response
     
     ### Symptoms
     - Browser warnings: "Your connection is not private" (ERR_CERT_DATE_INVALID)
     - API requests fail with SSL handshake errors
     - Health check endpoint unreachable via HTTPS
     
     ### Immediate Actions
     1. **Verify Platform Status**
        - Check Railway/Render status page for incidents
        - Review platform dashboard for error messages
     
     2. **Fallback Strategy (REJECT UNENCRYPTED TRANSPORT)**
        - **DO NOT**: Fallback to HTTP (violates FR-042, AC2)
        - **DO**: Reject all connections until HTTPS restored
        - **DO**: Display maintenance page with status updates
     
     3. **Manual Certificate Renewal**
        - Railway: Trigger manual renewal via CLI
          ```bash
          railway domain renew
          ```
        - Render: Contact Render support for manual renewal
     
     4. **Notify Operations Team**
        - Email: ops@patient-access.com
        - Slack: #platform-alerts channel
        - Include: Certificate expiry date, platform (Railway/Render), error logs
     
     ## Testing Certificate Configuration
     
     ### Local Testing (Development)
     ```bash
     # Generate self-signed certificate for local testing
     dotnet dev-certs https --trust
     
     # Run backend with HTTPS
     cd src/backend/PatientAccess.Web
     dotnet run --launch-profile https
     
     # Verify TLS 1.2+ enforcement
     curl --tlsv1.1 --verbose https://localhost:5001/health
     # Expected: TLS handshake failure (TLS 1.1 not supported)
     
     curl --tlsv1.2 --verbose https://localhost:5001/health
     # Expected: HTTP 200 OK
     ```
     
     ### Production Testing
     ```bash
     # Test HTTPS redirection
     curl -I http://api.patient-access.com/health
     # Expected: HTTP 307 Temporary Redirect
     # Location: https://api.patient-access.com/health
     
     # Test HSTS headers
     curl -I https://api.patient-access.com/health
     # Expected headers:
     # Strict-Transport-Security: max-age=31536000; includeSubDomains
     # X-Content-Type-Options: nosniff
     # X-Frame-Options: DENY
     
     # Test TLS version using OpenSSL
     openssl s_client -connect api.patient-access.com:443 -tls1_1
     # Expected: handshake failure (TLS 1.1 not supported)
     
     openssl s_client -connect api.patient-access.com:443 -tls1_2
     # Expected: successful connection
     ```
     
     ## Compliance Verification
     - **FR-042**: TLS 1.2+ encryption in transit ✅
     - **NFR-004**: Encrypt all data in transit ✅
     - **AC2 (US_056)**: HSTS headers, no HTTP downgrade ✅
     - **Edge Case (US_056)**: Certificate renewal failure → reject connections (no HTTP fallback) ✅
     ```

9. **Update Authentication Documentation**
   - File: `docs/AUTHENTICATION.md`
   - Add HTTPS-only authentication section:
     ```markdown
     ## HTTPS-Only Authentication (AC2 - US_056)
     
     All authentication endpoints require HTTPS. HTTP requests are automatically redirected to HTTPS with HTTP 307 status code.
     
     **Security Headers**:
     - `Strict-Transport-Security: max-age=31536000; includeSubDomains` (1-year HSTS)
     - `X-Content-Type-Options: nosniff`
     - `X-Frame-Options: DENY`
     - `Content-Security-Policy: frame-ancestors 'none';`
     
     **TLS Configuration**:
     - Minimum Protocol: TLS 1.2
     - Supported Protocols: TLS 1.2, TLS 1.3
     - Blocked Protocols: SSL 3.0, TLS 1.0, TLS 1.1
     
     **JWT Token Security**:
     - Tokens transmitted only over HTTPS
     - Secure flag set on authentication cookies
     - HttpOnly flag prevents JavaScript access
     - SameSite=Strict prevents CSRF attacks
     ```

## Current Project State

```
src/backend/
└── PatientAccess.Web/
    ├── Program.cs (existing)
    ├── appsettings.json
    ├── appsettings.Development.json
    └── appsettings.Production.json

docs/
├── AUTHENTICATION.md (existing)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Web/Middleware/SecurityHeadersMiddleware.cs | Security headers middleware |
| CREATE | docs/TLS_CERTIFICATE_MONITORING.md | Certificate monitoring procedure |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure TLS/HTTPS/HSTS |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add HTTPS redirection config |
| MODIFY | src/backend/PatientAccess.Web/appsettings.Production.json | Add HSTS/frontend URL config |
| MODIFY | docs/AUTHENTICATION.md | Document HTTPS-only authentication |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Security
- **Enforce HTTPS**: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
- **HSTS Middleware**: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl#http-strict-transport-security-protocol-hsts
- **Kestrel HTTPS Configuration**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints
- **Forwarded Headers**: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer

### TLS Best Practices
- **Mozilla SSL Configuration Generator**: https://ssl-config.mozilla.org/
- **OWASP TLS Cheat Sheet**: https://cheatsheetseries.owasp.org/cheatsheets/Transport_Layer_Security_Cheat_Sheet.html

### Design Requirements
- **FR-042**: TLS 1.2+ encryption in transit (spec.md)
- **NFR-004**: Encrypt all data in transit using TLS 1.2 or higher (design.md)

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build PatientAccess.sln

# Run backend with HTTPS (development)
cd PatientAccess.Web
dotnet run --launch-profile https

# Test HTTPS redirection
curl -I http://localhost:5000/health
# Expected: HTTP 307 Temporary Redirect

# Test HSTS headers (production)
curl -I https://api.patient-access.com/health | Select-String "Strict-Transport-Security"
# Expected: Strict-Transport-Security: max-age=31536000; includeSubDomains

# Test TLS 1.2+ enforcement (requires OpenSSL)
openssl s_client -connect localhost:5001 -tls1_1
# Expected: handshake failure

openssl s_client -connect localhost:5001 -tls1_2
# Expected: successful connection
```

## Validation Strategy

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/HttpsConfigurationTests.cs`
- Test cases:
  1. **Test_HttpRequest_RedirectsToHttps**
     - Request: HTTP GET http://localhost:5000/health
     - Assert: StatusCode = 307, Location header = https://localhost:5001/health
  2. **Test_HttpsRequest_ReturnsHstsHeader**
     - Request: HTTPS GET https://localhost:5001/health
     - Assert: Response headers contain "Strict-Transport-Security: max-age=31536000; includeSubDomains"
  3. **Test_HttpsRequest_ReturnsSecurityHeaders**
     - Request: HTTPS GET https://localhost:5001/health
     - Assert: Headers contain X-Content-Type-Options, X-Frame-Options, Content-Security-Policy
  4. **Test_CorsPolicy_RejectsHttpOriginInProduction**
     - Setup: Environment = Production, Origin = http://malicious.com
     - Request: OPTIONS /api/patients (preflight)
     - Assert: CORS rejected (no Access-Control-Allow-Origin header)

### Manual Testing
- File: `docs/TLS_CERTIFICATE_MONITORING.md` (Testing section)
- Test procedures:
  1. Test HTTPS redirection with curl
  2. Test HSTS headers with curl
  3. Test TLS 1.1 rejection with OpenSSL
  4. Test TLS 1.2 acceptance with OpenSSL
  5. Test certificate expiry with SSL Labs

### Acceptance Criteria Validation
- **AC2**: ✅ TLS 1.2+ enforced + HSTS headers + no HTTP downgrade + certificate renewal monitoring

## Success Criteria Checklist
- [MANDATORY] Kestrel configured for TLS 1.2+ minimum protocol
- [MANDATORY] Forwarded headers middleware configured for Railway/Render proxy
- [MANDATORY] HTTPS redirection middleware enabled (HTTP 307)
- [MANDATORY] HSTS middleware configured (max-age: 1 year, includeSubDomains)
- [MANDATORY] SecurityHeadersMiddleware created with X-Content-Type-Options, X-Frame-Options, CSP
- [MANDATORY] CORS policy enforces HTTPS-only origins in production
- [MANDATORY] appsettings.json updated with HTTPS redirection config
- [MANDATORY] appsettings.Production.json updated with HSTS and frontend URL
- [MANDATORY] TLS certificate monitoring procedure documented
- [MANDATORY] AUTHENTICATION.md updated with HTTPS-only security
- [MANDATORY] Integration test: HTTP redirects to HTTPS
- [MANDATORY] Integration test: HTTPS returns HSTS header
- [MANDATORY] Integration test: Security headers present
- [MANDATORY] Manual test: TLS 1.1 rejected
- [MANDATORY] Manual test: TLS 1.2 accepted
- [RECOMMENDED] SSL Labs test: A+ rating
- [RECOMMENDED] Automated certificate expiration monitoring (UptimeRobot)

## Estimated Effort
**3 hours** (Kestrel configuration + middlewares + CORS update + certificate monitoring docs + tests)
