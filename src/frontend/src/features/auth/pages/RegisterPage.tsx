import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../../../store/hooks';
import { selectRegistrationSuccess, resetRegistrationSuccess } from '../authSlice';
import RegistrationForm from '../components/RegistrationForm';

/**
 * Registration page component (FR-001, SCR-001).
 * Patient account creation entry point following wireframe design.
 * Implements WCAG 2.2 AA accessibility standards (UXR-201, UXR-203).
 */
export default function RegisterPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const registrationSuccess = useAppSelector(selectRegistrationSuccess);

  // Redirect to verification message on successful registration
  useEffect(() => {
    if (registrationSuccess) {
      // Show success message for 3 seconds, then redirect to login
      const timer = setTimeout(() => {
        dispatch(resetRegistrationSuccess());
        navigate('/login');
      }, 5000);

      return () => clearTimeout(timer);
    }
  }, [registrationSuccess, navigate, dispatch]);

  return (
    <div className="min-h-screen bg-neutral-50 flex items-center justify-center p-4">
      {/* Skip link for accessibility (UXR-201) */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:top-0 focus:left-0
          focus:bg-primary-500 focus:text-white focus:px-4 focus:py-2 focus:z-50
          focus:rounded-br-md"
      >
        Skip to main content
      </a>

      <main id="main-content" className="w-full max-w-lg" role="main">
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

        {/* Registration Card */}
        <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-8">
          {/* Success Message (AC1: verification email sent) */}
          {registrationSuccess ? (
            <div className="space-y-4">
              <div className="p-4 bg-success-light border border-success rounded-md">
                <div className="flex items-start gap-2">
                  <span className="text-success text-lg" aria-hidden="true">
                    ✓
                  </span>
                  <div>
                    <p className="font-medium text-success-dark">
                      Registration successful!
                    </p>
                    <p className="text-sm text-success-dark mt-1">
                      Please check your email to verify your account. You'll be redirected to
                      the login page shortly.
                    </p>
                  </div>
                </div>
              </div>
              <Link
                to="/login"
                className="block text-center text-sm text-primary-500 hover:text-primary-700 hover:underline"
              >
                Go to login now
              </Link>
            </div>
          ) : (
            <>
              {/* Header */}
              <div className="mb-6">
                <h1 className="text-3xl font-semibold text-neutral-900 mb-1">
                  Create your account
                </h1>
                <p className="text-sm text-neutral-500">
                  Join PatientAccess to manage your healthcare
                </p>
              </div>

              {/* Registration Form */}
              <RegistrationForm />

              {/* Login Link */}
              <div className="mt-4 text-center">
                <p className="text-sm text-neutral-500">
                  Already have an account?{' '}
                  <Link
                    to="/login"
                    className="text-primary-500 hover:text-primary-700 hover:underline
                      font-medium focus:outline-none focus:ring-2 focus:ring-primary-500
                      focus:ring-offset-2 rounded"
                  >
                    Sign in
                  </Link>
                </p>
              </div>
            </>
          )}
        </div>
      </main>
    </div>
  );
}
