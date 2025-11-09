/**
 * useContractorHistory Hook
 * Fetches and manages contractor history data with loading and error states
 */

import { useEffect, useState, useRef } from "react";
import { dispatcherService } from "@/services/dispatcherService";
import { ContractorHistory } from "@/types/Contractor";
import axios from "axios";

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
 * @returns State object with data, loading, error
 */
export const useContractorHistory = (
  contractorId: string,
  limit: number = 10,
  offset: number = 0
): UseContractorHistoryState => {
  const [data, setData] = useState<ContractorHistory | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const cancelTokenRef = useRef(axios.CancelToken.source());

  useEffect(() => {
    // Reset state when contractorId changes
    const fetchData = async () => {
      setLoading(true);
      setError(null);

      try {
        const history = await dispatcherService.getContractorHistory(
          contractorId,
          limit,
          offset,
          cancelTokenRef.current.token
        );
        setData(history);
      } catch (err) {
        if (!axios.isCancel(err)) {
          const message =
            err instanceof Error
              ? err.message
              : "Failed to load contractor history";
          setError(message);
        }
      } finally {
        setLoading(false);
      }
    };

    fetchData();

    // Cleanup: cancel pending request on unmount or when contractorId changes
    return () => {
      cancelTokenRef.current.cancel("Component unmounted");
      cancelTokenRef.current = axios.CancelToken.source();
    };
  }, [contractorId, limit, offset]);

  return { data, loading, error };
};
