/**
 * useJobAssignment Hook
 * Custom hook for managing job assignment workflow state and logic
 */

import { useState, useCallback, useRef, useEffect } from "react";
import axios from "axios";
import { dispatcherService } from "@/services/dispatcherService";
import {
  AssignmentResponse,
  AssignmentRequest,
  JobAssignmentState,
} from "@/types/Assignment";

/**
 * useJobAssignment hook
 * Manages assignment request state, error handling, and retry capability
 *
 * @returns Object with state and methods for job assignment
 */
export const useJobAssignment = () => {
  const [state, setState] = useState<JobAssignmentState>({
    isAssigning: false,
    error: null,
    successMessage: null,
    assignmentData: null,
  });

  // CancelToken for cleanup
  const cancelTokenSourceRef = useRef<ReturnType<
    typeof axios.CancelToken.source
  > | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

  /**
   * Assign a job to a contractor
   * @param jobId Job identifier
   * @param contractorId Contractor identifier
   */
  const assignJob = useCallback(
    async (jobId: string, contractorId: string): Promise<void> => {
      // Cancel any previous request
      if (cancelTokenSourceRef.current) {
        cancelTokenSourceRef.current.cancel("New assignment request initiated");
      }

      // Create new cancel token for this request
      cancelTokenSourceRef.current = axios.CancelToken.source();

      try {
        // Reset state and start loading
        setState((prev) => ({
          ...prev,
          isAssigning: true,
          error: null,
          successMessage: null,
          assignmentData: null,
        }));

        // Prepare request body
        const request: AssignmentRequest = {
          contractorId,
        };

        // Call API
        const response = await dispatcherService.assignJob(
          jobId,
          request,
          cancelTokenSourceRef.current.token
        );

        // Handle success
        setState((prev) => ({
          ...prev,
          isAssigning: false,
          assignmentData: response,
          successMessage: `Job assigned successfully`,
        }));
      } catch (err) {
        // Handle error (axios cancellation is not an error in this case)
        if (axios.isCancel(err)) {
          // Silently handle cancelled requests - they're expected when component unmounts or new request is initiated
          return;
        }

        const errorMessage =
          err instanceof Error ? err.message : "Failed to assign job";
        setState((prev) => ({
          ...prev,
          isAssigning: false,
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
   * Reset assignment state
   */
  const reset = useCallback(() => {
    // Don't cancel in-progress requests when resetting - let them complete
    setState({
      isAssigning: false,
      error: null,
      successMessage: null,
      assignmentData: null,
    });
  }, []);

  /**
   * Retry assignment with same parameters
   * Store last parameters for retry
   */
  const retryRef = useRef<{
    jobId: string;
    contractorId: string;
  } | null>(null);

  const retryAssignment = useCallback(async () => {
    if (retryRef.current) {
      const { jobId, contractorId } = retryRef.current;
      return assignJob(jobId, contractorId);
    }
  }, [assignJob]);

  /**
   * Update retry reference when assignJob is called
   */
  const assignJobWithRetry = useCallback(
    async (jobId: string, contractorId: string): Promise<void> => {
      retryRef.current = { jobId, contractorId };
      return assignJob(jobId, contractorId);
    },
    [assignJob]
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
    assignJob: assignJobWithRetry,
    reset,
    retry: retryAssignment,
  };
};
