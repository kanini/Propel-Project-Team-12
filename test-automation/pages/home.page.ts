import { Page, Locator } from "@playwright/test";
import { BasePage } from "./base.page";

/**
 * Home Page Object representing the main landing page.
 * Provides methods to interact with homepage elements.
 *
 * Example usage:
 * ```typescript
 * const homePage = new HomePage(page);
 * await homePage.navigate();
 * const heading = await homePage.getMainHeading();
 * ```
 */
export class HomePage extends BasePage {
  // Page elements (locators)
  private readonly mainHeading: Locator;
  private readonly welcomeMessage: Locator;
  private readonly counterButton: Locator;

  constructor(page: Page) {
    super(page);

    // Initialize locators
    this.mainHeading = page.getByRole("heading", { level: 1 });
    this.welcomeMessage = page.getByText(/welcome to/i);
    this.counterButton = page.getByRole("button", { name: /count is/i });
  }

  /**
   * Navigate to the home page.
   */
  async navigate(): Promise<void> {
    await super.navigate("/");
    await this.waitForElement(this.mainHeading);
  }

  /**
   * Get the main heading text.
   * @returns Main heading text
   */
  async getMainHeading(): Promise<string> {
    return await this.getText(this.mainHeading);
  }

  /**
   * Get the welcome message text.
   * @returns Welcome message text
   */
  async getWelcomeMessage(): Promise<string> {
    return await this.getText(this.welcomeMessage);
  }

  /**
   * Click the counter button.
   */
  async clickCounterButton(): Promise<void> {
    await this.click(this.counterButton);
  }

  /**
   * Get the current counter value from the button text.
   * @returns Counter value
   */
  async getCounterValue(): Promise<number> {
    const buttonText = await this.getText(this.counterButton);
    const match = buttonText.match(/count is (\d+)/i);
    return match ? parseInt(match[1], 10) : 0;
  }

  /**
   * Check if the homepage is displayed.
   * @returns True if on homepage, false otherwise
   */
  async isHomePageDisplayed(): Promise<boolean> {
    return await this.isVisible(this.mainHeading);
  }
}
