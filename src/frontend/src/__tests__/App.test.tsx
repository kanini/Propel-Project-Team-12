/**
 * Example test demonstrating Vitest + React Testing Library patterns
 *
 * This test file showcases:
 * - Component rendering and querying
 * - User interaction testing
 * - Assertion patterns with @testing-library/jest-dom matchers
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import App from "../App";

describe("App Component", () => {
  it("renders the main heading", () => {
    render(<App />);

    const heading = screen.getByRole("heading", {
      level: 1,
      name: /patient access platform/i,
    });

    expect(heading).toBeInTheDocument();
  });

  it("displays welcome message", () => {
    render(<App />);

    expect(
      screen.getByText(/welcome to the clinical intelligence/i),
    ).toBeInTheDocument();
  });

  it("shows frontend setup checklist", () => {
    render(<App />);

    expect(screen.getByText(/frontend setup complete/i)).toBeInTheDocument();
    expect(screen.getByText(/react 18 with typescript/i)).toBeInTheDocument();
    expect(screen.getByText(/vite build tool/i)).toBeInTheDocument();
    expect(screen.getByText(/tailwind css/i)).toBeInTheDocument();
    expect(screen.getByText(/redux toolkit/i)).toBeInTheDocument();
    expect(screen.getByText(/react router/i)).toBeInTheDocument();
  });

  it("increments counter when button is clicked", async () => {
    // Setup user event
    const user = userEvent.setup();

    render(<App />);

    // Find button with initial count
    const button = screen.getByRole("button", { name: /count is 0/i });
    expect(button).toBeInTheDocument();

    // Click button
    await user.click(button);

    // Verify counter incremented
    expect(
      screen.getByRole("button", { name: /count is 1/i }),
    ).toBeInTheDocument();

    // Click again
    await user.click(button);

    // Verify counter incremented again
    expect(
      screen.getByRole("button", { name: /count is 2/i }),
    ).toBeInTheDocument();
  });

  it("renders edit instruction with code snippet", () => {
    render(<App />);

    expect(screen.getByText(/edit/i)).toBeInTheDocument();
    expect(screen.getByText(/src\/app\.tsx/i)).toBeInTheDocument();
  });
});
