/**
 * Main App Component
 * Application root with routing setup
 */

import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { AuthProvider } from "@/contexts/AuthContext";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { Dashboard } from "@/features/dispatcher/Dashboard";
import { JobSubmissionPage } from "@/features/customer/JobSubmissionPage";

function App() {
  return (
    <Router>
      <AuthProvider>
        <Routes>
          {/* Dispatcher Dashboard */}
          <Route
            path="/dispatcher/dashboard"
            element={
              <ProtectedRoute requiredRole="Dispatcher">
                <Dashboard />
              </ProtectedRoute>
            }
          />

          {/* Customer Job Submission */}
          <Route
            path="/customer/submit-job"
            element={
              <ProtectedRoute requiredRole="Customer">
                <JobSubmissionPage />
              </ProtectedRoute>
            }
          />

          {/* Customer Job Tracking (TODO: implement in Story 4.2) */}
          <Route
            path="/customer/jobs/:jobId"
            element={
              <ProtectedRoute requiredRole="Customer">
                <div>Job Tracking View (TODO)</div>
              </ProtectedRoute>
            }
          />

          {/* Redirect root to dashboard */}
          <Route
            path="/"
            element={<Navigate to="/dispatcher/dashboard" replace />}
          />

          {/* Placeholder routes */}
          <Route path="/login" element={<div>Login Page (TODO)</div>} />
          <Route
            path="/unauthorized"
            element={<div>Unauthorized (TODO)</div>}
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </Router>
  );
}

export default App;
