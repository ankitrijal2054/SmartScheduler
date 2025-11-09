/**
 * JobStatusBadge Component
 * Status indicator with semantic coloring
 */

import React from "react";
import { JobStatus } from "@/types/Job";

interface JobStatusBadgeProps {
  status: JobStatus;
  contractorName?: string;
  contractorRating?: number;
}

const statusStyles: Record<
  JobStatus,
  { bg: string; text: string; label: string }
> = {
  Pending: { bg: "bg-yellow-100", text: "text-yellow-800", label: "Pending" },
  Assigned: { bg: "bg-blue-100", text: "text-blue-800", label: "Assigned" },
  InProgress: {
    bg: "bg-purple-100",
    text: "text-purple-800",
    label: "In Progress",
  },
  Completed: { bg: "bg-green-100", text: "text-green-800", label: "Completed" },
};

export const JobStatusBadge: React.FC<JobStatusBadgeProps> = ({
  status,
  contractorName,
  contractorRating,
}) => {
  const style = statusStyles[status] || statusStyles.Pending;
  const ratingText = contractorRating
    ? `(${contractorRating.toFixed(1)}★)`
    : "";

  return (
    <div
      className={`inline-flex flex-col gap-1 rounded-md px-3 py-2 text-sm font-medium ${style.bg} ${style.text}`}
    >
      <div className="flex items-center gap-2">
        <span aria-label={`Job status: ${status}`}>{style.label}</span>
      </div>
      {contractorName && (
        <div className="text-xs opacity-75">
          {contractorName} {ratingText}
        </div>
      )}
    </div>
  );
};
