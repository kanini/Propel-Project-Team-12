# Task - task_001_upstash_redis_configuration

## Requirement Reference
- User Story: us_006
- Story Location: .propel/context/tasks/EP-TECH/us_006/us_006.md
- Acceptance Criteria:
    - AC-1: Redis connection established using TLS with connection string from environment configuration
    - AC-2: Session tokens stored with 15-minute TTL and automatic eviction
    - AC-3: Only session tokens and non-PHI metadata cached (Zero-PHI caching strategy)
    - AC-4: Lookup completes within 10ms with TTL refresh on valid access (sliding expiration)
- Edge Case:
    - Redis temporarily unavailable should fall back to database-based session validation
    - Connection pool exhaustion should retry with exponential backoff

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

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET ASP.NET Core | 8.0 |
| Library | StackExchange.Redis | 2.x |
| Caching | Upstash Redis | Redis 7.x compatible |

**Note**: All code and libraries MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview
Configure Upstash Redis free-tier instance as distributed cache for session token storage with 15-minute TTL and sliding expiration. Implement Zero-PHI caching strategy ensuring no patient health information is stored in Redis. Configure TLS connection with connection pooling and implement graceful fallback to database when Redis is unavailable. This enables fast session validation (<10ms) while maintaining HIPAA compliance.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project structure

## Impacted Components
- **MODIFY** src/backend/PatientAccess.Web/PatientAccess.Web.csproj - Add StackExchange.Redis package
- **MODIFY** src/backend/PatientAccess.Web/Program.cs - Register Redis connection
- **MODIFY** src/backend/PatientAccess.Web/appsettings.json - Add Redis connection configuration
- **NEW** src/backend/PatientAccess.Business/Services/ISessionCacheService.cs - Session cache interface
- **NEW** src/backend/PatientAccess.Business/Services/SessionCacheService.cs - Redis implementation with fallback

## Implementation Plan
1. **Provision Upstash Redis**: Create free-tier Upstash account and Redis instance with TLS enabled
2. **Install StackExchange.Redis**: Add NuGet package for Redis client
3. **Configure Connection String**: Store Upstash connection string securely in environment configuration
4. **Implement SessionCacheService**: Create service for storing/retrieving session tokens with 15-minute TTL
5. **Configure Connection Multiplexer**: Setup Redis connection with pooling and retry policies
6. **Implement Sliding Expiration**: Refresh TTL on every valid session access
7. **Implement Database Fallback**: Gracefully handle Redis unavailability with database-based validation
8. **Verify Zero-PHI Policy**: Ensure only session tokens (no patient data) stored in cache

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── PatientAccess.Business/
│   │   └── Services/
│   └── PatientAccess.Data/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/PatientAccess.Web.csproj | Add StackExchange.Redis NuGet package |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add RedisSettings section with connection string placeholder |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register Redis ConnectionMultiplexer as Singleton |
| CREATE | src/backend/PatientAccess.Business/Services/ISessionCacheService.cs | Interface for session caching operations |
| CREATE | src/backend/PatientAccess.Business/Services/SessionCacheService.cs | Redis-based session cache with 15-min TTL and fallback |
| CREATE | docs/CACHING.md | Caching strategy documentation including Zero-PHI policy |

## External References
- Upstash Redis: https://upstash.com/docs/redis
- StackExchange.Redis Documentation: https://stackexchange.github.io/StackExchange.Redis/
- Distributed Caching in ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-8.0
- Redis Sliding Expiration: https://redis.io/commands/expire/
- HIPAA Caching Considerations: https://www.hhs.gov/hipaa/for-professionals/security/guidance/index.html

## Build Commands
```bash
# Install StackExchange.Redis
cd src/backend/PatientAccess.Business
dotnet add package StackExchange.Redis

# Test Redis connection
redis-cli -u "redis://user:password@host:port" PING
```

## Implementation Validation Strategy
- [ ] Unit tests pass (create tests for SessionCacheService with Redis mock)
- [ ] Integration tests pass (test with real Redis instance)
- [ ] Redis connection established successfully with TLS
- [ ] Session token stored in Redis with 15-minute TTL
- [ ] Session lookup completes within 10ms
- [ ] TTL refreshed on session access (sliding expiration verified)
- [ ] Fallback to database works when Redis is unavailable
- [ ] Zero-PHI policy verified (no patient data in Redis inspection)

## Implementation Checklist
- [ ] Create Upstash Redis free-tier account and provision instance
- [ ] Add StackExchange.Redis NuGet package to Business project
- [ ] Configure Redis connection string in appsettings.json and environment variables
- [ ] Register ConnectionMultiplexer as Singleton in DI container
- [ ] Create ISessionCacheService interface with Get/Set/Remove/Refresh methods
- [ ] Implement SessionCacheService with 15-minute TTL and sliding expiration
- [ ] Implement database fallback logic for Redis unavailable scenarios
- [ ] Document Zero-PHI caching policy in CACHING.md
