# Project Plan - Unified Patient Access & Clinical Intelligence Platform

## Executive Summary

**Project Name**: Unified Patient Access & Clinical Intelligence Platform

**Project Type**: Green-field

**Business Context**: Healthcare organizations experience disconnected data pipelines causing 15% no-show rates, 20+ minutes manual clinical data extraction, and fragmented solutions lacking clinical context. This project addresses critical operational inefficiencies impacting revenue, patient experience, and clinical safety.

**Solution Overview**: An intelligent, integration-ready aggregator providing intuitive appointment booking with dynamic slot swapping, AI-assisted patient intake, and a "Trust-First" 360-Degree Patient View that transforms clinical prep from 20 minutes to 2-minute verification. The platform serves three user groups: Patients (self-service booking, document upload, health dashboard), Staff (walk-in booking, queue management, clinical data verification), and Admin (user management, audit access).

**Key Stakeholders**:

- Healthcare Organization Leadership (sponsor)
- Clinical Staff (front desk, call center)
- IT Department (technical oversight)
- Patients (end users)
- Compliance/Legal (HIPAA oversight)

**AI-Paired Development**: Yes — AI reduction factors applied to development effort estimates (0.75 for simple/medium requirements, 0.85 for complex requirements)

## Project Scope

### In Scope

- **Patient Portal** - Self-service registration, authentication, provider browsing, appointment booking with dynamic slot swap, waitlist enrollment, dual-mode intake (AI/manual), clinical document upload, 360-Degree Patient View (read-only)
- **Staff Portal** - Walk-in booking, same-day queue management, arrival marking, patient search, clinical data verification, conflict resolution workflow
- **Admin Portal** - User management, role assignment, audit log access, system configuration
- **AI Capabilities** - Conversational intake, clinical document extraction, ICD-10/CPT code mapping, data aggregation, conflict detection
- **Integrations** - Google Calendar, Microsoft Outlook, SMS notifications (Twilio), Email notifications (SendGrid)
- **Infrastructure** - Free-tier hosting (Vercel, Railway, Supabase), PostgreSQL with pgvector, Upstash Redis

### Out of Scope

- Provider logins and provider-facing workflows (Phase 2)
- Direct EHR integration (integration-ready architecture only)
- Mobile native applications (web-responsive only)
- Non-English clinical document processing
- Document formats other than PDF
- Paid cloud infrastructure (AWS, Azure hosting)

## Objectives and Goals

### Business Objectives

| Objective ID | Objective | Success Metric | Target |
|-------------|-----------|----------------|--------|
| OBJ-001 | Reduce patient no-show rates | No-show rate reduction | >25% improvement from baseline (15%) |
| OBJ-002 | Accelerate clinical preparation | Clinical prep time | <2 minutes per patient |
| OBJ-003 | Ensure AI-assisted data quality | AI-Human Agreement Rate | >98% for clinical data suggestions |
| OBJ-004 | Maintain platform reliability | System uptime | 99.9% monthly availability |
| OBJ-005 | Protect patient data | HIPAA compliance | 100% compliant data handling |

### Project Goals

- Deliver a fully functional Patient Portal with self-service booking and intake by Week 14
- Implement AI-powered clinical document processing with >95% extraction recall by Week 18
- Achieve staff verification workflow with conflict detection and resolution by Week 20
- Complete integration testing with all external services by Week 22
- Deploy production-ready platform on free-tier infrastructure by Week 24

### Alignment to Requirements

| Objective | Mapped Requirements |
|-----------|---------------------|
| OBJ-001 | FR-009, FR-010, FR-022, FR-023, FR-026, NFR-016, NFR-017 |
| OBJ-002 | FR-028 to FR-032, AIR-002, AIR-005 to AIR-007, NFR-002 |
| OBJ-003 | FR-034 to FR-039, AIR-003, AIR-004, AIR-008, AIR-Q01 |
| OBJ-004 | NFR-001, NFR-008, NFR-011, TR-018 |
| OBJ-005 | FR-040 to FR-043, NFR-003, NFR-004, NFR-007, NFR-014, AIR-S01 |

