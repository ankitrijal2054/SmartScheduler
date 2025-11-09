/**
 * DashboardHeader Component
 * Shared header component for all user dashboards with logo, user info, and logout button
 */

import React from "react";
import { useNavigate } from "react-router-dom";
import { LogOut, User } from "lucide-react";
import { useAuth } from "@/hooks/useAuthContext";
import { NotificationBadge } from "@/components/shared/NotificationBadge";

interface DashboardHeaderProps {
  title: string;
  subtitle?: string;
  showNotificationBadge?: boolean;
}

export const DashboardHeader: React.FC<DashboardHeaderProps> = ({
  title,
  subtitle,
  showNotificationBadge = false,
}) => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const getUserDisplayName = () => {
    if (user?.name) {
      return user.name;
    }
    // Fallback to email or role-based greeting
    if (user?.email) {
      return user.email.split("@")[0];
    }
    return user?.role || "User";
  };

  return (
    <header className="bg-white shadow-sm border-b border-gray-200">
      <div className="px-6 py-4 flex items-center justify-between">
        {/* Left: Logo/Brand */}
        <div className="flex items-center gap-3">
          <div
            className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center cursor-pointer"
            onClick={() => navigate("/")}
          >
            <span className="text-white font-bold text-sm">SS</span>
          </div>
          <div>
            <h1 className="text-xl font-semibold text-gray-900">{title}</h1>
            {subtitle && (
              <p className="text-sm text-gray-600 mt-0.5">{subtitle}</p>
            )}
          </div>
        </div>

        {/* Right: User Info + Notification + Profile + Logout */}
        <div className="flex items-center gap-6">
          {/* User Welcome */}
          <div className="text-sm">
            <p className="text-gray-600">Welcome back,</p>
            <p className="font-semibold text-gray-900">
              {getUserDisplayName()}
            </p>
          </div>

          {/* Notification Badge (optional, mainly for contractors) */}
          {showNotificationBadge && <NotificationBadge />}

          {/* Profile Icon */}
          <button
            className="p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition"
            title="Profile"
            aria-label="Open profile"
            onClick={() => {
              // TODO: Navigate to profile page when implemented
              console.log("Profile clicked");
            }}
          >
            <User size={20} />
          </button>

          {/* Logout Button */}
          <button
            onClick={handleLogout}
            className="p-2 text-gray-600 hover:bg-red-50 hover:text-red-600 rounded-lg transition"
            title="Logout"
            aria-label="Logout"
          >
            <LogOut size={20} />
          </button>
        </div>
      </div>
    </header>
  );
};
