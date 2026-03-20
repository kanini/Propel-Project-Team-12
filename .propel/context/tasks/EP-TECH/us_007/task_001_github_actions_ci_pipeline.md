# Task - task_001_github_actions_ci_pipeline

## Requirement Reference
- User Story: us_007
- Story Location: .propel/context/tasks/EP-TECH/us_007/us_007.md
- Acceptance Criteria:
    - AC-1: CI workflow runs build, lint, and test steps for frontend and backend with pass/fail status on PR
    - AC-5: Separate workflows exist for CI (`ci.yml`), staging deployment (`cd-staging.yml`), production deployment (`cd-production.yml`)
- Edge Case:
    - Concurrent deployments prevented using concurrency groups

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
| CI/CD | GitHub Actions | Latest |
| Backend | .NET ASP.NET Core | 8.0 |
| Frontend | React + Vite | React 18.x, Vite 5.x |

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
Create GitHub Actions CI pipeline automating build, lint, and test execution for both frontend and backend on every pull request. Configure separate jobs for frontend (npm install, build, test) and backend (dotnet restore, build, test) with proper caching for dependencies. Implement concurrency groups to prevent simultaneous workflow runs and report status checks to pull requests.

## Dependent Tasks
- task_001_frontend_vite_react_scaffolding (US_001) - Requires frontend project
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project

## Impacted Components
- **NEW** .github/workflows/ci.yml - CI workflow for build, lint, test
- **NEW** .github/workflows/cd-staging.yml - Staging deployment workflow (placeholder)
- **NEW** .github/workflows/cd-production.yml - Production deployment workflow (placeholder)

## Implementation Plan
1. **Create CI Workflow File**: Define `.github/workflows/ci.yml` triggered on pull_request and push to main
2. **Configure Concurrency Groups**: Prevent concurrent workflow runs using concurrency: pull_request group
3. **Setup Frontend Job**: Configure Node.js environment, cache npm dependencies, run lint/build/test
4. **Setup Backend Job**: Configure .NET 8 environment, cache NuGet packages, run build/test
5. **Add Status Check Requirements**: Require CI workflow to pass before PR merge
6. **Configure Caching**: Use actions/cache for node_modules and NuGet packages to speed up builds
7. **Add Linting Steps**: Include ESLint for frontend and dotnet format for backend
8. **Test Workflow Execution**: Create test PR to verify all steps pass successfully

## Current Project State
```
Propel-Project-Team-12/
├── .github/
│   └── (no workflows yet)
├── src/frontend/
│   ├── package.json
│   └── (React project from US_001)
└── src/backend/
    ├── PatientAccess.sln
    └── (Backend projects from US_002)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | .github/workflows/ci.yml | CI pipeline with frontend and backend jobs |
| CREATE | .github/workflows/cd-staging.yml | Staging deployment workflow (placeholder) |
| CREATE | .github/workflows/cd-production.yml | Production deployment workflow (placeholder) |
| CREATE | docs/CI_CD.md | CI/CD pipeline documentation and troubleshooting guide |

## External References
- GitHub Actions Documentation: https://docs.github.com/en/actions
- GitHub Actions Workflow Syntax: https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions
- actions/cache: https://github.com/actions/cache
- actions/setup-node: https://github.com/actions/setup-node
- actions/setup-dotnet: https://github.com/actions/setup-dotnet
- Concurrency Groups: https://docs.github.com/en/actions/using-jobs/using-concurrency

## Build Commands
```yaml
# CI Workflow Commands (embedded in ci.yml)

# Frontend:
npm ci
npm run lint
npm run build
npm run test

# Backend:
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for workflow configuration)
- [ ] Integration tests pass (N/A for workflow configuration)
- [ ] CI workflow triggers on pull request creation
- [ ] Frontend job completes successfully (lint, build, test)
- [ ] Backend job completes successfully (build, test)
- [ ] Workflow status reported to pull request as check
- [ ] Concurrent workflow runs prevented by concurrency group
- [ ] Dependency caching reduces build time on subsequent runs
- [ ] Workflow fails if linting or tests fail

## Implementation Checklist
- [ ] Create `.github/workflows/ci.yml` with pull_request and push triggers
- [ ] Configure concurrency group to prevent simultaneous runs
- [ ] Add frontend job with Node.js 18 setup and dependency caching
- [ ] Add ESLint step to frontend job
- [ ] Add backend job with .NET 8 setup and NuGet caching
- [ ] Add dotnet format check to backend job
- [ ] Configure status checks to block PR merge on CI failure
- [ ] Create placeholder workflows for cd-staging.yml and cd-production.yml
