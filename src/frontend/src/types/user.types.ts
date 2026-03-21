/**
 * User management type definitions (US_021).
 * Maps to backend DTOs for user CRUD operations.
 */

/**
 * User role enumeration matching backend UserRole enum.
 */
export const UserRole = {
  Patient: 1,
  Staff: 2,
  Admin: 3
} as const;

export type UserRole = typeof UserRole[keyof typeof UserRole];

/**
 * User status enumeration matching backend UserStatus enum.
 */
export const UserStatus = {
  Active: 1,
  Suspended: 2,
  Inactive: 3
} as const;

export type UserStatus = typeof UserStatus[keyof typeof UserStatus];

/**
 * User DTO matching backend UserDto.
 * Excludes sensitive fields like PasswordHash.
 */
export interface User {
  userId: string;
  name: string;
  email: string;
  role: UserRole;
  status: UserStatus;
  createdAt: string;
  lastLogin: string | null;
}

/**
 * Request DTO for creating new Staff/Admin users (US_021 AC1).
 * System generates password and sends activation email.
 */
export interface CreateUserRequest {
  name: string;
  email: string;
  role: UserRole;
}

/**
 * Request DTO for updating user details (US_021 AC2).
 * Email is immutable after creation.
 */
export interface UpdateUserRequest {
  name: string;
  role: UserRole;
}

/**
 * User management slice state.
 */
export interface UserManagementState {
  users: User[];
  loading: boolean;
  error: string | null;
  activeAdminCount: number;
}
