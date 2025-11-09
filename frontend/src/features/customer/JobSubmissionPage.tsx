/**
 * JobSubmissionPage Component
 * Page container for job submission functionality
 * Handles role-based access control and navigation
 */

import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/useAuthContext";
import { JobSubmissionForm } from "./JobSubmissionForm";

export const JobSubmissionPage: React.FC = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();

  // Redirect if not authenticated or not a customer
  useEffect(() => {
    if (!isAuthenticated) {
      navigate("/login");
    } else if (user && user.role !== "Customer") {
      navigate("/");
    }
  }, [isAuthenticated, user, navigate]);

  // Handle successful job submission
  const handleJobSubmissionSuccess = (jobId: string) => {
    // Navigate to job tracking view for the submitted job
    navigate(`/customer/jobs/${jobId}`);
  };

  // Show loading state if not authenticated yet
  if (!isAuthenticated || (user && user.role !== "Customer")) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Verifying access...</p>
        </div>
      </div>
    );
  }

  return <JobSubmissionForm onSuccess={handleJobSubmissionSuccess} />;
};
