/**
 * JobInfoSection Component
 * Displays job details (type, location, time, duration, pay, description)
 * Reusable component for job modals and cards
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import React from "react";
import { MapPin, Clock, DollarSign, FileText } from "lucide-react";
import { format, parse } from "date-fns";
import { JobDetails } from "@/types/JobDetails";

interface JobInfoSectionProps {
  jobDetails: JobDetails;
  showDescription?: boolean;
}

/**
 * Get badge color based on job type
 */
const getJobTypeBadgeColor = (
  jobType: string
):
  | "bg-blue-100 text-blue-800"
  | "bg-orange-100 text-orange-800"
  | "bg-green-100 text-green-800"
  | "bg-purple-100 text-purple-800"
  | "bg-gray-100 text-gray-800" => {
  switch (jobType.toUpperCase()) {
    case "FLOORING":
      return "bg-blue-100 text-blue-800";
    case "HVAC":
      return "bg-orange-100 text-orange-800";
    case "PLUMBING":
      return "bg-green-100 text-green-800";
    case "ELECTRICAL":
      return "bg-purple-100 text-purple-800";
    default:
      return "bg-gray-100 text-gray-800";
  }
};

/**
 * Format job datetime to human-readable string
 */
const formatJobDateTime = (dateTimeStr: string): string => {
  try {
    // Try parsing as ISO string first
    const date = new Date(dateTimeStr);
    if (!isNaN(date.getTime())) {
      return format(date, "EEEE, MMMM d, yyyy 'at' h:mm a");
    }
  } catch (e) {
    // Fall through
  }
  return dateTimeStr;
};

/**
 * Format duration minutes to human-readable string
 */
const formatDuration = (minutes: number | undefined | null): string => {
  if (!minutes) return "Duration not specified";

  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;

  if (hours > 0 && mins > 0) {
    return `~${hours}h ${mins}m`;
  } else if (hours > 0) {
    return `~${hours}h`;
  } else {
    return `~${mins}m`;
  }
};

/**
 * Format pay amount to currency string
 */
const formatPay = (amount: number | undefined | null): string => {
  if (!amount) return "TBD";
  return `$${amount.toFixed(2)}`;
};

export const JobInfoSection: React.FC<JobInfoSectionProps> = ({
  jobDetails,
  showDescription = true,
}) => {
  const badgeColor = getJobTypeBadgeColor(jobDetails.jobType);

  return (
    <div className="space-y-5">
      {/* Job Type Badge */}
      <div>
        <span
          className={`inline-flex items-center px-4 py-2 rounded-full text-sm font-semibold ${badgeColor}`}
        >
          🔧 {jobDetails.jobType}
        </span>
      </div>

      {/* Location */}
      <div className="flex items-start gap-3">
        <MapPin size={20} className="text-gray-400 mt-1 flex-shrink-0" />
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
            Location
          </p>
          <p className="text-base font-medium text-gray-900 mt-1">
            {jobDetails.location}
          </p>
        </div>
      </div>

      {/* Scheduled Date & Time */}
      <div className="flex items-start gap-3">
        <Clock size={20} className="text-gray-400 mt-1 flex-shrink-0" />
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
            Scheduled Time
          </p>
          <p className="text-base font-medium text-gray-900 mt-1">
            {formatJobDateTime(jobDetails.desiredDateTime)}
          </p>
          {jobDetails.estimatedDuration && (
            <p className="text-sm text-gray-600 mt-2">
              📋 Duration: {formatDuration(jobDetails.estimatedDuration)}
            </p>
          )}
        </div>
      </div>

      {/* Estimated Pay */}
      <div className="flex items-start gap-3">
        <DollarSign size={20} className="text-gray-400 mt-1 flex-shrink-0" />
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
            Estimated Pay
          </p>
          <p className="text-lg font-semibold text-green-600 mt-1">
            {formatPay(jobDetails.estimatedPay)}
          </p>
        </div>
      </div>

      {/* Description */}
      {showDescription && jobDetails.description && (
        <div className="flex items-start gap-3">
          <FileText size={20} className="text-gray-400 mt-1 flex-shrink-0" />
          <div className="flex-1">
            <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
              Description
            </p>
            <p className="text-sm text-gray-700 mt-2 bg-gray-50 p-3 rounded-lg border border-gray-200">
              {jobDetails.description}
            </p>
          </div>
        </div>
      )}
    </div>
  );
};
