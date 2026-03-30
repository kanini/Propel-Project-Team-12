namespace PatientAccess.Web.Middleware;

/// <summary>
/// Middleware to add security headers to all HTTP responses (AC2 - US_056).
/// Complements HSTS with additional browser security protections against common vulnerabilities.
/// Epic: EP-010 - HIPAA Compliance & Security Hardening
/// Requirement: FR-042 (TLS 1.2+ encryption in transit), NFR-004 (Data protection standards)
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
        // Blocks browsers from interpreting files as a different MIME type than declared
        // Prevents execution of malicious scripts disguised as images or other safe file types
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Prevent clickjacking attacks
        // DENY: Never render page in iframe/frame/embed/object
        // Protects against UI redress attacks where attacker tricks user into clicking hidden elements
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection: Enable browser XSS filter (legacy browsers)
        // "1; mode=block": Enable XSS filter and block page rendering if attack detected
        // Note: Modern browsers rely on CSP, but this provides defense-in-depth for older browsers
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy: Control referrer information leakage
        // "strict-origin-when-cross-origin": Send full URL to same origin, only origin to cross-origin HTTPS
        // Prevents leaking sensitive URL parameters (appointment IDs, patient identifiers) to third parties
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content-Security-Policy: Restrict resource loading (mitigates XSS, clickjacking, data injection)
        // - default-src 'self': Only load resources from same origin by default
        // - script-src 'self' 'unsafe-inline' 'unsafe-eval': Allow scripts from same origin + inline scripts (required for React/Vite)
        // - style-src 'self' 'unsafe-inline': Allow styles from same origin + inline styles (required for Tailwind CSS)
        // - img-src 'self' data: https:: Allow images from same origin, data URIs, and any HTTPS source
        // - font-src 'self' data:: Allow fonts from same origin and data URIs
        // - connect-src 'self' https://...: Allow API requests to same origin and approved external services
        // - frame-ancestors 'none': Prevent embedding in iframes (redundant with X-Frame-Options, defense-in-depth)
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +  // Allow frontend JS frameworks (React/Vite)
            "style-src 'self' 'unsafe-inline'; " +                   // Allow inline styles (Tailwind CSS)
            "img-src 'self' data: https:; " +                        // Allow images from HTTPS sources
            "font-src 'self' data:; " +                              // Allow fonts from same origin and data URIs
            "connect-src 'self' https://api.openai.azure.com https://api.twilio.com https://api.sendgrid.com; " +  // Allow API requests to external services
            "frame-ancestors 'none';"                                // Prevent clickjacking (redundant with X-Frame-Options)
        );

        // Permissions-Policy: Restrict browser features (formerly Feature-Policy)
        // Disables unnecessary browser APIs that could leak sensitive data or be used for fingerprinting
        // - geolocation=(): Disable geolocation API (not needed for healthcare scheduling)
        // - microphone=(): Disable microphone access (no voice input required)
        // - camera=(): Disable camera access (no video calls or photo uploads in current scope)
        context.Response.Headers.Append(
            "Permissions-Policy",
            "geolocation=(), microphone=(), camera=()"
        );

        // IMPORTANT: Do NOT add Strict-Transport-Security (HSTS) here
        // HSTS is handled by UseHsts() middleware in Program.cs to respect environment context
        // (HSTS should only be enabled in production, not development)

        await _next(context);
    }
}

/// <summary>
/// Extension method to register SecurityHeadersMiddleware in the ASP.NET Core pipeline.
/// Usage in Program.cs: app.UseSecurityHeaders();
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds SecurityHeadersMiddleware to the application's request pipeline.
    /// Should be called early in the pipeline (after exception handling, before routing).
    /// </summary>
    /// <param name="builder">The IApplicationBuilder instance</param>
    /// <returns>The IApplicationBuilder instance for method chaining</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
