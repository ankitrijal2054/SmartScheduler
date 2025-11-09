/**
 * useJobs Hook
 * Custom hook for managing job list state, pagination, sorting, and real-time refresh
 */

import { useState, useEffect, useCallback, useRef } from "react";
import axios from "axios";
import {
  Job,
  JobsQueryParams,
  PaginationMeta,
  JobType,
  JobStatus,
} from "@/types/Job";
import { dispatcherService } from "@/services/dispatcherService";
import { config } from "@/utils/config";

interface UseJobsState {
  jobs: Job[];
  loading: boolean;
  error: string | null;
  pagination: PaginationMeta | null;
}

interface UseJobsReturn extends UseJobsState {
  fetchJobs: (filters?: JobsQueryParams) => Promise<void>;
  refreshJobs: () => Promise<void>;
  setPage: (page: number) => void;
  setLimit: (limit: number) => void;
  setSort: (sortBy: string, sortOrder: "asc" | "desc") => void;
}

export const useJobs = (): UseJobsReturn => {
  const [state, setState] = useState<UseJobsState>({
    jobs: [],
    loading: true,
    error: null,
    pagination: null,
  });

  const [params, setParams] = useState<JobsQueryParams>({
    page: 1,
    limit: 20,
    sortBy: "desiredDateTime",
    sortOrder: "asc",
  });

  const cancelTokenRef = useRef(axios.CancelToken.source());
  const pollingIntervalRef = useRef<ReturnType<typeof setInterval> | null>(
    null
  );

  /**
   * Fetch jobs from API
   */
  const fetchJobs = useCallback(
    async (filters?: JobsQueryParams) => {
      setState((prev) => ({ ...prev, loading: true, error: null }));

      try {
        const queryParams = { ...params, ...filters };
        const response = await dispatcherService.getJobs(
          queryParams,
          cancelTokenRef.current.token
        );

        // Normalize response data
        const jobs: Job[] = response.data.map((job) => ({
          id: job.id,
          customerId: job.customerId,
          customerName: job.customerName,
          location: job.location,
          desiredDateTime: job.desiredDateTime,
          jobType: (job.jobType as JobType) || "Other",
          description: job.description,
          status: (job.status as JobStatus) || "Pending",
          currentAssignedContractorId: job.currentAssignedContractorId,
          assignedContractorName: job.assignedContractorName,
          assignedContractorRating: job.assignedContractorRating,
          createdAt: job.createdAt,
          updatedAt: job.updatedAt,
        }));

        setState({
          jobs,
          loading: false,
          error: null,
          pagination: response.pagination,
        });

        if (filters) {
          setParams((prev) => ({ ...prev, ...filters }));
        }
      } catch (err) {
        if (!axios.isCancel(err)) {
          const message =
            err instanceof Error ? err.message : "Failed to load jobs";
          setState((prev) => ({ ...prev, loading: false, error: message }));
        }
      }
    },
    [params]
  );

  /**
   * Refresh jobs (used for polling)
   */
  const refreshJobs = useCallback(async () => {
    try {
      const response = await dispatcherService.getJobs(
        params,
        cancelTokenRef.current.token
      );

      const jobs: Job[] = response.data.map((job) => ({
        id: job.id,
        customerId: job.customerId,
        customerName: job.customerName,
        location: job.location,
        desiredDateTime: job.desiredDateTime,
        jobType: (job.jobType as JobType) || "Other",
        description: job.description,
        status: (job.status as JobStatus) || "Pending",
        currentAssignedContractorId: job.currentAssignedContractorId,
        assignedContractorName: job.assignedContractorName,
        assignedContractorRating: job.assignedContractorRating,
        createdAt: job.createdAt,
        updatedAt: job.updatedAt,
      }));

      setState((prev) => ({
        ...prev,
        jobs,
        pagination: response.pagination,
      }));
    } catch (err) {
      if (!axios.isCancel(err)) {
        console.error("Error refreshing jobs:", err);
      }
    }
  }, [params]);

  /**
   * Set current page
   */
  const setPage = useCallback((page: number) => {
    setParams((prev) => ({ ...prev, page }));
  }, []);

  /**
   * Set items per page
   */
  const setLimit = useCallback((limit: number) => {
    setParams((prev) => ({ ...prev, limit, page: 1 }));
  }, []);

  /**
   * Set sorting
   */
  const setSort = useCallback((sortBy: string, sortOrder: "asc" | "desc") => {
    setParams((prev) => ({ ...prev, sortBy, sortOrder, page: 1 }));
  }, []);

  /**
   * Fetch jobs on mount and when params change
   */
  useEffect(() => {
    fetchJobs();
  }, [params, fetchJobs]);

  /**
   * Set up polling for real-time updates
   */
  useEffect(() => {
    pollingIntervalRef.current = setInterval(() => {
      refreshJobs();
    }, config.polling.jobsRefreshInterval);

    const intervalId = pollingIntervalRef.current;
    return () => {
      if (intervalId) {
        clearInterval(intervalId);
      }
    };
  }, [refreshJobs]);

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
    fetchJobs,
    refreshJobs,
    setPage,
    setLimit,
    setSort,
  };
};
