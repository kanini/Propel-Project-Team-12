import { useState, type FormEvent, type ChangeEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { loginUser, selectAuthLoading, selectAuthError } from '../authSlice';
import { validateEmail } from '../../../utils/validators';

interface FormData {
  email: string;
  password: string;
}

interface FormErrors {
  email?: string;
  password?: string;
}

/**
 * Login form component (FR-002).
 * Implements user authentication with inline validation (UXR-601).
 * Follows wireframe SCR-002 design specifications.
 */
export default function LoginForm() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const isLoading = useAppSelector(selectAuthLoading);
  const apiError = useAppSelector(selectAuthError);

  const [formData, setFormData] = useState<FormData>({
    email: '',
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
      case 'email':
        if (!value) {
          error = 'Email address is required';
        } else if (!validateEmail(value)) {
          error = 'Please enter a valid email address';
        }
        break;

      case 'password':
        if (!value) {
          error = 'Password is required';
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
    const fields: (keyof FormData)[] = ['email', 'password'];

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

    // Dispatch login action (FR-002, AC1)
    const result = await dispatch(
      loginUser({
        email: formData.email,
        password: formData.password,
      })
    );

    // Handle successful login with role-based redirect (AC1)
    if (loginUser.fulfilled.match(result)) {
      const userRole = result.payload.role;

      // Role-based redirect
      switch (userRole.toLowerCase()) {
        case 'admin':
          navigate('/admin/dashboard', { replace: true });
          break;
        case 'staff':
          navigate('/staff/dashboard', { replace: true });
          break;
        case 'patient':
        default:
          navigate('/dashboard', { replace: true });
          break;
      }
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      aria-label="Login form"
      noValidate
      className="space-y-4"
    >
      {/* API Error Alert (AC3: Generic error message) */}
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
            placeholder="Enter your password"
            autoComplete="current-password"
            required
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
      </div>

      {/* Forgot Password Link */}
      <div className="flex justify-end">
        <button
          type="button"
          onClick={() => navigate('/forgot-password')}
          className="text-sm text-primary-500 hover:text-primary-700 hover:underline
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 rounded"
          aria-label="Reset your password"
        >
          Forgot password?
        </button>
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
            Signing in...
          </>
        ) : (
          'Sign in'
        )}
      </button>
    </form>
  );
}
