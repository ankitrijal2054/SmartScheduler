/**
 * ContractorJobCard Component
 * Displays a single job assignment as a card
 * Shows job type, location, scheduled time, customer name, and status
 */

import React, { useState } from "react";
import { Assignment } from "@/types/Assignment";
import { MapPin, Clock, User } from "lucide-react";
import { formatDistanceToNow, format } from "date-fns";
import { JobDetailsModal } from "@/features/contractor/JobDetailsModal";

interface ContractorJobCardProps {
  assignment: Assignment & {
    jobType?: string;
    location?: string;
    scheduledTime?: string;
    customerName?: string;
    description?: string;
  };
}

const STATUS_CONFIG = {
  Pending: {
    badge: "bg-yellow-100 text-yellow-800",
    dot: "bg-yellow-500",
    label: "Pending",
  },
  Accepted: {
    badge: "bg-green-100 text-green-800",
    dot: "bg-green-500",
    label: "Accepted",
  },
  InProgress: {
    badge: "bg-blue-100 text-blue-800",
    dot: "bg-blue-500",
    label: "In Progress",
  },
  Completed: {
    badge: "bg-gray-100 text-gray-800",
    dot: "bg-gray-500",
    label: "Completed",
  },
};

export const ContractorJobCard: React.FC<ContractorJobCardProps> = ({
  assignment,
}) => {
  const [showDetails, setShowDetails] = useState(false);
  const statusConfig =
    STATUS_CONFIG[assignment.status as keyof typeof STATUS_CONFIG] ||
    STATUS_CONFIG.Pending;

  const scheduledDate = assignment.scheduledTime
    ? new Date(assignment.scheduledTime)
    : null;
  const timeDisplay = scheduledDate
    ? format(scheduledDate, "MMM d, h:mm a")
    : "Time not set";

  const isPending = assignment.status === "Pending";

  return (
    <>
      <div
        className={`p-4 rounded-lg border transition cursor-pointer hover:shadow-md ${
          isPending
            ? "bg-yellow-50 border-yellow-200 hover:border-yellow-300"
            : "bg-white border-gray-200 hover:border-gray-300"
        }`}
        onClick={() => setShowDetails(true)}
      >
        <div className="flex items-start justify-between">
          {/* Left: Job Info */}
          <div className="flex-1 min-w-0">
            {/* Job Type + Status */}
            <div className="flex items-center gap-3 mb-3">
              <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-indigo-100 text-indigo-800">
                {assignment.jobType || "General Job"}
              </span>
              <span
                className={`inline-flex items-center px-2 py-1 rounded text-xs font-semibold ${statusConfig.badge}`}
              >
                <span
                  className={`w-2 h-2 rounded-full mr-1.5 ${statusConfig.dot}`}
                />
                {statusConfig.label}
              </span>
            </div>

            {/* Location */}
            <div className="flex items-center gap-2 text-gray-700 mb-2">
              <MapPin size={16} className="text-gray-500 flex-shrink-0" />
              <span className="text-sm truncate">
                {assignment.location || "Location TBD"}
              </span>
            </div>

            {/* Scheduled Time */}
            <div className="flex items-center gap-2 text-gray-700 mb-2">
              <Clock size={16} className="text-gray-500 flex-shrink-0" />
              <span className="text-sm">{timeDisplay}</span>
            </div>

            {/* Customer Name */}
            <div className="flex items-center gap-2 text-gray-700">
              <User size={16} className="text-gray-500 flex-shrink-0" />
              <span className="text-sm truncate">
                {assignment.customerName || "Customer"}
              </span>
            </div>
          </div>

          {/* Right: Action Button / Arrow */}
          <div className="ml-4 flex-shrink-0">
            <button
              onClick={(e) => {
                e.stopPropagation();
                setShowDetails(true);
              }}
              className="px-3 py-2 text-sm font-medium text-indigo-600 hover:bg-indigo-50 rounded-lg transition"
            >
              View Details
            </button>
          </div>
        </div>
      </div>

      {/* Job Details Modal */}
      <JobDetailsModal
        assignmentId={assignment.id}
        isOpen={showDetails}
        onClose={() => setShowDetails(false)}
      />
    </>
  );
};
