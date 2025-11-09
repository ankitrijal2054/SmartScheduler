/**
 * Authentication Provider Component
 * Provides user authentication state and methods globally
 */

import React, { useEffect, useState } from "react";
import { User, UserRole } from "@/types/Auth";
import { config } from "@/utils/config";
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
      setToken(storedToken);
      // TODO: Validate token and fetch user info from backend
    }
    setIsLoading(false);
  }, []);

  const login = async () => {
    setIsLoading(true);
    setError(null);
    try {
      // TODO: Implement login API call with proper email/password parameters
      // const response = await authService.login(email, password)
      // setToken(response.token)
      // setUser(response.user)
      // localStorage.setItem(config.auth.jwtStorageKey, response.token)
    } catch (err) {
      const message = err instanceof Error ? err.message : "Login failed";
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    setError(null);
    localStorage.removeItem(config.auth.jwtStorageKey);
  };

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
    logout,
    hasRole,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export { AuthContext };
export type { AuthContextType };
