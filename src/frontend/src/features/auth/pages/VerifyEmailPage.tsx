import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Logo } from '../../../components/common/Logo';

type VerificationStatus = 'loading' | 'success' | 'error';

/**
 * Email verification page component (FR-001, AC2).
 * Automatically verifies email using token from URL query parameter.
 * Activates account by calling GET /api/auth/verify?token=...
 */
export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  // Initialize state based on token presence
  const [status, setStatus] = useState<VerificationStatus>(() => 
    !token ? 'error' : 'loading'
  );
  const [message, setMessage] = useState(() => 
    !token ? 'Invalid verification link. No token was provided.' : ''
  );

  useEffect(() => {
    // Only run verification if token exists
    if (!token) {
      return;
    }

    const verifyEmail = async () => {
      try {
        const apiUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
        const response = await fetch(
          `${apiUrl}/api/auth/verify?token=${encodeURIComponent(token)}`
        );

        const data = await response.json();

        if (response.ok) {
          setStatus('success');
          setMessage(data.message || 'Email verified successfully. You can now log in.');
        } else {
          setStatus('error');
          setMessage(data.message || 'Verification failed. The link may be invalid or expired.');
        }
      } catch {
        setStatus('error');
        setMessage('An error occurred while verifying your email. Please try again later.');
      }
    };

    verifyEmail();
  }, [token]);

  return (
    <div className="min-h-screen bg-neutral-50 flex items-center justify-center p-4">
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
          <Logo size="md" />
          <span className="text-2xl font-semibold text-neutral-900">CareSync AI</span>
        </div>

        {/* Verification Card */}
        <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-8">
          <div className="text-center">
            {status === 'loading' && (
              <>
                {/* Loading Spinner */}
                <div className="mx-auto w-12 h-12 border-4 border-primary-200 border-t-primary-500 rounded-full animate-spin mb-4" role="status">
                  <span className="sr-only">Verifying your email...</span>
                </div>
                <h1 className="text-2xl font-semibold text-neutral-900 mb-2">
                  Verifying Your Email
                </h1>
                <p className="text-neutral-600">
                  Please wait while we verify your email address...
                </p>
              </>
            )}

            {status === 'success' && (
              <>
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
                  Email Verified!
                </h1>
                <p className="text-neutral-600 mb-6">{message}</p>

                <Link
                  to="/login"
                  className="inline-block w-full px-4 py-2 bg-primary-500 text-white
                    rounded-md hover:bg-primary-600 focus:outline-none focus:ring-2
                    focus:ring-primary-500 focus:ring-offset-2 font-medium transition-colors"
                >
                  Go to Login
                </Link>
              </>
            )}

            {status === 'error' && (
              <>
                {/* Error Icon */}
                <div className="mx-auto w-12 h-12 bg-danger-50 rounded-full flex items-center justify-center mb-4">
                  <svg
                    className="w-6 h-6 text-danger-600"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    aria-hidden="true"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </div>

                <h1 className="text-2xl font-semibold text-neutral-900 mb-2">
                  Verification Failed
                </h1>
                <p className="text-neutral-600 mb-6">{message}</p>

                <div className="space-y-3">
                  <Link
                    to="/login"
                    className="inline-block w-full px-4 py-2 bg-primary-500 text-white
                      rounded-md hover:bg-primary-600 focus:outline-none focus:ring-2
                      focus:ring-primary-500 focus:ring-offset-2 font-medium transition-colors"
                  >
                    Go to Login
                  </Link>
                  <Link
                    to="/register"
                    className="inline-block w-full px-4 py-2 border border-neutral-300
                      text-neutral-700 rounded-md hover:bg-neutral-50 focus:outline-none
                      focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 font-medium
                      transition-colors"
                  >
                    Create New Account
                  </Link>
                </div>
              </>
            )}
          </div>
        </div>
      </main>
    </div>
  );
}
