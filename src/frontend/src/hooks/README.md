# Hooks

This directory contains custom React hooks for reusable logic.

## Guidelines

- All hooks should start with "use" prefix (e.g., `useAuth`, `useApi`)
- Extract repeated logic from components into custom hooks
- Document hook parameters and return values
- Write tests for hooks with complex logic

## Examples

- `useAuth`: Authentication state and methods
- `useApi`: API call wrapper with loading/error states
- `useLocalStorage`: localStorage persistence hook
- `useDebounce`: Debounce value changes
