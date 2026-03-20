# Task - task_002_vercel_frontend_deployment

## Requirement Reference
- User Story: us_007
- Story Location: .propel/context/tasks/EP-TECH/us_007/us_007.md
- Acceptance Criteria:
    - AC-2: React frontend automatically deployed to Vercel with environment-specific configuration when CI succeeds on main branch
- Edge Case:
    - Deployment to Vercel fails due to free-tier limits should fail with clear error

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe  Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | React + Vite | React 18.x, Vite 5.x |
| Hosting | Vercel | Latest |
| CI/CD | GitHub Actions | Latest |

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
Configure automated frontend deployment to Vercel free-tier hosting triggered by successful CI pipeline runs on main branch. Setup Vercel project linked to GitHub repository with Vite build configuration. Configure environment variables for API base URL and deployment previews for pull requests. This enables zero-downtime deployments and automatic HTTPS provisioning.

## Dependent Tasks
- task_001_frontend_vite_react_scaffolding (US_001) - Requires frontend project
- task_001_github_actions_ci_pipeline (US_007) - Requires CI pipeline

## Impacted Components
- **NEW** vercel.json - Vercel configuration for build and routing
- **MODIFY** src/frontend/vite.config.ts - Add environment variable handling
- **MODIFY** .github/workflows/cd-production.yml - Add Vercel deployment step
- **NEW** .env.production - Production environment variables (gitignored)

## Implementation Plan
1. **Create Vercel Account**: Sign up for Vercel free tier and link GitHub account
2. **Connect Repository**: Import GitHub repository to Vercel and configure build settings
3. **Configure Build Settings**: Set build command to `npm run build` and output directory to `dist`
4. **Setup Environment Variables**: Configure API_BASE_URL in Vercel dashboard for production
5. **Create vercel.json**: Configure routing rules, headers, and redirects
6. **Update Vite Config**: Handle environment variables with VITE_ prefix
7. **Enable Preview Deployments**: Configure automatic preview deployments for pull requests
8. **Update CD Workflow**: Modify cd-production.yml to trigger on main branch push after CI success

## Current Project State
```
Propel-Project-Team-12/
├── .github/workflows/
│   ├── ci.yml
│   └── cd-production.yml (placeholder)
├── src/frontend/
│   ├── package.json
│   ├── vite.config.ts
│   └── (React project)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | vercel.json | Vercel configuration with build settings and routing rules |
| MODIFY | src/frontend/vite.config.ts | Environment variable configuration with VITE_ prefix |
| MODIFY | .github/workflows/cd-production.yml | Vercel deployment step triggered on main branch |
| CREATE | .env.production | Production environment variables (API base URL) - gitignored |
| CREATE | docs/DEPLOYMENT.md | Deployment guide for Vercel and environment configuration |

## External References
- Vercel Documentation: https://vercel.com/docs
- Vercel with Vite: https://vercel.com/docs/frameworks/vite
- Vercel Environment Variables: https://vercel.com/docs/concepts/projects/environment-variables
- Vercel GitHub Integration: https://vercel.com/docs/concepts/git/vercel-for-github
- Vite Environment Variables: https://vitejs.dev/guide/env-and-mode.html

## Build Commands
```bash
# Install Vercel CLI (optional for local testing)
npm install -g vercel

# Deploy to Vercel (manual)
vercel --prod

# Deploy preview (manual)
vercel

# Link local project to Vercel
vercel link
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for deployment configuration)
- [ ] Integration tests pass (N/A for deployment configuration)
- [ ] Vercel project successfully linked to GitHub repository
- [ ] Production deployment triggered on push to main branch
- [ ] Deployed frontend accessible at Vercel URL with HTTPS
- [ ] Environment variables correctly injected during build
- [ ] Preview deployments created for pull requests
- [ ] Deployment fails gracefully with clear error message on free-tier limits

## Implementation Checklist
- [ ] Create Vercel account and link GitHub repository
- [ ] Configure Vercel project with build command and output directory
- [ ] Create `vercel.json` with routing configuration and headers
- [ ] Update `vite.config.ts` to handle VITE_ prefixed environment variables
- [ ] Configure VITE_API_BASE_URL in Vercel dashboard environment variables
- [ ] Enable preview deployments for pull requests in Vercel settings
- [ ] Update `.github/workflows/cd-production.yml` to trigger on main branch success
- [ ] Test production deployment and verify frontend accessibility
