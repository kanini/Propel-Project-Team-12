/**
 * Registration form validation (FR-001 - User Registration)
 * Aggregates all validators for the registration form
 */

import { validatePassword } from './passwordValidator';
import { validateDateOfBirth } from './dateValidator';
import type { ValidationError, RegistrationFormData } from '../../types/auth.types';

/**
 * Validates email format
 * @param email - Email address to validate
 * @returns True if email format is valid
 */
export const validateEmail = (email: string): boolean => {
  if (!email) return false;

  // Email regex matching ASP.NET Core EmailAddress validation
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email) && email.length <= 255;
};

/**
 * Validates name field
 * @param name - Name to validate
 * @returns True if name is valid
 */
export const validateName = (name: string): boolean => {
  return name.trim().length >= 1 && name.length <= 100;
};

/**
 * Validates phone number (now required per wireframe)
 * @param phone - Phone number to validate
 * @returns True if phone is valid
 */
export const validatePhone = (phone: string): boolean => {
  if (!phone || phone.trim() === '') return false; // Now required

  // Basic phone validation (10-15 digits with optional formatting)
  const phoneRegex = /^[\d\s\-().+]{10,20}$/;
  return phoneRegex.test(phone);
};

/**
 * Validates entire registration form
 * @param formData - Registration form data
 * @returns Array of validation errors (empty if valid)
 */
export const validateRegistrationForm = (
  formData: RegistrationFormData
): ValidationError[] => {
  const errors: ValidationError[] = [];

  // Validate first name
  if (!validateName(formData.firstName)) {
    errors.push({
      field: 'firstName',
      message: 'First name is required',
    });
  }

  // Validate last name
  if (!validateName(formData.lastName)) {
    errors.push({
      field: 'lastName',
      message: 'Last name is required',
    });
  }

  // Validate email
  if (!validateEmail(formData.email)) {
    errors.push({
      field: 'email',
      message: 'Please enter a valid email address',
    });
  }

  // Validate password
  const passwordValidation = validatePassword(formData.password);
  if (!passwordValidation.isValid) {
    errors.push({
      field: 'password',
      message: passwordValidation.errors[0] || 'Invalid password', // Show first error
    });
  }

  // Validate date of birth
  const dobValidation = validateDateOfBirth(formData.dateOfBirth);
  if (!dobValidation.isValid) {
    errors.push({
      field: 'dateOfBirth',
      message: dobValidation.error || 'Invalid date of birth',
    });
  }

  // Validate phone (now required per wireframe)
  if (!validatePhone(formData.phone)) {
    errors.push({
      field: 'phone',
      message: 'Please enter a valid phone number',
    });
  }

  return errors;
};

/**
 * Validates single form field (for inline validation)
 * @param field - Field name to validate
 * @param value - Field value
 * @returns Error message or null if valid
 */
export const validateField = (
  field: keyof RegistrationFormData,
  value: string
): string | null => {
  switch (field) {
    case 'firstName':
      return validateName(value) ? null : 'First name is required';

    case 'lastName':
      return validateName(value) ? null : 'Last name is required';

    case 'email':
      return validateEmail(value) ? null : 'Please enter a valid email address';

    case 'password': {
      const result = validatePassword(value);
      return result.isValid ? null : (result.errors[0] || 'Invalid password');
    }

    case 'dateOfBirth': {
      const result = validateDateOfBirth(value);
      return result.error;
    }

    case 'phone':
      return validatePhone(value) ? null : 'Please enter a valid phone number';

    default:
      return null;
  }
};