## Project Timeline / Milestones

### High-Level Timeline

| Milestone ID | Milestone Title | Target Date | Deliverables | Dependencies |
|-------------|----------------|-------------|--------------|--------------|
| MS-001 | Project Kickoff | 2026-03-20 | Project charter, environment setup, CI/CD pipeline | — |
| MS-002 | Authentication & Access Control | 2026-04-17 | User registration, login, RBAC, session management, audit logging | MS-001 |
| MS-003 | Appointment Booking Core | 2026-05-15 | Provider browsing, slot selection, booking, waitlist, dynamic swap, cancel/reschedule | MS-002 |
| MS-004 | Staff Operations | 2026-06-05 | Walk-in booking, queue management, arrival marking, patient search | MS-002 |
| MS-005 | Patient Intake | 2026-06-26 | AI conversational intake, manual form, mode switching, insurance validation | MS-003 |
| MS-006 | Clinical Document Processing | 2026-07-17 | Document upload, AI extraction, processing status, data aggregation | MS-003, MS-005 |
| MS-007 | Clinical Intelligence & 360 View | 2026-08-07 | 360-Degree Patient View, conflict detection, verification workflow, medical coding | MS-006 |
| MS-008 | Integration Testing & UAT | 2026-08-21 | E2E testing, calendar integrations, notification testing, performance validation | MS-007 |
| MS-009 | Final Delivery / Go-Live | 2026-09-04 | Production deployment, documentation, training, handoff | MS-008 |

### Key Dates

| Event | Date | Notes |
|-------|------|-------|
| Project Kickoff | 2026-03-20 | Sprint 1 begins |
| MVP Delivery (Patient Intake) | 2026-06-26 | Core booking and intake functional |
| AI Features Complete | 2026-08-07 | Clinical intelligence fully operational |
| UAT Complete | 2026-08-21 | Stakeholder acceptance |
| Final Delivery | 2026-09-04 | Production go-live |

## Team Composition

### Recommended Team Structure

| Role | Count | Allocation % | Justification |
|------|-------|-------------|---------------|
| Frontend Developer | 1 | 100% | React + Redux Toolkit + TypeScript + Tailwind (TR-001); Patient, Staff, Admin portals |
| Backend Developer | 2 | 100% | .NET 8 ASP.NET Core (TR-002); API services, business logic, database layer |
| AI/ML Engineer | 1 | 100% | 28 AIR requirements; conversational intake, document extraction, code mapping |
| DevOps Engineer | 1 | 75% | GitHub Actions, free-tier hosting deployment (TR-006, TR-007) |
| QA Engineer | 1 | 100% | Test strategy, manual/automated testing, NFR-012 (80% coverage) |
| **Total** | **6** | | Derived from scope analysis (610 person-days / 24 weeks) |

### RACI Matrix

| Activity | Project Sponsor | Product Owner | Tech Lead | Backend Dev | Frontend Dev | AI/ML Engineer | QA Engineer | DevOps |
|----------|----------------|---------------|-----------|-------------|--------------|----------------|-------------|--------|
| Requirements Clarification | C | R | A | C | C | C | I | I |
| Architecture Design | I | C | R/A | C | C | C | I | C |
| Sprint Planning | I | R | A | C | C | C | C | I |
| Backend Development | I | I | A | R | I | C | I | I |
| Frontend Development | I | I | A | C | R | I | I | I |
| AI Feature Development | I | I | A | C | I | R | I | I |
| Testing & QA | I | C | A | C | C | C | R | I |
| Deployment | I | I | A | C | C | I | C | R |
| Code Review | I | I | R | R | R | R | C | C |
| Release Sign-off | A | R | C | I | I | I | C | C |

## Cost Baseline

### Effort Estimate

