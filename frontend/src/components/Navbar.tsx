import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/useAuthContext";

/**
 * Navigation Bar Component
 * Displays branding, user info, and logout button
 */
export const Navbar: React.FC = () => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [showMenu, setShowMenu] = useState(false);

  if (!user) {
    return null; // Don't show navbar if not authenticated
  }

  const handleLogout = async () => {
    try {
      await logout();
      navigate("/login");
    } catch (error) {
      console.error("Logout failed:", error);
    }
  };

  const getRoleColor = (role: string) => {
    switch (role) {
      case "Dispatcher":
        return "bg-purple-100 text-purple-800";
      case "Customer":
        return "bg-blue-100 text-blue-800";
      case "Contractor":
        return "bg-green-100 text-green-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <nav className="bg-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo & Brand */}
          <div
            className="flex items-center gap-3 cursor-pointer"
            onClick={() => navigate("/")}
          >
            <div className="inline-flex items-center justify-center w-8 h-8 bg-indigo-600 rounded-lg">
              <span className="text-white font-bold text-sm">SS</span>
            </div>
            <span className="font-bold text-lg text-gray-900">
              SmartScheduler
            </span>
          </div>

          {/* Right side - User Info & Menu */}
          <div className="flex items-center gap-4">
            {/* Role Badge */}
            <span
              className={`px-3 py-1 rounded-full text-sm font-semibold ${getRoleColor(
                user.role
              )}`}
            >
              {user.role}
            </span>

            {/* User Menu Dropdown */}
            <div className="relative">
              <button
                onClick={() => setShowMenu(!showMenu)}
                className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-gray-100 transition"
              >
                <div className="w-8 h-8 bg-indigo-600 rounded-full flex items-center justify-center text-white text-sm font-bold">
                  {user.email.charAt(0).toUpperCase()}
                </div>
                <svg
                  className={`w-4 h-4 text-gray-600 transition ${
                    showMenu ? "rotate-180" : ""
                  }`}
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M19 14l-7 7m0 0l-7-7m7 7V3"
                  />
                </svg>
              </button>

              {/* Dropdown Menu */}
              {showMenu && (
                <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-2 z-10">
                  <div className="px-4 py-2 border-b border-gray-200">
                    <p className="text-sm font-semibold text-gray-900">
                      {user.email}
                    </p>
                    <p className="text-xs text-gray-500">{user.role}</p>
                  </div>

                  <button
                    onClick={() => {
                      navigate("/settings");
                      setShowMenu(false);
                    }}
                    disabled
                    className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 disabled:text-gray-400 disabled:cursor-not-allowed"
                    title="Coming in Phase 2"
                  >
                    Settings
                  </button>

                  <button
                    onClick={handleLogout}
                    className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 border-t border-gray-200"
                  >
                    Logout
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
};


