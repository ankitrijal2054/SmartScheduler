/**
 * ReassignmentFlow Component
 * Orchestrates the job reassignment workflow
 */

import React, { useState, useCallback } from "react";
import { Job } from "@/types/Job";
import { useJobReassignment } from "@/hooks/useJobReassignment";
import { useToast, ToastContainer } from "@/components/shared/Toast";
import { RecommendationsModal } from "./RecommendationsModal";

interface ReassignmentFlowProps {
  job: Job;
  onSuccess?: () => void;
  onError?: (error: string) => void;
}

/**
 * ReassignmentFlow Component
 * Manages the reassignment workflow: show recommendations modal, confirm, execute reassignment
 */
export const ReassignmentFlow: React.FC<ReassignmentFlowProps> = ({
  job,
  onSuccess,
  onError,
}) => {
  const [showModal, setShowModal] = useState(false);
  const [selectedNewContractorId, setSelectedNewContractorId] = useState<
    string | null
  >(null);
  const [selectedNewContractorName, setSelectedNewContractorName] = useState<
    string | null
  >(null);

  const { isReassigning, error, successMessage, reassignJob, reset, retry } =
    useJobReassignment();

  const {
    toasts,
    removeToast,
    success: showSuccess,
    error: showError,
  } = useToast();

  /**
   * Handle opening recommendations modal for reassignment
   */
  const handleReassignClick = useCallback(() => {
    setShowModal(true);
    reset();
  }, [reset]);

  /**
   * Handle closing recommendations modal
   */
  const handleModalClose = useCallback(() => {
    setShowModal(false);
    setSelectedNewContractorId(null);
    setSelectedNewContractorName(null);
    reset();
  }, [reset]);

  /**
   * Handle successful reassignment selection from recommendations modal
   */
  const handleModalSuccess = useCallback(async () => {
    if (selectedNewContractorId) {
      try {
        await reassignJob(job.id, selectedNewContractorId);
      } catch (err) {
        const errorMsg =
          err instanceof Error ? err.message : "Reassignment failed";
        onError?.(errorMsg);
      }
    }
  }, [job.id, selectedNewContractorId, reassignJob, onError]);

  /**
   * Handle successful reassignment completion
   */
  React.useEffect(() => {
    if (
      !isReassigning &&
      !error &&
      successMessage &&
      selectedNewContractorName
    ) {
      showSuccess(`Job reassigned to ${selectedNewContractorName}`);
      setShowModal(false);
      setSelectedNewContractorId(null);
      setSelectedNewContractorName(null);
      onSuccess?.();
    }
  }, [
    isReassigning,
    error,
    successMessage,
    selectedNewContractorName,
    showSuccess,
    onSuccess,
  ]);

  /**
   * Handle reassignment error
   */
  React.useEffect(() => {
    if (error) {
      showError(error);
      onError?.(error);
    }
  }, [error, showError, onError]);

  // Only show Reassign button when job is assigned
  if (job.status !== "Assigned") {
    return null;
  }

  return (
    <>
      <button
        onClick={handleReassignClick}
        disabled={isReassigning}
        className="rounded-md bg-amber-50 px-3 py-1 text-xs font-medium text-amber-700 hover:bg-amber-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        aria-label="Reassign this job to a different contractor"
        type="button"
      >
        {isReassigning ? "Reassigning..." : "Reassign"}
      </button>

      {/* Recommendations Modal for Reassignment */}
      <RecommendationsModal
        isOpen={showModal}
        jobId={job.id}
        jobType={job.jobType}
        location={job.location}
        desiredDateTime={job.desiredDateTime}
        job={job}
        mode="reassign"
        currentContractorId={job.currentAssignedContractorId || undefined}
        currentContractorName={job.assignedContractorName}
        onClose={handleModalClose}
        onContractorSelect={(contractorId, contractorName) => {
          setSelectedNewContractorId(contractorId);
          setSelectedNewContractorName(contractorName);
        }}
        onAssignmentSuccess={handleModalSuccess}
      />

      {/* Toast Container */}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </>
  );
};
