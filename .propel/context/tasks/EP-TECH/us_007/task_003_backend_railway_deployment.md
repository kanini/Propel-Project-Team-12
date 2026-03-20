# Task - task_003_backend_railway_deployment

## Requirement Reference
- User Story: us_007
- Story Location: .propel/context/tasks/EP-TECH/us_007/us_007.md
- Acceptance Criteria:
    - AC-3: .NET 8 backend built, containerized, and deployed to Railway/Render when CI succeeds on main branch
- Edge Case:
    - Deployment failure should notify via GitHub status check

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
| Hosting | Railway | Latest |
| Containerization | Docker | Latest |

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
Configure automated backend deployment to Railway free-tier platform using Docker containerization. Create Dockerfile for .NET 8 Web API with multi-stage build for optimized image size. Setup Railway project linked to GitHub with automatic deployments on main branch. Configure environment variables for database connection string, JWT keys, and Redis connection.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project
- task_001_github_actions_ci_pipeline (US_007) - Requires CI pipeline

## Impacted Components
- **NEW** src/backend/Dockerfile - Multi-stage Docker build for .NET 8 API
- **NEW** src/backend/.dockerignore - Exclude unnecessary files from Docker context
- **MODIFY** .github/workflows/cd-production.yml - Add Railway deployment step

## Implementation Plan
1. **Create Railway Account**: Sign up for Railway free tier and create new project
2. **Create Dockerfile**: Write multi-stage Dockerfile (build -> publish -> runtime) for .NET 8
3. **Create .dockerignore**: Exclude bin/, obj/, .git/ from Docker build context
4. **Connect Repository**: Link GitHub repository to Railway and configure deployment trigger
5. **Configure Environment Variables**: Set DATABASE_URL, JWT_PRIVATE_KEY, REDIS_URL in Railway dashboard
6. **Setup Health Check**: Configure Railway health check endpoint pointing to `/health`
7. **Update CD Workflow**: Modify cd-production.yml to notify Railway deployment
8. **Test Deployment**: Verify backend accessibility at Railway-provided URL

## Current Project State
```
Propel-Project-Team-12/
├── .github/workflows/
│   ├── ci.yml
│   └── cd-production.yml
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   ├── PatientAccess.Business/
│   └── PatientAccess.Data/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/Dockerfile | Multi-stage Dockerfile for .NET 8 API (build, publish, runtime) |
| CREATE | src/backend/.dockerignore | Exclude bin/, obj/, .git/ from Docker context |
| MODIFY | .github/workflows/cd-production.yml | Add Railway deployment notification |
| CREATE | railway.json | Railway configuration for build and deployment |

## External References
- Railway Documentation: https://docs.railway.app/
- Railway Docker Deployment: https://docs.railway.app/deploy/dockerfiles
- .NET Docker Images: https://hub.docker.com/_/microsoft-dotnet-aspnet
- Docker Multi-Stage Builds: https://docs.docker.com/build/building/multi-stage/
- Railway Environment Variables: https://docs.railway.app/develop/variables

## Build Commands
```dockerfile
# Multi-stage Dockerfile

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PatientAccess.Web.dll"]
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for deployment configuration)
- [ ] Integration tests pass (N/A for deployment configuration)
- [ ] Docker image builds successfully locally
- [ ] Railway project linked to GitHub repository
- [ ] Backend deploys automatically on push to main branch
- [ ] Deployed backend accessible at Railway URL with HTTPS
- [ ] Environment variables correctly injected at runtime
- [ ] Health check endpoint returns 200 OK
- [ ] Deployment failure triggers GitHub status check notification

## Implementation Checklist
- [ ] Create Railway account and new project
- [ ] Write multi-stage Dockerfile for .NET 8 API
- [ ] Create `.dockerignore` file excluding bin/, obj/, .git/
- [ ] Link GitHub repository to Railway project
- [ ] Configure environment variables in Railway dashboard (DATABASE_URL, JWT keys, REDIS_URL)
- [ ] Set Railway health check endpoint to `/health`
- [ ] Test Docker build locally with `docker build -t patientaccess-api .`
- [ ] Verify backend deployment and accessibility at Railway URL
