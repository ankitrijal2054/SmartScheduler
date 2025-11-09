/**
 * Dispatcher Dashboard Container
 * Main dispatcher dashboard with job list and controls
 */

import React from "react";
import { useAuth } from "@/hooks/useAuthContext";
import { useJobs } from "@/hooks/useJobs";
import { Job } from "@/types/Job";
import { JobList } from "./JobList";

export const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const { jobs, loading, error, pagination, setPage, setSort } = useJobs();

  const handleJobClick = (job: Job) => {
    // TODO: Navigate to job detail view in Story 3.3
    console.log("Selected job:", job);
  };

  const handleGetRecommendations = () => {
    // TODO: Implement recommendations in Story 3.2
    alert("Get Recommendations feature coming in Story 3.2");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="border-b border-gray-200 bg-white shadow-sm">
        <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                Dispatcher Dashboard
              </h1>
              {user?.name && (
                <p className="mt-1 text-sm text-gray-600">
                  Welcome, {user.name}
                </p>
              )}
            </div>
            <button
              onClick={handleGetRecommendations}
              className="rounded-md bg-blue-600 px-6 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700"
              aria-label="Get job recommendations"
            >
              Get Recommendations
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Jobs Section */}
        <div className="mb-8">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-2xl font-semibold text-gray-900">Open Jobs</h2>
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
            onPageChange={setPage}
          />
        </div>

        {/* SignalR Integration Placeholder */}
        {/* TODO: Replace polling with SignalR in Story 6.6 */}
        {/* When NotificationContext receives a new job event, call refreshJobs() */}
        <div className="rounded-md border-l-4 border-blue-400 bg-blue-50 p-4">
          <p className="text-sm text-blue-700">
            💡 <strong>Note:</strong> Job list updates via polling every 30
            seconds. This will be replaced with real-time WebSocket updates in
            Story 6.6.
          </p>
        </div>
      </main>
    </div>
  );
};
