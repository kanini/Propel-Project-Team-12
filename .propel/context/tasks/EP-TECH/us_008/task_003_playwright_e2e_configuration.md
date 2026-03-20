# Task - task_003_playwright_e2e_configuration

## Requirement Reference
- User Story: us_008
- Story Location: .propel/context/tasks/EP-TECH/us_008/us_008.md
- Acceptance Criteria:
    - AC-3: Playwright configured as E2E framework, test runner launches browser instances with proper configuration
- Edge Case:
    - Playwright browsers not installed should be handled by setup script with `npx playwright install`
    - Flaky tests should have 1 retry with test isolation ensured

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
| Testing | Playwright | Latest |
| Frontend | React | 18.x |
| Backend | .NET ASP.NET Core | 8.0 |

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
Configure Playwright for end-to-end testing with Chromium browser support. Setup test project structure with Page Object Model pattern for maintainable E2E tests. Configure retry strategy for flaky tests, test isolation, and proper timeout settings. Create sample E2E test demonstrating full user flow from authentication to feature interaction.

## Dependent Tasks
- task_001_frontend_vite_react_scaffolding (US_001) - Requires frontend project
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend for E2E flows

## Impacted Components
- **CREATE** test-automation/playwright.config.ts - Playwright configuration
- **CREATE** test-automation/package.json - E2E test project dependencies
- **CREATE** test-automation/tests/example.spec.ts - Sample E2E test
- **CREATE** test-automation/pages/base.page.ts - Base page object class
- **CREATE** test-automation/.gitignore - Exclude test results and traces

## Implementation Plan
1. **Create Test Automation Project**: Initialize separate test-automation directory for Playwright tests
2. **Install Playwright**: Run `npm init playwright@latest` to setup Playwright with TypeScript
3. **Configure Playwright**: Update playwright.config.ts with base URL, timeouts, retries, and browser settings
4. **Install Browsers**: Run `npx playwright install chromium` to download browser binaries
5. **Setup Page Object Model**: Create base page class and example page objects
6. **Write Sample Test**: Create example.spec.ts demonstrating authentication and basic flow
7. **Configure Test Scripts**: Add scripts to package.json (test, test:headed, test:debug)
8. **Document E2E Testing Standards**: Create guide for writing Playwright tests following best practices

## Current Project State
```
Propel-Project-Team-12/
├── src/frontend/
│   └── (React project with Vitest)
├── src/backend/
│   └── (Backend with xUnit tests)
└── (No E2E test project exists yet)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | test-automation/package.json | E2E test project with Playwright dependency |
| CREATE | test-automation/playwright.config.ts | Playwright config with base URL, retries, timeouts |
| CREATE | test-automation/tests/example.spec.ts | Sample E2E test with authentication flow |
| CREATE | test-automation/pages/base.page.ts | Base page object with common methods |
| CREATE | test-automation/pages/login.page.ts | Login page object example |
| CREATE | test-automation/.gitignore | Exclude playwright-report, test-results |
| CREATE | docs/E2E_TESTING.md | Playwright testing standards and patterns guide |

## External References
- Playwright Documentation: https://playwright.dev/docs/intro
- Playwright Configuration: https://playwright.dev/docs/test-configuration
- Page Object Model: https://playwright.dev/docs/pom
- Playwright Best Practices: https://playwright.dev/docs/best-practices
- Flakiness Handling: https://playwright.dev/docs/test-retries

## Build Commands
```bash
# Create test-automation directory
mkdir test-automation
cd test-automation

# Initialize Playwright project
npm init playwright@latest

# Install browsers
npx playwright install chromium

# Run tests
npx playwright test

# Run tests in headed mode
npx playwright test --headed

# Run tests with UI
npx playwright test --ui

# Debug tests
npx playwright test --debug

# Generate test report
npx playwright show-report
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for E2E configuration)
- [ ] Integration tests pass (N/A for E2E configuration)
- [ ] Playwright test project initialized successfully
- [ ] Chromium browser installed and accessible
- [ ] Sample E2E test executes successfully in headless mode
- [ ] Test retries 1 time on failure (flakiness handling)
- [ ] Base URL configured correctly pointing to frontend dev server
- [ ] Test isolation ensured (each test independent)
- [ ] Playwright HTML report generated after test run

## Implementation Checklist
- [ ] Create `test-automation` directory at project root
- [ ] Initialize Playwright with `npm init playwright@latest` and TypeScript
- [ ] Update `playwright.config.ts` with base URL, 1 retry, 30s timeout
- [ ] Install Chromium browser with `npx playwright install chromium`
- [ ] Create base page class with common navigation and assertion methods
- [ ] Create sample login page object in `pages/login.page.ts`
- [ ] Write sample E2E test in `tests/example.spec.ts` covering authentication
- [ ] Document E2E testing standards in E2E_TESTING.md
