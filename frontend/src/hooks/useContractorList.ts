/**
 * useContractorList Hook
 * Custom hook for managing contractor list state and operations
 */

import { useState, useCallback, useRef } from "react";
import axios from "axios";
import { Contractor } from "@/types/Contractor";
import { dispatcherService } from "@/services/dispatcherService";

interface ContractorListState {
  myList: Contractor[];
  allContractors: Contractor[];
  totalContractors: number;
  loading: boolean;
  error: string | null;
  contractorListOnly: boolean;
}

/**
 * useContractorList - Manages contractor list state and operations
 * @returns Object containing state and methods (fetchMyList, fetchAvailableContractors, addContractor, removeContractor, toggleFilter, cleanup)
 */
export const useContractorList = () => {
  const [state, setState] = useState<ContractorListState>({
    myList: [],
    allContractors: [],
    totalContractors: 0,
    loading: false,
    error: null,
    contractorListOnly: false,
  });

  const cancelTokenRef = useRef<ReturnType<
    typeof axios.CancelToken.source
  > | null>(null);

  /**
   * Fetch dispatcher's personal contractor list
   */
  const fetchMyList = useCallback(async () => {
    if (cancelTokenRef.current) {
      cancelTokenRef.current.cancel("New request initiated");
    }

    cancelTokenRef.current = axios.CancelToken.source();

    setState((prevState) => ({
      ...prevState,
      loading: true,
      error: null,
    }));

    try {
      const result = await dispatcherService.getContractorList(
        cancelTokenRef.current.token
      );

      setState((prevState) => ({
        ...prevState,
        myList: result,
        loading: false,
        error: null,
      }));
    } catch (err) {
      if (!axios.isCancel(err)) {
        const errorMessage =
          err instanceof Error
            ? err.message
            : "Failed to fetch contractor list";
        setState((prevState) => ({
          ...prevState,
          loading: false,
          error: errorMessage,
        }));
      }
    }
  }, []);

  /**
   * Fetch available contractors for adding to list
   */
  const fetchAvailableContractors = useCallback(
    async (limit: number = 50, offset: number = 0, search?: string) => {
      if (cancelTokenRef.current) {
        cancelTokenRef.current.cancel("New request initiated");
      }

      cancelTokenRef.current = axios.CancelToken.source();

      setState((prevState) => ({
        ...prevState,
        loading: true,
        error: null,
      }));

      try {
        const result = await dispatcherService.getAvailableContractors(
          limit,
          offset,
          search,
          cancelTokenRef.current.token
        );

        setState((prevState) => ({
          ...prevState,
          allContractors: result.contractors,
          totalContractors: result.total,
          loading: false,
          error: null,
        }));
      } catch (err) {
        if (!axios.isCancel(err)) {
          const errorMessage =
            err instanceof Error ? err.message : "Failed to fetch contractors";
          setState((prevState) => ({
            ...prevState,
            loading: false,
            error: errorMessage,
          }));
        }
      }
    },
    []
  );

  /**
   * Add contractor to dispatcher's list
   */
  const addContractor = useCallback(async (contractorId: string) => {
    if (cancelTokenRef.current) {
      cancelTokenRef.current.cancel("New request initiated");
    }

    cancelTokenRef.current = axios.CancelToken.source();

    setState((prevState) => ({
      ...prevState,
      loading: true,
      error: null,
    }));

    try {
      const updatedList = await dispatcherService.addContractorToList(
        contractorId,
        cancelTokenRef.current.token
      );

      setState((prevState) => ({
        ...prevState,
        myList: updatedList,
        loading: false,
        error: null,
      }));

      return true;
    } catch (err) {
      if (!axios.isCancel(err)) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to add contractor";
        setState((prevState) => ({
          ...prevState,
          loading: false,
          error: errorMessage,
        }));
      }
      return false;
    }
  }, []);

  /**
   * Remove contractor from dispatcher's list
   */
  const removeContractor = useCallback(async (contractorId: string) => {
    if (cancelTokenRef.current) {
      cancelTokenRef.current.cancel("New request initiated");
    }

    cancelTokenRef.current = axios.CancelToken.source();

    setState((prevState) => ({
      ...prevState,
      loading: true,
      error: null,
    }));

    try {
      const updatedList = await dispatcherService.removeContractorFromList(
        contractorId,
        cancelTokenRef.current.token
      );

      setState((prevState) => ({
        ...prevState,
        myList: updatedList,
        loading: false,
        error: null,
      }));

      return true;
    } catch (err) {
      if (!axios.isCancel(err)) {
        const errorMessage =
          err instanceof Error ? err.message : "Failed to remove contractor";
        setState((prevState) => ({
          ...prevState,
          loading: false,
          error: errorMessage,
        }));
      }
      return false;
    }
  }, []);

  /**
   * Toggle contractor_list_only filter
   */
  const toggleFilter = useCallback(() => {
    setState((prevState) => ({
      ...prevState,
      contractorListOnly: !prevState.contractorListOnly,
    }));
  }, []);

  /**
   * Cleanup: Cancel pending requests on unmount
   */
  const cleanup = useCallback(() => {
    if (cancelTokenRef.current) {
      cancelTokenRef.current.cancel("Component unmounted");
    }
  }, []);

  return {
    ...state,
    fetchMyList,
    fetchAvailableContractors,
    addContractor,
    removeContractor,
    toggleFilter,
    cleanup,
  };
};
