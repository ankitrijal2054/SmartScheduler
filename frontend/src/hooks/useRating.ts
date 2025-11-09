/**
 * useRating Hook
 * Manages rating form state and API submission for customer job reviews
 */

import { useState, useCallback } from "react";
import { customerService } from "@/services/customerService";
import { Review } from "@/types/Customer";

interface UseRatingState {
  rating: number;
  comment: string;
  loading: boolean;
  error: string | null;
  success: boolean;
}

interface UseRatingReturn extends UseRatingState {
  setRating: (rating: number) => void;
  setComment: (comment: string) => void;
  submitRating: (jobId: string) => Promise<Review | null>;
  resetForm: () => void;
}

const COMMENT_MAX_LENGTH = 500;
const RATING_MIN = 1;
const RATING_MAX = 5;

/**
 * Hook for managing customer rating form state and submission
 * @returns Rating state and methods
 */
export function useRating(): UseRatingReturn {
  const [state, setState] = useState<UseRatingState>({
    rating: 0,
    comment: "",
    loading: false,
    error: null,
    success: false,
  });

  const setRating = useCallback((rating: number) => {
    if (rating >= 0 && rating <= RATING_MAX) {
      setState((prev) => ({ ...prev, rating, error: null }));
    }
  }, []);

  const setComment = useCallback((comment: string) => {
    if (comment.length <= COMMENT_MAX_LENGTH) {
      setState((prev) => ({ ...prev, comment, error: null }));
    }
  }, []);

  const submitRating = useCallback(
    async (jobId: string): Promise<Review | null> => {
      // Validation
      if (state.rating < RATING_MIN || state.rating > RATING_MAX) {
        setState((prev) => ({
          ...prev,
          error: "Please select a rating between 1 and 5 stars",
        }));
        return null;
      }

      if (state.comment && state.comment.length > COMMENT_MAX_LENGTH) {
        setState((prev) => ({
          ...prev,
          error: `Comment must be no more than ${COMMENT_MAX_LENGTH} characters`,
        }));
        return null;
      }

      setState((prev) => ({ ...prev, loading: true, error: null }));

      try {
        const request = {
          rating: state.rating,
          comment: state.comment || null,
        };

        const review = await customerService.submitRating(jobId, request);

        setState((prev) => ({
          ...prev,
          loading: false,
          success: true,
          error: null,
        }));

        return review;
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : "Failed to submit rating";
        setState((prev) => ({
          ...prev,
          loading: false,
          error: errorMessage,
          success: false,
        }));
        return null;
      }
    },
    [state.rating, state.comment]
  );

  const resetForm = useCallback(() => {
    setState({
      rating: 0,
      comment: "",
      loading: false,
      error: null,
      success: false,
    });
  }, []);

  return {
    ...state,
    setRating,
    setComment,
    submitRating,
    resetForm,
  };
}


