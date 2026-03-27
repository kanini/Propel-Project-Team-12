import { Link } from 'react-router-dom';
import LoginForm from '../components/LoginForm';

/**
 * Login page component (FR-002, SCR-002).
 * User authentication entry point following wireframe design.
 * Implements WCAG 2.2 AA accessibility standards (UXR-201, UXR-203).
 */
export default function LoginPage() {
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

        {/* Login Card */}
        <div className="bg-white border border-neutral-200 rounded-lg shadow-sm p-8">
          {/* Header */}
          <div className="mb-6">
            <h1 className="text-3xl font-semibold text-neutral-900 mb-1">Sign in</h1>
            <p className="text-sm text-neutral-500">Welcome back to CareSync AI</p>
          </div>

          {/* Login Form */}
          <LoginForm />

          {/* Registration Link */}
          <div className="mt-4 text-center">
            <p className="text-sm text-neutral-500">
              Don't have an account?{' '}
              <Link
                to="/register"
                className="text-primary-500 hover:text-primary-700 hover:underline
                  font-medium focus:outline-none focus:ring-2 focus:ring-primary-500
                  focus:ring-offset-2 rounded"
              >
                Create account
              </Link>
            </p>
          </div>
        </div>
      </main>
    </div>
  );
}
