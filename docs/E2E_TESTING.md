# E2E Testing Guide with Playwright

## Overview

This document outlines the end-to-end (E2E) testing strategy, patterns, and best practices for the Patient Access Platform using **Playwright**. We use the **Page Object Model (POM)** pattern for maintainable and scalable test automation.

## Technology Stack

| Tool       | Version | Purpose                    |
| ---------- | ------- | -------------------------- |
| Playwright | Latest  | E2E testing framework      |
| TypeScript | 5.x     | Type-safe test development |
| Chromium   | Latest  | Primary test browser       |

## Testing Philosophy

> **Test user journeys, not implementation details**

### Key Principles

1. **Page Object Model**: Encapsulate page interactions in reusable objects
2. **Test Isolation**: Each test should be independent and not rely on others
3. **Accessibility-First Selectors**: Use role-based and label-based selectors
4. **Retry Strategy**: Configure retries for flaky tests (1 retry in CI)
5. **Visual Feedback**: Capture screenshots and videos on failure

## Project Structure

```
test-automation/
├── pages/                    # Page Object Models
│   ├── base.page.ts         # Base page with common methods
│   ├── home.page.ts         # Homepage page object
│   └── login.page.ts        # Login page object
├── tests/                    # Test specifications
│   └── example.spec.ts      # Sample E2E tests
├── playwright.config.ts      # Playwright configuration
├── tsconfig.json            # TypeScript configuration
├── package.json             # Dependencies and scripts
└── .gitignore               # Exclude test artifacts

Generated at runtime:
├── playwright-report/        # HTML test report
├── test-results/            # Test execution results
├── screenshots/             # Failure screenshots
└── videos/                  # Failure recordings
```

## Configuration

### playwright.config.ts

```typescript
export default defineConfig({
  testDir: "./tests",
  timeout: 30 * 1000, // 30s per test
  retries: process.env.CI ? 1 : 0, // 1 retry in CI for flaky tests
  workers: process.env.CI ? 1 : undefined,

  use: {
    baseURL: "http://localhost:5173",
    trace: "on-first-retry",
    screenshot: "only-on-failure",
    video: "retain-on-failure",
    actionTimeout: 10 * 1000,
    navigationTimeout: 15 * 1000,
  },

  projects: [{ name: "chromium", use: { ...devices["Desktop Chrome"] } }],
});
```

### Key Settings

