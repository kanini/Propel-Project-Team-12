/**
 * Registration Form Component (FR-001 - User Registration)
 * Main registration form with inline validation and accessibility features
 * Matches wireframe SCR-001
 */

import React, { useState } from 'react';
import type { FormEvent, ChangeEvent } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { registerUser, clearRegistrationError } from '../../store/slices/authSlice';
import { PasswordStrengthIndicator } from './PasswordStrengthIndicator';
import type { RegistrationFormData } from '../../types/auth.types';

/**
 * Registration form component with:
 * - Inline validation (blur events)
 * - Real-time password strength indicator
 * - WCAG 2.2 AA accessibility
 * - Rate limiting feedback
 */
export const RegistrationForm: React.FC = () => {
  const dispatch = useAppDispatch();
  const { isLoading, isSuccess, error } = useAppSelector((state) => state.auth.registration);

  // Form state - matches wireframe field structure
  const [formData, setFormData] = useState<RegistrationFormData>({
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    email: '',
    phone: '',
    password: '',
  });

  // Field-level errors for inline validation
  const [fieldErrors, setFieldErrors] = useState<Partial<Record<keyof RegistrationFormData, string>>>({});

  // Password visibility toggle
  const [showPassword, setShowPassword] = useState(false);

  /**
   * Handle input change
   */
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    // Clear field error when user starts typing
    if (fieldErrors[name as keyof RegistrationFormData]) {
      setFieldErrors((prev) => ({
        ...prev,
        [name]: undefined,
      }));
    }

    // Clear global error when user modifies form
    if (error) {
      dispatch(clearRegistrationError());
    }
  };

  /**
   * Handle field blur for inline validation
   */
  const handleBlur = (e: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    const fieldName = name as keyof RegistrationFormData;

    // Basic validation on blur
    let errorMessage = '';
    
    if (fieldName === 'firstName' && !value.trim()) {
      errorMessage = 'First name is required';
    } else if (fieldName === 'lastName' && !value.trim()) {
      errorMessage = 'Last name is required';
    } else if (fieldName === 'email' && !value.match(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)) {
      errorMessage = 'Please enter a valid email address';
    } else if (fieldName === 'phone' && !value.trim()) {
      errorMessage = 'Phone number is required';
    } else if (fieldName === 'dateOfBirth' && !value) {
      errorMessage = 'Date of birth is required';
    } else if (fieldName === 'password' && value.length < 8) {
      errorMessage = 'Password must be at least 8 characters';
    }

    if (errorMessage) {
      setFieldErrors((prev) => ({
        ...prev,
        [fieldName]: errorMessage,
      }));
    }
  };

  /**
   * Handle form submission
   */
  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Validate all fields
    const errors: Partial<Record<keyof RegistrationFormData, string>> = {};
    
    if (!formData.firstName.trim()) errors.firstName = 'First name is required';
    if (!formData.lastName.trim()) errors.lastName = 'Last name is required';
    if (!formData.email.match(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)) errors.email = 'Please enter a valid email address';
    if (!formData.phone.trim()) errors.phone = 'Phone number is required';
    if (!formData.dateOfBirth) errors.dateOfBirth = 'Date of birth is required';
    if (formData.password.length < 8) errors.password = 'Password must be at least 8 characters';

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    // Combine first and last name for API
    const registrationData = {
      name: `${formData.firstName} ${formData.lastName}`.trim(),
      email: formData.email,
      password: formData.password,
      dateOfBirth: formData.dateOfBirth,
      phone: formData.phone,
    };

    await dispatch(registerUser(registrationData));
  };

  // Show success message after registration
  if (isSuccess) {
    return (
      <div className="p-4 bg-green-50 border border-green-300 rounded-lg flex items-start gap-2">
        <span className="text-green-600 flex-shrink-0" aria-hidden="true">✓</span>
        <div>
          <h2 className="text-lg font-semibold text-green-800 mb-2">Registration Successful!</h2>
          <p className="text-sm text-green-700 mb-2">
            We've sent a verification email to <strong>{formData.email}</strong>. Please check your inbox and click the verification link to activate your account.
          </p>
          <p className="text-xs text-green-600">
            The verification link will expire in 24 hours.
          </p>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="w-full" noValidate>
      <h1 className="text-2xl font-semibold text-gray-900 mb-1">Create your account</h1>
      <p className="text-sm text-gray-500 mb-6">Join PatientAccess to manage your healthcare</p>

      {/* Global error message */}
      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-300 rounded-lg flex items-start gap-2 text-sm text-red-800" role="alert">
          <span aria-hidden="true">⚠</span>
          <span>{error}</span>
        </div>
      )}

      {/* First Name and Last Name - Grid Row */}
      <div className="grid grid-cols-2 gap-4 mb-4">
        <div>
          <label htmlFor="firstName" className="block text-sm font-medium text-gray-700 mb-1">
            First name <span className="text-red-600" aria-label="required">*</span>
          </label>
          <input
            type="text"
            id="firstName"
            name="firstName"
            value={formData.firstName}
            onChange={handleChange}
            onBlur={handleBlur}
            placeholder="John"
            className={`w-full h-10 px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-3 transition-colors ${
              fieldErrors.firstName ? 'border-red-500 focus:ring-red-100' : 'border-gray-300 hover:border-gray-400 focus:border-blue-500 focus:ring-blue-100'
            }`}
            aria-required="true"
            aria-invalid={!!fieldErrors.firstName}
            aria-describedby={fieldErrors.firstName ? 'firstName-error' : undefined}
            disabled={isLoading}
            autoComplete="given-name"
          />
          {fieldErrors.firstName && (
            <p id="firstName-error" className="mt-1 text-xs text-red-600" role="alert">
              {fieldErrors.firstName}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="lastName" className="block text-sm font-medium text-gray-700 mb-1">
            Last name <span className="text-red-600" aria-label="required">*</span>
          </label>
          <input
            type="text"
            id="lastName"
            name="lastName"
            value={formData.lastName}
            onChange={handleChange}
            onBlur={handleBlur}
            placeholder="Doe"
            className={`w-full h-10 px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-3 transition-colors ${
              fieldErrors.lastName ? 'border-red-500 focus:ring-red-100' : 'border-gray-300 hover:border-gray-400 focus:border-blue-500 focus:ring-blue-100'
            }`}
            aria-required="true"
            aria-invalid={!!fieldErrors.lastName}
            aria-describedby={fieldErrors.lastName ? 'lastName-error' : undefined}
            disabled={isLoading}
            autoComplete="family-name"
          />
          {fieldErrors.lastName && (
            <p id="lastName-error" className="mt-1 text-xs text-red-600" role="alert">
              {fieldErrors.lastName}
            </p>
          )}
        </div>
      </div>

      {/* Date of Birth field */}
      <div className="mb-4">
        <label htmlFor="dateOfBirth" className="block text-sm font-medium text-gray-700 mb-1">
          Date of birth <span className="text-red-600" aria-label="required">*</span>
        </label>
        <input
          type="date"
          id="dateOfBirth"
          name="dateOfBirth"
          value={formData.dateOfBirth}
          onChange={handleChange}
          onBlur={handleBlur}
          max={new Date().toISOString().split('T')[0]}
          className={`w-full h-10 px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-3 transition-colors ${
            fieldErrors.dateOfBirth ? 'border-red-500 focus:ring-red-100' : 'border-gray-300 hover:border-gray-400 focus:border-blue-500 focus:ring-blue-100'
          }`}
          aria-required="true"
          aria-invalid={!!fieldErrors.dateOfBirth}
          aria-describedby={fieldErrors.dateOfBirth ? 'dateOfBirth-error' : undefined}
          disabled={isLoading}
          autoComplete="bday"
        />
        {fieldErrors.dateOfBirth && (
          <p id="dateOfBirth-error" className="mt-1 text-xs text-red-600" role="alert">
            {fieldErrors.dateOfBirth}
          </p>
        )}
      </div>

      {/* Email field */}
      <div className="mb-4">
        <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
          Email address <span className="text-red-600" aria-label="required">*</span>
        </label>
        <input
          type="email"
          id="email"
          name="email"
          value={formData.email}
          onChange={handleChange}
          onBlur={handleBlur}
          placeholder="john.doe@email.com"
          className={`w-full h-10 px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-3 transition-colors ${
            fieldErrors.email ? 'border-red-500 focus:ring-red-100' : 'border-gray-300 hover:border-gray-400 focus:border-blue-500 focus:ring-blue-100'
          }`}
          aria-required="true"
          aria-invalid={!!fieldErrors.email}
          aria-describedby={fieldErrors.email ? 'email-error' : undefined}
          disabled={isLoading}
          autoComplete="email"
        />
        {fieldErrors.email && (
          <p id="email-error" className="mt-1 text-xs text-red-600" role="alert">
            {fieldErrors.email}
          </p>
        )}
      </div>

      {/* Phone field - REQUIRED per wireframe */}
      <div className="mb-4">
        <label htmlFor="phone" className="block text-sm font-medium text-gray-700 mb-1">
          Phone number <span className="text-red-600" aria-label="required">*</span>
        </label>
        <input
          type="tel"
          id="phone"
          name="phone"
          value={formData.phone}
          onChange={handleChange}
          onBlur={handleBlur}
          placeholder="(555) 123-4567"
          className={`w-full h-10 px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-3 transition-colors ${
            fieldErrors.phone ? 'border-red-500 focus:ring-red-100' : 'border-gray-300 hover:border-gray-400 focus:border-blue-500 focus:ring-blue-100'
          }`}
          aria-required="true"
          aria-invalid={!!fieldErrors.phone}
          aria-describedby={fieldErrors.phone ? 'phone-error' : undefined}
          disabled={isLoading}
          autoComplete="tel"
        />
        {fieldErrors.phone && (
          <p id="phone-error" className="mt-1 text-xs text-red-600" role="alert">
            {fieldErrors.phone}
          </p>
        )}
      </div>

      {/* Password field */}
      <div className="mb-6">
        <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
          Password <span className="text-red-600" aria-label="required">*</span>
        </label>
        <div className="relative">
          <input
            type={showPassword ? 'text' : 'password'}
            id="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            onBlur={handleBlur}
            placeholder="Minimum 8 characters"
            className={`w-full h-10 px-3 py-2 pr-10 border rounded-lg text-sm focus:outline-none focus:ring-3 transition-colors ${
              fieldErrors.password ? 'border-red-500 focus:ring-red-100' : 'border-gray-300 hover:border-gray-400 focus:border-blue-500 focus:ring-blue-100'
            }`}
            aria-required="true"
            aria-invalid={!!fieldErrors.password}
            aria-describedby={fieldErrors.password ? 'password-error password-strength' : 'password-strength'}
            disabled={isLoading}
            autoComplete="new-password"
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
            aria-label={showPassword ? 'Hide password' : 'Show password'}
          >
            👁
          </button>
        </div>
        {fieldErrors.password && (
          <p id="password-error" className="mt-1 text-xs text-red-600" role="alert">
            {fieldErrors.password}
          </p>
        )}
        <PasswordStrengthIndicator password={formData.password} />
      </div>

      {/* Submit button */}
      <button
        type="submit"
        disabled={isLoading}
        className={`w-full h-11 px-4 rounded-lg font-medium text-white text-sm transition-colors flex items-center justify-center ${
          isLoading
            ? 'bg-gray-400 cursor-not-allowed'
            : 'bg-blue-500 hover:bg-blue-600 focus:outline-none focus:ring-3 focus:ring-blue-100'
        }`}
        aria-busy={isLoading}
      >
        {isLoading ? (
          <>
            <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></span>
            Creating account...
          </>
        ) : (
          'Create account'
        )}
      </button>

      <p className="mt-4 text-sm text-center text-gray-600">
        Already have an account?{' '}
        <a href="/login" className="text-blue-500 hover:text-blue-600 font-medium">
          Sign in
        </a>
      </p>
    </form>
  );
};
