# Types

This directory contains TypeScript type definitions and interfaces.

## Structure

- Shared types used across multiple modules
- API response/request types
- Domain model types
- Utility types

## Guidelines

- Use interfaces for object shapes
- Use type aliases for unions, intersections, and mapped types
- Export types with clear, descriptive names
- Group related types together in files

## Example

```typescript
// User types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export type UserRole = 'admin' | 'provider' | 'patient';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: User;
  token: string;
}
```
