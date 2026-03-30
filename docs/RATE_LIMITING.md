# Rate Limiting Policy (AC2 - US_057)

## Overview

All API endpoints are protected by IP-based rate limiting to prevent abuse and ensure fair resource allocation. Rate limits are enforced using AspNetCoreRateLimit middleware with Redis backend for distributed rate limiting across multiple server instances.

## Rate Limit Thresholds

### General Limits (All Endpoints)

- **100 requests per minute** per IP address
- **1,000 requests per hour** per IP address

### Endpoint-Specific Limits

#### Authentication Endpoints

- **POST /api/auth/login**: 5 requests per 5 minutes per IP (prevents brute force attacks)
- **POST /api/auth/register**: 3 requests per hour per IP (prevents account enumeration)

#### Patient Endpoints

- **GET /api/patients/***: 60 requests per minute per IP

#### Appointment Endpoints

- **POST /api/appointments**: 10 requests per minute per IP

### Whitelisted IPs

- **127.0.0.1** (localhost): 10,000 requests per second (development/testing)

## HTTP 429 Response (AC2)

When rate limit is exceeded, the API returns HTTP 429 Too Many Requests:

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
  "timestamp": "2026-03-27T00:00:00Z"
}
```

## Audit Logging (AC2)

All rate limit violations are logged to the audit system with:

- **Action Type**: `RateLimitExceeded`
- **Resource**: API endpoint path
- **IP Address**: Client's IP
- **User Agent**: Client's User-Agent header
- **Result**: `Denied`

## Security Alerts (AC4)

The system monitors for suspicious activity patterns:

### Trigger Conditions

- **>10 authorization failures (401/403)** from the same IP within **5 minutes**

### Alert Actions

1. **Log Security Alert**: Warning logged with failure count and threshold
2. **Audit Log Entry**: Security alert logged with action type `SecurityAlert`
3. **IP Throttling** (optional): Reduce rate limit to 5 requests/minute for 1 hour

### Alert Response Example

```
SECURITY ALERT: Suspicious activity detected from IP 192.0.2.1.
15 authorization failures in 5 minutes. Threshold: 10.
```

## Redis-Backed Rate Limiting

Rate limiting uses Redis for distributed storage:

- **Key Format**: `AspNetCoreRateLimit:{ip}:{endpoint}:{period}`
- **Expiry**: Sliding window based on period (1m, 1h)
- **Graceful Degradation**: If Redis is unavailable, rate limiting falls back to in-memory cache

## Configuration

Rate limits are configured in `appsettings.json`:

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Forwarded-For",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ]
  },
  "SecurityAlerts": {
    "MaxAuthFailuresInWindow": 10,
    "AlertWindowMinutes": 5,
    "ThrottleDurationHours": 1
  }
}
```

## Testing Rate Limits

### Manual Testing

```bash
# Test rate limit (should get 429 after 5 requests)
for i in {1..6}; do
  curl -X POST https://api.patient-access.com/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"test@example.com","password":"wrong"}'
  sleep 1
done
```

### Expected Behavior

- **Requests 1-5**: HTTP 401 Unauthorized (wrong credentials)
- **Request 6**: HTTP 429 Too Many Requests (rate limit exceeded)

## Monitoring

Rate limit violations are tracked in:

1. **Application Logs**: `LogWarning` entries
2. **Audit Log Database**: `audit_logs` table, action type `RateLimitExceeded`
3. **Security Alerts**: `audit_logs` table, action type `SecurityAlert`

## Best Practices

### For API Consumers

1. **Respect Retry-After**: Wait the indicated seconds before retrying
2. **Implement Exponential Backoff**: Don't retry immediately on 429
3. **Cache Responses**: Reduce duplicate requests
4. **Use Pagination**: Avoid fetching large data sets in single requests

### For Administrators

1. **Monitor Security Alerts**: Review `SecurityAlert` audit logs daily
2. **Adjust Thresholds**: Tune rate limits based on usage patterns
3. **Whitelist Trusted IPs**: Add partner systems to `ClientWhitelist`
4. **Review Throttled IPs**: Investigate IPs with repeated throttling

## Troubleshooting

### Rate Limit Triggered Incorrectly

**Symptom**: Legitimate users hitting 429 errors

**Possible Causes**:
- Shared IP behind NAT/proxy
- Mobile carrier sharing IP pools
- Aggressive client-side polling

**Solutions**:
- Increase rate limit for specific endpoints
- Whitelist specific IPs
- Implement user-based rate limiting (requires authentication)

### Redis Connection Issues

**Symptom**: Rate limiting not working across instances

**Possible Causes**:
- Redis connection timeout
- Network issues between app and Redis
- Redis service down

**Solutions**:
- Check Redis connection string in configuration
- Verify network connectivity
- Check Redis health: `redis-cli ping`

## OWASP Compliance

This implementation aligns with:

- **OWASP API Security Top 10 (2023)**:
  - API4:2023 - Unrestricted Resource Consumption
  - API3:2023 - Broken Object Property Level Authorization
- **OWASP ASVS v4.0**:
  - V11.1 - Business Logic Security

## Related Documentation

- [API_MONITORING.md](./API_MONITORING.md) - API monitoring and metrics
- [AUTHENTICATION.md](./AUTHENTICATION.md) - Authentication and authorization
- [GITHUB_SECRETS_SETUP.md](./GITHUB_SECRETS_SETUP.md) - Redis configuration
