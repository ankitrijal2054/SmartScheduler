/**
 * Dispatcher Dashboard Container
 * Main dispatcher dashboard with job list and controls
 */

import React, { useState } from "react";
import { useAuth } from "@/hooks/useAuthContext";
import { useJobs } from "@/hooks/useJobs";
import { useDispatcherNotifications } from "@/hooks/useDispatcherNotifications";
import { Job } from "@/types/Job";
import { DashboardHeader } from "@/components/DashboardHeader";
import { JobList } from "./JobList";
import { RecommendationsModal } from "./RecommendationsModal";
import { ContractorListPanel } from "./ContractorListPanel";
import { ContractorProfileModal } from "./ContractorProfileModal";

type DashboardTab = "jobs" | "contractors";

export const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const { jobs, loading, error, pagination, setPage, setSort, refreshJobs } =
    useJobs();

  // Initialize dispatcher notifications
  useDispatcherNotifications();

  // Dashboard tab state
  const [activeTab, setActiveTab] = useState<DashboardTab>("jobs");

  // Contractor list filter state
  const [contractorListOnly, setContractorListOnly] = useState(false);

  // Recommendations modal state
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedJobForRecommendations, setSelectedJobForRecommendations] =
    useState<Job | null>(null);

  // Contractor profile modal state
  const [profileModalOpen, setProfileModalOpen] = useState(false);
  const [selectedContractorId, setSelectedContractorId] = useState<
    string | null
  >(null);

  const handleJobClick = (job: Job) => {
    // TODO: Navigate to job detail view in Story 3.3
    console.log("Selected job:", job);
  };

  const handleGetRecommendations = (job: Job) => {
    setSelectedJobForRecommendations(job);
    setModalOpen(true);
  };

  const handleCloseRecommendationsModal = () => {
    setModalOpen(false);
    setSelectedJobForRecommendations(null);
  };

  const handleAssignmentSuccess = () => {
    // Refresh job list after successful assignment
    refreshJobs();
  };

  const handleContractorCardClick = (contractorId: string) => {
    setSelectedContractorId(contractorId);
    setProfileModalOpen(true);
  };

  const handleCloseProfileModal = () => {
    setProfileModalOpen(false);
    setSelectedContractorId(null);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <DashboardHeader
        title="Dispatcher Dashboard"
        subtitle={user?.email ? `Welcome, ${user.email}` : undefined}
        showNotificationBadge={true}
      />

      {/* Main Content */}
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Tabs */}
        <div className="mb-8 border-b border-gray-200">
          <div className="flex gap-4">
            <button
              onClick={() => setActiveTab("jobs")}
              className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
                activeTab === "jobs"
                  ? "border-blue-600 text-blue-600"
                  : "border-transparent text-gray-600 hover:text-gray-900"
              }`}
            >
              Open Jobs
            </button>
            <button
              onClick={() => setActiveTab("contractors")}
              className={`border-b-2 px-4 py-2 text-sm font-medium transition-colors ${
                activeTab === "contractors"
                  ? "border-blue-600 text-blue-600"
                  : "border-transparent text-gray-600 hover:text-gray-900"
              }`}
            >
              Contractor List
            </button>
          </div>
        </div>

        {/* Jobs Tab */}
        {activeTab === "jobs" && (
          <div className="mb-8">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-2xl font-semibold text-gray-900">
                Open Jobs
              </h2>
              <div className="flex gap-2 text-sm text-gray-600">
                <button
                  onClick={() => setSort("desiredDateTime", "asc")}
                  className="rounded-md bg-white px-3 py-1 hover:bg-gray-100"
                >
                  Sort by Date ↑
                </button>
              </div>
            </div>

            {/* Job List */}
            <JobList
              jobs={jobs}
              loading={loading}
              error={error}
              pagination={pagination}
              onJobClick={handleJobClick}
              onGetRecommendations={handleGetRecommendations}
              onPageChange={setPage}
              onReassignmentSuccess={handleAssignmentSuccess}
            />

            {/* SignalR Integration Placeholder */}
            {/* TODO: Replace polling with SignalR in Story 6.6 */}
            {/* When NotificationContext receives a new job event, call refreshJobs() */}
            <div className="mt-8 rounded-md border-l-4 border-blue-400 bg-blue-50 p-4">
              <p className="text-sm text-blue-700">
                💡 <strong>Note:</strong> Job list updates via polling every 30
                seconds. This will be replaced with real-time WebSocket updates
                in Story 6.6.
              </p>
            </div>
          </div>
        )}

        {/* Contractors Tab */}
        {activeTab === "contractors" && (
          <div>
            <h2 className="mb-6 text-2xl font-semibold text-gray-900">
              Contractor List Management
            </h2>
            <ContractorListPanel
              onFilterChange={setContractorListOnly}
              onContractorProfileClick={handleContractorCardClick}
            />
          </div>
        )}
      </main>

      {/* Recommendations Modal */}
      {selectedJobForRecommendations && (
        <RecommendationsModal
          isOpen={modalOpen}
          jobId={selectedJobForRecommendations.id}
          jobType={selectedJobForRecommendations.jobType}
          location={selectedJobForRecommendations.location}
          desiredDateTime={selectedJobForRecommendations.desiredDateTime}
          contractorListOnly={contractorListOnly}
          job={selectedJobForRecommendations}
          onClose={handleCloseRecommendationsModal}
          onAssignmentSuccess={handleAssignmentSuccess}
          onContractorProfileClick={handleContractorCardClick}
        />
      )}

      {/* Contractor Profile Modal */}
      {selectedContractorId && (
        <ContractorProfileModal
          contractorId={selectedContractorId}
          isOpen={profileModalOpen}
          onClose={handleCloseProfileModal}
        />
      )}
    </div>
  );
};