| Category | Value | Unit |
|---------|-------|------|
| Total Functional Requirements (FR) | 43 | requirements |
| Total Non-Functional Requirements (NFR) | 18 | requirements |
| Total Technical Requirements (TR) | 23 | requirements |
| Total Data Requirements (DR) | 16 | requirements |
| Total AI Requirements (AIR) | 28 | requirements |
| Total Use Cases (UC) | 16 | use cases |
| Estimated Complexity | High | (AI, integrations, green-field) |
| AI Reduction Factor Applied | Yes | 0.75 simple/medium, 0.85 complex |
| Complexity Multiplier | 1.43 | Green-field (1.3) × Integration (1.1) |
| Estimated Duration (Optimistic) | 16 | weeks |
| Estimated Duration (Likely) | 20 | weeks |
| Estimated Duration (Pessimistic) | 29 | weeks |
| Projected Story Points | 610 | story points (1 SP = 1 person-day) |

### Cost Breakdown

| Cost Category | Estimate | Basis |
|---------------|----------|-------|
| **Development Effort** | | |
| FR Implementation (AI-adjusted) | 127 person-days | 43 FR × complexity × AI factor |
| NFR Implementation | 27 person-days | 18 NFR × 1.5 days |
| TR Implementation | 46 person-days | 23 TR × 2 days |
| DR Implementation | 16 person-days | 16 DR × 1 day |
| AIR Implementation | 140 person-days | 28 AIR × 5 days |
| Complexity Multiplier Applied | 509 person-days | 356 × 1.43 |
| Contingency Buffer (20%) | 101 person-days | Risk mitigation |
| **Total Development** | **610 person-days** | |
| **Infrastructure** | | |
| Frontend Hosting (Vercel) | $0 | Free tier - 100GB bandwidth/month |
| Backend Hosting (Railway/Render) | $0 | Free tier - 500 hours/month |
| Database (Supabase PostgreSQL) | $0 | Free tier - 500MB storage |
| Cache (Upstash Redis) | $0 | Free tier - 10K requests/day |
| **Total Infrastructure** | **$0** | Per TR-006 free-tier constraint |
| **Third-Party / Licensing** | | |
| Azure OpenAI Service | Variable | Pay-per-use (HIPAA BAA included) |
| Azure Document Intelligence | Variable | Pay-per-use |
| Twilio SMS | Variable | Free tier + pay-per-use |
| SendGrid Email | $0 | Free tier - 100 emails/day |
| QuestPDF | $0 | Free for open source |
| **Total Third-Party** | **Variable** | Usage-based AI costs |

## Cost Control Plan

### Budget Monitoring

| Control Mechanism | Frequency | Owner |
|-------------------|-----------|-------|
| Burn-down tracking | Per sprint | Scrum Master |
| Cost variance report | Bi-weekly | Project Manager |
| AI usage monitoring | Weekly | DevOps Engineer |
| Scope change review | Per request | Change Advisory Board |
| Platform usage monitoring | Weekly | DevOps Engineer |

### Change Control Process

- All scope changes require formal change request submission
- Impact assessment includes effort, timeline, and AI budget implications
- Changes >5% of baseline require sponsor approval
- Emergency changes require post-facto documentation within 24 hours

## Risk Management Plan

### Risk Register

#### RK-001: Technology Stack Maturity Risk

**Probability**: Medium | **Impact**: Medium | **Risk Score**: 4

**Category**: Technical

**Description**: Green-field project with no existing codebase patterns to leverage. New team collaboration on unfamiliar architecture may cause initial velocity reduction.

**Mitigation Strategy**: Conduct architecture spike in Sprint 1; establish coding standards early; implement pair programming for knowledge sharing.

**Contingency Plan**: Extend Sprint 1-2 for foundation work; consider external consultant if architecture decisions stall.

**Owner**: Tech Lead

**Related Requirements**: AD-001, TR-001 to TR-003

**Status**: Open

---

#### RK-002: Integration Complexity Risk

**Probability**: Medium | **Impact**: High | **Risk Score**: 6

**Category**: Technical

**Description**: Multiple technology integrations (React with Redux, .NET 8 with Three-Layer Architecture, PostgreSQL with pgvector, Azure OpenAI, Redis, Pusher, external calendar/notification APIs) increase failure points and debugging complexity.

