import { useState, useEffect, useCallback, useRef } from 'react';
import { useAuth } from './useAuth';

interface UseSessionTimeoutReturn {
  showWarning: boolean;
  secondsRemaining: number;
  extendSession: () => void;
  dismissWarning: () => void;
}

const WARNING_TIME = 13 * 60 * 1000; // 13 minutes in milliseconds
const SESSION_DURATION = 15 * 60 * 1000; // 15 minutes in milliseconds
const STORAGE_KEY = 'lastActivityTime';

/**
 * Custom hook for session timeout management (US_022, AC4, AC5).
 * Tracks user activity, displays warning at 13-minute mark, handles session extension.
 */
export const useSessionTimeout = (): UseSessionTimeoutReturn => {
  const { isAuthenticated } = useAuth();
  const [showWarning, setShowWarning] = useState(false);
  const [secondsRemaining, setSecondsRemaining] = useState(0);
  const warningTimerRef = useRef<number | null>(null);
  const countdownTimerRef = useRef<number | null>(null);

  // Get last activity time from localStorage
  const getLastActivityTime = useCallback((): number => {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? parseInt(stored, 10) : Date.now();
  }, []);

  // Update last activity time in localStorage
  const updateLastActivityTime = useCallback(() => {
    const now = Date.now();
    localStorage.setItem(STORAGE_KEY, now.toString());
    return now;
  }, []);

  // Clear all timers
  const clearTimers = useCallback(() => {
    if (warningTimerRef.current) {
      clearTimeout(warningTimerRef.current);
      warningTimerRef.current = null;
    }
    if (countdownTimerRef.current) {
      clearInterval(countdownTimerRef.current);
      countdownTimerRef.current = null;
    }
  }, []);

  // Start countdown timer
  const startCountdown = useCallback((lastActivity: number) => {
    const updateCountdown = () => {
      const elapsed = Date.now() - lastActivity;
      const remaining = Math.max(0, SESSION_DURATION - elapsed);
      const seconds = Math.ceil(remaining / 1000);
      
      setSecondsRemaining(seconds);

      if (seconds <= 0) {
        // Session expired - trigger logout (handled by parent component)
        setShowWarning(false);
        clearTimers();
      }
    };

    // Initial update
    updateCountdown();

    // Update every second
    countdownTimerRef.current = setInterval(updateCountdown, 1000);
  }, [clearTimers]);

  // Start warning timer
  const startWarningTimer = useCallback(() => {
    clearTimers();
    
    const lastActivity = getLastActivityTime();
    const elapsed = Date.now() - lastActivity;
    const timeUntilWarning = Math.max(0, WARNING_TIME - elapsed);

    if (timeUntilWarning === 0) {
      // Warning should already be shown
      setShowWarning(true);
      startCountdown(lastActivity);
    } else {
      // Schedule warning
      warningTimerRef.current = setTimeout(() => {
        setShowWarning(true);
        startCountdown(getLastActivityTime());
      }, timeUntilWarning);
    }
  }, [clearTimers, getLastActivityTime, startCountdown]);

  // Handle user activity (AC5 - reset activity timer)
  const handleActivity = useCallback(() => {
    if (!isAuthenticated) return;

    updateLastActivityTime();

    // If warning is showing, hide it and restart timer
    if (showWarning) {
      setShowWarning(false);
    }

    startWarningTimer();
  }, [isAuthenticated, showWarning, updateLastActivityTime, startWarningTimer]);

  // Extend session (AC5)
  const extendSession = useCallback(() => {
    updateLastActivityTime();
    setShowWarning(false);
    startWarningTimer();
  }, [updateLastActivityTime, startWarningTimer]);

  // Dismiss warning without extending (for testing)
  const dismissWarning = useCallback(() => {
    setShowWarning(false);
  }, []);

  // Set up activity listeners
  useEffect(() => {
    if (!isAuthenticated) {
      clearTimers();
      setShowWarning(false);
      return;
    }

    // Initialize activity tracking
    const lastActivity = getLastActivityTime();
    const now = Date.now();
    const elapsed = now - lastActivity;

    // If session is still valid, start tracking
    if (elapsed < SESSION_DURATION) {
      startWarningTimer();
    } else {
      // Session already expired
      setShowWarning(false);
    }

    // Activity event handlers
    const events = ['mousemove', 'keydown', 'scroll', 'click', 'touchstart'];
    
    // Throttle activity updates to avoid excessive writes
    let activityTimeout: number | null = null;
    const throttledActivity = () => {
      if (activityTimeout) return;
      
      activityTimeout = setTimeout(() => {
        handleActivity();
        activityTimeout = null;
      }, 1000); // Throttle to once per second
    };

    events.forEach((event) => {
      window.addEventListener(event, throttledActivity);
    });

    // Listen for storage events (cross-tab sync)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === STORAGE_KEY && e.newValue) {
        // Another tab updated activity - restart our timer
        startWarningTimer();
        if (showWarning) {
          setShowWarning(false);
        }
      }
    };

    window.addEventListener('storage', handleStorageChange);

    // Cleanup
    return () => {
      clearTimers();
      events.forEach((event) => {
        window.removeEventListener(event, throttledActivity);
      });
      window.removeEventListener('storage', handleStorageChange);
      if (activityTimeout) {
        clearTimeout(activityTimeout);
      }
    };
  }, [isAuthenticated, clearTimers, getLastActivityTime, startWarningTimer, handleActivity, showWarning]);

  return {
    showWarning,
    secondsRemaining,
    extendSession,
    dismissWarning,
  };
};
