# Task - task_001_frontend_vitest_configuration

## Requirement Reference

- User Story: us_008
- Story Location: .propel/context/tasks/EP-TECH/us_008/us_008.md
- Acceptance Criteria:
  - AC-2: Vitest configured for frontend, unit tests execute for React components and Redux slices with coverage output
  - AC-4: CI pipeline fails if code coverage drops below 80% for business logic components
- Edge Case:
  - None specified

## Design References (Frontend Tasks Only)

| Reference Type         | Value |
| ---------------------- | ----- |
| **UI Impact**          | No    |
| **Figma URL**          | N/A   |
| **Wireframe Status**   | N/A   |
| **Wireframe Type**     | N/A   |
| **Wireframe Path/URL** | N/A   |
| **Screen Spec**        | N/A   |
| **UXR Requirements**   | N/A   |
| **Design Tokens**      | N/A   |

## Applicable Technology Stack

| Layer    | Technology            | Version |
| -------- | --------------------- | ------- |
| Frontend | React                 | 18.x    |
| Testing  | Vitest                | 1.x     |
| Testing  | React Testing Library | 14.x    |
| Testing  | jsdom                 | Latest  |

**Note**: All code and libraries MUST be compatible with versions above.

## AI References (AI Tasks Only)

| Reference Type           | Value |
| ------------------------ | ----- |
| **AI Impact**            | No    |
| **AIR Requirements**     | N/A   |
| **AI Pattern**           | N/A   |
| **Prompt Template Path** | N/A   |
| **Guardrails Config**    | N/A   |
| **Model Provider**       | N/A   |

## Mobile References (Mobile Tasks Only)

| Reference Type       | Value |
| -------------------- | ----- |
| **Mobile Impact**    | No    |
| **Platform Target**  | N/A   |
| **Min OS Version**   | N/A   |
| **Mobile Framework** | N/A   |

## Task Overview

Configure Vitest as the frontend unit testing framework with React Testing Library for component testing. Setup coverage reporting targeting 80% threshold for business logic components (services, hooks, utilities). Enable jsdom environment for DOM API access in tests. Integrate with CI pipeline to enforce coverage requirements and prevent merges that reduce coverage.

## Dependent Tasks

- task_001_frontend_vite_react_scaffolding (US_001) - Requires frontend project

## Impacted Components

- **MODIFY** src/frontend/package.json - Add Vitest and testing library dependencies
- **CREATE** src/frontend/vitest.config.ts - Vitest configuration with coverage thresholds
- **CREATE** src/frontend/src/setupTests.ts - Test environment setup file
- **CREATE** src/frontend/src/**tests**/example.test.tsx - Sample component test
- **MODIFY** .github/workflows/ci.yml - Add coverage threshold check

## Implementation Plan

1. **Install Testing Dependencies**: Add Vitest, @testing-library/react, @testing-library/jest-dom, jsdom
2. **Create Vitest Configuration**: Configure test environment (jsdom), coverage provider (v8), and 80% thresholds
3. **Setup Test Environment**: Create setupTests.ts importing @testing-library/jest-dom for extended matchers
4. **Configure Coverage Thresholds**: Set 80% coverage for lines, functions, statements, branches
5. **Add Test Scripts**: Update package.json with test, test:ui, test:coverage commands
6. **Create Sample Test**: Write example component test demonstrating React Testing Library patterns
7. **Integrate with CI**: Update ci.yml to run tests and fail on coverage below threshold
8. **Document Testing Standards**: Create guide for writing unit tests following testing-library principles

## Current Project State

```
Propel-Project-Team-12/
├── src/frontend/
│   ├── package.json
│   ├── vite.config.ts
│   ├── src/
│   │   ├── App.tsx
│   │   └── store/
│   └── (React project from US_001)
```

## Expected Changes

| Action | File Path                                   | Description                                                         |
| ------ | ------------------------------------------- | ------------------------------------------------------------------- |
| MODIFY | src/frontend/package.json                   | Add Vitest, React Testing Library, jsdom dependencies               |
| CREATE | src/frontend/vitest.config.ts               | Vitest configuration with coverage thresholds and jsdom environment |
| CREATE | src/frontend/src/setupTests.ts              | Test setup importing @testing-library/jest-dom                      |
| CREATE | src/frontend/src/**tests**/example.test.tsx | Sample React component test                                         |
| MODIFY | .github/workflows/ci.yml                    | Add coverage threshold enforcement step                             |
| CREATE | docs/FRONTEND_TESTING.md                    | Frontend testing standards and patterns guide                       |

## External References

- Vitest Documentation: https://vitest.dev/
- React Testing Library: https://testing-library.com/docs/react-testing-library/intro/
- Vitest Coverage: https://vitest.dev/guide/coverage.html
- Testing Library Best Practices: https://kentcdodds.com/blog/common-mistakes-with-react-testing-library
- Vitest with Vite: https://vitest.dev/guide/#configuring-vitest

## Build Commands

```bash
# Install testing dependencies
cd src/frontend
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event jsdom @vitest/coverage-v8

# Run tests
npm run test

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage

# Watch mode
npm run test:watch
```

## Implementation Validation Strategy

- [x] Unit tests pass (create and run sample component test)
- [x] Integration tests pass (N/A for test configuration)
- [x] Vitest runs tests successfully with jsdom environment
- [x] Coverage report generated showing line, function, statement, branch metrics
- [x] CI pipeline fails when coverage drops below 80%
- [x] React Testing Library matchers available (toBeInTheDocument, etc.)
- [x] Test UI accessible at http://localhost:51204 when running test:ui
- [x] Coverage threshold configured for business logic (exclude test files, config files)

## Implementation Checklist

- [x] Install Vitest, React Testing Library, jsdom, @vitest/coverage-v8
- [x] Create `vitest.config.ts` with jsdom environment and 80% coverage thresholds
- [x] Create `setupTests.ts` importing @testing-library/jest-dom
- [x] Add test scripts to package.json (test, test:ui, test:coverage, test:watch)
- [x] Configure coverage to exclude test files, config files, types
- [x] Write sample component test in `__tests__/example.test.tsx`
- [x] Update `.github/workflows/ci.yml` to run tests with coverage check
- [x] Document frontend testing standards in FRONTEND_TESTING.md
