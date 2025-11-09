/**
 * JobDetailsModal Component
 * Displays full details of a job assignment in a modal
 * Includes accept/decline workflow with customer profile and history
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 * Story 5.3: Job Status Management (In-Progress & Completion)
 */

import React, { useState, useEffect } from "react";
import { X } from "lucide-react";
import { useJobDetails } from "@/hooks/useJobDetails";
import { useAcceptDeclineJob } from "@/hooks/useAcceptDeclineJob";
import { useStatusTransition } from "@/hooks/useStatusTransition";
import { JobInfoSection } from "@/components/shared/JobInfoSection";
import { CustomerProfileCard } from "./CustomerProfileCard";
import { DeclineReasonModal } from "./DeclineReasonModal";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { Toast } from "@/components/shared/Toast";

interface JobDetailsModalProps {
  assignmentId: string | null;
  isOpen: boolean;
  onClose: () => void;
}

type ToastType = "success" | "error" | "info";

export const JobDetailsModal: React.FC<JobDetailsModalProps> = ({
  assignmentId,
  isOpen,
  onClose,
}) => {
  const [showDeclineModal, setShowDeclineModal] = useState(false);
  const [toastId] = useState(Math.random().toString(36).substr(2, 9));
  const [toast, setToast] = useState<{
    type: ToastType;
    message: string;
  } | null>(null);

  // Fetch job details
  const { jobDetails, loading, error, refetch } = useJobDetails(
    isOpen ? assignmentId : null
  );

  // Handle accept/decline
  const {
    isLoading,
    error: actionError,
    acceptJob,
    declineJob,
  } = useAcceptDeclineJob();

  // Handle status transitions (in-progress, completed)
  const {
    isLoading: isTransitioning,
    error: transitionError,
    markInProgress,
    markComplete,
  } = useStatusTransition();

  // Close toast after 3 seconds
  useEffect(() => {
    if (toast) {
      const timer = setTimeout(() => setToast(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [toast]);

  const handleMarkInProgress = async () => {
    if (!assignmentId) return;

    try {
      await markInProgress(assignmentId);
      setToast({ type: "success", message: "Job marked as in-progress! 🚀" });
      setTimeout(() => {
        onClose();
        window.location.reload(); // Refresh to update job status
      }, 500);
    } catch (err) {
      setToast({
        type: "error",
        message: "Failed to mark job as in-progress. Please try again.",
      });
    }
  };

  const handleMarkComplete = async () => {
    if (!assignmentId) return;

    try {
      await markComplete(assignmentId);
      setToast({ type: "success", message: "Job marked as completed! ✅" });
      setTimeout(() => {
        onClose();
        window.location.reload(); // Refresh to update job status
      }, 500);
    } catch (err) {
      setToast({
        type: "error",
        message: "Failed to mark job as completed. Please try again.",
      });
    }
  };

  if (!isOpen) return null;

  const handleAccept = async () => {
    if (!assignmentId) return;

    try {
      await acceptJob(assignmentId);
      setToast({ type: "success", message: "Job accepted! 🎉" });
      setTimeout(onClose, 500);
    } catch (err) {
      setToast({
        type: "error",
        message: "Failed to accept job. Please try again.",
      });
    }
  };

  const handleDeclineConfirm = async (reason?: string) => {
    if (!assignmentId) return;

    try {
      await declineJob(assignmentId, reason);
      setToast({ type: "success", message: "Job declined." });
      setShowDeclineModal(false);
      setTimeout(onClose, 500);
    } catch (err) {
      setToast({
        type: "error",
        message: "Failed to decline job. Please try again.",
      });
    }
  };

  // Loading state
  if (loading) {
    return (
      <>
        <div className="fixed inset-0 bg-black bg-opacity-50 z-40" />
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl p-8 flex items-center justify-center">
            <LoadingSpinner size="lg" />
          </div>
        </div>
      </>
    );
  }

  // Error state
  if (error) {
    return (
      <>
        <div
          className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
          onClick={onClose}
          role="presentation"
        />
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl">
            <div className="flex items-center justify-between p-6 border-b border-gray-200">
              <h2 className="text-lg font-semibold text-gray-900">Error</h2>
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600 transition"
                aria-label="Close"
              >
                <X size={20} />
              </button>
            </div>
            <div className="p-6 space-y-4">
              <p className="text-red-600 text-sm">{error}</p>
              <div className="flex gap-3">
                <button
                  onClick={onClose}
                  className="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Close
                </button>
                <button
                  onClick={() => refetch()}
                  className="flex-1 px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg"
                >
                  Retry
                </button>
              </div>
            </div>
          </div>
        </div>
      </>
    );
  }

  if (!jobDetails) {
    return null;
  }

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={onClose}
        role="presentation"
      />

      {/* Modal */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200 sticky top-0 bg-white z-10">
            <h2 className="text-xl font-bold text-gray-900">Job Details</h2>
            <button
              onClick={onClose}
              disabled={isLoading}
              className="text-gray-400 hover:text-gray-600 transition disabled:opacity-50"
              aria-label="Close modal"
            >
              <X size={20} />
            </button>
          </div>

          {/* Content */}
          <div className="p-6 space-y-8">
            {/* Job Information */}
            <section>
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-4">
                Job Information
              </h3>
              <JobInfoSection jobDetails={jobDetails} showDescription={true} />
            </section>

            {/* Customer Profile */}
            <section>
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wide mb-4">
                Customer Information
              </h3>
              <CustomerProfileCard
                customer={jobDetails.customer}
                pastReviews={jobDetails.pastReviews}
              />
            </section>
          </div>

          {/* Actions Footer */}
          <div className="p-6 border-t border-gray-200 bg-gray-50 flex gap-3 sticky bottom-0">
            <button
              onClick={onClose}
              disabled={isLoading}
              className="flex-1 px-4 py-3 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition disabled:opacity-50"
              aria-label="Close without action"
            >
              Close
            </button>

            {jobDetails.status === "Pending" && (
              <>
                <button
                  onClick={() => setShowDeclineModal(true)}
                  disabled={isLoading}
                  className="flex-1 px-4 py-3 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition disabled:opacity-50 flex items-center justify-center gap-2"
                  aria-label="Decline job"
                >
                  {isLoading ? (
                    <>
                      <LoadingSpinner size="sm" />
                      Declining...
                    </>
                  ) : (
                    "Decline"
                  )}
                </button>

                <button
                  onClick={handleAccept}
                  disabled={isLoading}
                  className="flex-1 px-4 py-3 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-lg transition disabled:opacity-50 flex items-center justify-center gap-2"
                  aria-label="Accept job"
                >
                  {isLoading ? (
                    <>
                      <LoadingSpinner size="sm" />
                      Accepting...
                    </>
                  ) : (
                    "Accept"
                  )}
                </button>
              </>
            )}

            {/* Status-based actions */}
            {jobDetails.status === "Accepted" && (
              <button
                onClick={handleMarkInProgress}
                disabled={isTransitioning}
                className="flex-1 px-4 py-3 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition disabled:opacity-50 flex items-center justify-center gap-2"
                aria-label="Mark job as in-progress"
              >
                {isTransitioning ? (
                  <>
                    <LoadingSpinner size="sm" />
                    Starting...
                  </>
                ) : (
                  "Mark In Progress"
                )}
              </button>
            )}

            {jobDetails.status === "InProgress" && (
              <button
                onClick={handleMarkComplete}
                disabled={isTransitioning}
                className="flex-1 px-4 py-3 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-lg transition disabled:opacity-50 flex items-center justify-center gap-2"
                aria-label="Mark job as completed"
              >
                {isTransitioning ? (
                  <>
                    <LoadingSpinner size="sm" />
                    Completing...
                  </>
                ) : (
                  "Mark Complete"
                )}
              </button>
            )}

            {jobDetails.status === "Completed" && (
              <div className="flex-1 px-4 py-3 text-sm font-medium text-gray-700 bg-green-50 rounded-lg flex items-center justify-center border border-green-200">
                ✅ <span className="ml-2 font-semibold">Completed</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Decline Reason Modal */}
      <DeclineReasonModal
        isOpen={showDeclineModal}
        isLoading={isLoading}
        onConfirm={handleDeclineConfirm}
        onCancel={() => setShowDeclineModal(false)}
      />

      {/* Toast Notifications */}
      {toast && (
        <Toast
          id={toastId}
          type={toast.type}
          message={toast.message}
          onClose={() => setToast(null)}
        />
      )}
    </>
  );
};
