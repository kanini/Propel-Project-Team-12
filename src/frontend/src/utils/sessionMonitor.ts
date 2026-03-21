/**
 * Session monitoring utility (UXR-604 - Session Timeout Warning)
 * Tracks session age and determines when to show timeout warning modal
 */

import { getLoginTime } from './tokenStorage';

/**
 * Session timeout duration in milliseconds (15 minutes)
 */
export const SESSION_TIMEOUT_MS = 15 * 60 * 1000;

/**
 * Warning threshold in milliseconds (13 minutes - 2 minutes before timeout)
 */
export const WARNING_THRESHOLD_MS = 13 * 60 * 1000;

/**
 * Warning duration in milliseconds (2 minutes)
 */
export const WARNING_DURATION_MS = 2 * 60 * 1000;

/**
 * Calculates the age of the current session in milliseconds
 * @returns Session age in milliseconds, or 0 if no login time found
 */
export const getSessionAge = (): number => {
  const loginTime = getLoginTime();
  if (!loginTime) return 0;
  
  return Date.now() - loginTime;
};

/**
 * Determines if session timeout warning should be shown
 * Warning appears at 13-minute mark (2 minutes before 15-minute expiry)
 * @returns True if session age >= 13 minutes, false otherwise
 */
export const shouldShowWarning = (): boolean => {
  const sessionAge = getSessionAge();
  return sessionAge >= WARNING_THRESHOLD_MS && sessionAge < SESSION_TIMEOUT_MS;
};

/**
 * Determines if session has expired (age >= 15 minutes)
 * @returns True if session expired, false otherwise
 */
export const isSessionExpired = (): boolean => {
  const sessionAge = getSessionAge();
  return sessionAge >= SESSION_TIMEOUT_MS;
};

/**
 * Calculates remaining time before session expires
 * @returns Remaining time in milliseconds, or 0 if expired
 */
export const getTimeRemaining = (): number => {
  const sessionAge = getSessionAge();
  const remaining = SESSION_TIMEOUT_MS - sessionAge;
  return Math.max(0, remaining);
};

/**
 * Calculates remaining time in warning period (countdown timer value)
 * Used for displaying countdown in session timeout modal
 * @returns Remaining warning time in milliseconds, or 0 if warning period over
 */
export const getWarningTimeRemaining = (): number => {
  const sessionAge = getSessionAge();
  if (sessionAge < WARNING_THRESHOLD_MS) return WARNING_DURATION_MS;
  if (sessionAge >= SESSION_TIMEOUT_MS) return 0;
  
  const timeSinceWarningStart = sessionAge - WARNING_THRESHOLD_MS;
  const remaining = WARNING_DURATION_MS - timeSinceWarningStart;
  return Math.max(0, remaining);
};

/**
 * Formats milliseconds to MM:SS format for countdown display
 * @param ms - Milliseconds to format
 * @returns Formatted time string (e.g., "02:00", "00:45")
 */
export const formatCountdown = (ms: number): string => {
  const totalSeconds = Math.ceil(ms / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  
  return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
};
