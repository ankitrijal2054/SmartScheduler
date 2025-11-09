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

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        Loading...
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Redirect to role-specific dashboard
  switch (user?.role) {
    case "Dispatcher":
      return <Navigate to="/dispatcher/dashboard" replace />;
    case "Customer":
      return <Navigate to="/customer/jobs" replace />;
    case "Contractor":
      return <Navigate to="/contractor/assignments" replace />;
    default:
      return <Navigate to="/login" replace />;
  }
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
