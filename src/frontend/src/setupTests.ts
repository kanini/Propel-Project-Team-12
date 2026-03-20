/**
 * Test environment setup for Vitest
 *
 * This file is automatically executed before each test suite.
 * It configures the testing environment with custom matchers and global setup.
 */

// Import @testing-library/jest-dom for extended DOM matchers
// Provides matchers like .toBeInTheDocument(), .toHaveTextContent(), etc.
import "@testing-library/jest-dom";

// Additional global test setup can be added here
// Examples:
// - Mock global objects (window, localStorage, etc.)
// - Configure test utilities
// - Set up test data factories
