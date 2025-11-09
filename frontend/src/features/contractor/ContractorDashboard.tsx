/**
 * ContractorDashboard Component
 * Main dashboard for contractors showing job list and notification system
 * Role-based redirect ensures only contractors can access this page
 */

import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthContext } from "@/hooks/useAuthContext";
import { useSignalRNotifications } from "@/hooks/useSignalRNotifications";
import { ContractorLayout } from "./ContractorLayout";
import { JobList } from "./JobList";
import { ContractorProfileTab } from "./ContractorProfileTab";
import { ContractorJobHistoryTab } from "./ContractorJobHistoryTab";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

type DashboardTab = "jobs" | "profile" | "history";

interface TabConfig {
  id: DashboardTab;
  label: string;
}

const TABS: TabConfig[] = [
  { id: "jobs", label: "Jobs" },
  { id: "profile", label: "Profile" },
  { id: "history", label: "History" },
];

export const ContractorDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { user, isLoading } = useAuthContext();
  const { isConnected, error: signalRError } = useSignalRNotifications();
  const [activeTab, setActiveTab] = useState<DashboardTab>("jobs");

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
              ? "Connected"
              : signalRError
              ? `Connection error: ${signalRError}`
              : "Connecting..."}
          </span>
        </div>

        {/* Page Title */}
        <div>
          <h2 className="text-3xl font-bold text-gray-900">
            Contractor Dashboard
          </h2>
          <p className="text-gray-600 mt-1">
            Manage your jobs and track your performance
          </p>
        </div>

        {/* Tab Navigation */}
        <div className="border-b border-gray-200 flex gap-8">
          {TABS.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`px-1 py-3 font-medium border-b-2 transition-colors ${
                activeTab === tab.id
                  ? "border-indigo-600 text-indigo-600"
                  : "border-transparent text-gray-600 hover:text-gray-900"
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* Tab Content */}
        <div className="pt-4">
          {activeTab === "jobs" && <JobList />}
          {activeTab === "profile" && <ContractorProfileTab />}
          {activeTab === "history" && <ContractorJobHistoryTab />}
        </div>
      </div>
    </ContractorLayout>
  );
};
