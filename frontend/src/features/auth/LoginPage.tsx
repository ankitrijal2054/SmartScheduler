import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "@/hooks/useAuthContext";

/**
 * Sign-In Page Component
 * Allows users to authenticate with email and password
 */
export const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const { login, isLoading, error } = useAuth();

  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });
  const [formError, setFormError] = useState<string | null>(null);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    // Clear error on user input
    setFormError(null);
  };

  const validateForm = (): boolean => {
    if (!formData.email.trim()) {
      setFormError("Email is required");
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      setFormError("Email must be a valid email address");
      return false;
    }

    if (!formData.password) {
      setFormError("Password is required");
      return false;
    }

    if (formData.password.length < 8) {
      setFormError("Password must be at least 8 characters");
      return false;
    }

    return true;
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!validateForm()) return;

    try {
      await login(formData.email, formData.password);
      // Login successful - redirect to dashboard (done by auth context)
      // The app should handle routing based on role
      navigate("/"); // Will be redirected by ProtectedRoute based on role
    } catch (err) {
      // Error is set in context, will be displayed below
      console.error("Login failed:", err);
    }
  };

  const displayError = formError || error;

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 to-blue-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Card Container */}
        <div className="bg-white rounded-lg shadow-xl p-8">
          {/* Logo & Branding */}
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-indigo-600 rounded-lg mb-4">
              <span className="text-white text-xl font-bold">SS</span>
            </div>
            <h1 className="text-3xl font-bold text-gray-900">SmartScheduler</h1>
            <p className="text-gray-500 text-sm mt-2">
              Sign in to your account
            </p>
          </div>

          {/* Error Message */}
          {displayError && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-700 text-sm">{displayError}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Email Field */}
            <div>
              <label
                htmlFor="email"
                className="block text-sm font-medium text-gray-700 mb-2"
              >
                Email Address
              </label>
              <input
                type="email"
                id="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                disabled={isLoading}
                placeholder="you@example.com"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed"
              />
            </div>

            {/* Password Field */}
            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-gray-700 mb-2"
              >
                Password
              </label>
              <input
                type="password"
                id="password"
                name="password"
                value={formData.password}
                onChange={handleChange}
                disabled={isLoading}
                placeholder="••••••••"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent outline-none transition disabled:bg-gray-100 disabled:cursor-not-allowed"
              />
            </div>

            {/* Sign In Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 text-white font-semibold py-2 px-4 rounded-lg transition disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <>
                  <span className="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                  Signing in...
                </>
              ) : (
                "Sign In"
              )}
            </button>
          </form>

          {/* Divider */}
          <div className="relative my-6">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-300" />
            </div>
          </div>

          {/* Links */}
          <div className="space-y-3 text-center text-sm">
            <div>
              <p className="text-gray-600">
                Don't have an account?{" "}
                <Link
                  to="/signup"
                  className="text-indigo-600 hover:text-indigo-700 font-semibold"
                >
                  Create one
                </Link>
              </p>
            </div>
            <div>
              <button
                type="button"
                className="text-gray-500 hover:text-gray-700"
                disabled
                title="Coming in Phase 2"
              >
                Forgot password?
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};


