namespace PatientAccess.Business.Enums;

/// <summary>
/// Redis key prefixes for zero-PHI caching strategy (AC4 - US_056).
/// ONLY these key types are permitted in cache. PHI data is NEVER cached.
/// Epic: EP-010 - HIPAA Compliance & Security Hardening
/// Requirement: NFR-004 (zero-PHI caching strategy), AD-005 (design.md)
/// </summary>
public enum RedisKeyPrefix
{
    /// <summary>
    /// Session tokens (JWT, refresh tokens).
    /// Format: "session:{userId}:{sessionId}"
    /// TTL: 15 minutes
    /// Example: session:123e4567-e89b-12d3-a456-426614174000:a7b3c5d9
    /// PHI Status: ✅ No PHI (only authentication tokens)
    /// </summary>
    Session,

    /// <summary>
    /// Aggregate appointment counts (no patient identifiers).
    /// Format: "aggregate:appointments:{providerId}:{date}"
    /// TTL: 5 minutes
    /// Example: aggregate:appointments:789:2026-03-23 → {"total": 12, "available": 4}
    /// PHI Status: ✅ No PHI (aggregate counts only, no patient names/IDs)
    /// </summary>
    AggregateAppointments,

    /// <summary>
    /// Aggregate waitlist counts (no patient identifiers).
    /// Format: "aggregate:waitlist:{providerId}"
    /// TTL: 5 minutes
    /// Example: aggregate:waitlist:789 → {"total": 15}
    /// PHI Status: ✅ No PHI (aggregate counts only)
    /// </summary>
    AggregateWaitlist,

    /// <summary>
    /// Provider timeslot availability (no patient data).
    /// Format: "timeslots:{providerId}:{date}"
    /// TTL: 10 minutes
    /// Example: timeslots:789:2026-03-23 → ["09:00", "10:00", "14:00"]
    /// PHI Status: ✅ No PHI (provider schedules only, no patient bookings)
    /// </summary>
    ProviderTimeslots,

    /// <summary>
    /// Rate limiting counters (no PHI).
    /// Format: "ratelimit:{ipAddress}:{endpoint}"
    /// TTL: 5 minutes
    /// Example: ratelimit:192.168.1.1:/api/appointments → 5
    /// PHI Status: ✅ No PHI (request counters only)
    /// </summary>
    RateLimit,

    /// <summary>
    /// Feature flags (no PHI).
    /// Format: "feature:{flagName}"
    /// TTL: 60 minutes
    /// Example: feature:enableWaitlist → true
    /// PHI Status: ✅ No PHI (configuration only)
    /// </summary>
    FeatureFlag
}
