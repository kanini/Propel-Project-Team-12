/**
 * Validation utility functions for form inputs (FR-001, UXR-601).
 * Provides real-time inline validation for registration and login forms.
 */

/**
 * Validates email address format using RFC 5322 standard.
 * @param email - Email address to validate
 * @returns True if valid email format, false otherwise
 */
export function validateEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Validates phone number format (US format).
 * Accepts formats: (555) 123-4567, 555-123-4567, 5551234567
 * @param phone - Phone number to validate
 * @returns True if valid phone format, false otherwise
 */
export function validatePhone(phone: string): boolean {
  const phoneRegex = /^(\+?1[-.\s]?)?(\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}$/;
  return phoneRegex.test(phone);
}

/**
 * Validates date of birth (rejects future dates, AC-Edge).
 * @param dob - Date of birth as ISO string or Date object
 * @returns True if valid past date, false if future or invalid
 */
export function validateDateOfBirth(dob: string | Date): boolean {
  const dobDate = typeof dob === 'string' ? new Date(dob) : dob;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  
  // Check if valid date
  if (isNaN(dobDate.getTime())) {
    return false;
  }
  
  // Check if not future date
  return dobDate <= today;
}

/**
 * Password strength levels
 */
export const PasswordStrength = {
  WEAK: 'weak',
  FAIR: 'fair',
  GOOD: 'good',
  STRONG: 'strong',
} as const;

export type PasswordStrengthType = typeof PasswordStrength[keyof typeof PasswordStrength];

/**
 * Password validation result with strength and missing requirements (AC4).
 */
export interface PasswordValidationResult {
  isValid: boolean;
  strength: PasswordStrengthType;
  score: number; // 0-4
  missingRequirements: string[];
}

/**
 * Validates password against security requirements (TR-013).
 * Requirements: 8+ characters, 1 uppercase, 1 lowercase, 1 number, 1 special character
 * @param password - Password to validate
 * @returns Validation result with strength and missing requirements
 */
export function validatePassword(password: string): PasswordValidationResult {
  const missingRequirements: string[] = [];
  let score = 0;

  // Check minimum length (8 characters)
  if (password.length < 8) {
    missingRequirements.push('At least 8 characters');
  } else {
    score++;
  }

  // Check for uppercase letter
  if (!/[A-Z]/.test(password)) {
    missingRequirements.push('One uppercase letter');
  } else {
    score++;
  }

  // Check for lowercase letter
  if (!/[a-z]/.test(password)) {
    missingRequirements.push('One lowercase letter');
  } else {
    score++;
  }

  // Check for number
  if (!/\d/.test(password)) {
    missingRequirements.push('One number');
  } else {
    score++;
  }

  // Check for special character
  if (!/[@$!%*?&]/.test(password)) {
    missingRequirements.push('One special character (@$!%*?&)');
  } else {
    score++;
  }

  // Determine strength based on score
  let strength: PasswordStrengthType;
  if (score <= 1) {
    strength = PasswordStrength.WEAK;
  } else if (score === 2 || score === 3) {
    strength = PasswordStrength.FAIR;
  } else if (score === 4) {
    strength = PasswordStrength.GOOD;
  } else {
    strength = PasswordStrength.STRONG;
  }

  // Additional bonus for longer passwords
  if (password.length >= 12 && score === 4) {
    strength = PasswordStrength.STRONG;
  }

  const isValid = missingRequirements.length === 0;

  return {
    isValid,
    strength,
    score,
    missingRequirements,
  };
}

/**
 * Validates full name (first + last name combined).
 * @param name - Full name to validate
 * @returns True if valid name (2-200 characters), false otherwise
 */
export function validateName(name: string): boolean {
  return name.trim().length >= 2 && name.trim().length <= 200;
}
