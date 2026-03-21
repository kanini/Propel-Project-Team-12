/**
 * Password Strength Indicator Component (FR-001 AC3)
 * Visual feedback for password strength with color-coded indicator
 */

import React from 'react';
import { getPasswordStrength } from '../../utils/validators/passwordValidator';

export interface PasswordStrengthIndicatorProps {
  password: string;
  className?: string;
}

/**
 * Password strength indicator with color-coded visual feedback
 * - Red (0-40): Weak
 * - Yellow (41-70): Medium  
 * - Green (71-100): Strong
 */
export const PasswordStrengthIndicator: React.FC<PasswordStrengthIndicatorProps> = ({
  password,
  className = '',
}) => {
  const strength = getPasswordStrength(password);

  // Determine color and label based on strength score
  let strengthColor = 'bg-gray-200';
  let strengthLabel = '';
  let strengthText = '';

  if (strength === 0) {
    strengthColor = 'bg-gray-200';
    strengthLabel = '';
    strengthText = '';
  } else if (strength <= 40) {
    strengthColor = 'bg-red-500';
    strengthLabel = 'Weak';
    strengthText = 'text-red-600';
  } else if (strength <= 70) {
    strengthColor = 'bg-yellow-500';
    strengthLabel = 'Medium';
    strengthText = 'text-yellow-600';
  } else {
    strengthColor = 'bg-green-500';
    strengthLabel = 'Strong';
    strengthText = 'text-green-600';
  }

  // Don't show indicator if password is empty
  if (!password) {
    return null;
  }

  return (
    <div className={`mt-2 ${className}`}>
      {/* Strength bar */}
      <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden" role="progressbar" aria-valuenow={strength} aria-valuemin={0} aria-valuemax={100} aria-label="Password strength">
        <div
          className={`h-full transition-all duration-300 ${strengthColor}`}
          style={{ width: `${strength}%` }}
        />
      </div>

      {/* Strength label */}
      {strengthLabel && (
        <p className={`mt-1 text-sm font-medium ${strengthText}`} aria-live="polite">
          Password strength: {strengthLabel}
        </p>
      )}
    </div>
  );
};
