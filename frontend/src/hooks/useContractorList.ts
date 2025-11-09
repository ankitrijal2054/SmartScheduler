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

  const myListCancelTokenRef = useRef<ReturnType<
    typeof axios.CancelToken.source
  > | null>(null);
  const availableCancelTokenRef = useRef<ReturnType<
    typeof axios.CancelToken.source
  > | null>(null);

  /**
   * Fetch dispatcher's personal contractor list
   */
  const fetchMyList = useCallback(async () => {
    // Cancel any pending my list request
    if (myListCancelTokenRef.current) {
      myListCancelTokenRef.current.cancel("New request initiated");
    }

    myListCancelTokenRef.current = axios.CancelToken.source();
    const currentToken = myListCancelTokenRef.current;

    setState((prevState) => ({
      ...prevState,
      loading: true,
      error: null,
    }));

    try {
      const result = await dispatcherService.getContractorList(
        currentToken.token
      );

      // Only update state if this request wasn't cancelled
      if (currentToken === myListCancelTokenRef.current) {
        setState((prevState) => ({
          ...prevState,
          myList: result,
          loading: false,
          error: null,
        }));
      }
    } catch (err) {
      // Only update state if this request wasn't cancelled
      if (
        currentToken === myListCancelTokenRef.current &&
        !axios.isCancel(err)
      ) {
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
      // Cancel any pending available contractors request
      if (availableCancelTokenRef.current) {
        availableCancelTokenRef.current.cancel("New request initiated");
      }

      availableCancelTokenRef.current = axios.CancelToken.source();
      const currentToken = availableCancelTokenRef.current;

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
          currentToken.token
        );

        // Only update state if this request wasn't cancelled
        if (currentToken === availableCancelTokenRef.current) {
          setState((prevState) => ({
            ...prevState,
            allContractors: result.contractors,
            totalContractors: result.total,
            loading: false,
            error: null,
          }));
        }
      } catch (err) {
        // Only update state if this request wasn't cancelled
        if (
          currentToken === availableCancelTokenRef.current &&
          !axios.isCancel(err)
        ) {
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
    // Don't cancel other requests for add/remove operations
    const cancelToken = axios.CancelToken.source();

    setState((prevState) => ({
      ...prevState,
      loading: true,
      error: null,
    }));

    try {
      const updatedList = await dispatcherService.addContractorToList(
        contractorId,
        cancelToken.token
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
    // Don't cancel other requests for add/remove operations
    const cancelToken = axios.CancelToken.source();

    setState((prevState) => ({
      ...prevState,
      loading: true,
      error: null,
    }));

    try {
      const updatedList = await dispatcherService.removeContractorFromList(
        contractorId,
        cancelToken.token
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
    if (myListCancelTokenRef.current) {
      myListCancelTokenRef.current.cancel("Component unmounted");
      myListCancelTokenRef.current = null;
    }
    if (availableCancelTokenRef.current) {
      availableCancelTokenRef.current.cancel("Component unmounted");
      availableCancelTokenRef.current = null;
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
