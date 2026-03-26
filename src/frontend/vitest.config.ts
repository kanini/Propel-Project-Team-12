import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vitest.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  test: {
    // Enable global test APIs (describe, it, expect, etc.)
    globals: true,

    // Use jsdom environment for DOM API access
    environment: "jsdom",

    // Setup files to run before tests
    setupFiles: ["./src/setupTests.ts"],

    // Coverage configuration with 80% thresholds
    coverage: {
      provider: "v8",
      reporter: ["text", "json", "html", "lcov"],
      exclude: [
        // Exclude non-business logic files
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
      // Coverage thresholds - increase incrementally as test coverage grows
      thresholds: {
        lines: 5,
        functions: 5,
        branches: 3,
        statements: 5,
      },
    },

    // Test file patterns
    include: ["src/**/*.{test,spec}.{ts,tsx}"],

    // Watch mode ignores
    watchExclude: ["node_modules/**", "dist/**"],
  },
});
