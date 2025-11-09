/**
 * Main App Component
 * Application root with routing setup
 */

import React from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
  useNavigate,
} from "react-router-dom";
import { AuthProvider } from "@/contexts/AuthContext";
import { NotificationProvider } from "@/contexts/NotificationContext";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { LoginPage } from "@/features/auth/LoginPage";
import { SignupPage } from "@/features/auth/SignupPage";
import { Dashboard } from "@/features/dispatcher/Dashboard";
import { JobSubmissionPage } from "@/features/customer/JobSubmissionPage";
import { JobTrackingPage } from "@/features/customer/JobTracking/JobTrackingPage";
import { contractorRoutes } from "@/features/contractor/ContractorRoutes";
import { useAuth } from "@/hooks/useAuthContext";

/**
 * Root Route - Redirects based on authentication status
 */
const RootRoute: React.FC = () => {
  const { isAuthenticated, isLoading, user } = useAuth();
  const navigate = useNavigate();

  // Use useEffect to handle navigation to prevent infinite loops
  React.useEffect(() => {
    if (isLoading) {
      return; // Wait for auth state to load
    }

    if (!isAuthenticated || !user) {
      navigate("/login", { replace: true });
      return;
    }

    // Redirect to role-specific dashboard
    switch (user.role) {
      case "Dispatcher":
        navigate("/dispatcher/dashboard", { replace: true });
        break;
      case "Customer":
        navigate("/customer/submit-job", { replace: true });
        break;
      case "Contractor":
        navigate("/contractor/assignments", { replace: true });
        break;
      default:
        navigate("/login", { replace: true });
        break;
    }
  }, [isAuthenticated, isLoading, user, navigate]);

  // Show loading state while redirecting
  return (
    <div className="flex items-center justify-center h-screen">
      <div className="text-center">
        <div className="inline-block w-8 h-8 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mb-4" />
        <p className="text-gray-600">Loading...</p>
      </div>
    </div>
  );
};

/**
 * Unauthorized Page
 */
const UnauthorizedPage: React.FC = () => (
  <div className="flex items-center justify-center h-screen">
    <div className="text-center">
      <h1 className="text-4xl font-bold text-gray-900 mb-4">403</h1>
      <p className="text-xl text-gray-600 mb-8">
        You don't have permission to access this page
      </p>
      <a
        href="/"
        className="text-indigo-600 hover:text-indigo-700 font-semibold"
      >
        Go to Dashboard
      </a>
    </div>
  </div>
);

function App() {
  return (
    <Router>
      <AuthProvider>
        <NotificationProvider>
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/signup" element={<SignupPage />} />

            {/* Protected Routes - Dispatcher */}
            <Route
              path="/dispatcher/dashboard"
              element={
                <ProtectedRoute requiredRole="Dispatcher">
                  <Dashboard />
                </ProtectedRoute>
              }
            />

            {/* Protected Routes - Customer */}
            <Route
              path="/customer/submit-job"
              element={
                <ProtectedRoute requiredRole="Customer">
                  <JobSubmissionPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/customer/jobs/:jobId"
              element={
                <ProtectedRoute requiredRole="Customer">
                  <JobTrackingPage />
                </ProtectedRoute>
              }
            />

            {/* Protected Routes - Contractor */}
            {contractorRoutes}

            {/* Error Pages */}
            <Route path="/unauthorized" element={<UnauthorizedPage />} />

            {/* Root Route - Smart Redirect */}
            <Route path="/" element={<RootRoute />} />

            {/* Catch-all - Redirect to root */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </NotificationProvider>
      </AuthProvider>
    </Router>
  );
}

export default App;
