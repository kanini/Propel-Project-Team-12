# Task - task_004_windows_deployment_support

## Requirement Reference
- User Story: us_007
- Story Location: .propel/context/tasks/EP-TECH/us_007/us_007.md
- Acceptance Criteria:
    - AC-4: Backend published using `dotnet publish` produces self-contained Windows executable for Windows Service or IIS deployment
- Edge Case:
    - None specified

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
| Deployment | Windows Services / IIS | Latest |

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
Configure .NET backend project for on-premise Windows deployment supporting both Windows Services and IIS hosting. Create self-contained publish profiles for Windows x64 runtime with all dependencies bundled. Document installation instructions for Windows Service registration and IIS application pool configuration. This enables enterprise customers to deploy on internal infrastructure without cloud dependencies.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project

## Impacted Components
- **NEW** src/backend/PatientAccess.Web/Properties/PublishProfiles/WindowsSelfContained.pubxml - Publish profile for Windows deployment
- **MODIFY** src/backend/PatientAccess.Web/Program.cs - Add Windows Service lifetime support
- **MODIFY** src/backend/PatientAccess.Web/PatientAccess.Web.csproj - Add Windows Service host package
- **NEW** deployment/windows/install-service.ps1 - PowerShellscript for Windows Service installation
- **NEW** deployment/windows/web.config - IIS configuration file

## Implementation Plan
1. **Install Windows Service Package**: Add `Microsoft.Extensions.Hosting.WindowsServices` NuGet package
2. **Configure Windows Service Support**: Call `UseWindowsService()` in Program.cs host builder
3. **Create Publish Profile**: Define WindowsSelfContained.pubxml with self-contained runtime (win-x64)
4. **Create Service Installation Script**: Write PowerShell script using `New-Service` cmdlet
5. **Create IIS Configuration**: Generate web.config with aspNetCore handler and environment variables
6. **Document Windows Service Deployment**: Create guide for service registration and management
7. **Document IIS Deployment**: Create guide for application pool setup and module configuration
8. **Test Windows Deployment**: Verify executable runs as standalone console app

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   │   ├── Program.cs
│   │   └── PatientAccess.Web.csproj
│   ├── PatientAccess.Business/
│   └── PatientAccess.Data/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/PatientAccess.Web.csproj | Add Microsoft.Extensions.Hosting.WindowsServices package |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Add UseWindowsService() to host builder |
| CREATE | src/backend/PatientAccess.Web/Properties/PublishProfiles/WindowsSelfContained.pubxml | Self-contained Windows publish profile |
| CREATE | deployment/windows/install-service.ps1 | PowerShell script for Windows Service registration |
| CREATE | deployment/windows/web.config | IIS configuration file with aspNetCore handler |
| CREATE | docs/WINDOWS_DEPLOYMENT.md | Windows deployment guide (Service and IIS) |

## External References
- Windows Service Hosting: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-8.0
- IIS Hosting: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-8.0
- Self-Contained Deployment: https://learn.microsoft.com/en-us/dotnet/core/deploying/
- PowerShell New-Service: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.management/new-service

## Build Commands
```bash
# Publish self-contained Windows executable
cd src/backend
dotnet publish PatientAccess.Web -c Release -r win-x64 --self-contained true -o ./publish/windows

# PowerShell: Install as Windows Service
$serviceName = "PatientAccessAPI"
$exePath = "C:\\PatientAccess\\PatientAccess.Web.exe"
New-Service -Name $serviceName -BinaryPathName $exePath -DisplayName "Patient Access API" -StartupType Automatic

# PowerShell: Start service
Start-Service -Name $serviceName

# PowerShell: Remove service
Stop-Service -Name $serviceName
Remove-Service -Name $serviceName
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for deployment configuration)
- [ ] Integration tests pass (N/A for deployment configuration)
- [ ] `dotnet publish` produces self-contained executable with all dependencies
- [ ] Executable runs standalone as console application on Windows
- [ ] Windows Service installation script executes without errors
- [ ] Service starts successfully and listens on configured port
- [ ] IIS web.config configures aspNetCore module correctly
- [ ] Health endpoint accessible via both Service and IIS deployments

## Implementation Checklist
- [ ] Add Microsoft.Extensions.Hosting.WindowsServices NuGet package to Web project
- [ ] Call `UseWindowsService()` in Program.cs host builder configuration
- [ ] Create `WindowsSelfContained.pubxml` publish profile for win-x64 runtime
- [ ] Write `install-service.ps1` PowerShell script for service registration
- [ ] Create `web.config` for IIS deployment with aspNetCore handler
- [ ] Document Windows Service deployment steps in WINDOWS_DEPLOYMENT.md
- [ ] Document IIS deployment steps including application pool configuration
- [ ] Test self-contained publish and verify executable runs on Windows
