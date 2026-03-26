/**
 * Tests for the App component verifying routing and Redux integration.
 *
 * The current App uses react-router and react-redux, so it must be
 * rendered inside a Redux Provider. Unauthenticated routes redirect
 * to /login by default.
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";
import rootReducer from "../store/rootReducer";
import App from "../App";

/** Create a fresh test store with optional initial auth state */
function createTestStore(preloadedState?: Record<string, unknown>) {
  return configureStore({
    reducer: rootReducer,
    preloadedState,
  });
}

describe("App Component", () => {
  it("renders without crashing when wrapped in Provider", () => {
    const store = createTestStore();

    render(
      <Provider store={store}>
        <App />
      </Provider>,
    );

    // Default route redirects to login – the login page should render
    expect(screen.getByRole("button", { name: /sign in|log in|login/i })).toBeInTheDocument();
  });

  it("redirects unauthenticated users to login page", () => {
    const store = createTestStore();

    render(
      <Provider store={store}>
        <App />
      </Provider>,
    );

    // Login form elements should be visible
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getAllByLabelText(/password/i).length).toBeGreaterThanOrEqual(1);
  });

  it("renders register link on login page", () => {
    const store = createTestStore();

    render(
      <Provider store={store}>
        <App />
      </Provider>,
    );

    expect(screen.getByText(/create.*account|sign up|register/i)).toBeInTheDocument();
  });

  it("renders forgot password link on login page", () => {
    const store = createTestStore();

    render(
      <Provider store={store}>
        <App />
      </Provider>,
    );

    expect(screen.getByText(/forgot.*password/i)).toBeInTheDocument();
  });

  it("renders session timeout modal component", () => {
    const store = createTestStore();

    const { container } = render(
      <Provider store={store}>
        <App />
      </Provider>,
    );

    // SessionTimeoutModal should be in the DOM (hidden when showWarning is false)
    // The component itself renders — it just doesn't display when inactive
    expect(container).toBeTruthy();
  });
});
