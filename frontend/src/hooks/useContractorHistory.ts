/**
 * useContractorHistory Hook
 * Fetches and manages contractor history data with loading and error states
 */

import { useEffect, useState, useRef } from "react";
import { dispatcherService } from "@/services/dispatcherService";
import { ContractorHistory } from "@/types/Contractor";
import axios, { CancelTokenSource } from "axios";

export interface UseContractorHistoryState {
  data: ContractorHistory | null;
  loading: boolean;
  error: string | null;
}

/**
 * Hook to fetch contractor history and performance stats
 * @param contractorId ID of contractor to fetch history for
 * @param limit Number of recent jobs to fetch (default 10)
 * @param offset Pagination offset (default 0)
 * @param enabled Whether to fetch data (default true)
 * @returns State object with data, loading, error
 */
export const useContractorHistory = (
  contractorId: string,
  limit: number = 10,
  offset: number = 0,
  enabled: boolean = true
): UseContractorHistoryState => {
  const [data, setData] = useState<ContractorHistory | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const cancelTokenRef = useRef<CancelTokenSource | null>(null);

  useEffect(() => {
    // Don't fetch if disabled or no contractorId
    if (!enabled || !contractorId) {
      setLoading(false);
      return;
    }

    // Reset state when contractorId changes
    const fetchData = async () => {
      setLoading(true);
      setError(null);

      // Cancel any pending request
      if (cancelTokenRef.current) {
        cancelTokenRef.current.cancel("New request initiated");
      }

      // Create new cancel token for this request
      cancelTokenRef.current = axios.CancelToken.source();
      const currentCancelToken = cancelTokenRef.current;

      try {
        const history = await dispatcherService.getContractorHistory(
          contractorId,
          limit,
          offset,
          currentCancelToken.token
        );

        // Only update state if this request wasn't cancelled
        // Check if the token is still valid (not cancelled)
        if (currentCancelToken === cancelTokenRef.current) {
          setData(history);
          setLoading(false);
        }
      } catch (err) {
        // Only update state if this request wasn't cancelled
        if (
          currentCancelToken === cancelTokenRef.current &&
          !axios.isCancel(err)
        ) {
          const message =
            err instanceof Error
              ? err.message
              : "Failed to load contractor history";
          setError(message);
          setLoading(false);
        }
        // If cancelled, don't update state - the cleanup will handle it
      }
    };

    fetchData();

    // Cleanup: cancel pending request only on unmount or when dependencies change
    return () => {
      if (cancelTokenRef.current) {
        cancelTokenRef.current.cancel(
          "Component unmounted or dependencies changed"
        );
        cancelTokenRef.current = null;
      }
    };
  }, [contractorId, limit, offset, enabled]);

  return { data, loading, error };
};
