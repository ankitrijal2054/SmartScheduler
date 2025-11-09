/**
 * Authentication Context Creation
 * Separate file for context to satisfy React Fast Refresh rules
 */

import { createContext } from "react";
import { User, UserRole } from "@/types/Auth";

export interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: (email: string, password: string) => Promise<void>;
  signup: (email: string, password: string, role: UserRole) => Promise<void>;
  logout: () => Promise<void>;
  hasRole: (role: UserRole) => boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(
  undefined
);
