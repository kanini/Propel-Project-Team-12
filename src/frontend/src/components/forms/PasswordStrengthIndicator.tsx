import { useMemo } from 'react';
import { validatePassword, PasswordStrength } from '../../utils/validators';

interface PasswordStrengthIndicatorProps {
  password: string;
  showRequirements?: boolean;
}

/**
 * Password strength indicator component (FR-001, AC4).
 * Displays visual feedback and missing requirements for password validation.
 * Implements real-time inline validation per UXR-601.
 */
export default function PasswordStrengthIndicator({
  password,
  showRequirements = true,
}: PasswordStrengthIndicatorProps) {
  // Validate password and get strength
  const validation = useMemo(() => validatePassword(password), [password]);

  // Only show indicator if password has content
  if (!password) {
    return null;
  }

  // Determine strength class and color
  const getStrengthColor = () => {
    switch (validation.strength) {
      case PasswordStrength.WEAK:
        return 'bg-error';
      case PasswordStrength.FAIR:
        return 'bg-warning';
      case PasswordStrength.GOOD:
        return 'bg-info';
      case PasswordStrength.STRONG:
        return 'bg-success';
      default:
        return 'bg-neutral-200';
    }
  };

  // Determine width percentage based on score
  const getWidthClass = () => {
    const widthMap: Record<number, string> = {
      0: 'w-0',
      1: 'w-1/4',
      2: 'w-2/4',
      3: 'w-3/4',
      4: 'w-full',
    };
    return widthMap[validation.score] || 'w-0';
  };

  // Get strength label
  const getStrengthLabel = () => {
    if (validation.strength === PasswordStrength.STRONG) {
      return 'Strong password';
    }
    if (validation.strength === PasswordStrength.GOOD) {
      return 'Good — Add a special character for a stronger password';
    }
    if (validation.strength === PasswordStrength.FAIR) {
      return 'Fair — Add more requirements for stronger password';
    }
    return 'Weak password';
  };

  return (
    <div className="mt-2">
      {/* Strength bar */}
      <div
        className="h-1 w-full bg-neutral-200 rounded-full overflow-hidden"
        role="progressbar"
        aria-valuenow={validation.score * 25}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label="Password strength"
      >
        <div
          className={`h-full rounded-full transition-all duration-300 ${getWidthClass()} ${getStrengthColor()}`}
        />
      </div>

      {/* Strength label */}
      <p className="text-xs text-neutral-500 mt-1" id="pw-strength-text">
        {getStrengthLabel()}
      </p>

      {/* Missing requirements (AC4: Inline validation displays specific missing password requirements) */}
      {showRequirements && validation.missingRequirements.length > 0 && (
        <div className="mt-2 text-xs text-error" role="alert">
          <p className="font-medium mb-1">Missing requirements:</p>
          <ul className="space-y-0.5 list-disc list-inside">
            {validation.missingRequirements.map((requirement, index) => (
              <li key={index}>{requirement}</li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
