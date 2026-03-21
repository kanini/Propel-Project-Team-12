/**
 * useSessionTimeout hook (UXR-604 - Session Timeout Warning)
 * Monitors session age and manages timeout warning state
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../store/hooks';
import { logout } from '../store/slices/authSlice';
import { shouldShowWarning, isSessionExpired, getWarningTimeRemaining } from '../utils/sessionMonitor';

/**
 * Session timeout monitoring interval in milliseconds (10 seconds)
 */
const MONITOR_INTERVAL_MS = 10 * 1000;

/**
 * Hook for monitoring session timeout and managing warning modal
 * @returns Object containing showWarning state, countdownTime, and dismissWarning function
 */
export const useSessionTimeout = () => {
  const [showWarning, setShowWarning] = useState(false);
  const [countdownTime, setCountdownTime] = useState(0);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  useEffect(() => {
    // Check session status immediately on mount
    const checkSessionStatus = () => {
      // Check if session expired
      if (isSessionExpired()) {
        // Clear auth and redirect to login with expiry message
        dispatch(logout());
        navigate('/login', { 
          state: { message: 'Your session has expired. Please log in again.' } 
        });
        return;
      }

      // Check if warning should be shown
      if (shouldShowWarning()) {
        setShowWarning(true);
        setCountdownTime(getWarningTimeRemaining());
      } else {
        setShowWarning(false);
      }
    };

    // Initial check
    checkSessionStatus();

    // Set up interval to check every 10 seconds
    const intervalId = setInterval(checkSessionStatus, MONITOR_INTERVAL_MS);

    // Cleanup interval on unmount
    return () => clearInterval(intervalId);
  }, [dispatch, navigate]);

  // Update countdown timer every second when warning is shown
  useEffect(() => {
    if (!showWarning) return;

    const countdownIntervalId = setInterval(() => {
      const remaining = getWarningTimeRemaining();
      setCountdownTime(remaining);

      // If countdown reaches 0, trigger logout
      if (remaining === 0) {
        dispatch(logout());
        navigate('/login', { 
          state: { message: 'Your session has expired. Please log in again.' } 
        });
      }
    }, 1000);

    return () => clearInterval(countdownIntervalId);
  }, [showWarning, dispatch, navigate]);

  // Listen for cross-tab session extension events
  useEffect(() => {
    const handleStorageEvent = (event: StorageEvent) => {
      // Check if session was extended in another tab
      if (event.key === 'session_extended') {
        // Dismiss warning in this tab
        setShowWarning(false);
        setCountdownTime(0);
      }
    };

    window.addEventListener('storage', handleStorageEvent);

    return () => window.removeEventListener('storage', handleStorageEvent);
  }, []);

  /**
   * Dismisses the warning modal (called when "Extend Session" succeeds)
   */
  const dismissWarning = () => {
    setShowWarning(false);
    setCountdownTime(0);
  };

  return {
    showWarning,
    countdownTime,
    dismissWarning,
  };
};
