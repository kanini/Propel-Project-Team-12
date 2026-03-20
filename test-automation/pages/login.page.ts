import { Page, Locator } from "@playwright/test";
import { BasePage } from "./base.page";

/**
 * Login Page Object representing the authentication page.
 * Provides methods to interact with login functionality.
 *
 * Example usage:
 * ```typescript
 * const loginPage = new LoginPage(page);
 * await loginPage.navigate();
 * await loginPage.login('user@example.com', 'password');
 * ```
 */
export class LoginPage extends BasePage {
  // Page elements (locators)
  private readonly emailInput: Locator;
  private readonly passwordInput: Locator;
  private readonly loginButton: Locator;
  private readonly errorMessage: Locator;
  private readonly heading: Locator;

  constructor(page: Page) {
    super(page);

    // Initialize locators using accessible selectors
    this.emailInput = page.getByLabel(/email/i);
    this.passwordInput = page.getByLabel(/password/i);
    this.loginButton = page.getByRole("button", { name: /sign in|login/i });
    this.errorMessage = page.getByRole("alert");
    this.heading = page.getByRole("heading", { name: /login|sign in/i });
  }

  /**
   * Navigate to the login page.
   */
  async navigate(): Promise<void> {
    await super.navigate("/login");
    await this.waitForElement(this.heading);
  }

  /**
   * Perform login with email and password.
   * @param email - User email address
   * @param password - User password
   */
  async login(email: string, password: string): Promise<void> {
    await this.fill(this.emailInput, email);
    await this.fill(this.passwordInput, password);
    await this.click(this.loginButton);
  }

  /**
   * Get the error message displayed on the login page.
   * @returns Error message text or null if no error
   */
  async getErrorMessage(): Promise<string | null> {
    try {
      return await this.getText(this.errorMessage);
    } catch {
      return null;
    }
  }

  /**
   * Check if the login page is displayed.
   * @returns True if on login page, false otherwise
   */
  async isLoginPageDisplayed(): Promise<boolean> {
    return await this.isVisible(this.heading);
  }

  /**
   * Fill email field.
   * @param email - Email address
   */
  async fillEmail(email: string): Promise<void> {
    await this.fill(this.emailInput, email);
  }

  /**
   * Fill password field.
   * @param password - Password
   */
  async fillPassword(password: string): Promise<void> {
    await this.fill(this.passwordInput, password);
  }

  /**
   * Click the login button.
   */
  async clickLoginButton(): Promise<void> {
    await this.click(this.loginButton);
  }
}
