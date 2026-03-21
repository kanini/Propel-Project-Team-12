/**
 * Date validation utilities (FR-001 - User Registration)
 * Validates date of birth for registration form
 */

/**
 * Validates date of birth is not in the future
 * @param dateOfBirth - Date string in YYYY-MM-DD format
 * @returns True if date is valid (not in future)
 */
export const validateDateNotInFuture = (dateOfBirth: string): boolean => {
  if (!dateOfBirth) return false;

  const date = new Date(dateOfBirth);
  const today = new Date();
  today.setHours(0, 0, 0, 0); // Normalize to start of day

  return date <= today;
};

/**
 * Validates date of birth is within reasonable age range
 * @param dateOfBirth - Date string in YYYY-MM-DD format
 * @returns True if age is between 0 and 120 years
 */
export const validateAge = (dateOfBirth: string): boolean => {
  if (!dateOfBirth) return false;

  const date = new Date(dateOfBirth);
  const today = new Date();
  const age = today.getFullYear() - date.getFullYear();
  const monthDiff = today.getMonth() - date.getMonth();

  // Adjust age if birthday hasn't occurred this year
  const adjustedAge =
    monthDiff < 0 || (monthDiff === 0 && today.getDate() < date.getDate())
      ? age - 1
      : age;

  return adjustedAge >= 0 && adjustedAge <= 120;
};

/**
 * Validates date of birth meets minimum age requirement
 * @param dateOfBirth - Date string in YYYY-MM-DD format
 * @param minAge - Minimum age requirement (default: 13 for COPPA compliance)
 * @returns True if user meets minimum age
 */
export const validateMinimumAge = (
  dateOfBirth: string,
  minAge: number = 13
): boolean => {
  if (!dateOfBirth) return false;

  const date = new Date(dateOfBirth);
  const today = new Date();
  const age = today.getFullYear() - date.getFullYear();
  const monthDiff = today.getMonth() - date.getMonth();

  const adjustedAge =
    monthDiff < 0 || (monthDiff === 0 && today.getDate() < date.getDate())
      ? age - 1
      : age;

  return adjustedAge >= minAge;
};

/**
 * Validates date string format (YYYY-MM-DD)
 * @param dateString - Date string to validate
 * @returns True if format is valid
 */
export const validateDateFormat = (dateString: string): boolean => {
  if (!dateString) return false;

  // Check format with regex (YYYY-MM-DD)
  const dateRegex = /^\d{4}-\d{2}-\d{2}$/;
  if (!dateRegex.test(dateString)) return false;

  // Verify valid date
  const date = new Date(dateString);
  return !isNaN(date.getTime());
};

/**
 * Comprehensive date of birth validation
 * @param dateOfBirth - Date string in YYYY-MM-DD format
 * @returns Validation result with error message
 */
export const validateDateOfBirth = (
  dateOfBirth: string
): { isValid: boolean; error: string | null } => {
  if (!dateOfBirth) {
    return { isValid: false, error: 'Date of birth is required' };
  }

  if (!validateDateFormat(dateOfBirth)) {
    return { isValid: false, error: 'Invalid date format' };
  }

  if (!validateDateNotInFuture(dateOfBirth)) {
    return { isValid: false, error: 'Date of birth cannot be in the future' };
  }

  if (!validateAge(dateOfBirth)) {
    return { isValid: false, error: 'Date of birth must be within reasonable age range' };
  }

  if (!validateMinimumAge(dateOfBirth, 13)) {
    return { isValid: false, error: 'You must be at least 13 years old to register' };
  }

  return { isValid: true, error: null };
};
