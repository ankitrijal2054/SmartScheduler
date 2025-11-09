/**
 * useRecommendations Hook
 * Custom hook for managing contractor recommendations state and sorting
 */

import { useState, useCallback, useRef } from "react";
import axios from "axios";
import {
  RecommendedContractor,
  RecommendationRequest,
  SortField,
  RecommendationsState,
} from "@/types/Contractor";
import { dispatcherService } from "@/services/dispatcherService";

/**
 * useRecommendations - Manages recommendations fetching, state, and sorting
 * @returns Object containing state (recommendations, loading, error, sortBy) and methods (fetchRecommendations, sortRecommendations)
 */
export const useRecommendations = () => {
  const [state, setState] = useState<RecommendationsState>({
    recommendations: [],
    loading: false,
    error: null,
    sortBy: "rank",
  });

  const cancelTokenRef = useRef<ReturnType<
    typeof axios.CancelToken.source
  > | null>(null);

  /**
   * Sort recommendations by the specified field
   */
  const sortRecommendations = useCallback((field: SortField) => {
    setState((prevState) => {
      const sorted = [...prevState.recommendations];

      switch (field) {
        case "rank":
          sorted.sort((a, b) => a.rank - b.rank);
          break;
        case "rating":
          // Sort by rating (highest first), use rank as tiebreaker
          sorted.sort((a, b) => {
            if ((b.avgRating ?? 0) !== (a.avgRating ?? 0)) {
              return (b.avgRating ?? 0) - (a.avgRating ?? 0);
            }
            return a.rank - b.rank;
          });
          break;
        case "distance":
          // Sort by distance (nearest first)
          sorted.sort((a, b) => a.distance - b.distance);
          break;
        case "travelTime":
          // Sort by travel time (shortest first)
          sorted.sort((a, b) => a.travelTime - b.travelTime);
          break;
        default:
          break;
      }

      return {
        ...prevState,
        recommendations: sorted,
        sortBy: field,
      };
    });
  }, []);

  /**
   * Fetch recommendations for a specific job
   */
  const fetchRecommendations = useCallback(
    async (
      request: RecommendationRequest
    ): Promise<RecommendedContractor[] | null> => {
      // Cancel previous request if still pending
      if (cancelTokenRef.current) {
        cancelTokenRef.current.cancel("New request initiated");
      }

      // Create new cancel token
      cancelTokenRef.current = axios.CancelToken.source();

      setState((prevState) => ({
        ...prevState,
        loading: true,
        error: null,
      }));

      try {
        const response = await dispatcherService.getRecommendations(
          request,
          cancelTokenRef.current.token
        );

        // Sort by rank by default
        const sortedByRank = response.data.sort((a, b) => a.rank - b.rank);

        setState({
          recommendations: sortedByRank,
          loading: false,
          error: null,
          sortBy: "rank",
        });

        return sortedByRank;
      } catch (err) {
        // Don't set error if request was cancelled
        if (!axios.isCancel(err)) {
          const errorMessage =
            err instanceof Error
              ? err.message
              : "Failed to fetch recommendations";
          setState((prevState) => ({
            ...prevState,
            loading: false,
            error: errorMessage,
          }));
        }
        return null;
      }
    },
    []
  );

  /**
   * Retry fetching recommendations with the same request
   * Store the last request to enable retries
   */
  const lastRequestRef = useRef<RecommendationRequest | null>(null);

  const retry = useCallback(async () => {
    if (lastRequestRef.current) {
      return fetchRecommendations(lastRequestRef.current);
    }
    return null;
  }, [fetchRecommendations]);

  /**
   * Enhanced fetchRecommendations that stores request for retry
   */
  const fetchRecommendationsWithRetry = useCallback(
    async (request: RecommendationRequest) => {
      lastRequestRef.current = request;
      return fetchRecommendations(request);
    },
    [fetchRecommendations]
  );

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
    fetchRecommendations: fetchRecommendationsWithRetry,
    sortRecommendations,
    retry,
    cleanup,
  };
};
