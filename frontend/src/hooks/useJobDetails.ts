/**
 * useJobDetails Hook
 * Fetches full job assignment details from API with caching
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import { useState, useEffect, useCallback, useRef } from "react";
import { JobDetails, UseJobDetailsState } from "@/types/JobDetails";
import { contractorService } from "@/services/contractorService";

const CACHE_DURATION_MS = 10000; // 10 seconds cache per story spec

interface CachedJobDetails {
  data: JobDetails;
  timestamp: number;
}

const cache = new Map<string, CachedJobDetails>();

/**
 * Hook to fetch detailed job assignment information
 * Used by JobDetailsModal to display full job details with customer history
 *
 * @param assignmentId - The assignment ID to fetch details for
 * @returns {UseJobDetailsState & { refetch: () => void }}
 *
 * @example
 * const { jobDetails, loading, error, refetch } = useJobDetails(assignmentId);
 */
export const useJobDetails = (assignmentId: string | null) => {
  const [state, setState] = useState<UseJobDetailsState>({
    jobDetails: null,
    loading: true,
    error: null,
  });

  const isMountedRef = useRef(true);
  const currentAssignmentIdRef = useRef(assignmentId);

  const fetchJobDetails = useCallback(
    async (forceRefresh = false) => {
      if (!assignmentId) {
        setState({ jobDetails: null, loading: false, error: null });
        return;
      }

      const now = Date.now();

      // Check cache
      if (!forceRefresh && cache.has(assignmentId)) {
        const cached = cache.get(assignmentId)!;
        if (now - cached.timestamp < CACHE_DURATION_MS) {
          if (isMountedRef.current) {
            setState({
              jobDetails: cached.data,
              loading: false,
              error: null,
            });
          }
          return;
        }
      }

      setState({ jobDetails: null, loading: true, error: null });

      try {
        const details = await contractorService.getJobDetails(assignmentId);
        if (
          isMountedRef.current &&
          assignmentId === currentAssignmentIdRef.current
        ) {
          cache.set(assignmentId, { data: details, timestamp: now });
          setState({
            jobDetails: details,
            loading: false,
            error: null,
          });
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to load job details";
        if (
          isMountedRef.current &&
          assignmentId === currentAssignmentIdRef.current
        ) {
          setState({
            jobDetails: null,
            loading: false,
            error: errorMessage,
          });
        }
      }
    },
    [assignmentId]
  );

  // Fetch on mount or when assignmentId changes
  useEffect(() => {
    isMountedRef.current = true;
    currentAssignmentIdRef.current = assignmentId;
    fetchJobDetails();

    return () => {
      isMountedRef.current = false;
    };
  }, [fetchJobDetails, assignmentId]);

  const refetch = useCallback(() => {
    fetchJobDetails(true);
  }, [fetchJobDetails]);

  return {
    ...state,
    refetch,
  };
};
