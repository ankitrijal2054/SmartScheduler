/**
 * Application Configuration
 * Centralized config for environment variables and app settings
 */

// Extend import.meta to include env variables
declare global {
  interface ImportMeta {
    env: {
      VITE_API_BASE_URL?: string;
      VITE_JWT_STORAGE_KEY?: string;
      VITE_REFRESH_TOKEN_STORAGE_KEY?: string;
    };
  }
}

export const config = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL || "http://localhost:5292",
  api: {
    baseUrl: import.meta.env.VITE_API_BASE_URL || "http://localhost:5292",
  },
  auth: {
    jwtStorageKey: import.meta.env.VITE_JWT_STORAGE_KEY || "auth_token",
    refreshTokenStorageKey:
      import.meta.env.VITE_REFRESH_TOKEN_STORAGE_KEY || "refresh_token",
  },
  polling: {
    jobsRefreshInterval: 30000, // 30 seconds
  },
};
