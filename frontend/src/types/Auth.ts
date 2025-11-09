/**
 * Authentication Type Definitions
 */

export type UserRole = "Dispatcher" | "Customer" | "Contractor" | "Admin";

export interface User {
  id: string;
  email: string;
  role: UserRole;
  name?: string;
}

export interface AuthContext {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}
