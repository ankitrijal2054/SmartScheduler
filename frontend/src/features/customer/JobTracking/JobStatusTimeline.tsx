/**
 * JobStatusTimeline Component
 * Displays visual timeline of job status progression with color coding and timestamps
 */

import React from "react";
import { JobStatus } from "@/types/Job";

interface TimelineStep {
  status: JobStatus;
  label: string;
  timestamp?: string;
  color: string;
  textColor: string;
}

interface JobStatusTimelineProps {
  currentStatus: JobStatus;
  updatedAt: string;
  createdAt: string;
}

/**
 * Map job status to timeline position
 */
const getStatusOrder = (status: JobStatus): number => {
  const order: Record<JobStatus, number> = {
    Pending: 1,
    Assigned: 2,
    InProgress: 3,
    Completed: 4,
  };
  return order[status];
};

/**
 * Format timestamp to human-readable date and time
 */
const formatTimestamp = (timestamp: string): string => {
  try {
    const date = new Date(timestamp);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return timestamp;
  }
};

export const JobStatusTimeline: React.FC<JobStatusTimelineProps> = ({
  currentStatus,
  updatedAt,
  createdAt,
}) => {
  const currentStatusOrder = getStatusOrder(currentStatus);

  const timelineSteps: TimelineStep[] = [
    {
      status: "Pending",
      label: "Job Submitted",
      timestamp: createdAt,
      color: "bg-slate-200",
      textColor: "text-slate-700",
    },
    {
      status: "Assigned",
      label: "Contractor Assigned",
      timestamp: currentStatusOrder >= 2 ? updatedAt : undefined,
      color: "bg-blue-100",
      textColor: "text-blue-700",
    },
    {
      status: "InProgress",
      label: "In Progress",
      timestamp: currentStatusOrder >= 3 ? updatedAt : undefined,
      color: "bg-yellow-100",
      textColor: "text-yellow-700",
    },
    {
      status: "Completed",
      label: "Completed",
      timestamp: currentStatusOrder >= 4 ? updatedAt : undefined,
      color: "bg-green-100",
      textColor: "text-green-700",
    },
  ];

  return (
    <div className="w-full py-8 px-4">
      <div
        className="flex justify-between items-center relative"
        role="progressbar"
        aria-valuenow={currentStatusOrder}
        aria-valuemin={1}
        aria-valuemax={4}
        aria-label={`Job status: ${currentStatus}`}
      >
        {/* Background connecting line */}
        <div className="absolute top-1/2 left-0 right-0 h-1 bg-gray-200 -translate-y-1/2 z-0" />

        {/* Progress line (filled) */}
        <div
          className="absolute top-1/2 left-0 h-1 bg-green-500 -translate-y-1/2 z-0 transition-all duration-500"
          style={{
            width: `${((currentStatusOrder - 1) / 3) * 100}%`,
          }}
        />

        {/* Timeline steps */}
        {timelineSteps.map((step, index) => {
          const isCompleted = getStatusOrder(step.status) <= currentStatusOrder;
          const isCurrent = step.status === currentStatus;

          return (
            <div
              key={step.status}
              className="flex flex-col items-center z-10 flex-1"
              aria-label={`${step.label}${
                step.timestamp ? ` on ${formatTimestamp(step.timestamp)}` : ""
              }`}
            >
              {/* Status circle */}
              <div
                className={`w-12 h-12 rounded-full flex items-center justify-center font-bold text-sm transition-all duration-300 ${
                  isCurrent
                    ? `${step.color} ${
                        step.textColor
                      } ring-4 ring-offset-2 ring-${
                        step.textColor.split("-")[1]
                      }-300 scale-110`
                    : isCompleted
                    ? "bg-green-500 text-white"
                    : "bg-gray-300 text-gray-600"
                }`}
              >
                {isCompleted ? "✓" : index + 1}
              </div>

              {/* Status label */}
              <p
                className={`mt-3 font-semibold text-xs sm:text-sm text-center ${
                  isCurrent ? `${step.textColor}` : "text-gray-600"
                }`}
              >
                {step.label}
              </p>

              {/* Timestamp */}
              {step.timestamp && (
                <p className="mt-1 text-xs text-gray-500 text-center">
                  {formatTimestamp(step.timestamp)}
                </p>
              )}
            </div>
          );
        })}
      </div>

      {/* Current status badge */}
      <div className="mt-8 flex justify-center">
        <div
          className={`px-4 py-2 rounded-full font-semibold text-white transition-all duration-300 ${
            currentStatus === "Pending"
              ? "bg-slate-500"
              : currentStatus === "Assigned"
              ? "bg-blue-500"
              : currentStatus === "InProgress"
              ? "bg-yellow-500"
              : "bg-green-500"
          }`}
        >
          {currentStatus === "Pending"
            ? "Awaiting Assignment"
            : currentStatus === "Assigned"
            ? "Contractor Assigned"
            : currentStatus === "InProgress"
            ? "Work in Progress"
            : "Job Completed"}
        </div>
      </div>
    </div>
  );
};
