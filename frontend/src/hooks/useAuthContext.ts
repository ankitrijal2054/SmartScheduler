/**
 * useAuth Hook
 * Custom hook to access authentication context
 * Exported in separate file for proper React Fast Refresh
 */

import { useContext } from "react";
import { AuthContext } from "@/contexts/AuthContext";

export type { AuthContextType } from "@/contexts/AuthContext";

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}

// Alias for backward compatibility
export const useAuthContext = useAuth;
