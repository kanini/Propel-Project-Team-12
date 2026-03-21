/**
 * Token storage utilities (FR-002 - User Authentication)
 * Manages JWT token persistence in sessionStorage for 15-minute session timeout
 */

const TOKEN_KEY = 'auth_token';
const USER_KEY = 'auth_user';
const LOGIN_TIME_KEY = 'login_time';

/**
 * Stores authentication token in sessionStorage
 * @param token - JWT authentication token
 */
export const setToken = (token: string): void => {
  sessionStorage.setItem(TOKEN_KEY, token);
};

/**
 * Retrieves authentication token from sessionStorage
 * @returns JWT token if present, null otherwise
 */
export const getToken = (): string | null => {
  return sessionStorage.getItem(TOKEN_KEY);
};

/**
 * Removes authentication token from sessionStorage
 * Used during logout or session expiration
 */
export const removeToken = (): void => {
  sessionStorage.removeItem(TOKEN_KEY);
};

/**
 * Stores user data in sessionStorage
 * @param userId - User ID
 * @param name - User name
 * @param role - User role
 */
export const setUser = (userId: string, name: string, role: string): void => {
  const userData = { userId, name, role };
  sessionStorage.setItem(USER_KEY, JSON.stringify(userData));
};

/**
 * Retrieves user data from sessionStorage
 * @returns User data if present, null otherwise
 */
export const getUser = (): { userId: string; name: string; role: string } | null => {
  const userData = sessionStorage.getItem(USER_KEY);
  if (!userData) return null;
  
  try {
    return JSON.parse(userData);
  } catch {
    return null;
  }
};

/**
 * Removes user data from sessionStorage
 */
export const removeUser = (): void => {
  sessionStorage.removeItem(USER_KEY);
};

/**
 * Checks if user is authenticated
 * @returns True if token exists, false otherwise
 */
export const isAuthenticated = (): boolean => {
  return getToken() !== null;
};

/**
 * Stores login timestamp in sessionStorage (UXR-604 - Session timeout tracking)
 * Used to calculate session age and trigger timeout warning
 * @param timestamp - Login timestamp in milliseconds (Date.now())
 */
export const setLoginTime = (timestamp: number): void => {
  sessionStorage.setItem(LOGIN_TIME_KEY, timestamp.toString());
};

/**
 * Retrieves login timestamp from sessionStorage
 * @returns Login timestamp in milliseconds, or null if not set
 */
export const getLoginTime = (): number | null => {
  const loginTime = sessionStorage.getItem(LOGIN_TIME_KEY);
  if (!loginTime) return null;
  
  const parsed = parseInt(loginTime, 10);
  return isNaN(parsed) ? null : parsed;
};

/**
 * Clears all authentication data from sessionStorage
 * Used during logout
 */
export const clearAuth = (): void => {
  removeToken();
  removeUser();
  sessionStorage.removeItem(LOGIN_TIME_KEY);
};

/**
 * Decodes JWT token to extract payload (without verification)
 * Note: This is for client-side parsing only. Token verification happens server-side.
 * @param token - JWT token
 * @returns Decoded payload or null if invalid
 */
export const decodeToken = (token: string): Record<string, unknown> | null => {
  try {
    const parts = token.split('.');
    if (parts.length !== 3 || !parts[1]) return null;
    
    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
};

/**
 * Extracts role from JWT token
 * @param token - JWT token
 * @returns User role or null
 */
export const getRoleFromToken = (token: string): string | null => {
  const payload = decodeToken(token);
  if (!payload) return null;
  
  // JWT role claim can be in different formats
  return (payload.role as string) || (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as string) || null;
};

/**
 * Gets the current user's role from stored JWT token (NFR-006 RBAC)
 * @returns User role (Patient, Staff, or Admin) or null if not authenticated
 */
export function getUserRole(): string | null {
  const token = getToken();
  if (!token) return null;
  return getRoleFromToken(token);
}

/**
 * Gets the current user's ID from stored JWT token
 * @returns User ID (sub claim) or null if not authenticated
 */
export function getUserId(): string | null {
  const token = getToken();
  if (!token) return null;

  const decoded = decodeToken(token);
  if (!decoded) return null;

  // JWT sub claim contains the user ID
  return (decoded['sub'] as string) || (decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] as string) || null;
}

/**
 * Checks if the stored JWT token is expired
 * @returns True if token is expired or missing, false if still valid
 */
export function isTokenExpired(): boolean {
  const token = getToken();
  if (!token) return true;

  const decoded = decodeToken(token);
  if (!decoded || !decoded.exp) return true;

  // exp is in seconds, Date.now() is in milliseconds
  return (decoded.exp as number) * 1000 < Date.now();
}

