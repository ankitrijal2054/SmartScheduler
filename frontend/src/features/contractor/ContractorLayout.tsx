/**
 * ContractorLayout Component
 * Main layout wrapper for contractor portal with header, sidebar, and notification badge
 */

import React from "react";
import { useNavigate } from "react-router-dom";
import { useAuthContext } from "@/hooks/useAuthContext";
import { Bell, LogOut, User } from "lucide-react";
import { NotificationBadge } from "@/components/shared/NotificationBadge";

interface ContractorLayoutProps {
  children: React.ReactNode;
}

export const ContractorLayout: React.FC<ContractorLayoutProps> = ({
  children,
}) => {
  const navigate = useNavigate();
  const { user, logout } = useAuthContext();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="px-6 py-4 flex items-center justify-between">
          {/* Left: Logo/Brand */}
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center">
              <span className="text-white font-bold text-sm">SS</span>
            </div>
            <h1 className="text-xl font-semibold text-gray-900">
              SmartScheduler
            </h1>
          </div>

          {/* Right: Contractor Info + Notification + Profile + Logout */}
          <div className="flex items-center gap-6">
            {/* Contractor Name */}
            <div className="text-sm">
              <p className="text-gray-600">Welcome back,</p>
              <p className="font-semibold text-gray-900">{user?.name || "Contractor"}</p>
            </div>

            {/* Notification Badge */}
            <NotificationBadge />

            {/* Profile Icon */}
            <button
              className="p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition"
              title="Profile"
              aria-label="Open profile"
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

      {/* Main Content */}
      <main className="px-6 py-8">{children}</main>
    </div>
  );
};



