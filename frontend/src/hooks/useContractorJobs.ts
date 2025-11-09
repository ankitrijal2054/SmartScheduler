/**
 * useContractorJobs Hook
 * Fetches contractor's assigned jobs from the API with caching
 * Supports filtering by status
 */

import { useState, useEffect, useCallback, useRef } from "react";
import { Assignment } from "@/types/Assignment";
import { contractorService } from "@/services/contractorService";

interface UseContractorJobsState {
  jobs:
    | (Assignment & {
        jobType?: string;
        location?: string;
        scheduledTime?: string;
        customerName?: string;
      })[]
    | null;
  loading: boolean;
  error: string | null;
}

const CACHE_DURATION_MS = 30000; // 30 seconds

interface CachedData {
  data: UseContractorJobsState["jobs"];
  timestamp: number;
}

const cache = new Map<string, CachedData>();

// Export function to clear cache (useful for testing)
export const clearContractorJobsCache = () => {
  cache.clear();
};

export const useContractorJobs = (
  status?: "Pending" | "Accepted" | "InProgress" | "Completed"
) => {
  const [state, setState] = useState<UseContractorJobsState>({
    jobs: null,
    loading: true,
    error: null,
  });

  const isMountedRef = useRef(true);
  const cacheKeyRef = useRef(`jobs_${status || "all"}`);

  const fetchJobs = useCallback(
    async (forceRefresh = false) => {
      const cacheKey = cacheKeyRef.current;
      const now = Date.now();

      // Check cache
      if (!forceRefresh && cache.has(cacheKey)) {
        const cached = cache.get(cacheKey)!;
        if (now - cached.timestamp < CACHE_DURATION_MS) {
          if (isMountedRef.current) {
            setState({ jobs: cached.data, loading: false, error: null });
          }
          return;
        }
      }

      setState({ jobs: null, loading: true, error: null });

      try {
        const jobs = await contractorService.getAssignments(status);
        if (isMountedRef.current) {
          cache.set(cacheKey, { data: jobs, timestamp: Date.now() });
          setState({ jobs, loading: false, error: null });
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to fetch jobs";
        if (isMountedRef.current) {
          setState({ jobs: null, loading: false, error: errorMessage });
        }
      }
    },
    [status]
  );

  // Fetch on mount
  useEffect(() => {
    isMountedRef.current = true;
    fetchJobs();

    return () => {
      isMountedRef.current = false;
    };
  }, [fetchJobs]);

  const refetch = useCallback(() => {
    fetchJobs(true);
  }, [fetchJobs]);

  return {
    ...state,
    refetch,
  };
};
