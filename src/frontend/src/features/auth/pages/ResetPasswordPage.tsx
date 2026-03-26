import { useState, useEffect, type FormEvent, type ChangeEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';

interface FormData {
  newPassword: string;
  confirmPassword: string;
}

interface FormErrors {
  newPassword?: string;
  confirmPassword?: string;
}

/**
 * Reset Password page component.
 * Allows users to set a new password using reset token from email.
 * Implements WCAG 2.2 AA accessibility standards.
 */
export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');

  const [formData, setFormData] = useState<FormData>({
    newPassword: '',
    confirmPassword: '',
  });
  const [errors, setErrors] = useState<FormErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  // Check if token exists
  useEffect(() => {
    if (!token) {
      setErrorMessage('Invalid password reset link. Please request a new one.');
    }
  }, [token]);

  // Handle input change
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    setFormData((prev) => ({ ...prev, [id]: value }));

    // Clear error when user starts typing
    if (errors[id as keyof FormErrors]) {
      setErrors((prev) => ({ ...prev, [id]: undefined }));
    }
  };

  // Handle input blur
  const handleBlur = (field: keyof FormData) => {
    setTouched((prev) => ({ ...prev, [field]: true }));
    validateField(field);
  };

  // Validate individual field
  const validateField = (field: keyof FormData): boolean => {
    const newErrors: FormErrors = { ...errors };

    if (field === 'newPassword') {
      if (!formData.newPassword) {
        newErrors.newPassword = 'Password is required';
      } else if (formData.newPassword.length < 8) {
        newErrors.newPassword = 'Password must be at least 8 characters';
      } else if (
        !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(
          formData.newPassword
        )
      ) {
        newErrors.newPassword =
          'Password must contain uppercase, lowercase, digit, and special character';
      } else {
        delete newErrors.newPassword;
      }
    }

    if (field === 'confirmPassword') {
      if (!formData.confirmPassword) {
        newErrors.confirmPassword = 'Please confirm your password';
      } else if (formData.confirmPassword !== formData.newPassword) {
        newErrors.confirmPassword = 'Passwords do not match';
      } else {
        delete newErrors.confirmPassword;
      }
    }

    setErrors(newErrors);
    return !newErrors[field];
  };

  // Validate all fields
  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.newPassword) {
      newErrors.newPassword = 'Password is required';
    } else if (formData.newPassword.length < 8) {
      newErrors.newPassword = 'Password must be at least 8 characters';
    } else if (
      !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(
        formData.newPassword
      )
    ) {
      newErrors.newPassword =
        'Password must contain uppercase, lowercase, digit, and special character';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password';
    } else if (formData.confirmPassword !== formData.newPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!token) {
      setErrorMessage('Invalid password reset link.');
      return;
    }

    // Mark all fields as touched
    setTouched({ newPassword: true, confirmPassword: true });

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      const apiUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
      const response = await fetch(`${apiUrl}/api/auth/reset-password`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          token,
          newPassword: formData.newPassword,
          confirmPassword: formData.confirmPassword,
        }),
      });

      if (response.ok) {
        setIsSuccess(true);
        // Redirect to login after 3 seconds
        setTimeout(() => {
          navigate('/login');
        }, 3000);
      } else {
        const errorData = await response.json();
        setErrorMessage(errorData.message || 'Failed to reset password.');
      }
    } catch (error) {
      console.error('Reset password error:', error);
      setErrorMessage('An error occurred. Please try again later.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isSuccess) {
    return (
      <div className="min-h-screen bg-neutral-50 flex items-center justify-center p-4">
        <main id="main-content" className="w-full max-w-md" role="main">
          {/* Logo */}
          <div className="flex items-center justify-center gap-2 mb-8">
            <div
              className="w-8 h-8 bg-primary-500 rounded-md flex items-center justify-center
                text-white font-bold text-lg"
              aria-hidden="true"
            >
              +
            </div>
            <span className="text-2xl font-semibold text-neutral-900">PatientAccess</span>
          </div>

          {/* Success Card */}
          <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-8">
            <div className="text-center">
              {/* Success Icon */}
              <div className="mx-auto w-12 h-12 bg-success-100 rounded-full flex items-center justify-center mb-4">
                <svg
                  className="w-6 h-6 text-success-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M5 13l4 4L19 7"
                  />
                </svg>
              </div>

              <h1 className="text-2xl font-semibold text-neutral-900 mb-2">
                Password Reset Successful!
              </h1>
              <p className="text-neutral-600 mb-6">
                Your password has been reset successfully. You can now log in with your new
                password.
              </p>

              <p className="text-sm text-neutral-500 mb-4">
                Redirecting to login page...
              </p>

              <Link
                to="/login"
                className="inline-block w-full px-4 py-2 bg-primary-500 text-white
                  rounded-md hover:bg-primary-600 focus:outline-none focus:ring-2
                  focus:ring-primary-500 focus:ring-offset-2 font-medium transition-colors"
              >
                Go to Login
              </Link>
            </div>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-neutral-50 flex items-center justify-center p-4">
      {/* Skip link for accessibility */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:top-0 focus:left-0
          focus:bg-primary-500 focus:text-white focus:px-4 focus:py-2 focus:z-50
          focus:rounded-br-md"
      >
        Skip to main content
      </a>

      <main id="main-content" className="w-full max-w-md" role="main">
        {/* Logo */}
        <div className="flex items-center justify-center gap-2 mb-8">
          <div
            className="w-8 h-8 bg-primary-500 rounded-md flex items-center justify-center
              text-white font-bold text-lg"
            aria-hidden="true"
          >
            +
          </div>
          <span className="text-2xl font-semibold text-neutral-900">PatientAccess</span>
        </div>

        {/* Reset Password Card */}
        <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-8">
          {/* Header */}
          <div className="mb-6">
            <h1 className="text-3xl font-semibold text-neutral-900 mb-1">
              Reset Password
            </h1>
            <p className="text-sm text-neutral-500">
              Enter your new password below.
            </p>
          </div>

          {/* Error Alert */}
          {errorMessage && (
            <div
              className="mb-4 p-3 bg-danger-50 border border-danger-200 rounded-md"
              role="alert"
            >
              <p className="text-sm text-danger-700">{errorMessage}</p>
            </div>
          )}

          {/* Reset Password Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* New Password Field */}
            <div>
              <label
                htmlFor="newPassword"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                New Password
              </label>
              <div className="relative">
                <input
                  id="newPassword"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  value={formData.newPassword}
                  onChange={handleChange}
                  onBlur={() => handleBlur('newPassword')}
                  className={`w-full px-3 py-2 pr-10 border rounded-md focus:outline-none
                    focus:ring-2 focus:ring-primary-500 transition-colors ${
                      touched.newPassword && errors.newPassword
                        ? 'border-danger-500 focus:ring-danger-500'
                        : 'border-neutral-300'
                    }`}
                  aria-invalid={touched.newPassword && errors.newPassword ? 'true' : 'false'}
                  aria-describedby={
                    touched.newPassword && errors.newPassword
                      ? 'password-error'
                      : 'password-hint'
                  }
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 text-neutral-500
                    hover:text-neutral-700 focus:outline-none focus:ring-2
                    focus:ring-primary-500 rounded p-1"
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
                      />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                      />
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                      />
                    </svg>
                  )}
                </button>
              </div>
              {touched.newPassword && errors.newPassword && (
                <p id="password-error" className="mt-1 text-sm text-danger-600" role="alert">
                  {errors.newPassword}
                </p>
              )}
              <p id="password-hint" className="mt-1 text-xs text-neutral-500">
                Must be at least 8 characters with uppercase, lowercase, digit, and special
                character.
              </p>
            </div>

            {/* Confirm Password Field */}
            <div>
              <label
                htmlFor="confirmPassword"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                Confirm New Password
              </label>
              <div className="relative">
                <input
                  id="confirmPassword"
                  type={showConfirmPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  onBlur={() => handleBlur('confirmPassword')}
                  className={`w-full px-3 py-2 pr-10 border rounded-md focus:outline-none
                    focus:ring-2 focus:ring-primary-500 transition-colors ${
                      touched.confirmPassword && errors.confirmPassword
                        ? 'border-danger-500 focus:ring-danger-500'
                        : 'border-neutral-300'
                    }`}
                  aria-invalid={
                    touched.confirmPassword && errors.confirmPassword ? 'true' : 'false'
                  }
                  aria-describedby={
                    touched.confirmPassword && errors.confirmPassword
                      ? 'confirm-password-error'
                      : undefined
                  }
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute right-2 top-1/2 -translate-y-1/2 text-neutral-500
                    hover:text-neutral-700 focus:outline-none focus:ring-2
                    focus:ring-primary-500 rounded p-1"
                  aria-label={showConfirmPassword ? 'Hide password' : 'Show password'}
                >
                  {showConfirmPassword ? (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
                      />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                      />
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                      />
                    </svg>
                  )}
                </button>
              </div>
              {touched.confirmPassword && errors.confirmPassword && (
                <p
                  id="confirm-password-error"
                  className="mt-1 text-sm text-danger-600"
                  role="alert"
                >
                  {errors.confirmPassword}
                </p>
              )}
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting || !token}
              className="w-full px-4 py-2 bg-primary-500 text-white rounded-md
                hover:bg-primary-600 focus:outline-none focus:ring-2
                focus:ring-primary-500 focus:ring-offset-2 font-medium
                transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSubmitting ? 'Resetting Password...' : 'Reset Password'}
            </button>
          </form>

          {/* Back to Login Link */}
          <div className="mt-6 text-center">
            <Link
              to="/login"
              className="text-sm text-primary-500 hover:text-primary-700 hover:underline
                font-medium focus:outline-none focus:ring-2 focus:ring-primary-500
                focus:ring-offset-2 rounded inline-flex items-center gap-1"
            >
              <svg
                className="w-4 h-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M10 19l-7-7m0 0l7-7m-7 7h18"
                />
              </svg>
              Back to Login
            </Link>
          </div>
        </div>
      </main>
    </div>
  );
}
