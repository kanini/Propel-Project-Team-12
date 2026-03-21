/**
 * Axios Interceptor Configuration (NFR-006 RBAC, NFR-007 Audit Trail)
 * 
 * Configures axios to:
 * - Add JWT token to all API requests
 * - Handle 401 Unauthorized (redirect to login)
 * - Handle 403 Forbidden (redirect to role-appropriate dashboard)
 */

import axios from 'axios';
import type { AxiosResponse, InternalAxiosRequestConfig, AxiosError } from 'axios';
import { getToken, getUserRole } from './tokenStorage';
import { getRedirectPath } from './navigationConfig';

/**
 * Setup axios interceptors for authentication and authorization
 * Should be called once at application startup (in App.tsx or main.tsx)
 */
export function setupAxiosInterceptors(): void {
  // Response interceptor for handling 401 and 403 errors
  axios.interceptors.response.use(
    (response: AxiosResponse) => response,
    (error: AxiosError) => {
      if (error.response) {
        const { status } = error.response;

        // 401 Unauthorized - token expired or invalid
        if (status === 401) {
          // Redirect to login
          window.location.href = '/login';
          return Promise.reject(error);
        }

        // 403 Forbidden - user lacks permission for this resource (NFR-014 minimum necessary access violation)
        if (status === 403) {
          const role = getUserRole();
          if (role) {
            // Redirect to user's appropriate dashboard
            const redirectPath = getRedirectPath(role);
            window.location.href = redirectPath;
          } else {
            // No role found, redirect to login
            window.location.href = '/login';
          }
          return Promise.reject(error);
        }
      }

      // Pass through other errors
      return Promise.reject(error);
    }
  );

  // Request interceptor to add JWT token to all requests
  axios.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      const token = getToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error: AxiosError) => {
      return Promise.reject(error);
    }
  );
}
