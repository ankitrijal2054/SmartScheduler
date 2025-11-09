/**
 * RecommendationsModal Component
 * Modal/Dialog for displaying contractor recommendations with sorting and filtering
 */

import React, { useEffect } from "react";
import { RecommendationRequest, SortField } from "@/types/Contractor";
import { useRecommendations } from "@/hooks/useRecommendations";
import { ContractorRecommendationCard } from "./ContractorRecommendationCard";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

interface RecommendationsModalProps {
  isOpen: boolean;
  jobId: string;
  jobType: string;
  location: string;
  desiredDateTime: string;
  contractorListOnly?: boolean;
  onClose: () => void;
}

/**
 * Skeleton loader for contractor cards
 */
const SkeletonCard: React.FC = () => (
  <div className="animate-pulse space-y-3 rounded-lg border border-gray-200 bg-white p-4">
    <div className="flex items-start justify-between gap-3">
      <div className="flex-1 space-y-2">
        <div className="h-5 w-32 rounded bg-gray-300" />
        <div className="h-3 w-20 rounded bg-gray-200" />
      </div>
      <div className="h-10 w-10 rounded-full bg-gray-300" />
    </div>
    <div className="h-5 w-40 rounded bg-gray-300" />
    <div className="flex gap-4">
      <div className="h-4 w-24 rounded bg-gray-200" />
      <div className="h-4 w-24 rounded bg-gray-200" />
    </div>
    <div className="h-8 rounded bg-green-100" />
  </div>
);

export const RecommendationsModal: React.FC<RecommendationsModalProps> = ({
  isOpen,
  jobId,
  jobType,
  location,
  desiredDateTime,
  contractorListOnly = false,
  onClose,
}) => {
  const {
    recommendations,
    loading,
    error,
    sortBy,
    fetchRecommendations,
    sortRecommendations,
    retry,
    cleanup,
  } = useRecommendations();

  // Fetch recommendations when modal opens
  useEffect(() => {
    if (isOpen) {
      const request: RecommendationRequest = {
        jobId,
        jobType: jobType as any, // Already validated from Job type
        location,
        desiredDateTime,
        contractor_list_only: contractorListOnly,
      };
      fetchRecommendations(request);
    }

    // Cleanup on unmount or when modal closes
    return () => {
      cleanup();
    };
  }, [
    isOpen,
    jobId,
    jobType,
    location,
    desiredDateTime,
    contractorListOnly,
    fetchRecommendations,
    cleanup,
  ]);

  // Close modal on Escape key
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const handleBackdropClick = (e: React.MouseEvent) => {
    // Close only if clicking directly on the backdrop
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 z-40 bg-black bg-opacity-50 transition-opacity"
        onClick={handleBackdropClick}
        role="presentation"
      />

      {/* Modal */}
      <div
        className="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
        aria-labelledby="recommendations-title"
        aria-describedby="recommendations-desc"
      >
        <div className="max-h-[90vh] w-full max-w-3xl overflow-hidden rounded-lg bg-white shadow-xl">
          {/* Header */}
          <div className="sticky top-0 z-10 border-b border-gray-200 bg-white px-6 py-4">
            <div className="flex items-center justify-between">
              <div className="flex-1">
                <h2
                  id="recommendations-title"
                  className="text-xl font-bold text-gray-900"
                >
                  Contractor Recommendations
                </h2>
                <p
                  id="recommendations-desc"
                  className="mt-1 text-sm text-gray-600"
                >
                  Top recommended contractors for this job
                </p>
              </div>

              {/* Close Button */}
              <button
                onClick={onClose}
                className="rounded-lg p-2 text-gray-500 hover:bg-gray-100 hover:text-gray-700"
                aria-label="Close recommendations modal"
                type="button"
              >
                <span className="text-2xl">✕</span>
              </button>
            </div>

            {/* Sorting Dropdown */}
            {recommendations.length > 0 && !loading && (
              <div className="mt-4 flex items-center gap-2">
                <label
                  htmlFor="sort-field"
                  className="text-sm font-medium text-gray-700"
                >
                  Sort by:
                </label>
                <select
                  id="sort-field"
                  value={sortBy}
                  onChange={(e) =>
                    sortRecommendations(e.target.value as SortField)
                  }
                  className="rounded border border-gray-300 bg-white px-3 py-2 text-sm font-medium text-gray-900 hover:border-gray-400 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  aria-label="Sort recommendations by field"
                >
                  <option value="rank">Rank (Default)</option>
                  <option value="rating">Highest Rating</option>
                  <option value="distance">Nearest Distance</option>
                  <option value="travelTime">Shortest Travel Time</option>
                </select>
              </div>
            )}
          </div>

          {/* Content */}
          <div className="overflow-y-auto p-6">
            {/* Loading State */}
            {loading && (
              <div className="space-y-4">
                <div className="flex flex-col items-center gap-3">
                  <LoadingSpinner />
                  <p className="text-sm text-gray-600">
                    Fetching contractor recommendations...
                  </p>
                </div>
                <div className="space-y-4">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <SkeletonCard key={i} />
                  ))}
                </div>
              </div>
            )}

            {/* Error State */}
            {error && !loading && (
              <div className="rounded-lg border border-red-200 bg-red-50 p-4">
                <h3 className="font-semibold text-red-900">
                  Failed to Load Recommendations
                </h3>
                <p className="mt-1 text-sm text-red-800">{error}</p>
                <button
                  onClick={() => retry()}
                  className="mt-3 rounded bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700"
                  type="button"
                >
                  Try Again
                </button>
              </div>
            )}

            {/* Empty State */}
            {recommendations.length === 0 && !loading && !error && (
              <div className="rounded-lg border border-gray-200 bg-gray-50 p-8 text-center">
                <p className="text-lg font-semibold text-gray-900">
                  No Available Contractors
                </p>
                <p className="mt-2 text-gray-600">
                  No contractors are available for this time slot. Please try a
                  different time.
                </p>
              </div>
            )}

            {/* Recommendations List */}
            {recommendations.length > 0 && !loading && (
              <div className="space-y-4">
                {recommendations.map((contractor) => (
                  <ContractorRecommendationCard
                    key={contractor.contractorId}
                    contractor={contractor}
                  />
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
};
