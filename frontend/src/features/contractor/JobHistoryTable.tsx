/**
 * JobHistoryTable Component
 * Displays job history in a table format with pagination
 * Used in Story 5.4 - Contractor Rating & Earnings History
 */

import React from "react";
import { JobHistoryItem } from "@/types/ContractorProfile";
import { Star, ChevronLeft, ChevronRight } from "lucide-react";
import { format } from "date-fns";

interface JobHistoryTableProps {
  jobs: JobHistoryItem[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  loading?: boolean;
  onPageChange: (page: number) => void;
}

/**
 * Component to display job history in a table
 * Includes: Date, Location, Customer, Job Type, Status, Customer Rating
 */
export const JobHistoryTable: React.FC<JobHistoryTableProps> = ({
  jobs,
  totalCount,
  currentPage,
  pageSize,
  loading = false,
  onPageChange,
}) => {
  const totalPages = Math.ceil(totalCount / pageSize);

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Completed":
        return "bg-green-100 text-green-800";
      case "InProgress":
        return "bg-blue-100 text-blue-800";
      case "Accepted":
        return "bg-indigo-100 text-indigo-800";
      case "Cancelled":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const renderStars = (rating: number | null) => {
    if (rating === null) return "-";

    return (
      <div className="flex gap-0.5">
        {Array.from({ length: 5 }).map((_, i) => (
          <Star
            key={i}
            size={14}
            className={
              i < rating ? "fill-yellow-400 text-yellow-400" : "text-gray-300"
            }
          />
        ))}
      </div>
    );
  };

  if (jobs.length === 0 && !loading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 border border-gray-200 text-center">
        <p className="text-gray-600">
          No jobs found. Try adjusting your filters.
        </p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                Date
              </th>
              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                Location
              </th>
              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                Customer
              </th>
              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                Job Type
              </th>
              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                Status
              </th>
              <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                Customer Rating
              </th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={6} className="px-6 py-8 text-center">
                  <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                  <p className="mt-2 text-gray-600">Loading...</p>
                </td>
              </tr>
            ) : (
              jobs.map((job, index) => (
                <tr
                  key={job.id}
                  className={`border-b border-gray-200 hover:bg-gray-50 transition-colors ${
                    index % 2 === 0 ? "bg-white" : "bg-gray-50"
                  }`}
                >
                  <td className="px-6 py-4 text-sm text-gray-900">
                    {format(new Date(job.scheduledDateTime), "MMM dd, yyyy")}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-700">
                    {job.location}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-900 font-medium">
                    {job.customerName}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-700">
                    {job.jobType}
                  </td>
                  <td className="px-6 py-4">
                    <span
                      className={`inline-block px-3 py-1 rounded-full text-xs font-semibold ${getStatusColor(
                        job.status
                      )}`}
                    >
                      {job.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-sm">
                    {renderStars(job.customerRating)}
                    {job.customerRating && (
                      <span className="ml-2 text-gray-600">
                        {job.customerRating}/5
                      </span>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between bg-gray-50">
          <p className="text-sm text-gray-600">
            Page {currentPage} of {totalPages} • {totalCount} total results
          </p>

          <div className="flex gap-2">
            <button
              onClick={() => onPageChange(currentPage - 1)}
              disabled={currentPage === 1 || loading}
              className="flex items-center gap-1 px-3 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronLeft size={16} />
              Previous
            </button>

            <button
              onClick={() => onPageChange(currentPage + 1)}
              disabled={currentPage === totalPages || loading}
              className="flex items-center gap-1 px-3 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Next
              <ChevronRight size={16} />
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
