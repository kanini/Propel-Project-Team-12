import { useState, type FormEvent, type ChangeEvent } from 'react';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { registerUser, selectAuthLoading, selectAuthError } from '../authSlice';
import PasswordStrengthIndicator from '../../../components/forms/PasswordStrengthIndicator';
import {
  validateEmail,
  validatePhone,
  validateDateOfBirth,
  validatePassword,
  validateName,
} from '../../../utils/validators';

interface FormData {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  phone: string;
  password: string;
}

interface FormErrors {
  firstName?: string;
  lastName?: string;
  dateOfBirth?: string;
  email?: string;
  phone?: string;
  password?: string;
}

/**
 * Registration form component (FR-001).
 * Implements patient account creation with inline validation (UXR-601).
 * Follows wireframe SCR-001 design specifications.
 */
export default function RegistrationForm() {
  const dispatch = useAppDispatch();
  const isLoading = useAppSelector(selectAuthLoading);
  const apiError = useAppSelector(selectAuthError);

  const [formData, setFormData] = useState<FormData>({
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    email: '',
    phone: '',
    password: '',
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});
  const [showPassword, setShowPassword] = useState(false);

  // Handle input change
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    setFormData((prev) => ({ ...prev, [id]: value }));

    // Clear error when user starts typing
    if (errors[id as keyof FormErrors]) {
      setErrors((prev) => ({ ...prev, [id]: undefined }));
    }
  };

  // Handle input blur (for inline validation)
  const handleBlur = (field: keyof FormData) => {
    setTouched((prev) => ({ ...prev, [field]: true }));
    validateField(field, formData[field]);
  };

  // Validate individual field
  const validateField = (field: keyof FormData, value: string): boolean => {
    let error: string | undefined;

    switch (field) {
      case 'firstName':
      case 'lastName':
        if (!value.trim()) {
          error = `${field === 'firstName' ? 'First' : 'Last'} name is required`;
        } else if (!validateName(value)) {
          error = 'Name must be between 2 and 200 characters';
        }
        break;

      case 'dateOfBirth':
        if (!value) {
          error = 'Date of birth is required';
        } else if (!validateDateOfBirth(value)) {
          error = 'Invalid date. Future dates are not allowed.';
        }
        break;

      case 'email':
        if (!value) {
          error = 'Email address is required';
        } else if (!validateEmail(value)) {
          error = 'Please enter a valid email address';
        }
        break;

      case 'phone':
        if (!value) {
          error = 'Phone number is required';
        } else if (!validatePhone(value)) {
          error = 'Please enter a valid phone number (e.g., (555) 123-4567)';
        }
        break;

      case 'password':
        const passwordValidation = validatePassword(value);
        if (!value) {
          error = 'Password is required';
        } else if (!passwordValidation.isValid) {
          error = 'Password does not meet requirements';
        }
        break;
    }

    if (error) {
      setErrors((prev) => ({ ...prev, [field]: error }));
      return false;
    } else {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
      return true;
    }
  };

  // Validate all fields
  const validateForm = (): boolean => {
    const fields: (keyof FormData)[] = [
      'firstName',
      'lastName',
      'dateOfBirth',
      'email',
      'phone',
      'password',
    ];

    let isValid = true;
    fields.forEach((field) => {
      const fieldValid = validateField(field, formData[field]);
      if (!fieldValid) {
        isValid = false;
      }
    });

    // Mark all fields as touched to show errors
    const allTouched: Record<string, boolean> = {};
    fields.forEach((field) => {
      allTouched[field] = true;
    });
    setTouched(allTouched);

    return isValid;
  };

  // Handle form submission
  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    // Validate form
    if (!validateForm()) {
      return;
    }

    // Combine first and last name
    const fullName = `${formData.firstName.trim()} ${formData.lastName.trim()}`;

    // Dispatch registration action (FR-001, AC1)
    await dispatch(
      registerUser({
        name: fullName,
        email: formData.email,
        dateOfBirth: formData.dateOfBirth,
        phone: formData.phone,
        password: formData.password,
      })
    );
  };

  return (
    <form
      onSubmit={handleSubmit}
      aria-labelledby="reg-heading"
      noValidate
      className="space-y-4"
    >
      {/* API Error Alert */}
      {apiError && (
        <div
          className="p-3 bg-error-light border border-error rounded-md flex items-start gap-2"
          role="alert"
        >
          <span className="text-error text-sm flex-shrink-0" aria-hidden="true">
            ⚠
          </span>
          <span className="text-sm text-error-dark">{apiError}</span>
        </div>
      )}

      {/* First Name & Last Name Row */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* First Name */}
        <div>
          <label
            htmlFor="firstName"
            className="block text-sm font-medium text-neutral-700 mb-1"
          >
            First name <span className="text-error" aria-label="required">*</span>
          </label>
          <input
            type="text"
            id="firstName"
            value={formData.firstName}
            onChange={handleChange}
            onBlur={() => handleBlur('firstName')}
            placeholder="John"
            autoComplete="given-name"
            required
            className={`w-full h-10 px-3 border rounded-md text-sm transition-colors
              ${
                touched.firstName && errors.firstName
                  ? 'border-error focus:ring-2 focus:ring-error-light'
                  : 'border-neutral-300 hover:border-neutral-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-100'
              }
              focus:outline-none`}
          />
          {touched.firstName && errors.firstName && (
            <p className="text-xs text-error mt-1" role="alert">
              {errors.firstName}
            </p>
          )}
        </div>

        {/* Last Name */}
        <div>
          <label
            htmlFor="lastName"
            className="block text-sm font-medium text-neutral-700 mb-1"
          >
            Last name <span className="text-error" aria-label="required">*</span>
          </label>
          <input
            type="text"
            id="lastName"
            value={formData.lastName}
            onChange={handleChange}
            onBlur={() => handleBlur('lastName')}
            placeholder="Doe"
            autoComplete="family-name"
            required
            className={`w-full h-10 px-3 border rounded-md text-sm transition-colors
              ${
                touched.lastName && errors.lastName
                  ? 'border-error focus:ring-2 focus:ring-error-light'
                  : 'border-neutral-300 hover:border-neutral-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-100'
              }
              focus:outline-none`}
          />
          {touched.lastName && errors.lastName && (
            <p className="text-xs text-error mt-1" role="alert">
              {errors.lastName}
            </p>
          )}
        </div>
      </div>

      {/* Date of Birth */}
      <div>
        <label
          htmlFor="dateOfBirth"
          className="block text-sm font-medium text-neutral-700 mb-1"
        >
          Date of birth <span className="text-error" aria-label="required">*</span>
        </label>
        <input
          type="date"
          id="dateOfBirth"
          value={formData.dateOfBirth}
          onChange={handleChange}
          onBlur={() => handleBlur('dateOfBirth')}
          autoComplete="bday"
          required
          max={new Date().toISOString().split('T')[0]} // Prevent future dates in date picker
          className={`w-full h-10 px-3 border rounded-md text-sm transition-colors
            ${
              touched.dateOfBirth && errors.dateOfBirth
                ? 'border-error focus:ring-2 focus:ring-error-light'
                : 'border-neutral-300 hover:border-neutral-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-100'
            }
            focus:outline-none`}
        />
        {touched.dateOfBirth && errors.dateOfBirth && (
          <p className="text-xs text-error mt-1" role="alert">
            {errors.dateOfBirth}
          </p>
        )}
      </div>

      {/* Email */}
      <div>
        <label
          htmlFor="email"
          className="block text-sm font-medium text-neutral-700 mb-1"
        >
          Email address <span className="text-error" aria-label="required">*</span>
        </label>
        <input
          type="email"
          id="email"
          value={formData.email}
          onChange={handleChange}
          onBlur={() => handleBlur('email')}
          placeholder="john.doe@email.com"
          autoComplete="email"
          required
          className={`w-full h-10 px-3 border rounded-md text-sm transition-colors
            ${
              touched.email && errors.email
                ? 'border-error focus:ring-2 focus:ring-error-light'
                : 'border-neutral-300 hover:border-neutral-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-100'
            }
            focus:outline-none`}
        />
        {touched.email && errors.email && (
          <p className="text-xs text-error mt-1" role="alert">
            {errors.email}
          </p>
        )}
      </div>

      {/* Phone */}
      <div>
        <label
          htmlFor="phone"
          className="block text-sm font-medium text-neutral-700 mb-1"
        >
          Phone number <span className="text-error" aria-label="required">*</span>
        </label>
        <input
          type="tel"
          id="phone"
          value={formData.phone}
          onChange={handleChange}
          onBlur={() => handleBlur('phone')}
          placeholder="(555) 123-4567"
          autoComplete="tel"
          required
          className={`w-full h-10 px-3 border rounded-md text-sm transition-colors
            ${
              touched.phone && errors.phone
                ? 'border-error focus:ring-2 focus:ring-error-light'
                : 'border-neutral-300 hover:border-neutral-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-100'
            }
            focus:outline-none`}
        />
        {touched.phone && errors.phone && (
          <p className="text-xs text-error mt-1" role="alert">
            {errors.phone}
          </p>
        )}
      </div>

      {/* Password */}
      <div>
        <label
          htmlFor="password"
          className="block text-sm font-medium text-neutral-700 mb-1"
        >
          Password <span className="text-error" aria-label="required">*</span>
        </label>
        <div className="relative">
          <input
            type={showPassword ? 'text' : 'password'}
            id="password"
            value={formData.password}
            onChange={handleChange}
            onBlur={() => handleBlur('password')}
            placeholder="Minimum 8 characters"
            autoComplete="new-password"
            required
            aria-describedby="pw-strength-text"
            className={`w-full h-10 px-3 pr-10 border rounded-md text-sm transition-colors
              ${
                touched.password && errors.password
                  ? 'border-error focus:ring-2 focus:ring-error-light'
                  : 'border-neutral-300 hover:border-neutral-400 focus:border-primary-500 focus:ring-2 focus:ring-primary-100'
              }
              focus:outline-none`}
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-neutral-400 hover:text-neutral-600 transition-colors"
            aria-label={showPassword ? 'Hide password' : 'Show password'}
          >
            <span className="text-base">{showPassword ? '👁️' : '👁'}</span>
          </button>
        </div>
        {touched.password && errors.password && (
          <p className="text-xs text-error mt-1" role="alert">
            {errors.password}
          </p>
        )}
        <PasswordStrengthIndicator password={formData.password} />
      </div>

      {/* Submit Button */}
      <button
        type="submit"
        disabled={isLoading}
        className="w-full h-10 bg-primary-500 text-white font-medium rounded-md
          hover:bg-primary-600 active:bg-primary-700 disabled:bg-neutral-200
          disabled:text-neutral-400 disabled:cursor-not-allowed transition-colors
          flex items-center justify-center gap-2"
      >
        {isLoading ? (
          <>
            <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            Creating account...
          </>
        ) : (
          'Create account'
        )}
      </button>
    </form>
  );
}
