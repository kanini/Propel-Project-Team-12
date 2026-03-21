/**
 * Password strength validation (TR-013 - Password Complexity)
 * Enforces password complexity requirements matching backend validation
 */

export interface PasswordValidationResult {
  isValid: boolean;
  errors: string[];
  strength: 'weak' | 'medium' | 'strong';
}

/**
 * Password complexity requirements (matches RegisterUserRequest.cs DataAnnotations)
 * - Minimum 8 characters
 * - At least one uppercase letter
 * - At least one lowercase letter
 * - At least one number
 * - At least one special character
 */

/**
 * Validates password against complexity requirements
 * @param password - The password to validate
 * @returns Validation result with errors and strength indicator
 */
export const validatePassword = (password: string): PasswordValidationResult => {
  const errors: string[] = [];
  let strength: 'weak' | 'medium' | 'strong' = 'weak';

  // Check minimum length
  if (password.length < 8) {
    errors.push('Password must be at least 8 characters long');
  }

  // Check for uppercase letter
  if (!/[A-Z]/.test(password)) {
    errors.push('Password must contain at least one uppercase letter');
  }

  // Check for lowercase letter
  if (!/[a-z]/.test(password)) {
    errors.push('Password must contain at least one lowercase letter');
  }

  // Check for number
  if (!/\d/.test(password)) {
    errors.push('Password must contain at least one number');
  }

  // Check for special character
  if (!/[^\w\s]/.test(password)) {
    errors.push('Password must contain at least one special character (@, #, $, %, etc.)');
  }

  // Determine strength
  if (errors.length === 0) {
    // All requirements met - determine strength by length and character diversity
    if (password.length >= 12) {
      strength = 'strong';
    } else if (password.length >= 10) {
      strength = 'medium';
    } else {
      strength = 'medium';
    }
  }

  return {
    isValid: errors.length === 0,
    errors,
    strength,
  };
};

/**
 * Validates password confirmation match
 * @param password - Original password
 * @param confirmPassword - Confirmation password
 * @returns True if passwords match
 */
export const validatePasswordMatch = (
  password: string,
  confirmPassword: string
): boolean => {
  return password === confirmPassword && password.length > 0;
};

/**
 * Get password strength score (0-100)
 * Used for visual strength meter
 * @param password - The password to score
 * @returns Score from 0 to 100
 */
export const getPasswordStrength = (password: string): number => {
  let score = 0;

  if (password.length >= 8) score += 20;
  if (password.length >= 10) score += 10;
  if (password.length >= 12) score += 10;
  if (/[a-z]/.test(password)) score += 15;
  if (/[A-Z]/.test(password)) score += 15;
  if (/\d/.test(password)) score += 15;
  if (/[^\w\s]/.test(password)) score += 15;

  return Math.min(score, 100);
};
