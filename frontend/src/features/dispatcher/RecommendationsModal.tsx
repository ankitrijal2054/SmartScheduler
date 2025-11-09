/**
 * RecommendationsModal Component
 * Modal/Dialog for displaying contractor recommendations with sorting and filtering
 */

import React, { useEffect, useState } from "react";
import {
  RecommendationRequest,
  RecommendedContractor,
  SortField,
} from "@/types/Contractor";
import { Job } from "@/types/Job";
import { ReassignmentMode } from "@/types/Reassignment";
import { useRecommendations } from "@/hooks/useRecommendations";
import { useJobAssignment } from "@/hooks/useJobAssignment";
import { useJobReassignment } from "@/hooks/useJobReassignment";
import { useToast, ToastContainer } from "@/components/shared/Toast";
import { ContractorRecommendationCard } from "./ContractorRecommendationCard";
import { AssignmentConfirmationDialog } from "./AssignmentConfirmationDialog";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

interface RecommendationsModalProps {
  isOpen: boolean;
  jobId: string;
  jobType: string;
  location: string;
  desiredDateTime: string;
  contractorListOnly?: boolean;
  job?: Job | null;
  mode?: ReassignmentMode; // "assign" (default) or "reassign"
  currentContractorId?: string; // For reassignment mode: current assigned contractor
  currentContractorName?: string | null; // For reassignment mode: current contractor name
  onClose: () => void;
  onAssignmentSuccess?: () => void;
  onContractorSelect?: (contractorId: string, contractorName: string) => void;
  onContractorProfileClick?: (contractorId: string) => void; // For opening contractor profile
}

/**
 * Skeleton loader for contractor cards
 */
const SkeletonCard: React.FC = () => (
  <div className="animate-pulse space-y-3 rounded-lg border border-gray-200 bg-white p-4">
    <div className="flex items-start justify-between gap-3">
      <div className="flex-1 space-y-2">
        <div className="h-5 w-32 rounded bg-gray-300" />
        <div className="h-3 w-20 rounded bg-gray-200" />
      </div>
      <div className="h-10 w-10 rounded-full bg-gray-300" />
    </div>
    <div className="h-5 w-40 rounded bg-gray-300" />
    <div className="flex gap-4">
      <div className="h-4 w-24 rounded bg-gray-200" />
      <div className="h-4 w-24 rounded bg-gray-200" />
    </div>
    <div className="h-8 rounded bg-green-100" />
  </div>
);

