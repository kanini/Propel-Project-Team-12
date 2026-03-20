# Caching Strategy

## Overview

The Patient Access Platform implements a **Zero-PHI Distributed Caching Strategy** using Upstash Redis for session token storage with database fallback. This approach balances performance optimization with strict HIPAA compliance requirements.

## Zero-PHI Caching Policy

### What is Cached

**ALLOWED in Redis:**
- Session tokens (JWT tokens)
- User IDs (non-PHI identifiers)
- Session metadata (expiration timestamps, last access time)
- Non-sensitive configuration data

**PROHIBITED in Redis:**
- Patient health information (diagnoses, medications, allergies, vitals)
- Clinical documents or document content
- Appointment details (visit reasons, provider notes)
- Patient demographics (name, date of birth, contact information)
- Insurance information
- Audit trail data

### Rationale

Protected Health Information (PHI) is **NEVER** stored in Redis to:
1. **HIPAA Compliance**: Minimize PHI exposure surface area
2. **Security**: Reduce risk of PHI leakage through cache poisoning or unauthorized access
3. **Privacy**: Ensure PHI remains in encrypted database with full audit trail
4. **Simplicity**: Avoid complex cache invalidation for clinical data updates

## Redis Configuration

### Upstash Redis Free Tier

The platform uses Upstash Redis free tier for distributed caching:
- **Provider**: Upstash (https://upstash.com)
- **Plan**: Free tier (10,000 commands/day, 256MB storage)
- **TLS**: Enabled by default
- **Region**: Choose region closest to database for minimal latency

### Connection Configuration

Redis connection is configured in `appsettings.json`:

```json
{
  "RedisSettings": {
    "ConnectionString": "redis://default:password@host:port",
    "Enabled": true,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "AbortOnConnectFail": false
  }
}
```

**Configuration Parameters:**
- `ConnectionString`: Upstash Redis connection string (store in environment variables for production)
- `Enabled`: Set to `false` to disable Redis and use database-only mode
- `ConnectTimeout`: Maximum time (ms) to wait for Redis connection (default: 5000ms)
- `SyncTimeout`: Maximum time (ms) for Redis operations (default: 5000ms)
- `AbortOnConnectFail`: Set to `false` to enable graceful degradation when Redis is unavailable

### Environment Variables (Production)

For production deployments, override settings using environment variables:

```bash
REDIS__CONNECTIONSTRING="redis://default:your-password@your-upstash-endpoint:port"
REDIS__ENABLED="true"
```

## Session Caching

### Session Token Storage

Session tokens are cached in Redis with the following strategy:

**Key Format:**
```
session:{userId}
```

**Value:**
```
JWT token string
```

**TTL (Time To Live):**
- **Initial TTL**: 15 minutes (NFR-005 - automatic session timeout)
- **Sliding Expiration**: TTL is refreshed on every session access (read operation)

### Session Lifecycle

1. **Login**: JWT token generated and stored in Redis with 15-minute TTL
2. **Session Access**: Every API request refreshes TTL to 15 minutes (sliding expiration)
3. **Session Expiry**: After 15 minutes of inactivity, session is automatically removed from Redis
4. **Logout**: Session explicitly removed from Redis and database

### Sliding Expiration

Sliding expiration ensures active users remain authenticated without manual re-login:

- User makes API request → Session retrieved from Redis → TTL refreshed to 15 minutes
- User inactive for 15 minutes → Session expires and is removed from cache
- User must re-authenticate after expiration

## Database Fallback Strategy

### Graceful Degradation

The platform implements **graceful degradation** when Redis is unavailable:

1. **Redis Unavailable**: Application detects connection failure or timeout
2. **Automatic Fallback**: Session operations fall back to database storage
3. **Continued Operation**: Application continues functioning without Redis
4. **Health Check**: Redis marked as `Degraded` (not `Unhealthy`) in `/health` endpoint

### Fallback Behavior

| Operation | Redis Available | Redis Unavailable |
|-----------|----------------|-------------------|
| **SetSession** | Store in Redis (15-min TTL) | Store in database |
| **GetSession** | Retrieve from Redis + refresh TTL | Retrieve from database |
| **RemoveSession** | Remove from Redis + database | Remove from database |
| **RefreshSession** | Refresh TTL in Redis | Update expiration in database |

### Database Session Storage

**TODO: Database session storage not yet implemented.**

Database fallback requires a `SessionTokens` table:

```sql
CREATE TABLE SessionTokens (
    UserId VARCHAR(255) PRIMARY KEY,
    Token TEXT NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_session_expiration ON SessionTokens(ExpiresAt);
```

**Implementation Status:**
- ✅ Redis caching fully implemented
- ⚠️ Database fallback methods stubbed (return false/null)
- ❌ Database session storage pending (requires migration)

## Performance Targets

### Session Validation Performance

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Session Lookup** | < 10ms | Redis GET operation |
| **Session Store** | < 10ms | Redis SET operation |
| **TTL Refresh** | < 5ms | Redis EXPIRE operation |
| **Fallback Lookup** | < 50ms | Database query with index |

### Cache Hit Rate

**Target**: > 95% cache hit rate for session validation

**Monitoring**:
- Track Redis cache hits vs. database fallback calls
- Log cache miss events for analysis
- Monitor Redis availability percentage

## Security Considerations

### HIPAA Compliance

**Zero-PHI Policy:**
- Redis cache is **PHI-free zone** - no patient health information stored
- Session tokens are cryptographically signed JWT tokens (not PHI)
- User IDs are non-PHI identifiers (not patient medical record numbers)

**Encryption:**
- **In Transit**: Redis connections use TLS 1.2+ (Upstash default)
- **At Rest**: Session tokens encrypted in database (fallback storage)
- **Token Security**: JWT tokens expire after 15 minutes (NFR-005)

### Access Control

**Redis Access:**
- Redis connection string stored in environment variables (not committed to source control)
- Redis credentials rotated periodically
- Upstash dashboard access restricted to authorized DevOps personnel

## Monitoring and Health Checks

### Health Check Endpoint

Redis health is exposed via `/health` endpoint:

**Response (Healthy):**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

**Response (Redis Degraded):**
```json
{
  "status": "Degraded",
  "checks": {
    "database": "Healthy",
    "redis": "Degraded"
  }
}
```

**HTTP Status Codes:**
- `200 OK`: All systems healthy or degraded (application functional)
- `503 Service Unavailable`: Database unhealthy (application non-functional)

### Logging

Redis operations are logged for diagnostics:

**Log Levels:**
- `Debug`: Cache hits, TTL refreshes
- `Information`: Redis connection established, fallback mode activated
- `Warning`: Redis connection failures, timeout errors
- `Error`: Unexpected Redis exceptions

**Example Logs:**
```
[Debug] Session cached in Redis for user abc-123 with 15-minute TTL
[Warning] Redis unavailable for GetSession (user: abc-123). Falling back to database.
[Information] Redis caching disabled in configuration. Application will use database-only session management.
```

## Setup Instructions

### 1. Create Upstash Redis Instance

1. Sign up at https://upstash.com (free tier)
2. Create a new Redis database
3. Enable TLS (enabled by default)
4. Copy the connection string (format: `redis://default:password@endpoint:port`)

### 2. Configure Application

**Development (appsettings.Development.json):**
```json
{
  "RedisSettings": {
    "ConnectionString": "your-upstash-connection-string",
    "Enabled": true
  }
}
```

**Production (Environment Variables):**
```bash
export REDIS__CONNECTIONSTRING="redis://default:password@endpoint:port"
export REDIS__ENABLED="true"
```

### 3. Verify Configuration

Start the application and check logs:

```bash
dotnet run --project src/backend/PatientAccess.Web
```

**Expected Logs:**
```
info: Program[0]
      Redis connection established successfully
info: Program[0]
      Redis session caching enabled. Session tokens will be cached with 15-minute TTL.
```

### 4. Test Health Check

```bash
curl http://localhost:5000/health
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

## Troubleshooting

### Redis Connection Failures

**Symptom:** Logs show "Failed to connect to Redis"

**Solutions:**
1. Verify connection string format: `redis://default:password@host:port`
2. Check Upstash dashboard for database status
3. Verify TLS is enabled in Upstash settings
4. Check network connectivity to Upstash endpoint
5. Verify credentials are correct

**Temporary Workaround:** Set `RedisSettings:Enabled` to `false` to use database-only mode

### Slow Redis Operations

**Symptom:** Session validation taking > 10ms

**Solutions:**
1. Check Upstash region - choose region closest to database
2. Review free tier command limits (10,000/day)
3. Monitor connection pool exhaustion
4. Increase `ConnectTimeout` and `SyncTimeout` if needed

### Database Fallback Not Working

**Symptom:** Session validation fails when Redis is down

**Action Required:** Database session storage not yet implemented. See "Database Fallback Strategy" section for implementation requirements.

## Future Enhancements

### Planned Improvements

1. **Database Session Storage**: Implement full database fallback (currently stubbed)
2. **Connection Pooling Metrics**: Track Redis connection pool utilization
3. **Cache Warming**: Pre-load frequently accessed sessions on startup
4. **Multi-Region Redis**: Configure Redis replicas for geo-distributed deployments
5. **Session Revocation**: Implement session revocation list for explicit token invalidation

## References

- **Upstash Documentation**: https://upstash.com/docs/redis
- **StackExchange.Redis**: https://stackexchange.github.io/StackExchange.Redis/
- **HIPAA Security Rule**: https://www.hhs.gov/hipaa/for-professionals/security/index.html
- **NFR-005**: 15-minute automatic session timeout requirement
- **AG-001**: 99.9% platform uptime with graceful degradation
