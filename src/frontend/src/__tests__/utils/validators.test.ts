import { describe, it, expect } from 'vitest';
import {
  validateEmail,
  validatePhone,
  validateDateOfBirth,
  validatePassword,
  validateName,
  PasswordStrength,
} from '../../utils/validators';

describe('validateEmail', () => {
  it('returns true for valid email', () => {
    expect(validateEmail('user@example.com')).toBe(true);
  });

  it('returns false for missing @', () => {
    expect(validateEmail('userexample.com')).toBe(false);
  });

  it('returns false for empty string', () => {
    expect(validateEmail('')).toBe(false);
  });
});

describe('validatePhone', () => {
  it('returns true for 10-digit number', () => {
    expect(validatePhone('5551234567')).toBe(true);
  });

  it('returns true for formatted number', () => {
    expect(validatePhone('(555) 123-4567')).toBe(true);
  });

  it('returns false for short number', () => {
    expect(validatePhone('123')).toBe(false);
  });
});

describe('validateDateOfBirth', () => {
  it('returns true for past date string', () => {
    expect(validateDateOfBirth('1990-01-01')).toBe(true);
  });

  it('returns true for Date object in the past', () => {
    expect(validateDateOfBirth(new Date('2000-06-15'))).toBe(true);
  });

  it('returns false for future date', () => {
    const future = new Date();
    future.setFullYear(future.getFullYear() + 1);
    expect(validateDateOfBirth(future)).toBe(false);
  });

  it('returns false for invalid date string', () => {
    expect(validateDateOfBirth('not-a-date')).toBe(false);
  });
});

describe('validatePassword', () => {
  it('returns strong for fully compliant long password', () => {
    const result = validatePassword('MyStr0ng!Pass');
    expect(result.isValid).toBe(true);
    expect(result.strength).toBe(PasswordStrength.STRONG);
    expect(result.missingRequirements).toHaveLength(0);
  });

  it('returns weak for short password', () => {
    const result = validatePassword('ab');
    expect(result.isValid).toBe(false);
    expect(result.strength).toBe(PasswordStrength.WEAK);
  });

  it('lists all missing requirements for empty password', () => {
    const result = validatePassword('');
    expect(result.missingRequirements.length).toBeGreaterThanOrEqual(4);
  });

  it('returns fair for password meeting some requirements', () => {
    const result = validatePassword('Abcdefgh');
    expect(result.strength).toBe(PasswordStrength.FAIR);
  });

  it('returns good when 4 out of 5 requirements met', () => {
    const result = validatePassword('Abcdefg1');
    expect(result.strength).toBe(PasswordStrength.GOOD);
  });
});

describe('validateName', () => {
  it('returns true for valid name', () => {
    expect(validateName('John Doe')).toBe(true);
  });

  it('returns false for single character', () => {
    expect(validateName('A')).toBe(false);
  });

  it('returns false for empty string', () => {
    expect(validateName('')).toBe(false);
  });

  it('trims whitespace before checking', () => {
    expect(validateName('  A  ')).toBe(false);
  });
});