export const RecommendationsModal: React.FC<RecommendationsModalProps> = ({
  isOpen,
  jobId,
  jobType,
  location,
  desiredDateTime,
  contractorListOnly = false,
  job = null,
  mode = "assign", // default to assignment mode
  currentContractorId,
  currentContractorName,
  onClose,
  onAssignmentSuccess,
  onContractorSelect,
  onContractorProfileClick,
}) => {
  const {
    recommendations,
    loading,
    error,
    sortBy,
    fetchRecommendations,
    sortRecommendations,
    retry,
    cleanup,
  } = useRecommendations();

  const {
    isAssigning,
    error: assignmentError,
    successMessage,
    assignJob,
    reset: resetAssignment,
    retry: retryAssignment,
  } = useJobAssignment();

  const {
    isReassigning,
    error: reassignmentError,
    successMessage: reassignmentSuccess,
    reassignJob,
    reset: resetReassignment,
    retry: retryReassignment,
  } = useJobReassignment();

  const {
    toasts,
    removeToast,
    success: showSuccess,
    error: showError,
  } = useToast();

  // Determine if we're in reassignment mode
  const isReassignmentMode = mode === "reassign";

  // Confirmation dialog state
  const [confirmationOpen, setConfirmationOpen] = useState(false);
  const [selectedContractor, setSelectedContractor] =
    useState<RecommendedContractor | null>(null);

  // Fetch recommendations when modal opens
  useEffect(() => {
    if (!isOpen) {
      // Cleanup when modal closes
      cleanup();
      return;
    }

    const request: RecommendationRequest = {
      jobId,
      jobType: jobType as any, // Already validated from Job type
      location,
      desiredDateTime,
      contractor_list_only: contractorListOnly,
    };
    fetchRecommendations(request);

    // Cleanup on unmount
    return () => {
      cleanup();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    isOpen,
    jobId,
    jobType,
    location,
    desiredDateTime,
    contractorListOnly,
    // Note: fetchRecommendations and cleanup are stable callbacks from useRecommendations hook
  ]);

  // Close modal on Escape key
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, onClose]);

  // Reset confirmation dialog when modal closes
  useEffect(() => {
    if (!isOpen) {
      setConfirmationOpen(false);
      setSelectedContractor(null);
      resetAssignment();
    }
  }, [isOpen, resetAssignment]);

  // Handle assign button click
  const handleAssignClick = (contractor: RecommendedContractor) => {
    setSelectedContractor(contractor);
    // Notify parent component about contractor selection if in reassignment mode
    if (isReassignmentMode) {
      onContractorSelect?.(contractor.contractorId, contractor.name);
    }
    setConfirmationOpen(true);
  };

  // Handle confirmation dialog close
  const handleConfirmationClose = () => {
    setConfirmationOpen(false);
    setSelectedContractor(null);
    resetAssignment();
    resetReassignment();
  };

  // Handle assignment confirmation
  const handleAssignmentConfirm = async () => {
    if (selectedContractor && job) {
      if (isReassignmentMode) {
        // Reassignment mode: call reassignJob hook
        await reassignJob(jobId, selectedContractor.contractorId);
      } else {
        // Assignment mode: call assignJob hook
        await assignJob(jobId, selectedContractor.contractorId);
      }
    }
  };

  // Show toast for assignment/reassignment success
  useEffect(() => {
    if (successMessage && selectedContractor) {
      const action = isReassignmentMode ? "reassigned to" : "assigned to";
      showSuccess(`Job ${action} ${selectedContractor.name}`);
    }
    if (reassignmentSuccess && selectedContractor) {
      const action = isReassignmentMode ? "reassigned to" : "assigned to";
      showSuccess(`Job ${action} ${selectedContractor.name}`);
    }
  }, [
    successMessage,
    reassignmentSuccess,
    selectedContractor,
    showSuccess,
    isReassignmentMode,
  ]);

  // Show toast for assignment/reassignment error
  useEffect(() => {
    if (assignmentError && selectedContractor && !isReassignmentMode) {
      showError(assignmentError);
    }
    if (reassignmentError && selectedContractor && isReassignmentMode) {
      showError(reassignmentError);
    }
  }, [
    assignmentError,
    reassignmentError,
    selectedContractor,
    showError,
    isReassignmentMode,
  ]);

  // Handle assignment/reassignment success
  useEffect(() => {
    const isLoading = isReassignmentMode ? isReassigning : isAssigning;
    const hasError = isReassignmentMode ? reassignmentError : assignmentError;
    const message = isReassignmentMode ? reassignmentSuccess : successMessage;

    if (!isLoading && !hasError && selectedContractor && message) {
      // Assignment/Reassignment was successful
      handleConfirmationClose();
      onClose(); // Close recommendations modal
      onAssignmentSuccess?.(); // Notify parent to refresh job list
    }
  }, [
    isReassigning,
    isAssigning,
    reassignmentError,
    assignmentError,
    selectedContractor,
    reassignmentSuccess,
    successMessage,
    onClose,
    onAssignmentSuccess,
    isReassignmentMode,
  ]);

  if (!isOpen) return null;

  const handleBackdropClick = (e: React.MouseEvent) => {
    // Close only if clicking directly on the backdrop
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 z-40 bg-black bg-opacity-50 transition-opacity"
        onClick={handleBackdropClick}
        role="presentation"
      />

      {/* Modal */}
      <div
        className="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
        aria-labelledby="recommendations-title"
        aria-describedby="recommendations-desc"
      >
        <div className="max-h-[90vh] w-full max-w-3xl overflow-hidden rounded-lg bg-white shadow-xl">
          {/* Header */}
          <div className="sticky top-0 z-10 border-b border-gray-200 bg-white px-6 py-4">
            <div className="flex items-center justify-between">
              <div className="flex-1">
                <h2
                  id="recommendations-title"
                  className="text-xl font-bold text-gray-900"
                >
                  {isReassignmentMode
                    ? "Select Replacement Contractor"
                    : "Contractor Recommendations"}
                </h2>
                <p
                  id="recommendations-desc"
                  className="mt-1 text-sm text-gray-600"
                >
                  {isReassignmentMode
                    ? currentContractorName
                      ? `Current contractor: ${currentContractorName}`
                      : "Select a new contractor for reassignment"
                    : "Top recommended contractors for this job"}
                </p>
              </div>

              {/* Close Button */}
              <button
                onClick={onClose}
                className="rounded-lg p-2 text-gray-500 hover:bg-gray-100 hover:text-gray-700"
                aria-label="Close recommendations modal"
                type="button"
              >
                <span className="text-2xl">✕</span>
              </button>
            </div>

            {/* Sorting Dropdown */}
            {recommendations.length > 0 && !loading && (
              <div className="mt-4 flex items-center gap-2">
                <label
                  htmlFor="sort-field"
                  className="text-sm font-medium text-gray-700"
                >
                  Sort by:
                </label>
                <select
                  id="sort-field"
                  value={sortBy}
                  onChange={(e) =>
                    sortRecommendations(e.target.value as SortField)
                  }
                  className="rounded border border-gray-300 bg-white px-3 py-2 text-sm font-medium text-gray-900 hover:border-gray-400 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  aria-label="Sort recommendations by field"
                >
                  <option value="rank">Rank (Default)</option>
                  <option value="rating">Highest Rating</option>
                  <option value="distance">Nearest Distance</option>
                  <option value="travelTime">Shortest Travel Time</option>
                </select>
              </div>
            )}
          </div>

          {/* Content */}
          <div className="overflow-y-auto p-6">
            {/* Loading State */}
            {loading && (
              <div className="space-y-4">
                <div className="flex flex-col items-center gap-3">
                  <LoadingSpinner />
                  <p className="text-sm text-gray-600">
                    Fetching contractor recommendations...
                  </p>
                </div>
                <div className="space-y-4">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <SkeletonCard key={i} />
                  ))}
                </div>
              </div>
            )}

            {/* Error State */}
            {error && !loading && (
              <div className="rounded-lg border border-red-200 bg-red-50 p-4">
                <h3 className="font-semibold text-red-900">
                  Failed to Load Recommendations
                </h3>
                <p className="mt-1 text-sm text-red-800">{error}</p>
                <button
                  onClick={() => retry()}
                  className="mt-3 rounded bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700"
                  type="button"
                >
                  Try Again
                </button>
              </div>
            )}

            {/* Empty State */}
            {recommendations.length === 0 && !loading && !error && (
              <div className="rounded-lg border border-gray-200 bg-gray-50 p-8 text-center">
                <p className="text-lg font-semibold text-gray-900">
                  No Available Contractors
                </p>
                <p className="mt-2 text-gray-600">
                  No contractors are available for this time slot. Please try a
                  different time.
                </p>
              </div>
            )}

            {/* Recommendations List */}
            {recommendations.length > 0 && !loading && (
              <div className="space-y-4">
                {recommendations.map((contractor) => {
                  const isCurrentlyProcessing = isReassignmentMode
                    ? isReassigning
                    : isAssigning;
                  return (
                    <ContractorRecommendationCard
                      key={contractor.contractorId}
                      contractor={contractor}
                      onClick={() =>
                        onContractorProfileClick?.(contractor.contractorId)
                      }
                      onAssign={handleAssignClick}
                      isAssigning={
                        isCurrentlyProcessing &&
                        selectedContractor?.contractorId ===
                          contractor.contractorId
                      }
                    />
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Assignment Confirmation Dialog */}
      <AssignmentConfirmationDialog
        isOpen={confirmationOpen}
        contractor={selectedContractor}
        job={job}
        isAssigning={isReassignmentMode ? isReassigning : isAssigning}
        error={isReassignmentMode ? reassignmentError : assignmentError}
        onConfirm={handleAssignmentConfirm}
        onCancel={handleConfirmationClose}
        onRetry={isReassignmentMode ? retryReassignment : retryAssignment}
        mode={mode}
        currentContractorName={currentContractorName}
      />

      {/* Toast Container */}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </>
  );
};
