import axios, { AxiosInstance, InternalAxiosRequestConfig } from "axios";
import { config } from "@/utils/config";
import { authService } from "./authService";

/**
 * Create and configure axios instance with:
 * - JWT token injection in Authorization header
 * - Automatic token refresh on 401 response
 * - Error handling
 */

let refreshPromise: Promise<void> | null = null;

const apiClient: AxiosInstance = axios.create({
  baseURL: config.apiBaseUrl,
  timeout: 10000,
  withCredentials: true,
});

/**
 * Request interceptor: Attach JWT token to all requests
 */
apiClient.interceptors.request.use(
  (requestConfig: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem(config.auth.jwtStorageKey);

    if (token) {
      requestConfig.headers.Authorization = `Bearer ${token}`;
    }

    return requestConfig;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/**
 * Response interceptor: Handle 401 and attempt token refresh
 */
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retried) {
      originalRequest._retried = true;

      try {
        // If already refreshing, wait for it to complete
        if (refreshPromise) {
          await refreshPromise;
          // Retry with new token
          return apiClient(originalRequest);
        }

        // Start refresh
        refreshPromise = (async () => {
          try {
            const refreshToken = localStorage.getItem(
              config.auth.refreshTokenStorageKey
            );

            if (!refreshToken) {
              throw new Error("No refresh token available");
            }

            // Attempt to refresh token
            const newTokenResponse = await authService.refreshToken(
              refreshToken
            );

            // Store new token
            localStorage.setItem(
              config.auth.jwtStorageKey,
              newTokenResponse.accessToken
            );

            // Update Authorization header for this request
            originalRequest.headers.Authorization = `Bearer ${newTokenResponse.accessToken}`;
          } finally {
            refreshPromise = null;
          }
        })();

        // Wait for refresh to complete
        await refreshPromise;

        // Retry original request with new token
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed - clear auth state and redirect to login
        localStorage.removeItem(config.auth.jwtStorageKey);
        localStorage.removeItem(config.auth.refreshTokenStorageKey);

        // Dispatch custom event for app to handle logout
        window.dispatchEvent(new Event("auth:logout"));

        return Promise.reject(refreshError);
      }
    }

    // For other errors, reject as-is
    return Promise.reject(error);
  }
);

export default apiClient;


