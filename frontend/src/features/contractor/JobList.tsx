/**
 * JobList Component
 * Displays contractor's assigned jobs with tabs: Pending, Active, Completed
 * Supports filtering by status and pagination
 */

import React, { useState, useMemo } from "react";
import { useContractorJobs } from "@/hooks/useContractorJobs";
import { ContractorJobCard } from "@/components/shared/ContractorJobCard";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { EmptyState } from "@/components/common/EmptyState";
import { Assignment } from "@/types/Assignment";

type JobStatus = "Pending" | "Active" | "Completed";

interface TabConfig {
  id: JobStatus;
  label: string;
  color: string;
}

const TABS: TabConfig[] = [
  { id: "Pending", label: "Pending", color: "text-yellow-600" },
  { id: "Active", label: "Active", color: "text-green-600" },
  { id: "Completed", label: "Completed", color: "text-gray-600" },
];

const JOBS_PER_PAGE = 10;

export const JobList: React.FC = () => {
  const [activeTab, setActiveTab] = useState<JobStatus>("Pending");
  const [currentPage, setCurrentPage] = useState(1);
  const { jobs, loading, error } = useContractorJobs();

  // Filter jobs by status
  const filteredJobs = useMemo(() => {
    if (!jobs) return [];

    switch (activeTab) {
      case "Pending":
        return jobs.filter((job) => job.status === "Pending");
      case "Active":
        return jobs.filter(
          (job) => job.status === "Accepted" || job.status === "InProgress"
        );
      case "Completed":
        return jobs.filter((job) => job.status === "Completed");
      default:
        return jobs;
    }
  }, [jobs, activeTab]);

  // Paginate jobs
  const paginatedJobs = useMemo(() => {
    const startIdx = (currentPage - 1) * JOBS_PER_PAGE;
    return filteredJobs.slice(startIdx, startIdx + JOBS_PER_PAGE);
  }, [filteredJobs, currentPage]);

  const totalPages = Math.ceil(filteredJobs.length / JOBS_PER_PAGE);

  // Reset to page 1 when tab changes
  const handleTabChange = (tab: JobStatus) => {
    setActiveTab(tab);
    setCurrentPage(1);
  };

  return (
    <div className="space-y-6">
      {/* Tab Selector */}
      <div className="flex gap-2 border-b border-gray-200">
        {TABS.map((tab) => (
          <button
            key={tab.id}
            onClick={() => handleTabChange(tab.id)}
            className={`px-4 py-3 font-medium text-sm transition border-b-2 ${
              activeTab === tab.id
                ? "border-indigo-600 text-indigo-600"
                : "border-transparent text-gray-600 hover:text-gray-900"
            }`}
            aria-current={activeTab === tab.id ? "page" : undefined}
          >
            {tab.label}
            {filteredJobs.length > 0 && (
              <span className="ml-2 inline-flex items-center justify-center px-2 py-1 text-xs font-semibold leading-none text-gray-600 bg-gray-100 rounded-full">
                {filteredJobs.length}
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Loading State */}
      {loading && (
        <div className="flex items-center justify-center py-12">
          <LoadingSpinner />
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800 font-medium">Error loading jobs</p>
          <p className="text-red-700 text-sm mt-1">{error}</p>
        </div>
      )}

      {/* Empty State */}
      {!loading && !error && paginatedJobs.length === 0 && (
        <EmptyState
          title={`No ${activeTab.toLowerCase()} jobs`}
          description={
            activeTab === "Pending"
              ? "You have no pending job assignments right now."
              : activeTab === "Active"
                ? "You have no active jobs at the moment."
                : "You have not completed any jobs yet."
          }
        />
      )}

      {/* Job List */}
      {!loading && !error && paginatedJobs.length > 0 && (
        <>
          <div className="space-y-4">
            {paginatedJobs.map((job) => (
              <ContractorJobCard key={job.id} assignment={job} />
            ))}
          </div>

          {/* Pagination Controls */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between pt-6 border-t border-gray-200">
              <p className="text-sm text-gray-600">
                Page {currentPage} of {totalPages}
              </p>
              <div className="flex gap-2">
                <button
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
                >
                  Previous
                </button>
                <button
                  onClick={() =>
                    setCurrentPage((p) => Math.min(totalPages, p + 1))
                  }
                  disabled={currentPage === totalPages}
                  className="px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
};



