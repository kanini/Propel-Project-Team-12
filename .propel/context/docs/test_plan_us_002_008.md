---
id: test_plan_us_002_008
title: Test Plan - EP-TECH Foundation User Stories (US_002-008)
version: 1.0.0
status: draft
author: AI Assistant
created: 2026-03-23
scope: "EP-TECH technical setup and infrastructure"
---

# Test Plan: EP-TECH Technical Foundation (US_002-008)

## Overview

This test plan covers 7 foundational technical stories essential for backend infrastructure setup. These stories establish the project structure, database, authentication, and deployment pipeline that support all downstream functional work.

**User Stories Covered:**
- US_002: Backend Project Scaffolding with .NET 8
- US_003: Database Provisioning with PostgreSQL and pgvector
- US_004: Authentication Infrastructure with JWT and BCrypt
- US_005: API Documentation and Health Check Endpoints
- US_006: Session Caching with Upstash Redis
- US_007: CI/CD Pipeline and Free-Tier Deployment
- US_008: Test Infrastructure Setup

---

## 1. US_002: Backend Project Scaffolding with .NET 8

### Test Objectives
- Verify .NET 8 solution structure with correct three-layer architecture
- Validate project references follow dependency inversion pattern
- Confirm build succeeds without errors
- Validate DI registration in Program.cs

### Test Cases

#### TC-US-002-HP-01: Solution Compiles Successfully
| Field | Value |
|-------|-------|
| Requirement | TR-002 |
| Type | happy_path |
| Priority | P0 |

**Given**: Solution file exists with three projects (Web, Business, Data)  
**When**: Execute `dotnet build`  
**Then**: Build completes with 0 errors, 0 warnings (or acceptable warnings)

**Expected Results:**
- [ ] Solution builds successfully
- [ ] All projects target .NET 8.0
- [ ] No compilation errors
- [ ] Dependencies resolved correctly

---

#### TC-US-002-HP-02: Three-Layer Architecture Implemented
| Field | Value |
|-------|-------|
| Requirement | TR-002, AD-001 |
| Type | happy_path |
| Priority | P0 |

**Given**: Repository contains PatientAccess.sln  
**When**: Inspect project structure  
**Then**: Three projects exist: PatientAccess.Web, PatientAccess.Business, PatientAccess.Data

**Expected Results:**
- [ ] Web layer exists and references Business layer only
- [ ] Business layer exists and references Data layer only
- [ ] Data layer exists with no reference to Web layer
- [ ] Dependency flow: Web → Business → Data (no circular refs)

---

#### TC-US-002-ER-01: Prevent Circular Dependencies
| Field | Value |
|-------|-------|
| Requirement | TR-002, AD-001 |
| Type | error |
| Priority | P1 |

**Given**: Solution structure defined  
**When**: Attempt to add reference from Data to Web  
**Then**: Build fails or review identifies architectural violation

---

#### TC-US-002-HP-03: Folder Structure Follows Design
| Field | Value |
|-------|-------|
| Requirement | TR-002 |
| Type | happy_path |
| Priority | P1 |

**Given**: Repository cloned  
**When**: Inspect folder layout  
**Then**: Controllers/, Services/, Repositories/, Models/, DTOs/, Middleware/ exist in appropriate layers

---

### Related Requirements
- **FR-037-043**: All require backend structure for implementation
- **NFR-012**: Code coverage depends on test infrastructure in projects

---

## 2. US_003: Database Provisioning with PostgreSQL and pgvector

### Test Objectives
- Verify PostgreSQL 16 connectivity via Supabase
- Confirm pgvector extension enabled and functional
- Validate EF Core DbContext configuration
- Test migration generation and application

### Test Cases

#### TC-US-003-HP-01: Database Connection Succeeds
| Field | Value |
|-------|-------|
| Requirement | TR-003, DR-001 |
| Type | happy_path |
| Priority | P0 |

**Given**: Supabase PostgreSQL 16 provisioned  
**When**: Application starts  
**Then**: Database connection established successfully

**Expected Results:**
- [ ] Connection string loaded from environment (not hardcoded)
- [ ] Connection test completes within 5 seconds
- [ ] No sensitive credentials in logs

---

