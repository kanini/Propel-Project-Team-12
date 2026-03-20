# Task - task_002_backend_xunit_configuration

## Requirement Reference
- User Story: us_008
- Story Location: .propel/context/tasks/EP-TECH/us_008/us_008.md
- Acceptance Criteria:
    - AC-1: xUnit configured as backend test framework, tests execute with code coverage collection and report generation
    - AC-4: CI pipeline fails if code coverage drops below 80% for business logic components
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
| Testing | xUnit | 2.x |
| Testing | Moq | 4.x |
| Testing | coverlet.collector | Latest |

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
Configure xUnit as the backend unit testing framework with Moq for mocking dependencies and coverlet for code coverage collection. Create test project following AAA (Arrange-Act-Assert) pattern with proper test organization. Setup coverage reporting targeting 80% threshold for business logic (Services, Repositories). Integrate with CI pipeline to enforce coverage requirements.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project

## Impacted Components
- **CREATE** src/backend/PatientAccess.Tests/PatientAccess.Tests.csproj - xUnit test project
- **CREATE** src/backend/PatientAccess.Tests/Services/ExampleServiceTests.cs - Sample service test
- **CREATE** src/backend/PatientAccess.Tests/coverlet.runsettings - Coverage configuration
- **MODIFY** src/backend/PatientAccess.sln - Add test project to solution
- **MODIFY** .github/workflows/ci.yml - Add backend coverage threshold check

## Implementation Plan
1. **Create xUnit Test Project**: Add xUnit test project to solution referencing Business and Data layers
2. **Install Testing Packages**: Add xUnit, xUnit.runner.visualstudio, Moq, FluentAssertions, coverlet.collector
3. **Configure Coverage Settings**: Create coverlet.runsettings with 80% thresholds and exclusions
4. **Create Test Structure**: Setup folder structure mirroring source projects (Services/, Repositories/, Controllers/)
5. **Write Sample Tests**: Create example service test demonstrating AAA pattern and mocking
6. **Configure Test Runner**: Update csproj with RunSettingsFilePath for consistent coverage settings
7. **Integrate with CI**: Update ci.yml to run tests with coverage collection and threshold enforcement
8. **Document Testing Standards**: Create guide for writing unit tests following xUnit best practices

## Current Project State
```
Propel-Project-Team-12/
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   ├── PatientAccess.Business/
│   └── PatientAccess.Data/
└── (No test project exists yet)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Tests/PatientAccess.Tests.csproj | xUnit test project with testing dependencies |
| CREATE | src/backend/PatientAccess.Tests/Services/ | Test folder for service layer tests |
| CREATE | src/backend/PatientAccess.Tests/Services/ExampleServiceTests.cs | Sample service test using AAA pattern |
| CREATE | src/backend/PatientAccess.Tests/coverlet.runsettings | Coverage settings with 80% thresholds |
| MODIFY | src/backend/PatientAccess.sln | Add PatientAccess.Tests project to solution |
| MODIFY | .github/workflows/ci.yml | Add backend test execution with coverage check |
| CREATE | docs/BACKEND_TESTING.md | Backend testing standards and xUnit patterns guide |

## External References
- xUnit Documentation: https://xunit.net/docs/getting-started/netcore/cmdline
- Moq Quickstart: https://github.com/moq/moq4
- coverlet Documentation: https://github.com/coverlet-coverage/coverlet
- FluentAssertions: https://fluentassertions.com/
- AAA Pattern: https://automationpanda.com/2020/07/07/arrange-act-assert-a-pattern-for-writing-good-tests/
- .NET Test Coverage: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage

## Build Commands
```bash
# Create test project
cd src/backend
dotnet new xunit -n PatientAccess.Tests
dotnet sln add PatientAccess.Tests/PatientAccess.Tests.csproj

# Install testing packages
cd PatientAccess.Tests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package coverlet.collector

# Add project references
dotnet add reference ../PatientAccess.Business/PatientAccess.Business.csproj
dotnet add reference ../PatientAccess.Data/PatientAccess.Data.csproj

# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80 /p:ThresholdType=line,branch,method

# Generate HTML coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Implementation Validation Strategy
- [ ] Unit tests pass (create and run sample service test)
- [ ] Integration tests pass (N/A for test configuration)
- [ ] Test project compiles and references Business/Data layers correctly
- [ ] `dotnet test` executes tests successfully
- [ ] Coverage report generated with line, branch, method metrics
- [ ] CI pipeline fails when coverage drops below 80%
- [ ] Moq able to mock interfaces for dependency injection
- [ ] FluentAssertions provides readable assertions in tests

## Implementation Checklist
- [ ] Create xUnit test project with `dotnet new xunit -n PatientAccess.Tests`
- [ ] Add test project to solution with `dotnet sln add`
- [ ] Install xUnit.runner.visualstudio, Moq, FluentAssertions, coverlet.collector
- [ ] Add project references to PatientAccess.Business and PatientAccess.Data
- [ ] Create `coverlet.runsettings` with 80% thresholds excluding test files and generated code
- [ ] Create folder structure mirroring source (Services/, Repositories/, Controllers/)
- [ ] Write sample service test in `Services/ExampleServiceTests.cs` using AAA pattern
- [ ] Update `.github/workflows/ci.yml` to run backend tests with coverage enforcement
