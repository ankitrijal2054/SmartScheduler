/**
 * useStatusTransition Hook
 * Handles marking jobs as in-progress and completed
 * Story 5.3: Job Status Management (In-Progress & Completion)
 */

import { useState, useCallback } from "react";
import { contractorService } from "@/services/contractorService";
import { Assignment } from "@/types/Assignment";

interface UseStatusTransitionState {
  isLoading: boolean;
  error: string | null;
}

/**
 * Hook to manage status transitions for assignments
 * Handles API calls with loading and error states
 *
 * @returns Object with markInProgress/markComplete methods and state
 *
 * @example
 * const { isLoading, error, markInProgress, markComplete } = useStatusTransition();
 * // Then:
 * await markInProgress(assignmentId);
 * await markComplete(assignmentId);
 */
export const useStatusTransition = () => {
  const [state, setState] = useState<UseStatusTransitionState>({
    isLoading: false,
    error: null,
  });

  /**
   * Mark a job assignment as in-progress
   * Transitions from 'Accepted' to 'InProgress'
   * Publishes JobInProgressEvent for customer notification
   */
  const markInProgress = useCallback(
    async (assignmentId: string): Promise<Assignment> => {
      setState({ isLoading: true, error: null });

      try {
        const result = await contractorService.markInProgress(assignmentId);
        setState({ isLoading: false, error: null });
        return result;
      } catch (err) {
        const errorMessage =
          err instanceof Error
            ? err.message
            : "Failed to mark job as in-progress";
        setState({ isLoading: false, error: errorMessage });
        throw err;
      }
    },
    []
  );

  /**
   * Mark a job assignment as completed
   * Transitions from 'InProgress' to 'Completed'
   * Publishes JobCompletedEvent to trigger email notification
   */
  const markComplete = useCallback(
    async (assignmentId: string): Promise<Assignment> => {
      setState({ isLoading: true, error: null });

      try {
        const result = await contractorService.markComplete(assignmentId);
        setState({ isLoading: false, error: null });
        return result;
      } catch (err) {
        const errorMessage =
          err instanceof Error
            ? err.message
            : "Failed to mark job as completed";
        setState({ isLoading: false, error: errorMessage });
        throw err;
      }
    },
    []
  );

  return {
    isLoading: state.isLoading,
    error: state.error,
    markInProgress,
    markComplete,
  };
};
