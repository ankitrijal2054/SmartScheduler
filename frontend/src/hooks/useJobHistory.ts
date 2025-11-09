/**
 * useJobHistory Hook
 * Custom hook for fetching contractor's job history with filtering and pagination
 * Used in Story 5.4 - Contractor Rating & Earnings History
 */

import { useState, useEffect, useCallback } from "react";
import {
  JobHistoryItem,
  JobHistoryFilterOptions,
  JobHistoryPaginationParams,
} from "@/types/ContractorProfile";
import { contractorService } from "@/services/contractorService";
import toast from "react-hot-toast";

interface UseJobHistoryState {
  jobs: JobHistoryItem[];
  totalCount: number;
  loading: boolean;
  error: string | null;
  currentPage: number;
  pageSize: number;
}

interface UseJobHistoryReturn extends UseJobHistoryState {
  goToPage: (page: number) => Promise<void>;
  refetch: () => Promise<void>;
  setFilters: (filters: JobHistoryFilterOptions | undefined) => Promise<void>;
}

/**
 * Hook to fetch contractor's job history with filtering and pagination support
 * @param initialPageSize - Items per page (default 20)
 */
export const useJobHistory = (
  initialPageSize: number = 20
): UseJobHistoryReturn => {
  const [state, setState] = useState<UseJobHistoryState>({
    jobs: [],
    totalCount: 0,
    loading: false,
    error: null,
    currentPage: 1,
    pageSize: initialPageSize,
  });

  const [filters, setFiltersState] = useState<
    JobHistoryFilterOptions | undefined
  >(undefined);

  /**
   * Fetch job history with current filters and pagination
   */
  const fetchJobHistory = useCallback(
    async (page: number = 1, currentFilters?: JobHistoryFilterOptions) => {
      setState((prev) => ({
        ...prev,
        loading: true,
        error: null,
        currentPage: page,
      }));

      try {
        const pagination: JobHistoryPaginationParams = {
          skip: (page - 1) * state.pageSize,
          take: state.pageSize,
        };

        const response = await contractorService.getJobHistory(
          pagination,
          currentFilters || filters
        );

        setState((prev) => ({
          ...prev,
          jobs: response.assignments,
          totalCount: response.totalCount,
          loading: false,
          error: null,
          currentPage: page,
        }));
      } catch (err) {
        const message =
          err instanceof Error ? err.message : "Failed to load job history";
        setState((prev) => ({
          ...prev,
          loading: false,
          error: message,
          jobs: [],
          totalCount: 0,
        }));
        toast.error(message);
      }
    },
    [state.pageSize, filters]
  );

  /**
   * Fetch on mount with initial filters
   */
  useEffect(() => {
    fetchJobHistory(1, filters);
  }, []);

  /**
   * Navigate to specific page
   */
  const goToPage = useCallback(
    async (page: number) => {
      const totalPages = Math.ceil(state.totalCount / state.pageSize);
      if (page < 1 || page > totalPages) {
        toast.error("Invalid page number");
        return;
      }
      await fetchJobHistory(page);
    },
    [state.totalCount, state.pageSize, fetchJobHistory]
  );

  /**
   * Update filters and reset to page 1
   */
  const updateFilters = useCallback(
    async (newFilters: JobHistoryFilterOptions | undefined) => {
      setFiltersState(newFilters);
      await fetchJobHistory(1, newFilters);
    },
    [fetchJobHistory]
  );

  /**
   * Refetch with current filters and page
   */
  const refetch = useCallback(async () => {
    await fetchJobHistory(state.currentPage, filters);
  }, [state.currentPage, filters, fetchJobHistory]);

  return {
    ...state,
    goToPage,
    refetch,
    setFilters: updateFilters,
  };
};
