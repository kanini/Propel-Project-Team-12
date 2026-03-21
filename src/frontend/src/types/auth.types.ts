/**
 * Authentication type definitions (FR-001 - User Registration)
 * TypeScript interfaces for registration API requests and responses
 */

/**
 * Registration request payload
 * Matches PatientAccess.Business.DTOs.RegisterUserRequest
 */
export interface RegistrationRequest {
  name: string;
  email: string;
  password: string;
  dateOfBirth: string; // ISO 8601 format: YYYY-MM-DD
  phone?: string;
}

/**
 * Registration response payload
 * Matches PatientAccess.Business.DTOs.RegisterUserResponse
 */
export interface RegistrationResponse {
  userId: string; // GUID
  email: string;
  message: string;
}

/**
 * Email verification request
 * Matches PatientAccess.Business.DTOs.VerifyEmailRequest
 */
export interface VerifyEmailRequest {
  token: string;
}

/**
 * API error response (ASP.NET Core ProblemDetails)
 */
export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

/**
 * Form field validation error
 */
export interface ValidationError {
  field: string;
  message: string;
}

/**
 * Registration form state
 */
export interface RegistrationFormData {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  phone: string;
  password: string;
}

/**
 * Registration state in Redux store
 */
export interface RegistrationState {
  isLoading: boolean;
  isSuccess: boolean;
  error: string | null;
  userId: string | null;
  registeredEmail: string | null;
}

/**
 * Login request payload (FR-002 - User Authentication)
 * Matches PatientAccess.Business.DTOs.LoginRequest
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Login response payload (FR-002 - User Authentication)
 * Matches PatientAccess.Business.DTOs.LoginResponse
 */
export interface LoginResponse {
  token: string;
  role: string; // Patient | Staff | Admin
  userId: string;
  name: string;
}

/**
 * Login state in Redux store
 */
export interface LoginState {
  isLoading: boolean;
  isAuthenticated: boolean;
  error: string | null;
  token: string | null;
  role: string | null;
  userId: string | null;
  name: string | null;
}
