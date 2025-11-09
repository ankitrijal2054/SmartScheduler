/**
 * JobDetailsCard Component
 * Displays static job information: type, location, desired date/time, description, and assignment details
 */

import React from "react";
import { JobDetail } from "@/types/Job";

interface JobDetailsCardProps {
  job: JobDetail;
}

/**
 * Format date and time for display
 */
const formatDateTime = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      weekday: "short",
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return dateString;
  }
};

/**
 * Map job type to display label
 */
const getJobTypeLabel = (jobType: string): string => {
  const labels: Record<string, string> = {
    Flooring: "🏠 Flooring",
    HVAC: "❄️ HVAC",
    Plumbing: "🔧 Plumbing",
    Electrical: "⚡ Electrical",
    Other: "📋 Other",
  };
  return labels[jobType] || jobType;
};

/**
 * Map assignment status to color
 */
const getAssignmentStatusColor = (status: string): string => {
  const colors: Record<string, string> = {
    Pending: "bg-yellow-100 text-yellow-800",
    Accepted: "bg-green-100 text-green-800",
    Declined: "bg-red-100 text-red-800",
    Cancelled: "bg-gray-100 text-gray-800",
  };
  return colors[status] || "bg-gray-100 text-gray-800";
};

export const JobDetailsCard: React.FC<JobDetailsCardProps> = ({ job }) => {
  return (
    <div className="w-full bg-white rounded-lg shadow-md overflow-hidden">
      {/* Header */}
      <div className="border-b px-6 py-4 bg-gray-50">
        <div className="flex items-start justify-between">
          <div>
            <h2 className="text-lg sm:text-xl font-bold text-gray-900">
              Job Details
            </h2>
            <p className="text-xs sm:text-sm text-gray-500 mt-1">
              Job ID: {job.id.substring(0, 8)}...
            </p>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="px-6 py-6 space-y-6">
        {/* Job Type */}
        <div className="grid grid-cols-2 gap-4 sm:gap-6">
          <div>
            <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
              Job Type
            </label>
            <p className="text-sm sm:text-base font-medium text-gray-900">
              {getJobTypeLabel(job.jobType)}
            </p>
          </div>

          {/* Location */}
          <div>
            <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
              Location
            </label>
            <p className="text-sm sm:text-base font-medium text-gray-900">
              {job.location}
            </p>
          </div>
        </div>

        {/* Desired Date & Time */}
        <div>
          <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
            Desired Date & Time
          </label>
          <p className="text-sm sm:text-base font-medium text-gray-900">
            {formatDateTime(job.desiredDateTime)}
          </p>
        </div>

        {/* Description */}
        <div>
          <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
            Description
          </label>
          <p className="text-sm text-gray-700 leading-relaxed max-h-24 overflow-y-auto">
            {job.description || "No description provided"}
          </p>
        </div>

        {/* Assignment Details */}
        {job.assignment && (
          <div className="border-t pt-6">
            <h3 className="text-sm font-semibold text-gray-900 mb-4">
              Assignment Details
            </h3>

            <div className="space-y-4">
              {/* Assignment Status */}
              <div className="flex items-center justify-between">
                <label className="text-xs font-semibold text-gray-600 uppercase tracking-wide">
                  Assignment Status
                </label>
                <span
                  className={`px-3 py-1 text-xs rounded-full font-semibold ${getAssignmentStatusColor(
                    job.assignment.status
                  )}`}
                >
                  {job.assignment.status}
                </span>
              </div>

              {/* Assigned Date */}
              <div>
                <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-1">
                  Assigned Date
                </label>
                <p className="text-sm text-gray-700">
                  {formatDateTime(job.assignment.assignedAt)}
                </p>
              </div>

              {/* Accepted Date (if available) */}
              {job.assignment.acceptedAt && (
                <div>
                  <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-1">
                    Accepted Date
                  </label>
                  <p className="text-sm text-gray-700">
                    {formatDateTime(job.assignment.acceptedAt)}
                  </p>
                </div>
              )}

              {/* Completed Date (if available) */}
              {job.assignment.completedAt && (
                <div>
                  <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-1">
                    Completed Date
                  </label>
                  <p className="text-sm text-gray-700">
                    {formatDateTime(job.assignment.completedAt)}
                  </p>
                </div>
              )}

              {/* ETA (if available) */}
              {job.assignment.estimatedArrivalTime && (
                <div>
                  <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-1">
                    Estimated Arrival Time
                  </label>
                  <p className="text-sm text-gray-700">
                    {formatDateTime(job.assignment.estimatedArrivalTime)}
                  </p>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Job Created & Updated Info */}
        <div className="border-t pt-4 flex justify-between text-xs text-gray-500">
          <span>Created: {formatDateTime(job.createdAt)}</span>
          <span>Updated: {formatDateTime(job.updatedAt)}</span>
        </div>
      </div>
    </div>
  );
};
