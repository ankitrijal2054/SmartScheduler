/**
 * useContractorProfile Hook
 * Custom hook for fetching contractor profile with reviews
 */

import { useState, useEffect, useCallback, useRef } from "react";
import axios from "axios";
import {
  ContractorProfileResponse,
  ReviewWithCustomer,
} from "@/types/Customer";
import { Contractor } from "@/types/Contractor";
import { customerService } from "@/services/customerService";

interface UseContractorProfileState {
  contractor: Contractor | null;
  reviews: ReviewWithCustomer[];
  loading: boolean;
  error: string | null;
}

interface UseContractorProfileReturn extends UseContractorProfileState {
  refetch: () => Promise<void>;
}

export const useContractorProfile = (
  contractorId: string | null
): UseContractorProfileReturn => {
  const [state, setState] = useState<UseContractorProfileState>({
    contractor: null,
    reviews: [],
    loading: false,
    error: null,
  });

  const cancelTokenRef = useRef(axios.CancelToken.source());

  /**
   * Fetch contractor profile and reviews from API
   */
  const fetchContractorProfile = useCallback(async () => {
    if (!contractorId) {
      setState({
        contractor: null,
        reviews: [],
        loading: false,
        error: null,
      });
      return;
    }

    setState((prev) => ({ ...prev, loading: true, error: null }));

    try {
      const response: ContractorProfileResponse =
        await customerService.getContractorProfile(contractorId);

      const contractor: Contractor = {
        id: response.contractor.id,
        name: response.contractor.name,
        rating: response.contractor.averageRating,
        reviewCount: response.contractor.reviewCount,
        location: "",
        tradeType: "Other",
        isActive: response.contractor.isActive,
      };

      setState({
        contractor,
        reviews: response.reviews,
        loading: false,
        error: null,
      });
    } catch (err) {
      if (!axios.isCancel(err)) {
        const message =
          err instanceof Error
            ? err.message
            : "Failed to load contractor profile";
        setState((prev) => ({ ...prev, loading: false, error: message }));
      }
    }
  }, [contractorId]);

  /**
   * Fetch on mount or when contractorId changes
   */
  useEffect(() => {
    fetchContractorProfile();
  }, [contractorId, fetchContractorProfile]);

  /**
   * Cleanup on unmount
   */
  useEffect(() => {
    const currentCancelToken = cancelTokenRef.current;
    return () => {
      currentCancelToken.cancel("Component unmounted");
    };
  }, []);

  return {
    ...state,
    refetch: fetchContractorProfile,
  };
};
