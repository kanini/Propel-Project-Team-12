# Frontend Testing Guide

## Overview

This document outlines the frontend testing strategy, patterns, and best practices for the Patient Access Platform. We use **Vitest** as our test runner with **React Testing Library** for component testing.

## Technology Stack

| Tool                        | Version | Purpose                     |
| --------------------------- | ------- | --------------------------- |
| Vitest                      | 4.x     | Test runner and framework   |
| React Testing Library       | 14.x    | Component testing utilities |
| @testing-library/jest-dom   | Latest  | Extended DOM matchers       |
| @testing-library/user-event | Latest  | User interaction simulation |
| jsdom                       | Latest  | DOM environment simulation  |
| @vitest/coverage-v8         | Latest  | Coverage reporting          |

## Testing Philosophy

We follow the **Testing Library** guiding principles:

> **The more your tests resemble the way your software is used, the more confidence they can give you.**

### Key Principles

1. **Test behavior, not implementation**: Focus on what the user sees and does
2. **Query by accessibility**: Use semantic queries (getByRole, getByLabelText)
3. **Avoid testing implementation details**: Don't test state, props, or internal methods directly
4. **Write maintainable tests**: Tests should be easy to understand and modify

## Project Structure

```
src/
├── __tests__/              # Test files organized by feature
│   ├── App.test.tsx
│   ├── components/
│   │   └── Button.test.tsx
│   ├── pages/
│   │   └── Login.test.tsx
│   └── utils/
│       └── formatDate.test.spec.ts
├── setupTests.ts           # Global test setup
└── components/
    └── Button.tsx
```

## Configuration

### vitest.config.ts

```typescript
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/setupTests.ts"],
    coverage: {
      provider: "v8",
      reporter: ["text", "json", "html", "lcov"],
      exclude: [
        "node_modules/**",
        "src/setupTests.ts",
        "src/**/*.test.{ts,tsx}",
        "src/**/*.spec.{ts,tsx}",
        "src/**/__tests__/**",
        "src/**/__mocks__/**",
        "src/main.tsx",
        "src/vite-env.d.ts",
        "**/*.d.ts",
        "**/*.config.{ts,js}",
        "**/types/**",
        "dist/**",
      ],
      thresholds: {
        lines: 80,
        functions: 80,
        branches: 80,
        statements: 80,
      },
    },
  },
});
```

### Coverage Thresholds

- **80% minimum** for business logic components:
  - Lines: 80%
  - Functions: 80%
  - Branches: 80%
  - Statements: 80%

**Excluded from coverage:**

- Test files (_.test.tsx, _.spec.ts)
- Configuration files (\*.config.ts)
- Type definitions (\*.d.ts)
- Entry points (main.tsx)
- Mock files (**mocks**/)

## Writing Tests

### Component Testing Pattern

```typescript
import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Button } from './Button'

describe('Button Component', () => {
  it('renders with correct text', () => {
    render(<Button>Click Me</Button>)
    expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument()
  })

  it('calls onClick handler when clicked', async () => {
    const handleClick = vi.fn()
    const user = userEvent.setup()

    render(<Button onClick={handleClick}>Click Me</Button>)

    await user.click(screen.getByRole('button', { name: /click me/i }))

    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Click Me</Button>)
    expect(screen.getByRole('button')).toBeDisabled()
  })
})
```

### Query Priority

Use queries in this order (most preferred first):

1. **Accessible to everyone**:
   - `getByRole`
   - `getByLabelText`
   - `getByPlaceholderText`
   - `getByText`

2. **Semantic queries**:
   - `getByAltText`
   - `getByTitle`

3. **Test IDs** (last resort):
   - `getByTestId`

### User Interactions

Always use `@testing-library/user-event` for simulating user actions:

```typescript
import userEvent from '@testing-library/user-event'

it('handles form submission', async () => {
  const user = userEvent.setup()
  render(<LoginForm />)

  // Type into inputs
  await user.type(screen.getByLabelText(/email/i), 'test@example.com')
  await user.type(screen.getByLabelText(/password/i), 'password123')

  // Click button
  await user.click(screen.getByRole('button', { name: /sign in/i }))

  // Assert outcome
  expect(screen.getByText(/welcome/i)).toBeInTheDocument()
})
```

### Async Testing

Use `findBy*` queries for elements that appear asynchronously:

```typescript
it('displays error message on failed login', async () => {
  render(<LoginForm />)

  // Trigger async action
  await user.click(screen.getByRole('button', { name: /sign in/i }))

  // Wait for error to appear
  expect(await screen.findByText(/invalid credentials/i)).toBeInTheDocument()
})
```

### Testing Redux Connected Components

```typescript
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import userReducer from '@/store/userSlice'

const renderWithRedux = (
  component: React.ReactElement,
  initialState = {}
) => {
  const store = configureStore({
    reducer: { user: userReducer },
    preloadedState: initialState,
  })

  return render(
    <Provider store={store}>
      {component}
    </Provider>
  )
}

it('displays user name from Redux store', () => {
  renderWithRedux(<UserProfile />, {
    user: { name: 'John Doe', email: 'john@example.com' },
  })

  expect(screen.getByText(/john doe/i)).toBeInTheDocument()
})
```

