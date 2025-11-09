/**
 * Authentication Provider Component
 * Provides user authentication state and methods globally
 */

import React, { useEffect, useState, useCallback } from "react";
import { User, UserRole } from "@/types/Auth";
import { config } from "@/utils/config";
import { authService } from "@/services/authService";
import { AuthContext, AuthContextType } from "./auth.context";

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Initialize auth state from localStorage on mount
  useEffect(() => {
    const storedToken = localStorage.getItem(config.auth.jwtStorageKey);
    if (storedToken) {
      // Check if token is expired
      if (!authService.isTokenExpired(storedToken)) {
        setToken(storedToken);
        // Extract user info from token
        const role = authService.extractRole(storedToken);
        const decoded = authService.decodeToken(storedToken);
        if (role && decoded && decoded.email) {
          setUser({
            id: (decoded as any).sub || (decoded as any).jti || "",
            email: decoded.email,
            role: role as UserRole,
          });
        }
      } else {
        // Token expired, clear it
        localStorage.removeItem(config.auth.jwtStorageKey);
        localStorage.removeItem(config.auth.refreshTokenStorageKey);
      }
    }
    setIsLoading(false);
  }, []);

  // Handle logout event (triggered by apiClient interceptor on 401 after failed refresh)
  useEffect(() => {
    const handleLogout = () => {
      logout();
    };
    window.addEventListener("auth:logout", handleLogout);
    return () => window.removeEventListener("auth:logout", handleLogout);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const tokenResponse = await authService.login(email, password);
      setToken(tokenResponse.accessToken);

      // Store tokens
      localStorage.setItem(
        config.auth.jwtStorageKey,
        tokenResponse.accessToken
      );
      if (tokenResponse.refreshToken) {
        localStorage.setItem(
          config.auth.refreshTokenStorageKey,
          tokenResponse.refreshToken
        );
      }

      // Extract user info from token
      const role = authService.extractRole(tokenResponse.accessToken);
      const decoded = authService.decodeToken(tokenResponse.accessToken);
      if (role && decoded && decoded.email) {
        setUser({
          id: (decoded as any).sub || (decoded as any).jti || "",
          email: decoded.email,
          role: role as UserRole,
        });
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : "Login failed";
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const signup = useCallback(
    async (email: string, password: string, role: UserRole) => {
      setIsLoading(true);
      setError(null);
      try {
        const tokenResponse = await authService.signup(
          email,
          password,
          role as "Dispatcher" | "Customer" | "Contractor"
        );
        setToken(tokenResponse.accessToken);

        // Store tokens
        localStorage.setItem(
          config.auth.jwtStorageKey,
          tokenResponse.accessToken
        );
        if (tokenResponse.refreshToken) {
          localStorage.setItem(
            config.auth.refreshTokenStorageKey,
            tokenResponse.refreshToken
          );
        }

        // Extract user info from token
        const decodedRole = authService.extractRole(tokenResponse.accessToken);
        const decoded = authService.decodeToken(tokenResponse.accessToken);
        if (decodedRole && decoded && decoded.email) {
          setUser({
            id: (decoded as any).sub || (decoded as any).jti || "",
            email: decoded.email,
            role: decodedRole as UserRole,
          });
        }
      } catch (err) {
        const message = err instanceof Error ? err.message : "Signup failed";
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  const logout = useCallback(async () => {
    try {
      const refreshToken = localStorage.getItem(
        config.auth.refreshTokenStorageKey
      );
      if (refreshToken) {
        await authService.logout(refreshToken);
      }
    } catch (err) {
      console.error("Logout API call failed:", err);
    } finally {
      // Clear local state regardless of API success
      setUser(null);
      setToken(null);
      setError(null);
      localStorage.removeItem(config.auth.jwtStorageKey);
      localStorage.removeItem(config.auth.refreshTokenStorageKey);
    }
  }, []);

  const refreshToken = useCallback(async () => {
    try {
      const storedRefreshToken = localStorage.getItem(
        config.auth.refreshTokenStorageKey
      );
      if (!storedRefreshToken) {
        throw new Error("No refresh token available");
      }

      const tokenResponse = await authService.refreshToken(storedRefreshToken);
      setToken(tokenResponse.accessToken);
      localStorage.setItem(
        config.auth.jwtStorageKey,
        tokenResponse.accessToken
      );
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Token refresh failed";
      setError(message);
      // On refresh failure, logout
      logout();
      throw err;
    }
  }, [logout]);

  const hasRole = (role: UserRole): boolean => {
    return user?.role === role;
  };

  const value: AuthContextType = {
    user,
    token,
    isAuthenticated: !!token && !!user,
    isLoading,
    error,
    login,
    signup,
    logout,
    hasRole,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export { AuthContext };
export type { AuthContextType };