#### TC-US-003-HP-02: pgvector Extension Enabled
| Field | Value |
|-------|-------|
| Requirement | TR-003, AIR-R04 |
| Type | happy_path |
| Priority | P0 |

**Given**: Connected to PostgreSQL 16  
**When**: Execute `SELECT * FROM pg_extension WHERE extname='vector'`  
**Then**: pgvector extension is created and available

**Expected Results:**
- [ ] pgvector extension shown in extensions list
- [ ] 1536-dimensional vector columns can be created
- [ ] Vector similarity operations functional

---

#### TC-US-003-HP-03: EF Core DbContext Configured
| Field | Value |
|-------|-------|
| Requirement | TR-003 |
| Type | happy_path |
| Priority | P0 |

**Given**: PatientAccess.Data project exists  
**When**: Inspect PatientAccessDbContext.cs  
**Then**: Class inherits from DbContext with proper configuration

**Expected Results:**
- [ ] DbContext registered in DI container
- [ ] Connection string resolved from IConfiguration
- [ ] DbSets defined for all entities
- [ ] Fluent API configurations applied

---

#### TC-US-003-HP-04: Initial Migration Generates
| Field | Value |
|-------|-------|
| Requirement | DR-008, DR-009 |
| Type | happy_path |
| Priority | P1 |

**Given**: DbContext configured with initial entities  
**When**: Execute `dotnet ef migrations add InitialCreate`  
**Then**: Migration file generated with Up/Down methods

**Expected Results:**
- [ ] Migration file created with timestamp
- [ ] Creates all tables with proper schema
- [ ] Down method includes drop statements

---

### Related Requirements
- **DR-001-016**: All data storage depends on this database setup
- **AIR-R04**: Vector storage for medical code embeddings

---

## 3. US_004: Authentication Infrastructure with JWT and BCrypt

### Test Objectives
- Verify JWT Bearer authentication with RS256 signing
- Confirm BCrypt implementation with cost factor 12
- Validate CORS policy enforcement
- Test authentication failures return appropriate errors

### Test Cases

#### TC-US-004-HP-01: JWT Token Generation Works
| Field | Value |
|-------|-------|
| Requirement | TR-012, FR-002 |
| Type | happy_path |
| Priority | P0 |

**Given**: Valid user credentials provided  
**When**: Call authentication endpoint  
**Then**: JWT token returned with user claims

**Expected Results:**
- [ ] Token includes user ID claim
- [ ] Token includes email claim
- [ ] Token includes role claim (Patient/Staff/Admin)
- [ ] Token signed with RS256 algorithm
- [ ] Default TTL = 15 minutes
- [ ] Token is parseable and claims extractable

---

#### TC-US-004-HP-02: JWT Token Validation Works
| Field | Value |
|-------|-------|
| Requirement | TR-012, NFR-005 |
| Type | happy_path |
| Priority | P0 |

**Given**: Valid JWT token provided  
**When**: Request protected endpoint with token in Authorization header  
**Then**: Request authorized and processed

**Expected Results:**
- [ ] Valid issuer accepted
- [ ] Valid audience accepted
- [ ] Token lifetime validated (not expired)
- [ ] User identity extracted from claims

---

#### TC-US-004-HP-03: BCrypt Password Hashing
| Field | Value |
|-------|-------|
| Requirement | TR-013, FR-001 |
| Type | happy_path |
| Priority | P0 |

**Given**: Plain text password "SecurePass123!"  
**When**: Hash password using BCrypt service  
**Then**: Irreversible hash generated with cost factor 12

**Expected Results:**
- [ ] Hash length consistent ($2b$12$ format)
- [ ] Repeated hashing of same password produces different result
- [ ] VerifyPassword("SecurePass123!", hash) returns true
- [ ] VerifyPassword("WrongPassword", hash) returns false

---

#### TC-US-004-ER-01: Unauthenticated Request Rejected
| Field | Value |
|-------|-------|
| Requirement | NFR-006, FR-043 |
| Type | error |
| Priority | P1 |

**Given**: No JWT token provided  
**When**: Request protected endpoint without Authentication header  
**Then**: API returns 401 Unauthorized

**Expected Results:**
- [ ] Response status code is 401
- [ ] No sensitive information in response body
- [ ] Error type distinguishable in logs

