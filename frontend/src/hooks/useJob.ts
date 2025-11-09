/**
 * useJob Hook
 * Custom hook for fetching and managing a single job's details including contractor and assignment info
 */

import { useState, useEffect, useCallback, useRef } from "react";
import axios from "axios";
import { JobDetail } from "@/types/Job";
import { customerService } from "@/services/customerService";

interface UseJobState {
  job: JobDetail | null;
  loading: boolean;
  error: string | null;
}

interface UseJobReturn extends UseJobState {
  fetchJob: () => Promise<void>;
  refreshJob: () => Promise<void>;
}

export const useJob = (jobId: string): UseJobReturn => {
  const [state, setState] = useState<UseJobState>({
    job: null,
    loading: true,
    error: null,
  });

  const cancelTokenRef = useRef(axios.CancelToken.source());
  const pollingIntervalRef = useRef<ReturnType<typeof setInterval> | null>(
    null
  );

  /**
   * Fetch job details from API
   */
  const fetchJob = useCallback(async () => {
    setState((prev) => ({ ...prev, loading: true, error: null }));

    try {
      const job = await customerService.getJobById(jobId);
      setState({
        job,
        loading: false,
        error: null,
      });
    } catch (err) {
      if (!axios.isCancel(err)) {
        const message =
          err instanceof Error ? err.message : "Failed to load job";
        setState((prev) => ({ ...prev, loading: false, error: message }));
      }
    }
  }, [jobId]);

  /**
   * Refresh job details (used for polling)
   */
  const refreshJob = useCallback(async () => {
    try {
      const job = await customerService.getJobById(jobId);
      setState((prev) => ({
        ...prev,
        job,
      }));
    } catch (err) {
      if (!axios.isCancel(err)) {
        console.error("Error refreshing job:", err);
      }
    }
  }, [jobId]);

  /**
   * Fetch job on mount or when jobId changes
   */
  useEffect(() => {
    fetchJob();
  }, [jobId, fetchJob]);

  /**
   * Set up polling for real-time updates
   */
  useEffect(() => {
    pollingIntervalRef.current = setInterval(() => {
      refreshJob();
    }, 30000); // 30-second polling interval

    const intervalId = pollingIntervalRef.current;
    return () => {
      if (intervalId) {
        clearInterval(intervalId);
      }
    };
  }, [refreshJob]);

  /**
   * Cleanup on unmount
   */
  useEffect(() => {
    const currentCancelToken = cancelTokenRef.current;
    return () => {
      currentCancelToken.cancel("Component unmounted");
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
      }
    };
  }, []);

  return {
    ...state,
    fetchJob,
    refreshJob,
  };
};
