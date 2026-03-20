import { Page, Locator } from "@playwright/test";

/**
 * Base Page Object containing common methods for all page objects.
 * Implements Page Object Model pattern for maintainable E2E tests.
 *
 * All page objects should extend this class to inherit common functionality:
 * - Navigation helpers
 * - Wait utilities
 * - Common assertions
 * - Element interaction wrappers
 */
export abstract class BasePage {
  protected readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  /**
   * Navigate to a specific URL path (relative to baseURL).
   * @param path - URL path to navigate to
   */
  async navigate(path: string): Promise<void> {
    await this.page.goto(path);
  }

  /**
   * Get the current page title.
   * @returns Current page title
   */
  async getTitle(): Promise<string> {
    return await this.page.title();
  }

  /**
   * Get the current page URL.
   * @returns Current page URL
   */
  getURL(): string {
    return this.page.url();
  }

  /**
   * Wait for a specific element to be visible.
   * @param locator - Playwright locator for the element
   * @param timeout - Optional timeout in milliseconds
   */
  async waitForElement(locator: Locator, timeout?: number): Promise<void> {
    await locator.waitFor({ state: "visible", timeout });
  }

  /**
   * Wait for navigation to complete.
   * @param options - Optional wait options
   */
  async waitForNavigation(options?: {
    url?: string | RegExp;
    timeout?: number;
  }): Promise<void> {
    await this.page.waitForURL(options?.url || "*", {
      timeout: options?.timeout,
      waitUntil: "networkidle",
    });
  }

  /**
   * Click an element and wait for navigation.
   * @param locator - Playwright locator for the element
   */
  async clickAndWaitForNavigation(locator: Locator): Promise<void> {
    await Promise.all([
      this.page.waitForLoadState("networkidle"),
      locator.click(),
    ]);
  }

  /**
   * Fill a form input field.
   * @param locator - Playwright locator for the input
   * @param value - Value to fill
   */
  async fill(locator: Locator, value: string): Promise<void> {
    await locator.fill(value);
  }

  /**
   * Click an element.
   * @param locator - Playwright locator for the element
   */
  async click(locator: Locator): Promise<void> {
    await locator.click();
  }

  /**
   * Get text content of an element.
   * @param locator - Playwright locator for the element
   * @returns Text content
   */
  async getText(locator: Locator): Promise<string> {
    return (await locator.textContent()) || "";
  }

  /**
   * Check if an element is visible.
   * @param locator - Playwright locator for the element
   * @returns True if visible, false otherwise
   */
  async isVisible(locator: Locator): Promise<boolean> {
    return await locator.isVisible();
  }

  /**
   * Wait for a specific amount of time.
   * @param ms - Milliseconds to wait
   */
  async wait(ms: number): Promise<void> {
    await this.page.waitForTimeout(ms);
  }

  /**
   * Take a screenshot.
   * @param name - Screenshot filename
   */
  async takeScreenshot(name: string): Promise<void> {
    await this.page.screenshot({
      path: `screenshots/${name}.png`,
      fullPage: true,
    });
  }

  /**
   * Reload the current page.
   */
  async reload(): Promise<void> {
    await this.page.reload();
  }

  /**
   * Go back to the previous page.
   */
  async goBack(): Promise<void> {
    await this.page.goBack();
  }

  /**
   * Execute custom JavaScript in the page context.
   * @param script - JavaScript code to execute
   * @returns Result of the script execution
   */
  async executeScript<T>(script: string): Promise<T> {
    return (await this.page.evaluate(script)) as T;
  }
}
