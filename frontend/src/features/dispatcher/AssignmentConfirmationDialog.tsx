/**
 * AssignmentConfirmationDialog Component
 * Modal confirmation dialog for job assignment with contractor details
 */

import React, { useEffect } from "react";
import { format } from "date-fns";
import { RecommendedContractor } from "@/types/Contractor";
import { Job } from "@/types/Job";
import { ReassignmentMode } from "@/types/Reassignment";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

interface AssignmentConfirmationDialogProps {
  isOpen: boolean;
  contractor: RecommendedContractor | null;
  job: Job | null;
  isAssigning: boolean;
  error: string | null;
  onConfirm: () => void;
  onCancel: () => void;
  onRetry: () => void;
  mode?: ReassignmentMode; // "assign" (default) or "reassign"
  currentContractorName?: string | null; // For reassignment mode
}

/**
 * Helper to format job details
 */
const formatJobDetails = (job: Job): string => {
  try {
    const date = new Date(job.desiredDateTime);
    return `${job.jobType} - ${format(date, "MMM d, h:mm a")}`;
  } catch {
    return `${job.jobType} - ${job.desiredDateTime}`;
  }
};

/**
 * Helper to format contractor info
 */
const formatContractorInfo = (contractor: RecommendedContractor): string => {
  const rating =
    contractor.avgRating !== null ? contractor.avgRating.toFixed(1) : "N/A";
  return `${contractor.name} - ${rating} ⭐ (${contractor.distance.toFixed(
    1
  )} mi)`;
};

export const AssignmentConfirmationDialog: React.FC<
  AssignmentConfirmationDialogProps
> = ({
  isOpen,
  contractor,
  job,
  isAssigning,
  error,
  onConfirm,
  onCancel,
  onRetry,
  mode = "assign",
  currentContractorName,
}) => {
  const isReassignmentMode = mode === "reassign";
  // Close dialog on Escape key
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !isAssigning) {
        onCancel();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, isAssigning, onCancel]);

  if (!isOpen || !contractor || !job) return null;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 z-50 bg-black bg-opacity-50 transition-opacity"
        role="presentation"
      />

      {/* Dialog */}
      <div
        className="fixed inset-0 z-60 flex items-center justify-center p-4"
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="assignment-dialog-title"
        aria-describedby="assignment-dialog-desc"
      >
        <div className="w-full max-w-md transform rounded-lg bg-white shadow-xl transition-all">
          {/* Header */}
          <div className="border-b border-gray-200 px-6 py-4">
            <h2
              id="assignment-dialog-title"
              className="text-lg font-bold text-gray-900"
            >
              {isReassignmentMode
                ? "Confirm Reassignment"
                : "Confirm Assignment"}
            </h2>
          </div>

          {/* Content */}
          <div className="space-y-4 px-6 py-4">
            {/* Error State */}
            {error && (
              <div className="rounded-md border border-red-200 bg-red-50 p-3">
                <p className="text-sm text-red-800">
                  <span className="font-semibold">Error: </span>
                  {error}
                </p>
              </div>
            )}

            {/* Confirmation Message */}
            {!error && (
              <>
                <p
                  id="assignment-dialog-desc"
                  className="text-sm text-gray-700"
                >
                  {isReassignmentMode ? (
                    <>
                      Reassign from{" "}
                      <span className="font-semibold">
                        {currentContractorName}
                      </span>{" "}
                      to{" "}
                      <span className="font-semibold">{contractor.name}</span>?
                    </>
                  ) : (
                    <>
                      Assign{" "}
                      <span className="font-semibold">{contractor.name}</span>{" "}
                      to this job?
                    </>
                  )}
                </p>

                {/* Contractor Details */}
                <div className="rounded-md bg-blue-50 p-4 space-y-3">
                  <div>
                    <p className="text-xs font-semibold text-blue-900">
                      CONTRACTOR
                    </p>
                    <p className="mt-1 text-sm text-blue-800">
                      {contractor.name}
                    </p>
                    <p className="mt-1 flex items-center gap-1 text-sm text-blue-800">
                      <span>⭐</span>
                      {contractor.avgRating !== null
                        ? `${contractor.avgRating.toFixed(1)} (${
                            contractor.reviewCount
                          } reviews)`
                        : "No ratings yet"}
                    </p>
                    <p className="mt-1 text-sm text-blue-800">
                      📍 {contractor.distance.toFixed(1)} miles away
                    </p>
                    <p className="mt-1 text-sm text-blue-800">
                      🕐 {contractor.travelTime} min travel time
                    </p>
                  </div>
                </div>

                {/* Job Details */}
                <div className="rounded-md bg-green-50 p-4 space-y-2">
                  <p className="text-xs font-semibold text-green-900">
                    JOB DETAILS
                  </p>
                  <p className="text-sm text-green-800">
                    <span className="font-semibold">{job.jobType}</span> - Job
                    ID: {job.id.slice(0, 8)}...
                  </p>
                  <p className="text-sm text-green-800">📍 {job.location}</p>
                  <p className="text-sm text-green-800">
                    🕐 {format(new Date(job.desiredDateTime), "MMM d, h:mm a")}
                  </p>
                </div>
              </>
            )}
          </div>

          {/* Footer */}
          <div className="border-t border-gray-200 bg-gray-50 px-6 py-4">
            <div className="flex gap-3">
              {error ? (
                <>
                  <button
                    onClick={onCancel}
                    className="flex-1 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                    disabled={isAssigning}
                    type="button"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={onRetry}
                    className="flex-1 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 flex items-center justify-center gap-2"
                    disabled={isAssigning}
                    type="button"
                    aria-label="Retry assignment"
                  >
                    {isAssigning && <LoadingSpinner />}
                    {isAssigning ? "Retrying..." : "Retry"}
                  </button>
                </>
              ) : (
                <>
                  <button
                    onClick={onCancel}
                    className="flex-1 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                    disabled={isAssigning}
                    type="button"
                    aria-label="Cancel assignment"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={onConfirm}
                    className="flex-1 rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50 flex items-center justify-center gap-2"
                    disabled={isAssigning}
                    type="button"
                    aria-label={
                      isReassignmentMode
                        ? "Confirm reassignment"
                        : "Confirm assignment"
                    }
                  >
                    {isAssigning && <LoadingSpinner />}
                    {isAssigning
                      ? isReassignmentMode
                        ? "Reassigning..."
                        : "Assigning..."
                      : "Confirm"}
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
};