---

#### TC-US-004-ER-02: Expired Token Rejected
| Field | Value |
|-------|-------|
| Requirement | NFR-005 |
| Type | error |
| Priority | P1 |

**Given**: JWT token expired (created 16+ minutes ago)  
**When**: Request protected endpoint with expired token  
**Then**: API returns 401 Unauthorized with token_expired error code

---

#### TC-US-004-ER-03: CORS Policy Enforced
| Field | Value |
|-------|-------|
| Requirement | TR-014, NFR-004 |
| Type | error |
| Priority | P1 |

**Given**: Request from non-allowed origin  
**When**: Browser sends CORS preflight request  
**Then**: 403 Forbidden returned; request blocked

**Expected Results:**
- [ ] Allowed origins return 200 OK for preflight
- [ ] Disallowed origins return 403 Forbidden
- [ ] CORS headers properly configured

---

### Related Requirements
- **FR-001-004**: User registration and authentication depend on this
- **NFR-005**: Session timeout enforcement
- **NFR-006**: RBAC foundation

---

## 4. US_005: API Documentation and Health Check Endpoints

### Test Objectives
- Verify Swagger UI accessible and comprehensive
- Confirm OpenAPI 3.0 spec valid and complete
- Test health endpoints monitor all dependencies
- Validate health check timeout protection

### Test Cases

#### TC-US-005-HP-01: Swagger UI Accessible
| Field | Value |
|-------|-------|
| Requirement | TR-005 |
| Type | happy_path |
| Priority | P1 |

**Given**: Application running in development  
**When**: Navigate to `/swagger`  
**Then**: Swagger UI displays all endpoints

**Expected Results:**
- [ ] Swagger UI loads successfully
- [ ] All controllers and endpoints listed
- [ ] Authentication requirements shown
- [ ] Example payloads provided

---

#### TC-US-005-HP-02: OpenAPI 3.0 Spec Valid
| Field | Value |
|-------|-------|
| Requirement | TR-005, AD-002 |
| Type | happy_path |
| Priority | P1 |

**Given**: API running  
**When**: Fetch `/swagger/v1/swagger.json`  
**Then**: Valid OpenAPI 3.0 JSON specification returned

**Expected Results:**
- [ ] JSON parses without errors
- [ ] OpenAPI version is 3.0.x
- [ ] All endpoints documented
- [ ] Request/response schemas defined

---

#### TC-US-005-HP-03: Health Check Endpoint Works
| Field | Value |
|-------|-------|
| Requirement | TR-018, NFR-008 |
| Type | happy_path |
| Priority | P0 |

**Given**: All dependencies healthy  
**When**: Call `GET /health`  
**Then**: 200 OK returned with healthy status

**Expected Results:**
- [ ] Response status is 200
- [ ] JSON includes database connectivity status
- [ ] JSON includes Redis connectivity status
- [ ] Overall status is "Healthy"
- [ ] Response time < 2 seconds

---

#### TC-US-005-ER-01: Health Check Detects Unhealthy Dependencies
| Field | Value |
|-------|-------|
| Requirement | TR-018 |
| Type | error |
| Priority | P1 |

**Given**: Database unavailable  
**When**: Call `GET /health`  
**Then**: 503 Service Unavailable returned

**Expected Results:**
- [ ] Response status is 503
- [ ] Database status shown as "Unhealthy"
- [ ] Other statuses accurate
- [ ] Specific unhealthy component identified

---

### Related Requirements
- **AD-002**: API-first design documentation
- **NFR-008**: Uptime monitoring via health checks

---

## 5. US_006: Session Caching with Upstash Redis

### Test Objectives
- Verify Upstash Redis connection established
- Confirm session tokens stored with 15-minute TTL
- Test token expiration and eviction
- Validate PHI data NOT cached (Zero-PHI strategy)

### Test Cases

#### TC-US-006-HP-01: Redis Connection Established
| Field | Value |
|-------|-------|
| Requirement | TR-001, AD-005 |
| Type | happy_path |
| Priority | P0 |

**Given**: Upstash Redis provisioned  
**When**: Application starts  
**Then**: Redis connection established via TLS

