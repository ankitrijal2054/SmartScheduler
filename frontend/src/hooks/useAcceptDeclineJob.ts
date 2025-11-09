/**
 * useAcceptDeclineJob Hook
 * Handles accept/decline job mutations for contractor workflow
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import { useState, useCallback } from "react";
import { contractorService } from "@/services/contractorService";

interface UseAcceptDeclineJobState {
  isLoading: boolean;
  error: string | null;
}

/**
 * Hook to manage accept/decline job actions
 * Handles API calls with loading and error states
 *
 * @returns Object with accept/decline methods and state
 *
 * @example
 * const { isLoading, error, acceptJob, declineJob } = useAcceptDeclineJob();
 * // Then:
 * await acceptJob(assignmentId);
 */
export const useAcceptDeclineJob = () => {
  const [state, setState] = useState<UseAcceptDeclineJobState>({
    isLoading: false,
    error: null,
  });

  /**
   * Accept a job assignment
   * Sets status to "Accepted" and notifies dispatcher/customer via backend
   */
  const acceptJob = useCallback(async (assignmentId: string): Promise<void> => {
    setState({ isLoading: true, error: null });

    try {
      await contractorService.acceptAssignment(assignmentId);
      setState({ isLoading: false, error: null });
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to accept job";
      setState({ isLoading: false, error: errorMessage });
      throw err;
    }
  }, []);

  /**
   * Decline a job assignment
   * Optionally includes reason for decline
   * Job becomes available for other contractors
   */
  const declineJob = useCallback(
    async (assignmentId: string, reason?: string): Promise<void> => {
      setState({ isLoading: true, error: null });

      try {
        await contractorService.declineAssignment(assignmentId, reason);
        setState({ isLoading: false, error: null });
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to decline job";
        setState({ isLoading: false, error: errorMessage });
        throw err;
      }
    },
    []
  );

  return {
    isLoading: state.isLoading,
    error: state.error,
    acceptJob,
    declineJob,
  };
};
