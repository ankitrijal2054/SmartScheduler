/**
 * ProtectedRoute Component
 * Route guard that checks authentication and role-based access
 */

import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "@/hooks/useAuthContext";
import { UserRole } from "@/types/Auth";
import { LoadingSpinner } from "./shared/LoadingSpinner";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: UserRole;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredRole,
}) => {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) {
    return <LoadingSpinner fullHeight />;
  }

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && user.role !== requiredRole) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <>{children}</>;
};
