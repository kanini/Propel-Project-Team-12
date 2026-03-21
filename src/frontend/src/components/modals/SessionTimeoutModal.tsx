/**
 * SessionTimeoutModal component (UXR-604 - Session Timeout Warning)
 * Displays warning modal 2 minutes before session expiry with countdown timer
 */

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../../store/hooks';
import { logout } from '../../store/slices/authSlice';
import { formatCountdown } from '../../utils/sessionMonitor';
import { setLoginTime } from '../../utils/tokenStorage';

interface SessionTimeoutModalProps {
  /** Whether modal is visible */
  isOpen: boolean;
  /** Countdown time in milliseconds */
  countdownMs: number;
  /** Callback when session is extended successfully */
  onExtend: () => void;
}

/**
 * Modal that warns users of impending session timeout
 * Appears at 13-minute mark with 2-minute countdown
 * Cannot be dismissed by clicking outside (force user choice)
 */
export const SessionTimeoutModal = ({ isOpen, countdownMs, onExtend }: SessionTimeoutModalProps) => {
  const [isExtending, setIsExtending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  // ARIA live region update for accessibility
  const [announcement, setAnnouncement] = useState('');

  useEffect(() => {
    if (!isOpen) return;

    // Announce countdown updates every 30 seconds
    const totalSeconds = Math.ceil(countdownMs / 1000);
    if (totalSeconds % 30 === 0 && totalSeconds > 0) {
      const minutes = Math.floor(totalSeconds / 60);
      const seconds = totalSeconds % 60;
      setAnnouncement(`Session timeout in ${minutes} minute${minutes !== 1 ? 's' : ''} and ${seconds} second${seconds !== 1 ? 's' : ''}`);
    }
  }, [isOpen, countdownMs]);

  // Reset error when modal closes
  useEffect(() => {
    if (!isOpen) {
      setError(null);
      setIsExtending(false);
    }
  }, [isOpen]);

  const handleExtendSession = async () => {
    setIsExtending(true);
    setError(null);

    try {
      const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';
      const token = sessionStorage.getItem('auth_token');

      if (!token) {
        throw new Error('No authentication token found');
      }

      const response = await fetch(`${API_BASE_URL}/api/auth/refresh-session`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error('Session not found. Please log in again.');
        }
        throw new Error('Failed to extend session');
      }

      // Update login time to reset session age
      setLoginTime(Date.now());

      // Sync across tabs
      localStorage.setItem('session_extended', Date.now().toString());

      // Call parent callback to dismiss modal
      onExtend();

      // Show success message
      setAnnouncement('Session extended successfully');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to extend session';
      setError(errorMessage);
      setAnnouncement(errorMessage);
    } finally {
      setIsExtending(false);
    }
  };

  const handleLogoutNow = () => {
    dispatch(logout());
    navigate('/login', { 
      state: { message: 'You have been logged out.' } 
    });
  };

  if (!isOpen) return null;

  const countdownDisplay = formatCountdown(countdownMs);

  return (
    <>
      {/* Modal overlay */}
      <div 
        className="fixed inset-0 bg-black bg-opacity-50 z-50"
        aria-hidden="true"
      />

      {/* Modal content */}
      <div 
        className="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
        aria-labelledby="session-timeout-title"
        aria-describedby="session-timeout-description"
      >
        <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
          {/* Warning icon */}
          <div className="flex items-center justify-center w-12 h-12 mx-auto mb-4 rounded-full bg-yellow-100">
            <svg 
              className="w-6 h-6 text-yellow-600" 
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path 
                strokeLinecap="round" 
                strokeLinejoin="round" 
                strokeWidth={2} 
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" 
              />
            </svg>
          </div>

          {/* Title */}
          <h2 
            id="session-timeout-title" 
            className="text-xl font-semibold text-center text-gray-900 mb-2"
          >
            Session Timeout Warning
          </h2>

          {/* Description */}
          <p 
            id="session-timeout-description" 
            className="text-center text-gray-600 mb-4"
          >
            Your session will expire in:
          </p>

          {/* Countdown timer */}
          <div className="text-center mb-6">
            <div className="text-5xl font-bold text-yellow-600 font-mono">
              {countdownDisplay}
            </div>
            <p className="text-sm text-gray-500 mt-2">
              Please extend your session or you will be logged out
            </p>
          </div>

          {/* Error message */}
          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          )}

          {/* Action buttons */}
          <div className="flex gap-3">
            <button
              type="button"
              onClick={handleExtendSession}
              disabled={isExtending}
              className="flex-1 px-4 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:bg-blue-300 disabled:cursor-not-allowed transition-colors"
            >
              {isExtending ? 'Extending...' : 'Extend Session'}
            </button>
            <button
              type="button"
              onClick={handleLogoutNow}
              disabled={isExtending}
              className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-md hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 disabled:bg-gray-100 disabled:cursor-not-allowed transition-colors"
            >
              Logout Now
            </button>
          </div>

          {/* ARIA live region for screen readers */}
          <div 
            className="sr-only" 
            role="status" 
            aria-live="polite" 
            aria-atomic="true"
          >
            {announcement}
          </div>
        </div>
      </div>
    </>
  );
};