**Mitigation Strategy**: API-first design with OpenAPI contracts (AD-002); contract testing; mock external services during development.

**Contingency Plan**: Prioritize core flows over integrations; defer calendar integration to post-MVP if needed.

**Owner**: Tech Lead

**Related Requirements**: TR-005, TR-008 to TR-011, TR-015, TR-016

**Status**: Open

---

#### RK-003: AI Model Uncertainty Risk

**Probability**: Medium | **Impact**: High | **Risk Score**: 6

**Category**: Technical

**Description**: 28 AIR requirements including 98% AI-Human Agreement Rate target. AI model behavior may vary with real clinical documents. Document quality and format variability affects extraction accuracy.

**Mitigation Strategy**: Trust-First verification workflow (AD-004); mandatory staff verification before clinical use; implement confidence scoring with thresholds; comprehensive fallback to manual entry (AD-006).

**Contingency Plan**: Reduce AI scope to core extraction features; increase manual verification requirement; extend AI development sprint.

**Owner**: AI/ML Engineer

**Related Requirements**: AIR-001 to AIR-009, AIR-Q01, AIR-S04

**Status**: Open

---

#### RK-004: External Service Dependency Risk

**Probability**: Low | **Impact**: Medium | **Risk Score**: 2

**Category**: External

**Description**: Platform depends on Google Calendar, Microsoft Graph, Twilio, and SendGrid APIs. Service outages or API changes could affect notifications and calendar sync.

**Mitigation Strategy**: Define API contracts early; implement mock services for testing; design for graceful degradation.

**Contingency Plan**: Defer calendar integration; implement email-only notifications; manual appointment confirmation.

**Owner**: Backend Developer

**Related Requirements**: FR-022 to FR-025, TR-008 to TR-011

**Status**: Open

---

#### RK-005: Azure AI Service Dependency Risk

**Probability**: Low | **Impact**: High | **Risk Score**: 3

**Category**: External

**Description**: Core AI features depend on Azure OpenAI and Document Intelligence services. Service unavailability would disable AI-assisted intake, extraction, and coding.

**Mitigation Strategy**: Circuit breaker pattern (AIR-O02) with exponential backoff; progressive degradation per AD-006; queue document processing (AIR-O04).

**Contingency Plan**: Full manual entry mode; cache previously verified code mappings; offline document queue with retry.

**Owner**: AI/ML Engineer

**Related Requirements**: TR-015, TR-016, AIR-O02, NFR-015

**Status**: Open

---

#### RK-006: Schedule Risk

**Probability**: Medium | **Impact**: High | **Risk Score**: 6

**Category**: Schedule

**Description**: 24-week project timeline with complex AI features, multiple integrations, and green-field development creates schedule pressure.

**Mitigation Strategy**: 20% buffer already included in estimates; 2-week sprint cadence for early issue detection; prioritized backlog with MVP definition.

**Contingency Plan**: Defer non-critical features (calendar sync, AI intake); extend timeline by 4 weeks; add contractor resources.

**Owner**: Project Manager

**Related Requirements**: All milestones

**Status**: Open

---

#### RK-007: Free-Tier Platform Constraint Risk

**Probability**: Medium | **Impact**: Medium | **Risk Score**: 4

**Category**: Technical

**Description**: Free-tier hosting platforms (Vercel, Railway, Supabase) have bandwidth, storage, and compute limits that may constrain scalability.

**Mitigation Strategy**: Monitor platform usage weekly; design for horizontal scaling; implement caching layer; optimize assets and queries.

**Contingency Plan**: Migrate to paid tier if volume exceeds limits; distribute load across multiple free instances; implement rate limiting.

**Owner**: DevOps Engineer

**Related Requirements**: TR-006, NFR-008, NFR-009

**Status**: Open

### Risk Summary Matrix

