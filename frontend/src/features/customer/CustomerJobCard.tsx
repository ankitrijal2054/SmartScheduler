/**
 * CustomerJobCard Component
 * Simple job card for customer dashboard showing their jobs
 */

import React from "react";
import { format } from "date-fns";
import { Job } from "@/types/Job";
import { JobStatusBadge } from "@/components/shared/JobStatusBadge";
import { useNavigate } from "react-router-dom";

interface CustomerJobCardProps {
  job: Job;
}

export const CustomerJobCard: React.FC<CustomerJobCardProps> = ({ job }) => {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate(`/customer/jobs/${job.id}`);
  };

  const formattedDate = format(
    new Date(job.desiredDateTime),
    "MMM dd, yyyy HH:mm"
  );

  return (
    <div
      onClick={handleClick}
      className="rounded-lg border border-gray-200 bg-white p-4 transition-all hover:shadow-md cursor-pointer hover:border-blue-300"
      role="article"
      aria-label={`Job ${job.id}: ${job.jobType} - ${job.location}`}
    >
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-3 mb-2">
            <h3 className="text-lg font-semibold text-gray-900">
              {job.jobType}
            </h3>
            <JobStatusBadge
              status={job.status}
              contractorName={job.assignedContractorName}
              contractorRating={job.assignedContractorRating}
            />
          </div>
          <p className="text-sm text-gray-600 mb-2">{job.location}</p>
          <p className="text-xs text-gray-500 mb-2">
            Scheduled: {formattedDate}
          </p>
          {job.description && (
            <p className="text-sm text-gray-700 line-clamp-2">
              {job.description}
            </p>
          )}
        </div>
        <div className="ml-4">
          <svg
            className="w-5 h-5 text-gray-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M9 5l7 7-7 7"
            />
          </svg>
        </div>
      </div>
    </div>
  );
};
