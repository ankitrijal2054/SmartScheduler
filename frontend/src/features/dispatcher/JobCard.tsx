/**
 * JobCard Component
 * Individual job row/card component for dispatcher dashboard
 */

import React from "react";
import { format } from "date-fns";
import { Job } from "@/types/Job";
import { JobStatusBadge } from "@/components/shared/JobStatusBadge";

interface JobCardProps {
  job: Job;
  onClick?: (job: Job) => void;
}

export const JobCard: React.FC<JobCardProps> = ({ job, onClick }) => {
  const handleClick = () => {
    onClick?.(job);
  };

  const formattedDate = format(
    new Date(job.desiredDateTime),
    "MMM dd, yyyy HH:mm"
  );

  return (
    <div
      onClick={handleClick}
      className={`rounded-lg border border-gray-200 bg-white p-4 transition-all hover:shadow-md ${
        onClick ? "cursor-pointer hover:border-blue-300" : ""
      }`}
      role="article"
      aria-label={`Job ${job.id}: ${job.customerName} - ${job.location}`}
    >
      <div className="grid grid-cols-1 gap-4 md:grid-cols-5">
        {/* Job ID & Customer */}
        <div className="flex flex-col">
          <p className="text-xs font-semibold uppercase text-gray-500">
            Job ID
          </p>
          <p className="font-mono text-sm text-gray-900">
            {job.id.slice(0, 8)}...
          </p>
          <p className="mt-2 text-xs font-semibold uppercase text-gray-500">
            Customer
          </p>
          <p className="text-sm text-gray-900">
            {job.customerName || "Unknown"}
          </p>
        </div>

        {/* Location & Type */}
        <div className="flex flex-col">
          <p className="text-xs font-semibold uppercase text-gray-500">
            Location
          </p>
          <p className="text-sm text-gray-900">{job.location}</p>
          <p className="mt-2 text-xs font-semibold uppercase text-gray-500">
            Type
          </p>
          <p className="text-sm text-gray-900">{job.jobType}</p>
        </div>

        {/* Desired DateTime */}
        <div className="flex flex-col">
          <p className="text-xs font-semibold uppercase text-gray-500">
            Desired DateTime
          </p>
          <p className="text-sm text-gray-900">{formattedDate}</p>
        </div>

        {/* Description */}
        <div className="flex flex-col">
          <p className="text-xs font-semibold uppercase text-gray-500">
            Description
          </p>
          <p className="line-clamp-2 text-sm text-gray-600">
            {job.description}
          </p>
        </div>

        {/* Status & Contractor */}
        <div className="flex flex-col items-start justify-center">
          <JobStatusBadge
            status={job.status}
            contractorName={job.assignedContractorName}
            contractorRating={job.assignedContractorRating}
          />
        </div>
      </div>
    </div>
  );
};
