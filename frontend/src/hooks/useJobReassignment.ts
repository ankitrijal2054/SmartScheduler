/**
 * useJobReassignment Hook
 * Custom hook for managing job reassignment workflow state and logic
 */

import { useState, useCallback, useRef, useEffect } from "react";
import axios from "axios";
import { dispatcherService } from "@/services/dispatcherService";
import {
  ReassignmentResponse,
  ReassignmentRequest,
  JobReassignmentState,
} from "@/types/Reassignment";

/**
 * useJobReassignment hook
 * Manages reassignment request state, error handling, and retry capability
 *
 * @returns Object with state and methods for job reassignment
 */
export const useJobReassignment = () => {
  const [state, setState] = useState<JobReassignmentState>({
    isReassigning: false,
    error: null,
    successMessage: null,
    reassignmentData: null,
  });

  // CancelToken for cleanup
  const cancelTokenSourceRef = useRef<ReturnType<
    typeof axios.CancelToken.source
  > | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

  /**
   * Reassign a job to a different contractor
   * @param jobId Job identifier
   * @param newContractorId New contractor identifier
   * @param reason Optional reason for reassignment
   */
  const reassignJob = useCallback(
    async (
      jobId: string,
      newContractorId: string,
      reason?: string
    ): Promise<void> => {
      // Cancel any previous request
      if (cancelTokenSourceRef.current) {
        cancelTokenSourceRef.current.cancel(
          "New reassignment request initiated"
        );
      }

      // Create new cancel token for this request
      cancelTokenSourceRef.current = axios.CancelToken.source();

      try {
        // Reset state and start loading
        setState((prev) => ({
          ...prev,
          isReassigning: true,
          error: null,
          successMessage: null,
          reassignmentData: null,
        }));

        // Prepare request body
        const request: ReassignmentRequest = {
          newContractorId,
          reason,
        };

        // Call API
        const response = await dispatcherService.reassignJob(
          jobId,
          request,
          cancelTokenSourceRef.current.token
        );

        // Handle success
        setState((prev) => ({
          ...prev,
          isReassigning: false,
          reassignmentData: response,
          successMessage: `Job reassigned successfully`,
        }));
      } catch (err) {
        // Handle error (axios cancellation is not an error in this case)
        if (axios.isCancel(err)) {
          // Silently handle cancelled requests - they're expected when component unmounts or new request is initiated
          return;
        }

        const errorMessage =
          err instanceof Error ? err.message : "Failed to reassign job";
        setState((prev) => ({
          ...prev,
          isReassigning: false,
          error: errorMessage,
        }));
      } finally {
        // Clear cancel token after request completes
        cancelTokenSourceRef.current = null;
      }
    },
    []
  );

  /**
   * Reset reassignment state
   */
  const reset = useCallback(() => {
    // Don't cancel in-progress requests when resetting - let them complete
    setState({
      isReassigning: false,
      error: null,
      successMessage: null,
      reassignmentData: null,
    });
  }, []);

  /**
   * Retry reassignment with same parameters
   * Store last parameters for retry
   */
  const retryRef = useRef<{
    jobId: string;
    newContractorId: string;
    reason?: string;
  } | null>(null);

  const retryReassignment = useCallback(async () => {
    if (retryRef.current) {
      const { jobId, newContractorId, reason } = retryRef.current;
      return reassignJob(jobId, newContractorId, reason);
    }
  }, [reassignJob]);

  /**
   * Update retry reference when reassignJob is called
   */
  const reassignJobWithRetry = useCallback(
    async (
      jobId: string,
      newContractorId: string,
      reason?: string
    ): Promise<void> => {
      retryRef.current = { jobId, newContractorId, reason };
      return reassignJob(jobId, newContractorId, reason);
    },
    [reassignJob]
  );

  /**
   * Cleanup on unmount
   */
  useEffect(() => {
    return () => {
      // Cancel any pending requests only on unmount
      if (cancelTokenSourceRef.current) {
        cancelTokenSourceRef.current.cancel("Component unmounted");
      }
      abortControllerRef.current?.abort();
    };
  }, []);

  return {
    ...state,
    reassignJob: reassignJobWithRetry,
    reset,
    retry: retryReassignment,
  };
};