**Expected Results:**
- [ ] Connection succeeds
- [ ] TLS certificate validated
- [ ] No plaintext connection possible
- [ ] Connection string from environment only

---

#### TC-US-006-HP-02: Session Token Cached with TTL
| Field | Value |
|-------|-------|
| Requirement | AD-005, NFR-005 |
| Type | happy_path |
| Priority | P0 |

**Given**: User authenticated  
**When**: Session token generated  
**Then**: Token stored in Redis with 15-minute TTL

**Expected Results:**
- [ ] Token exists in Redis
- [ ] TTL set to 900 seconds (15 min)
- [ ] Token value retrievable
- [ ] Token encrypted at rest (if applicable)

---

#### TC-US-006-HP-03: Session Expires After 15 Minutes
| Field | Value |
|-------|-------|
| Requirement | NFR-005 |
| Type | happy_path |
| Priority | P1 |

**Given**: Valid session token with 15-minute TTL  
**When**: Wait 15+ minutes  
**Then**: Token automatically evicted from Redis

**Expected Results:**
- [ ] Token no longer in Redis after TTL
- [ ] Application treats as invalid/expired
- [ ] User redirected to login

---

#### TC-US-006-ER-01: PHI Not Cached in Redis
| Field | Value |
|-------|-------|
| Requirement | AD-005, NFR-014 |
| Type | error |
| Priority | P0 |

**Given**: Session caching configured  
**When**: Audit Redis contents  
**Then**: Only session tokens present, NO patient health data

**Expected Results:**
- [ ] No medications in Redis
- [ ] No allergies in Redis
- [ ] No medical history in Redis
- [ ] No PHI accessible via Redis keys

---

### Related Requirements
- **NFR-005**: Session timeout enforcement
- **AD-005**: Zero-PHI caching strategy
- **NFR-014**: Minimum necessary access

---

## 6. US_007: CI/CD Pipeline and Free-Tier Deployment

### Test Objectives
- Verify GitHub Actions pipeline executes successfully
- Confirm Vercel frontend deployment works
- Test Railway/Render backend deployment
- Validate environment-specific configuration

### Test Cases

#### TC-US-007-HP-01: GitHub Actions Build Pipeline Succeeds
| Field | Value |
|-------|-------|
| Requirement | TR-006, TR-007 |
| Type | happy_path |
| Priority | P0 |

**Given**: PR opened with code changes  
**When**: GitHub Actions workflow triggered  
**Then**: Build pipeline completes successfully

**Expected Results:**
- [ ] Checkout step succeeds
- [ ] Dependencies installed
- [ ] Frontend builds without errors
- [ ] Backend builds without errors
- [ ] Linting passes
- [ ] Tests pass

---

#### TC-US-007-HP-02: Vercel Frontend Deployment
| Field | Value |
|-------|-------|
| Requirement | TR-006 |
| Type | happy_path |
| Priority | P1 |

**Given**: GitHub Actions pipeline passes  
**When**: Code merged to main branch  
**Then**: React app automatically deployed to Vercel

**Expected Results:**
- [ ] Vercel build triggered automatically
- [ ] React app builds successfully
- [ ] Environment variables set correctly
- [ ] App accessible at Vercel URL
- [ ] Assets cached properly

---

#### TC-US-007-HP-03: Railway/Render Backend Deployment
| Field | Value |
|-------|-------|
| Requirement | TR-006 |
| Type | happy_path |
| Priority | P1 |

**Given**: GitHub Actions pipeline passes  
**When**: Code merged to main branch  
**Then**: .NET API deployed to Railway/Render

**Expected Results:**
- [ ] Docker image built
- [ ] Container pushed to registry
- [ ] Railway/Render deployment triggered
- [ ] API health check passes
- [ ] Database migrations applied
- [ ] API accessible at deployed URL

---

#### TC-US-007-ER-01: Failed Build Prevents Deployment
| Field | Value |
|-------|-------|
| Requirement | TR-006 |
| Type | error |
| Priority | P1 |

**Given**: PR with failing tests  
**When**: GitHub Actions pipeline runs  
**Then**: Build fails; deployment blocked

