# CI/CD Pipeline Scripts

## Overview

Generated GitHub Actions workflow files for the Unified Patient Access & Clinical Intelligence Platform. These workflows implement the requirements defined in [cicd-spec.md](../../devops/cicd-spec.md).

## Workflow Files

| Workflow | File | Purpose | Trigger |
|----------|------|---------|---------|
| CI Pipeline | `ci.yml` | Build, lint, unit tests, dependency audit, secrets scan | Push/PR to develop, main, feature branches |
| Security Scan | `security-scan.yml` | CodeQL SAST, SCA, SBOM generation, GitLeaks | Push to main/develop, weekly schedule, manual |
| CD Frontend | `cd-frontend.yml` | Deploy frontend to InfinityFree via FTP | Manual dispatch, or after CI passes on main |
| CD Backend | `cd-backend.yml` | Deploy backend to MonsterASP.NET via FTP | Manual dispatch, or after CI passes on main |

## CICD Requirement Coverage

| CICD ID | Requirement | Workflow | Status |
|---------|-------------|----------|--------|
| CICD-001 | Frontend build (TypeScript strict) | ci.yml, cd-frontend.yml | Implemented |
| CICD-002 | npm dependency resolution | ci.yml, cd-frontend.yml | Implemented |
| CICD-004 | Fail on TypeScript errors | ci.yml | Implemented |
| CICD-005 | Optimized production bundle | ci.yml, cd-frontend.yml | Implemented |
| CICD-006 | .NET build/publish | ci.yml, cd-backend.yml | Implemented |
| CICD-007 | NuGet package restore | ci.yml, cd-backend.yml | Implemented |
| CICD-009 | Fail on C# compilation errors | ci.yml | Implemented |
| CICD-010 | ESLint zero tolerance | ci.yml | Implemented |
| CICD-011 | Frontend coverage >= 80% | ci.yml | Implemented |
| CICD-013 | Frontend coverage reports | ci.yml | Implemented |
| CICD-015 | Backend coverage >= 80% | ci.yml | Implemented |
| CICD-018 | Backend coverage reports | ci.yml | Implemented |
| CICD-020 | CodeQL (JS/TS + C#) | security-scan.yml | Implemented |
| CICD-024 | npm audit | ci.yml, security-scan.yml | Implemented |
| CICD-025 | .NET vulnerable packages | ci.yml, security-scan.yml | Implemented |
| CICD-027 | SBOM (CycloneDX) | security-scan.yml | Implemented |
| CICD-028 | GitLeaks secrets detection | ci.yml, security-scan.yml | Implemented |
| CICD-029 | Fail on detected secrets | ci.yml, security-scan.yml | Implemented |
| CICD-030 | Frontend unit tests | ci.yml | Implemented |
| CICD-031 | Backend unit tests | ci.yml | Implemented |
| CICD-033 | Test reports (JUnit/TRX) | ci.yml | Implemented |
| CICD-050 | Frontend FTP deployment | cd-frontend.yml | Implemented |
| CICD-051 | Backend FTP deployment | cd-backend.yml | Implemented |
| CICD-055 | Deployment tagging (SHA) | cd-frontend.yml, cd-backend.yml | Implemented |

## Required GitHub Secrets

Before deploying, configure the following secrets in your GitHub repository settings:

### Frontend Deployment

| Secret | Description | Example |
|--------|-------------|---------|
| `FTP_FRONTEND_SERVER` | FTP server hostname | `ftpupload.net` |
| `FTP_FRONTEND_USERNAME` | FTP username | *(from InfinityFree control panel)* |
| `FTP_FRONTEND_PASSWORD` | FTP password | *(from InfinityFree control panel)* |

### Backend Deployment

| Secret | Description | Example |
|--------|-------------|---------|
| `FTP_BACKEND_HOST` | FTP server URL | `ftp://site59724.siteasp.net` |
| `FTP_BACKEND_USERNAME` | FTP username | *(from MonsterASP.NET control panel)* |
| `FTP_BACKEND_PASSWORD` | FTP password | *(from MonsterASP.NET control panel)* |

### Security Scanning (Optional)

| Secret | Description | Required By |
|--------|-------------|-------------|
| `SNYK_TOKEN` | Snyk API token (if using Snyk) | security-scan.yml |

## Setup Instructions

### 1. Configure GitHub Secrets

1. Navigate to repository **Settings** > **Secrets and variables** > **Actions**
2. Add each secret from the tables above
3. Verify secrets are accessible to the workflows

### 2. Copy Workflows to Repository

Copy the workflow files from this directory to `.github/workflows/`:

```powershell
# From the repository root
Copy-Item -Recurse ".propel/context/pipelines/github-actions/.github/workflows/*.yml" ".github/workflows/"
```

### 3. Remove Hardcoded Credentials

After copying, **delete** the existing `frontend.yml` and `backend.yml` that contain hardcoded FTP credentials. The new `cd-frontend.yml` and `cd-backend.yml` replace them.

### 4. Scrub Git History

The current workflow files have hardcoded credentials committed to Git history. Remove them:

```bash
# Using BFG Repo-Cleaner (recommended)
bfg --replace-text passwords.txt

# Or using git filter-branch
git filter-branch --force --index-filter \
  'git rm --cached --ignore-unmatch .github/workflows/frontend.yml .github/workflows/backend.yml' \
  --prune-empty -- --all
```

### 5. Rotate Credentials

After scrubbing history, rotate all FTP passwords through the respective hosting control panels.

## Manual Trigger

All CD workflows support manual dispatch:

1. Go to **Actions** tab in GitHub
2. Select the workflow (e.g., "CD - Deploy Frontend")
3. Click **Run workflow**
4. Select branch and click **Run**

## Pipeline Architecture

```
Push/PR to develop/main/feature
        │
        ▼
┌──────────────────┐
│   CI Pipeline    │  ci.yml
│  ┌─────────────┐ │
│  │ Build (FE)  │ │
│  │ Build (BE)  │ │
│  │ Lint (FE)   │ │
│  │ Test (FE)   │ │
│  │ Test (BE)   │ │
│  │ Dep Audit   │ │
│  │ Secrets Scan│ │
│  └──────┬──────┘ │
│         │ CI Gate│
└─────────┼────────┘
          │
          ▼ (on main branch, CI success)
┌──────────────────┐    ┌──────────────────┐
│  CD Frontend     │    │   CD Backend     │
│  cd-frontend.yml │    │  cd-backend.yml  │
│  FTP → htdocs/   │    │  FTP → wwwroot/  │
└──────────────────┘    └──────────────────┘

Weekly/Push to main/develop
        │
        ▼
┌──────────────────┐
│  Security Scan   │  security-scan.yml
│  CodeQL (JS+C#)  │
│  SCA (npm+NuGet) │
│  SBOM Generation │
│  GitLeaks        │
└──────────────────┘
```
