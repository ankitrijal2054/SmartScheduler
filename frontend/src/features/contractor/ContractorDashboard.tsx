/**
 * ContractorDashboard Component
 * Main dashboard for contractors showing job list and notification system
 * Role-based redirect ensures only contractors can access this page
 */

import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthContext } from "@/hooks/useAuthContext";
import { useSignalRNotifications } from "@/hooks/useSignalRNotifications";
import { ContractorLayout } from "./ContractorLayout";
import { JobList } from "./JobList";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

export const ContractorDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { user, isLoading } = useAuthContext();
  const { isConnected, error: signalRError } = useSignalRNotifications();

  // Role-based redirect
  useEffect(() => {
    if (!isLoading && user?.role !== "Contractor") {
      navigate("/login");
    }
  }, [user, isLoading, navigate]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <LoadingSpinner />
      </div>
    );
  }

  if (user?.role !== "Contractor") {
    return null;
  }

  return (
    <ContractorLayout>
      <div className="space-y-6">
        {/* Connection Status */}
        <div className="flex items-center gap-2">
          <div
            className={`w-2 h-2 rounded-full ${
              isConnected ? "bg-green-500" : "bg-red-500"
            }`}
          />
          <span className="text-sm text-gray-600">
            {isConnected
              ? "Connected to live updates"
              : signalRError
                ? `Connection error: ${signalRError}`
                : "Connecting..."}
          </span>
        </div>

        {/* Page Title */}
        <div>
          <h2 className="text-3xl font-bold text-gray-900">My Jobs</h2>
          <p className="text-gray-600 mt-1">
            View and manage your assigned jobs
          </p>
        </div>

        {/* Job List with Tabs */}
        <JobList />
      </div>
    </ContractorLayout>
  );
};

