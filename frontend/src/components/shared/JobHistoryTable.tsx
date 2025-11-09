/**
 * JobHistoryTable Component
 * Displays paginated job history for a contractor
 */

import React, { useState } from "react";
import { JobHistoryItem } from "@/types/Contractor";

interface JobHistoryTableProps {
  jobs: JobHistoryItem[];
  isLoading?: boolean;
  onPageChange?: (offset: number) => void;
  hasMore?: boolean;
}

/**
 * Format date to readable format (e.g., "Nov 15, 2025")
 */
const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  return date.toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
};

/**
 * Status Badge Component
 */
const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const statusStyles: Record<string, string> = {
    completed: "bg-green-100 text-green-800",
    cancelled: "bg-red-100 text-red-800",
    "in-progress": "bg-blue-100 text-blue-800",
  };

  const style = statusStyles[status] || "bg-gray-100 text-gray-800";

  return (
    <span
      className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${style}`}
    >
      {status.charAt(0).toUpperCase() + status.slice(1)}
    </span>
  );
};

/**
 * Star Rating Component
 */
const RatingStars: React.FC<{ rating: number | null }> = ({ rating }) => {
  if (rating === null) return <span className="text-gray-500">Not rated</span>;

  const filledStars = Math.round(rating);
  const emptyStars = 5 - filledStars;

  return (
    <div className="flex items-center gap-1">
      {Array(filledStars)
        .fill(0)
        .map((_, i) => (
          <span key={`filled-${i}`} className="text-yellow-400 text-sm">
            ★
          </span>
        ))}
      {Array(emptyStars)
        .fill(0)
        .map((_, i) => (
          <span key={`empty-${i}`} className="text-gray-300 text-sm">
            ★
          </span>
        ))}
    </div>
  );
};

export const JobHistoryTable: React.FC<JobHistoryTableProps> = ({
  jobs,
  isLoading = false,
  onPageChange,
  hasMore = false,
}) => {
  const [currentPage, setCurrentPage] = useState(0);
  const itemsPerPage = 10;

  // Calculate pagination
  const startIdx = currentPage * itemsPerPage;
  const endIdx = startIdx + itemsPerPage;
  const paginatedJobs = jobs.slice(startIdx, endIdx);
  const totalPages = Math.ceil(jobs.length / itemsPerPage);

  const handlePreviousPage = () => {
    if (currentPage > 0) {
      const newPage = currentPage - 1;
      setCurrentPage(newPage);
      onPageChange?.(newPage * itemsPerPage);
    }
  };

  const handleNextPage = () => {
    if (currentPage < totalPages - 1 || hasMore) {
      const newPage = currentPage + 1;
      setCurrentPage(newPage);
      onPageChange?.(newPage * itemsPerPage);
    }
  };

  if (isLoading) {
    return (
      <div className="w-full bg-white rounded-lg border border-gray-200 p-8 flex items-center justify-center">
        <div className="text-gray-500">Loading job history...</div>
      </div>
    );
  }

  if (jobs.length === 0) {
    return (
      <div className="w-full bg-white rounded-lg border border-gray-200 p-8 text-center">
        <div className="text-gray-500 text-lg">No job history available</div>
        <div className="text-gray-400 text-sm mt-2">
          This contractor hasn't completed any jobs yet
        </div>
      </div>
    );
  }

  return (
    <div className="w-full bg-white rounded-lg border border-gray-200">
      {/* Desktop Table View */}
      <div className="hidden md:block overflow-x-auto">
        <table className="w-full">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-semibold text-gray-600 uppercase">
                Date
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-gray-600 uppercase">
                Job Type
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-gray-600 uppercase">
                Customer
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-gray-600 uppercase">
                Status
              </th>
              <th className="px-6 py-3 text-left text-xs font-semibold text-gray-600 uppercase">
                Rating
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {paginatedJobs.map((job) => (
              <tr key={job.jobId} className="hover:bg-gray-50 transition">
                <td className="px-6 py-4 text-sm text-gray-900">
                  {formatDate(job.createdAt)}
                </td>
                <td className="px-6 py-4 text-sm text-gray-900">
                  {job.jobType}
                </td>
                <td className="px-6 py-4 text-sm text-gray-900">
                  {job.customerName}
                </td>
                <td className="px-6 py-4 text-sm">
                  <StatusBadge status={job.status} />
                </td>
                <td className="px-6 py-4 text-sm">
                  <RatingStars rating={job.customerRating} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile Card View */}
      <div className="md:hidden space-y-4 p-4">
        {paginatedJobs.map((job) => (
          <div
            key={job.jobId}
            className="border border-gray-200 rounded-lg p-4 space-y-3"
          >
            <div className="flex justify-between items-start">
              <div>
                <div className="text-xs text-gray-500 uppercase font-semibold">
                  Date
                </div>
                <div className="text-sm font-medium text-gray-900">
                  {formatDate(job.createdAt)}
                </div>
              </div>
              <StatusBadge status={job.status} />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="text-xs text-gray-500 uppercase font-semibold">
                  Job Type
                </div>
                <div className="text-sm font-medium text-gray-900">
                  {job.jobType}
                </div>
              </div>
              <div>
                <div className="text-xs text-gray-500 uppercase font-semibold">
                  Customer
                </div>
                <div className="text-sm font-medium text-gray-900">
                  {job.customerName}
                </div>
              </div>
            </div>

            <div>
              <div className="text-xs text-gray-500 uppercase font-semibold">
                Customer Rating
              </div>
              <div className="mt-1">
                <RatingStars rating={job.customerRating} />
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between border-t border-gray-200 bg-gray-50 px-6 py-3">
          <div className="text-xs text-gray-600">
            Page {currentPage + 1} of {totalPages} ({jobs.length} total)
          </div>
          <div className="flex gap-2">
            <button
              onClick={handlePreviousPage}
              disabled={currentPage === 0}
              className="px-3 py-1 border border-gray-300 rounded text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
              aria-label="Previous page"
            >
              ← Previous
            </button>
            <button
              onClick={handleNextPage}
              disabled={currentPage >= totalPages - 1 && !hasMore}
              className="px-3 py-1 border border-gray-300 rounded text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
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
