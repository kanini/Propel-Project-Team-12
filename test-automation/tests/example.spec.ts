import { test, expect } from "@playwright/test";
import { HomePage } from "../pages/home.page";

/**
 * Example E2E Test Suite for Patient Access Platform
 *
 * Demonstrates Playwright testing patterns:
 * - Page Object Model usage
 * - Test isolation
 * - Assertion patterns
 * - Browser automation
 */

test.describe("Patient Access Platform - Homepage", () => {
  let homePage: HomePage;

  // Setup: Initialize page objects before each test
  test.beforeEach(async ({ page }) => {
    homePage = new HomePage(page);
    await homePage.navigate();
  });

  test("should display the main heading", async () => {
    // Verify homepage loaded correctly
    await expect(homePage.isHomePageDisplayed()).resolves.toBe(true);

    // Check main heading text
    const heading = await homePage.getMainHeading();
    expect(heading).toContain("Patient Access Platform");
  });

  test("should display welcome message", async () => {
    // Verify welcome message is present
    const welcomeMessage = await homePage.getWelcomeMessage();
    expect(welcomeMessage).toContain("Welcome to the Clinical Intelligence");
  });

  test("should have interactive counter button", async () => {
    // Get initial counter value
    const initialCount = await homePage.getCounterValue();
    expect(initialCount).toBe(0);

    // Click counter button
    await homePage.clickCounterButton();

    // Verify counter incremented
    const newCount = await homePage.getCounterValue();
    expect(newCount).toBe(1);
  });

  test("should increment counter multiple times", async () => {
    // Click counter button multiple times
    for (let i = 0; i < 3; i++) {
      await homePage.clickCounterButton();
    }

    // Verify counter shows correct value
    const finalCount = await homePage.getCounterValue();
    expect(finalCount).toBe(3);
  });

  test("should have correct page title", async () => {
    // Verify page title
    const title = await homePage.getTitle();
    expect(title).toContain("Vite");
  });

  test("should display frontend setup checklist", async ({ page }) => {
    // Verify setup checklist is visible
    const checklist = page.getByText(/Frontend Setup Complete/i);
    await expect(checklist).toBeVisible();

    // Verify checklist items
    const items = [
      "React 18 with TypeScript",
      "Vite Build Tool",
      "Tailwind CSS",
      "Redux Toolkit",
      "React Router",
    ];

    for (const item of items) {
      await expect(page.getByText(item, { exact: false })).toBeVisible();
    }
  });
});

test.describe("Navigation and Page Load", () => {
  test("should load homepage within timeout", async ({ page }) => {
    const homePage = new HomePage(page);

    // Measure page load time
    const startTime = Date.now();
    await homePage.navigate();
    const loadTime = Date.now() - startTime;

    // Verify page loaded successfully
    await expect(homePage.isHomePageDisplayed()).resolves.toBe(true);

    // Log load time for performance monitoring
    console.log(`Homepage loaded in ${loadTime}ms`);

    // Expect reasonable load time (< 5 seconds)
    expect(loadTime).toBeLessThan(5000);
  });

  test("should handle page reload", async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.navigate();

    // Reload the page
    await homePage.reload();

    // Verify page still displays correctly
    await expect(homePage.isHomePageDisplayed()).resolves.toBe(true);
  });
});

test.describe("Browser and Viewport", () => {
  test("should render correctly at desktop viewport", async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.navigate();

    // Verify viewport size
    const viewport = page.viewportSize();
    expect(viewport?.width).toBeGreaterThanOrEqual(1024);

    // Verify content is visible
    await expect(homePage.isHomePageDisplayed()).resolves.toBe(true);
  });
});
