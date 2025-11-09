/**
 * JobTrackingPage Component
 * Main page for customer to track their job status in real-time
 * Integrates with useJob hook, useSignalR hook, and displays child components
 */

import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useJob } from "@/hooks/useJob";
import { useSignalR } from "@/hooks/useSignalR";
import { useContractorProfile } from "@/hooks/useContractorProfile";
import { JobStatusTimeline } from "./JobStatusTimeline";
import { JobDetailsCard } from "./JobDetailsCard";
import { ContractorInfoCard } from "./ContractorInfoCard";
import { ContractorProfileModal } from "./ContractorProfileModal";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { JobDetail } from "@/types/Job";

export const JobTrackingPage: React.FC = () => {
  const { jobId } = useParams<{ jobId: string }>();
  const navigate = useNavigate();

  // Fetch job details
  const { job, loading, error, refreshJob } = useJob(jobId || "");
  const [displayedJob, setDisplayedJob] = useState<JobDetail | null>(null);

  // Modal state for contractor profile
  const [showContractorProfile, setShowContractorProfile] = useState(false);
  const contractorId = displayedJob?.contractor?.id || null;
  const {
    contractor,
    reviews,
    loading: profileLoading,
  } = useContractorProfile(showContractorProfile ? contractorId : null);

  // Subscribe to real-time updates
  const { isConnected } = useSignalR({
    onJobStatusUpdate: (event) => {
      // Only update if this is the current job
      if (event.jobId === jobId) {
        // Refresh job data when status updates
        refreshJob();
      }
    },
  });

  // Update displayed job when fetched job changes
  useEffect(() => {
    if (job) {
      setDisplayedJob(job);
    }
  }, [job]);

  // Error handling
  if (error && error.includes("404")) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-2">
            Job Not Found
          </h1>
          <p className="text-gray-600 mb-6">
            The job you're looking for doesn't exist or has been deleted.
          </p>
          <button
            onClick={() => navigate(-1)}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Go Back
          </button>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-600 mb-2">
            Error Loading Job
          </h1>
          <p className="text-gray-600 mb-6">{error}</p>
          <button
            onClick={() => navigate(-1)}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Go Back
          </button>
        </div>
      </div>
    );
  }

  if (loading || !displayedJob) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-50">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b shadow-sm sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div>
              <button
                onClick={() => navigate(-1)}
                className="inline-flex items-center text-blue-600 hover:text-blue-800 mb-2"
              >
                ← Back to Jobs
              </button>
              <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">
                Track Your Job
              </h1>
              <p className="text-sm text-gray-500 mt-1">
                {displayedJob.jobType} • {displayedJob.location}
              </p>
            </div>

            {/* Connection status */}
            <div className="flex items-center gap-2">
              <div
                className={`w-2 h-2 rounded-full ${
                  isConnected ? "bg-green-500" : "bg-yellow-500"
                }`}
                title={
                  isConnected
                    ? "Live updates connected"
                    : "Using polling for updates"
                }
              />
              <span className="text-xs text-gray-600">
                {isConnected ? "Live" : "Polling"}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Main content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Status Timeline - Full width */}
        <div className="mb-8">
          <JobStatusTimeline
            currentStatus={displayedJob.status}
            updatedAt={displayedJob.updatedAt}
            createdAt={displayedJob.createdAt}
          />
        </div>

        {/* Two-column layout on desktop, single column on mobile */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Left column: Job Details (2 cols on desktop) */}
          <div className="lg:col-span-2">
            <JobDetailsCard job={displayedJob} />
          </div>

          {/* Right column: Contractor Info (1 col on desktop) */}
          <div>
            <ContractorInfoCard
              contractor={displayedJob.contractor}
              jobStatus={displayedJob.status}
              estimatedArrivalTime={
                displayedJob.assignment?.estimatedArrivalTime || undefined
              }
              onViewProfile={() => setShowContractorProfile(true)}
            />
          </div>
        </div>

        {/* Action bar */}
        <div className="mt-8 flex flex-col sm:flex-row gap-4 justify-center">
          <button
            onClick={() => refreshJob()}
            className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-medium text-gray-700"
          >
            🔄 Refresh
          </button>
          <button
            onClick={() => navigate(-1)}
            className="px-6 py-2 bg-gray-900 text-white rounded-lg hover:bg-gray-800 transition-colors font-medium"
          >
            ← Back to Jobs List
          </button>
        </div>
      </div>

      {/* Contractor Profile Modal */}
      <ContractorProfileModal
        isOpen={showContractorProfile}
        onClose={() => setShowContractorProfile(false)}
        contractor={contractor}
        reviews={reviews}
        loading={profileLoading}
      />
    </div>
  );
};
