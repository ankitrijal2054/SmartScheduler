/**
 * JobList Component
 * Displays paginated list of jobs with filtering and sorting
 */

import React from "react";
import { Job } from "@/types/Job";
import { JobCard } from "./JobCard";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { EmptyState } from "@/components/common/EmptyState";
import { PaginationMeta } from "@/types/Job";

interface JobListProps {
  jobs: Job[];
  loading: boolean;
  error: string | null;
  pagination: PaginationMeta | null;
  onJobClick?: (job: Job) => void;
  onGetRecommendations?: (job: Job) => void;
  onPageChange?: (page: number) => void;
}

export const JobList: React.FC<JobListProps> = ({
  jobs,
  loading,
  error,
  pagination,
  onJobClick,
  onGetRecommendations,
  onPageChange,
}) => {
  if (loading) {
    return <LoadingSpinner />;
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800">
        <p className="font-semibold">Error loading jobs</p>
        <p className="text-sm">{error}</p>
      </div>
    );
  }

  if (!jobs || jobs.length === 0) {
    return (
      <EmptyState
        title="No jobs at this time"
        description="There are currently no jobs to display. Check back soon!"
        icon="📋"
      />
    );
  }

  const handlePrevPage = () => {
    if (pagination && pagination.page > 1) {
      onPageChange?.(pagination.page - 1);
    }
  };

  const handleNextPage = () => {
    if (pagination && pagination.page < pagination.totalPages) {
      onPageChange?.(pagination.page + 1);
    }
  };

  return (
    <div className="space-y-4">
      {/* Jobs List */}
      <div className="space-y-3">
        {jobs.map((job) => (
          <JobCard
            key={job.id}
            job={job}
            onClick={onJobClick}
            onGetRecommendations={onGetRecommendations}
          />
        ))}
      </div>

      {/* Pagination Controls */}
      {pagination && pagination.totalPages > 1 && (
        <div className="flex items-center justify-between rounded-lg border border-gray-200 bg-white p-4">
          <div className="text-sm text-gray-600">
            Page <span className="font-semibold">{pagination.page}</span> of{" "}
            <span className="font-semibold">{pagination.totalPages}</span> (
            <span className="font-semibold">{pagination.total}</span> total)
          </div>

          <div className="flex gap-2">
            <button
              onClick={handlePrevPage}
              disabled={pagination.page === 1}
              className="rounded-md bg-gray-200 px-4 py-2 text-sm font-medium text-gray-900 transition-colors hover:bg-gray-300 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Previous page"
            >
              ← Previous
            </button>

            <button
              onClick={handleNextPage}
              disabled={pagination.page === pagination.totalPages}
              className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              aria-label="Next page"
            >
              Next →
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