### Testing React Router Components

```typescript
import { MemoryRouter } from 'react-router-dom'

const renderWithRouter = (component: React.ReactElement, initialRoute = '/') => {
  return render(
    <MemoryRouter initialEntries={[initialRoute]}>
      {component}
    </MemoryRouter>
  )
}

it('navigates to dashboard after login', async () => {
  const user = userEvent.setup()
  renderWithRouter(<App />, '/login')

  // Perform login
  await user.type(screen.getByLabelText(/email/i), 'test@example.com')
  await user.type(screen.getByLabelText(/password/i), 'password123')
  await user.click(screen.getByRole('button', { name: /sign in/i }))

  // Verify navigation
  expect(await screen.findByText(/dashboard/i)).toBeInTheDocument()
})
```

## Mocking

### Mock Functions

```typescript
import { vi } from "vitest";

const mockFn = vi.fn();
const mockFn = vi.fn(() => "return value");
const mockFn = vi.fn().mockResolvedValue("async value");
```

### Mock Modules

```typescript
// Mock external API
vi.mock("@/services/api", () => ({
  fetchUser: vi.fn().mockResolvedValue({ id: 1, name: "John" }),
}));
```

### Mock Date/Time

```typescript
import { beforeEach, afterEach, vi } from "vitest";

beforeEach(() => {
  vi.useFakeTimers();
  vi.setSystemTime(new Date("2024-01-01"));
});

afterEach(() => {
  vi.useRealTimers();
});
```

## Running Tests

### CLI Commands

```bash
# Run all tests
npm run test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage

# Run tests with UI
npm run test:ui

# Run specific test file
npm run test -- App.test.tsx

# Run tests matching pattern
npm run test -- --grep="Button"
```

### Coverage Report

After running `npm run test:coverage`, reports are generated in:

- `coverage/index.html` - Interactive HTML report
- `coverage/lcov.info` - LCOV format for CI tools
- `coverage/coverage-summary.json` - JSON summary

## CI/CD Integration

Tests run automatically in GitHub Actions on:

- Every push to `main` or `develop`
- Every pull request

**Coverage enforcement:**

- Build fails if coverage drops below 80% for any metric
- Coverage report is posted as PR comment

## Common Patterns

### Testing Forms

```typescript
it('validates email format', async () => {
  const user = userEvent.setup()
  render(<RegistrationForm />)

  await user.type(screen.getByLabelText(/email/i), 'invalid-email')
  await user.click(screen.getByRole('button', { name: /submit/i }))

  expect(await screen.findByText(/invalid email/i)).toBeInTheDocument()
})
```

### Testing Error Boundaries

```typescript
it('renders error fallback when child throws', () => {
  const ThrowError = () => {
    throw new Error('Test error')
  }

  render(
    <ErrorBoundary>
      <ThrowError />
    </ErrorBoundary>
  )

  expect(screen.getByText(/something went wrong/i)).toBeInTheDocument()
})
```

### Testing Loading States

```typescript
it('shows loading spinner while fetching data', async () => {
  render(<UserList />)

  // Loading state
  expect(screen.getByText(/loading/i)).toBeInTheDocument()

  // Data loaded
  expect(await screen.findByText(/john doe/i)).toBeInTheDocument()
  expect(screen.queryByText(/loading/i)).not.toBeInTheDocument()
})
```

## Best Practices

### ✅ DO

- Use semantic HTML and ARIA roles
- Test user-facing behavior
- Use `userEvent` for interactions
- Wait for async operations with `findBy*`
- Use `screen` for queries (better error messages)
- Keep tests simple and focused
- Use descriptive test names
- Mock external dependencies (APIs, localStorage)

### ❌ DON'T

- Test implementation details (state, props)
- Use `container.querySelector()` (breaks accessibility)
- Rely on CSS selectors or class names
- Test library code (React, Redux)
- Write huge test files (split into smaller files)
- Forget to clean up timers, mocks, side effects

## Troubleshooting

### "Not wrapped in act(...)" Warning

Use `await` with `user.click()` and `findBy*` queries:

```typescript
// ❌ Missing await
user.click(button);

// ✅ Correct
await user.click(button);
```

### Element Not Found

1. Check if element rendered: `screen.debug()`
2. Use `findBy*` for async elements
3. Check query (role, label, text)

### Tests Pass Locally But Fail in CI

- Check timezone/locale differences
- Ensure consistent Node.js versions
- Mock Date/Math.random for deterministic tests

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)
- [Testing Library Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)
- [User Event API](https://testing-library.com/docs/user-event/intro/)

## Support

For questions or issues:

1. Check this documentation
2. Review existing tests for patterns
3. Consult Testing Library documentation
4. Ask in team Slack channel