| Risk ID | Risk | Probability | Impact | Score | Mitigation | Owner |
|---------|------|-------------|--------|-------|------------|-------|
| RK-001 | Technology Stack Maturity | Medium | Medium | 4 | Architecture spike, coding standards | Tech Lead |
| RK-002 | Integration Complexity | Medium | High | 6 | API-first design, contract testing | Tech Lead |
| RK-003 | AI Model Uncertainty | Medium | High | 6 | Trust-First workflow, fallbacks | AI/ML Engineer |
| RK-004 | External Service Dependency | Low | Medium | 2 | Mock services, graceful degradation | Backend Dev |
| RK-005 | Azure AI Service Dependency | Low | High | 3 | Circuit breaker, manual fallback | AI/ML Engineer |
| RK-006 | Schedule Risk | Medium | High | 6 | 20% buffer, prioritized backlog | Project Manager |
| RK-007 | Free-Tier Constraints | Medium | Medium | 4 | Usage monitoring, caching | DevOps Engineer |

## Communication Plan

| Artifact / Ceremony | Frequency | Audience | Owner | Channel |
|--------------------|-----------|----------|-------|---------|
| Daily Standup | Daily (sync) | Dev Team | Scrum Master | Teams/Slack |
| Sprint Planning | Every 2 weeks | Full Team + PO | Product Owner | Teams Meeting |
| Sprint Review | Every 2 weeks | Team + Stakeholders | Product Owner | Teams Meeting |
| Sprint Retrospective | Every 2 weeks | Dev Team | Scrum Master | Teams Meeting |
| Backlog Refinement | Weekly | Dev Team + PO | Product Owner | Teams Meeting |
| Weekly Status Report | Weekly | Stakeholders | Project Manager | Email |
| Risk Review | Bi-weekly | Team + PM | Project Manager | Teams Meeting |

## Success Metrics

| Criterion | Metric | Target | Measurement Method |
|----------|--------|--------|-------------------|
| On-time Delivery | Milestone variance | ≤10% | Sprint tracking vs. baseline |
| Scope Delivery | Requirement coverage | 100% FR delivered | Traceability matrix audit |
| Quality - Defects | Defect density | <0.5 defects/SP | Test reports, bug tracking |
| Quality - Coverage | Code test coverage | ≥80% | Automated coverage reports |
| Budget | Cost variance | ≤20% (buffer) | AI usage + effort tracking |
| AI Accuracy | AI-Human Agreement Rate | >98% | Staff verification logs |
| Performance | API response time | <500ms (95th percentile) | Application monitoring |
| Performance | Clinical prep time | <2 minutes | User acceptance testing |
| Availability | Platform uptime | 99.9% monthly | Health check monitoring |

## Sprint Planning Bridge

| Parameter | Value | Basis |
|-----------|-------|-------|
| Projected Story Points | 610 | 1 SP = 1 person-day (agile-methodology-guidelines.md) |
| Recommended Sprint Duration | 2 weeks | Project 3-6 months duration |
| Recommended Team Size | 6 | Derived from scope complexity analysis |
| Estimated Sprint Count | 10 | 20 weeks ÷ 2-week sprints |
| Recommended Velocity | ~10 SP/sprint/person | 610 SP ÷ 10 sprints ÷ 6 team members |
| Total Team Velocity | ~61 SP/sprint | Required to meet timeline |

## Traceability

### Requirement Coverage

| Category | Total | In Scope | Coverage % |
|----------|-------|----------|------------|
| FR-XXX | 43 | 43 | 100% |
| NFR-XXX | 18 | 18 | 100% |
| TR-XXX | 23 | 23 | 100% |
| DR-XXX | 16 | 16 | 100% |
| AIR-XXX | 28 | 28 | 100% |
| UC-XXX | 16 | 16 | 100% |

### Deferred Items

| Item | Reason | Target Phase |
|------|--------|--------------|
| Provider logins | Out of Phase 1 scope | Phase 2 |
| Direct EHR integration | Standalone system first | Phase 2 |
| Mobile native apps | Web-responsive prioritized | Phase 2 |
| Non-English document processing | AI model limitation | Future |
| Non-PDF document formats | Scope constraint | Future |