- **Timeout**: 30s per test (configurable per test)
- **Retries**: 1 retry in CI to handle flaky tests
- **Base URL**: Frontend dev server (default: http://localhost:5173)
- **Trace**: Captured on first retry for debugging
- **Screenshots/Videos**: Only on failure to save space

## Writing Tests

### Page Object Model Pattern

#### Base Page (`base.page.ts`)

```typescript
export abstract class BasePage {
  protected readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async navigate(path: string): Promise<void> {
    await this.page.goto(path);
  }

  async fill(locator: Locator, value: string): Promise<void> {
    await locator.fill(value);
  }

  async click(locator: Locator): Promise<void> {
    await locator.click();
  }

  // ... other common methods
}
```

#### Page Object Example (`login.page.ts`)

```typescript
export class LoginPage extends BasePage {
  private readonly emailInput: Locator;
  private readonly passwordInput: Locator;
  private readonly loginButton: Locator;

  constructor(page: Page) {
    super(page);
    this.emailInput = page.getByLabel(/email/i);
    this.passwordInput = page.getByLabel(/password/i);
    this.loginButton = page.getByRole("button", { name: /sign in/i });
  }

  async navigate(): Promise<void> {
    await super.navigate("/login");
  }

  async login(email: string, password: string): Promise<void> {
    await this.fill(this.emailInput, email);
    await this.fill(this.passwordInput, password);
    await this.click(this.loginButton);
  }
}
```

### Test Structure

```typescript
import { test, expect } from "@playwright/test";
import { HomePage } from "../pages/home.page";

test.describe("Feature Name", () => {
  let homePage: HomePage;

  // Setup before each test
  test.beforeEach(async ({ page }) => {
    homePage = new HomePage(page);
    await homePage.navigate();
  });

  test("should perform expected action", async () => {
    // Arrange - prepare test data
    const expectedText = "Welcome";

    // Act - perform actions
    const actualText = await homePage.getWelcomeMessage();

    // Assert - verify outcomes
    expect(actualText).toContain(expectedText);
  });
});
```

## Selector Strategy

Use accessibility-first selectors (ordered by preference):

1. **Role-based**: `getByRole('button', { name: 'Submit' })`
2. **Label-based**: `getByLabel('Email')`
3. **Placeholder**: `getByPlaceholder('Enter email')`
4. **Text**: `getByText('Welcome')`
5. **Test ID** (last resort): `getByTestId('submit-button')`

### ❌ Avoid

```typescript
// Don't use fragile CSS selectors
page.locator(".btn-primary");
page.locator("#submit-btn");
page.locator("div > button:nth-child(2)");
```

### ✅ Prefer

```typescript
// Use accessible selectors
page.getByRole("button", { name: /submit/i });
page.getByLabel(/email/i);
```

## Common Patterns

### Navigation and Waiting

```typescript
// Navigate and wait for page load
await homePage.navigate();

// Wait for specific element
await page.getByText("Dashboard").waitFor();

// Wait for network idle
await page.waitForLoadState("networkidle");

// Click and wait for navigation
await Promise.all([
  page.waitForURL("**/dashboard"),
  page.getByRole("button", { name: "Go" }).click(),
]);
```

### Form Interactions

```typescript
// Fill out form
await page.getByLabel("Email").fill("user@example.com");
await page.getByLabel("Password").fill("password123");
await page.getByRole("button", { name: "Sign In" }).click();

// Select from dropdown
await page.getByLabel("Country").selectOption("USA");

// Check checkbox
await page.getByLabel("Remember me").check();

// Upload file
await page.getByLabel("Upload").setInputFiles("path/to/file.pdf");
```

### Assertions

```typescript
// Visibility assertions
await expect(page.getByText("Success")).toBeVisible();
await expect(page.getByText("Error")).not.toBeVisible();

// Content assertions
await expect(page.getByRole("heading")).toHaveText("Dashboard");
await expect(page.getByLabel("Email")).toHaveValue("user@example.com");

// State assertions
await expect(page.getByRole("button", { name: "Submit" })).toBeEnabled();
await expect(page.getByRole("button", { name: "Save" })).toBeDisabled();

// Count assertions
await expect(page.getByRole("listitem")).toHaveCount(5);

// URL assertions
await expect(page).toHaveURL(/.*dashboard/);
await expect(page).toHaveTitle(/Dashboard/);
```

### Handling Dialogs

```typescript
// Handle alert dialogs
page.on("dialog", async (dialog) => {
  await dialog.accept();
});

// Handle confirmation with custom message
page.on("dialog", async (dialog) => {
  console.log(dialog.message());
  await dialog.dismiss();
});
```

### Multi-Tab Handling

```typescript
// Open new tab and switch
const [newPage] = await Promise.all([
  context.waitForEvent("page"),
  page.getByText("Open in new tab").click(),
]);
await newPage.waitForLoadState();
```

## Running Tests

### CLI Commands

```bash
# Run all tests
npm test

# Run tests in headed mode (visible browser)
npm run test:headed

# Run tests with UI (interactive mode)
npm run test:ui

# Run tests in debug mode
npm run test:debug

# Run specific test file
npx playwright test tests/example.spec.ts

# Run tests matching pattern
npx playwright test --grep "login"

# Run tests on specific project
npm run test:chromium

# Generate HTML report
npm run test:report
```

### Required Setup

Before running tests, ensure the frontend dev server is running:

```bash
# Terminal 1: Start frontend dev server
cd src/frontend
npm run dev

# Terminal 2: Run E2E tests
cd test-automation
npm test
```

## Debugging Tests

### VS Code Debugging

1. Install Playwright extension
2. Open test file
3. Click on green play button next to test
4. Or use "Debug Test" right-click option

### Command Line Debugging

```bash
# Debug mode (step through tests)
npx playwright test --debug

# Headed mode (see browser)
npx playwright test --headed

# UI mode (interactive runner)
npx playwright test --ui

# Slow motion (see actions clearly)
npx playwright test --headed --slow-mo=1000
```

### Playwright Inspector

```bash
# Open Playwright Inspector
npx playwright test --debug

# Useful commands in inspector:
# - Step over
# - Continue
# - Pause
# - Pick selector
```

### Viewing Traces

```bash
# Open trace viewer
npx playwright show-trace test-results/traces/trace.zip
```

## Best Practices

### ✅ DO

- Use Page Object Model for all page interactions
- Use accessibility-first selectors (getByRole, getByLabel)
- Make tests independent and isolated
- Use meaningful test descriptions
- Keep tests focused on user journeys
- Use auto-waiting (built into Playwright)
- Capture screenshots/videos only on failure
- Use `test.beforeEach` for common setup
- Group related tests with `test.describe`

### ❌ DON'T

- Don't use fragile CSS selectors or XPath
- Don't share state between tests
- Don't test implementation details
- Don't use fixed waits (`page.waitForTimeout`)
- Don't ignore flaky tests (fix or retry)
- Don't hardcode test data in multiple places
- Don't test third-party libraries
- Don't write overly long tests

## Handling Flaky Tests

### Retry Strategy

Configured in `playwright.config.ts`:

```typescript
retries: process.env.CI ? 1 : 0;
```

Tests automatically retry once in CI if they fail.

### Making Tests More Stable

```typescript
// Bad: Fixed timeout
await page.waitForTimeout(3000);

// Good: Wait for specific condition
await page.getByText("Loaded").waitFor();

// Bad: Racing with async operations
await page.click(button);
expect(result).toBe("Success");

// Good: Wait for outcome
await page.click(button);
await expect(page.getByText("Success")).toBeVisible();
```

## Test Isolation

Each test should:

- Start from a known state
- Not depend on previous tests
- Clean up after itself

```typescript
test.beforeEach(async ({ page }) => {
  // Reset to clean state
  await page.goto("/");
  // Clear local storage
  await page.evaluate(() => localStorage.clear());
});

test.afterEach(async ({ page }) => {
  // Cleanup if needed
  await page.close();
});
```

## CI/CD Integration

Tests can be integrated into GitHub Actions:

```yaml
- name: Install Playwright browsers
  run: npx playwright install chromium

- name: Run E2E tests
  run: npm test
  working-directory: ./test-automation

- name: Upload test results
  if: always()
  uses: actions/upload-artifact@v4
  with:
    name: playwright-report
    path: test-automation/playwright-report/
```

## Performance Tips

1. **Run in parallel**: Playwright runs tests in parallel by default
2. **Use chromium only**: Faster than testing all browsers
3. **Optimize selectors**: Use efficient locators
4. **Reuse authentication**: Save auth state and reuse
5. **Mock external APIs**: Avoid real API calls when possible

## Authentication Patterns

### Save and Reuse Auth State

```typescript
// auth.setup.ts
test("authenticate", async ({ page }) => {
  await page.goto("/login");
  await page.getByLabel("Email").fill("user@example.com");
  await page.getByLabel("Password").fill("password");
  await page.getByRole("button", { name: "Sign in" }).click();

  // Save authenticated state
  await page.context().storageState({ path: "auth.json" });
});

// Use in tests
test.use({ storageState: "auth.json" });
```

## Troubleshooting

### Tests failing to find elements

1. Check selector is correct: Use Playwright Inspector
2. Increase timeout if element loads slowly
3. Wait for network idle before asserting
4. Check element is in viewport

### Browser not launching

```bash
# Reinstall browsers
npx playwright install chromium --force
```

### Tests pass locally but fail in CI

1. Check viewport size matches
2. Verify base URL is correct
3. Ensure timing is not environment-dependent
4. Review CI logs and artifacts

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [Page Object Model](https://playwright.dev/docs/pom)
- [Best Practices](https://playwright.dev/docs/best-practices)
- [Selectors](https://playwright.dev/docs/selectors)
- [Test Retry](https://playwright.dev/docs/test-retries)
- [Debugging](https://playwright.dev/docs/debug)

## Support

For questions or issues:

1. Check this documentation
2. Review Playwright documentation
3. Check existing tests for patterns
4. Ask in team Slack channel