**Expected Results:**
- [ ] Pipeline exits with non-zero status
- [ ] PR status shows failure
- [ ] No deployment occurs
- [ ] Notifications sent to PR author

---

### Related Requirements
- **TR-006**: Free-tier deployment requirement
- **NFR-008**: 99.9% uptime via reliable deployment

---

## 7. US_008: Test Infrastructure Setup

### Test Objectives
- Verify xUnit test framework configured for backend
- Confirm Vitest configured for frontend
- Test Playwright E2E framework functional
- Validate test coverage reporting at 80%+ target

### Test Cases

#### TC-US-008-HP-01: xUnit Backend Tests Execute
| Field | Value |
|-------|-------|
| Requirement | NFR-012, TR-008 |
| Type | happy_path |
| Priority | P0 |

**Given**: xUnit test project created  
**When**: Execute `dotnet test`  
**Then**: Tests run successfully

**Expected Results:**
- [ ] Test project compiles
- [ ] All tests execute
- [ ] Coverage report generated
- [ ] Test results displayed
- [ ] Exit code 0 on success

---

#### TC-US-008-HP-02: Vitest Frontend Tests Execute
| Field | Value |
|-------|-------|
| Requirement | NFR-012, TR-008 |
| Type | happy_path |
| Priority | P0 |

**Given**: Vitest configured in React project  
**When**: Execute `npm run test`  
**Then**: Component tests execute successfully

**Expected Results:**
- [ ] Test discovery works
- [ ] Tests compile with TypeScript
- [ ] Test results displayed
- [ ] Coverage output generated
- [ ] Watch mode functional

---

#### TC-US-008-HP-03: Playwright E2E Tests Execute
| Field | Value |
|-------|-------|
| Requirement | NFR-012, TR-008 |
| Type | happy_path |
| Priority | P1 |

**Given**: Playwright configured  
**When**: Execute Playwright tests  
**Then**: Browser automation tests run

**Expected Results:**
- [ ] Browser launches (Chromium minimum)
- [ ] Tests navigate to base URL
- [ ] Assertions evaluate correctly
- [ ] Screenshots/traces captured on failure
- [ ] Reports generated

---

#### TC-US-008-HP-04: Code Coverage at 80%+ Target
| Field | Value |
|-------|-------|
| Requirement | NFR-012 |
| Type | happy_path |
| Priority | P1 |

**Given**: Test suites executed with coverage collection  
**When**: Coverage report generated  
**Then**: Target 80%+ coverage achieved for business logic

**Expected Results:**
- [ ] Line coverage ≥80%
- [ ] Branch coverage ≥75%
- [ ] Function coverage ≥80%
- [ ] Coverage report accessible
- [ ] Trend tracking available

---

### Related Requirements
- **NFR-012**: Code coverage 80% target
- **TR-008**: Test framework setup

---

## Test Execution Strategy

### Execution Order
1. US_002: Project structure (prerequisite for all)
2. US_003: Database (prerequisite for data work)
3. US_004: Authentication (prerequisite for protected endpoints)
4. US_005: Documentation (can run in parallel)
5. US_006: Caching (can run in parallel)
6. US_007: CI/CD (system-level testing)
7. US_008: Tests (ongoing throughout)

### Environment Requirements
| Component | Environment | Tech Stack |
|-----------|-------------|-----------|
| Backend Build | Windows/Linux | .NET 8 SDK, Visual Studio 2022+ |
| Frontend Build | Windows/Linux | Node.js 18+, npm/yarn |
| Database | Cloud | Supabase PostgreSQL 16 |
| Cache | Cloud | Upstash Redis |
| CI/CD | Cloud | GitHub Actions |
| Deployment | Cloud | Vercel, Railway/Render |

---

## Success Criteria

- [ ] All 7 user stories have comprehensive test plans
- [ ] 100% acceptance criteria mapped to test cases
- [ ] All technical requirements (TR-001 through TR-008) covered
- [ ] Foundation established for downstream stories
- [ ] No blocking issues in technical foundation

---

## Sign-Off

**Status**: ✅ **READY FOR IMPLEMENTATION**  
**Scope**: EP-TECH technical foundation  
**Coverage**: 7 user stories, 31 test cases  
**Completion Target**: Prior to EP-001 (Authentication stories)
