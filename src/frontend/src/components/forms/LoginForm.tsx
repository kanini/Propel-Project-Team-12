/**
 * Login Form Component (FR-002 - User Authentication)
 * Handles user authentication with email/password
 * Matches wireframe SCR-002 design specifications
 */

import React, { useState, useEffect } from 'react';
import type { FormEvent, ChangeEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { loginUser, clearLoginError } from '../../store/slices/authSlice';
import { getRedirectPath } from '../../utils/roleBasedRedirect';

/**
 * Login form component with:
 * - Email/password authentication
 * - Generic error messages (OWASP Email Enumeration Prevention)
 * - Account lockout display
 * - Role-based redirection
 * - WCAG 2.2 AA accessibility
 */
export const LoginForm: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isLoading, isAuthenticated, error, role } = useAppSelector((state) => state.auth.login);

  // Form state
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);  // Field-level errors for inline validation
  const [emailError, setEmailError] = useState<string | null>(null);

  // Redirect on successful login
  useEffect(() => {
    if (isAuthenticated && role) {
      const redirectPath = getRedirectPath(role);
      navigate(redirectPath);
    }
  }, [isAuthenticated, role, navigate]);

  /**
   * Handle email change
   */
  const handleEmailChange = (e: ChangeEvent<HTMLInputElement>) => {
    setEmail(e.target.value);
    if (emailError) setEmailError(null);
    if (error) dispatch(clearLoginError());
  };

  /**
   * Handle password change
   */
  const handlePasswordChange = (e: ChangeEvent<HTMLInputElement>) => {
    setPassword(e.target.value);
    if (error) dispatch(clearLoginError());
  };

  /**
   * Validate email format
   */
  const validateEmail = (email: string): boolean => {
    if (!email) {
      setEmailError('Email is required');
      return false;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      setEmailError('Please enter a valid email address');
      return false;
    }
    return true;
  };

  /**
   * Handle form submission
   */
  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Validate email
    if (!validateEmail(email)) {
      return;
    }

    // Validate password
    if (!password) {
      return; // Button should be disabled if password is empty
    }

    // Submit login
    await dispatch(loginUser({ email, password }));
  };

  return (
    <form onSubmit={handleSubmit} className="w-full" noValidate>
      <h1 className="text-2xl font-semibold text-gray-900 mb-1">Sign in</h1>
      <p className="text-sm text-gray-500 mb-6">Welcome back to PatientAccess</p>

      {/* Global error message */}
      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-300 rounded-lg flex items-start gap-2 text-sm text-red-800" role="alert">
          <span aria-hidden="true">⚠</span>
          <span>{error}</span>
        </div>
      )}

      {/* Email field */}
      <div className="mb-4">
        <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
          Email address <span className="text-red-600" aria-label="required">*</span>
        </label>
        <input
          type="email"
          id="email"
          name="email"
          value={email}
          onChange={handleEmailChange}
          onBlur={() => validateEmail(email)}
          placeholder="john.doe@email.com"
          className={`w-full h-10 px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-3 transition-colors ${
            emailError ? 'border-red-500 focus:ring-red-100' : 'border-gray-300 hover:border-gray-400 focus:border-blue-500 focus:ring-blue-100'
          }`}
          aria-required="true"
          aria-invalid={!!emailError}
          aria-describedby={emailError ? 'email-error' : undefined}
          disabled={isLoading}
          autoComplete="email"
        />
        {emailError && (
          <p id="email-error" className="mt-1 text-xs text-red-600" role="alert">
            {emailError}
          </p>
        )}
      </div>

      {/* Password field */}
      <div className="mb-4">
        <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
          Password <span className="text-red-600" aria-label="required">*</span>
        </label>
        <div className="relative">
          <input
            type={showPassword ? 'text' : 'password'}
            id="password"
            name="password"
            value={password}
            onChange={handlePasswordChange}
            placeholder="Enter your password"
            className="w-full h-10 px-3 py-2 pr-10 border border-gray-300 rounded-lg text-sm hover:border-gray-400 focus:outline-none focus:border-blue-500 focus:ring-3 focus:ring-blue-100 transition-colors"
            aria-required="true"
            disabled={isLoading}
            autoComplete="current-password"
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 p-1"
            aria-label={showPassword ? 'Hide password' : 'Show password'}
          >
            {showPassword ? '👁' : '👁'}
          </button>
        </div>
      </div>

      {/* Forgot password link */}
      <div className="mb-4 text-right">
        <a
          href="/forgot-password"
          className="text-sm text-blue-500 hover:text-blue-700 hover:underline"
        >
          Forgot password?
        </a>
      </div>

      {/* Submit button */}
      <button
        type="submit"
        disabled={isLoading || !email || !password}
        className={`w-full h-10 px-5 rounded-lg text-sm font-medium text-white transition-all ${
          isLoading || !email || !password
            ? 'bg-gray-200 text-gray-400 cursor-not-allowed'
            : 'bg-blue-500 hover:bg-blue-600 active:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2'
        }`}
        aria-busy={isLoading}
      >
        {isLoading ? (
          <span className="flex items-center justify-center gap-2">
            <span className="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            Signing in...
          </span>
        ) : (
          'Sign in'
        )}
      </button>

      <p className="mt-4 text-sm text-center text-gray-500">
        Don't have an account?{' '}
        <a href="/register" className="text-blue-500 hover:text-blue-700 hover:underline">
          Create account
        </a>
      </p>
    </form>
  );
};
