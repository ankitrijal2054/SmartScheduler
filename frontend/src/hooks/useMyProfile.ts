/**
 * useMyProfile Hook
 * Custom hook for fetching the current contractor's own profile with aggregated statistics
 * Used in Story 5.4 - Contractor Rating & Earnings History
 */

import { useState, useEffect, useCallback } from "react";
import { ContractorProfileData } from "@/types/ContractorProfile";
import { contractorService } from "@/services/contractorService";
import toast from "react-hot-toast";

interface UseMyProfileState {
  profile: ContractorProfileData | null;
  loading: boolean;
  error: string | null;
}

interface UseMyProfileReturn extends UseMyProfileState {
  refetch: () => Promise<void>;
}

/**
 * Hook to fetch current contractor's profile with statistics and reviews
 * Automatically refetches when component mounts
 */
export const useMyProfile = (): UseMyProfileReturn => {
  const [state, setState] = useState<UseMyProfileState>({
    profile: null,
    loading: false,
    error: null,
  });

  /**
   * Fetch contractor's own profile from API
   */
  const fetchProfile = useCallback(async () => {
    setState((prev) => ({ ...prev, loading: true, error: null }));

    try {
      const profile = await contractorService.getProfile();
      setState({
        profile,
        loading: false,
        error: null,
      });
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load profile";
      setState((prev) => ({
        ...prev,
        loading: false,
        error: message,
      }));
      toast.error(message);
    }
  }, []);

  /**
   * Fetch on mount
   */
  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  return {
    ...state,
    refetch: fetchProfile,
  };
};
