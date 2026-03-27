import { useState, type FormEvent, type ChangeEvent } from 'react';
import { Link } from 'react-router-dom';
import { validateEmail } from '../../../utils/validators';

interface FormData {
  email: string;
}

interface FormErrors {
  email?: string;
}

/**
 * Forgot Password page component.
 * Allows users to initiate password reset workflow by entering their email.
 * Implements WCAG 2.2 AA accessibility standards.
 */
export default function ForgotPasswordPage() {
  const [formData, setFormData] = useState<FormData>({ email: '' });
  const [errors, setErrors] = useState<FormErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

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

    if (field === 'email') {
      if (!formData.email) {
        newErrors.email = 'Email is required';
      } else if (!validateEmail(formData.email)) {
        newErrors.email = 'Invalid email address';
      } else {
        delete newErrors.email;
      }
    }

    setErrors(newErrors);
    return !newErrors[field];
  };

  // Validate all fields
  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.email) {
      newErrors.email = 'Email is required';
    } else if (!validateEmail(formData.email)) {
      newErrors.email = 'Invalid email address';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Mark all fields as touched
    setTouched({ email: true });

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      const apiUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
      const response = await fetch(`${apiUrl}/api/auth/forgot-password`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      });

      if (response.ok) {
        setIsSuccess(true);
      } else {
        const errorData = await response.json();
        setErrorMessage(errorData.message || 'Failed to process password reset request.');
      }
    } catch (error) {
      console.error('Forgot password error:', error);
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
            <span className="text-2xl font-semibold text-neutral-900">CareSync AI</span>
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
                Check Your Email
              </h1>
              <p className="text-neutral-600 mb-6">
                If an account exists with this email, a password reset link will be sent.
                Check your inbox and follow the instructions to reset your password.
              </p>

              <div className="bg-warning-50 border border-warning-200 rounded-md p-4 mb-6 text-left">
                <p className="text-sm text-warning-800">
                  <strong>Note:</strong> The reset link will expire in 1 hour. If you don't
                  receive the email, please check your spam folder.
                </p>
              </div>

              <Link
                to="/login"
                className="inline-block w-full px-4 py-2 bg-primary-500 text-white
                  rounded-md hover:bg-primary-600 focus:outline-none focus:ring-2
                  focus:ring-primary-500 focus:ring-offset-2 font-medium transition-colors"
              >
                Return to Login
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
          <span className="text-2xl font-semibold text-neutral-900">CareSync AI</span>
        </div>

        {/* Forgot Password Card */}
        <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-8">
          {/* Header */}
          <div className="mb-6">
            <h1 className="text-3xl font-semibold text-neutral-900 mb-1">
              Forgot Password?
            </h1>
            <p className="text-sm text-neutral-500">
              Enter your email address and we'll send you a link to reset your password.
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

          {/* Forgot Password Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Email Field */}
            <div>
              <label
                htmlFor="email"
                className="block text-sm font-medium text-neutral-700 mb-1"
              >
                Email Address
              </label>
              <input
                id="email"
                type="email"
                autoComplete="email"
                value={formData.email}
                onChange={handleChange}
                onBlur={() => handleBlur('email')}
                className={`w-full px-3 py-2 border rounded-md focus:outline-none
                  focus:ring-2 focus:ring-primary-500 transition-colors ${
                    touched.email && errors.email
                      ? 'border-danger-500 focus:ring-danger-500'
                      : 'border-neutral-300'
                  }`}
                aria-invalid={touched.email && errors.email ? 'true' : 'false'}
                aria-describedby={touched.email && errors.email ? 'email-error' : undefined}
              />
              {touched.email && errors.email && (
                <p id="email-error" className="mt-1 text-sm text-danger-600" role="alert">
                  {errors.email}
                </p>
              )}
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full px-4 py-2 bg-primary-500 text-white rounded-md
                hover:bg-primary-600 focus:outline-none focus:ring-2
                focus:ring-primary-500 focus:ring-offset-2 font-medium
                transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSubmitting ? 'Sending...' : 'Send Reset Link'}
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
