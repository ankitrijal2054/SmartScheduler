/**
 * RatingForm Component
 * Form for customers to submit ratings and feedback for completed jobs
 * Displays only when job.status === "Completed"
 */

import React, { useEffect } from "react";
import { useRating } from "@/hooks/useRating";
import { StarRatingInput } from "./StarRatingInput";
import { RatingSuccessMessage } from "./RatingSuccessMessage";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { JobStatus } from "@/types/Job";

interface RatingFormProps {
  jobId: string;
  contractorId: string;
  contractorName?: string;
  jobStatus: JobStatus;
  onRatingSubmitted?: () => void;
}

export const RatingForm: React.FC<RatingFormProps> = ({
  jobId,
  contractorId,
  contractorName,
  jobStatus,
  onRatingSubmitted,
}) => {
  const {
    rating,
    comment,
    loading,
    error,
    success,
    setRating,
    setComment,
    submitRating,
    resetForm,
  } = useRating();

  // Only show if job is completed
  if (jobStatus !== "Completed") {
    return null;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const result = await submitRating(jobId);
    if (result) {
      onRatingSubmitted?.();
    }
  };

  return (
    <div className="mt-8 bg-white rounded-lg border border-gray-200 shadow-sm p-6">
      {/* Section header */}
      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-1">
          Rate this job
        </h2>
        <p className="text-sm text-gray-600">
          Your feedback helps us improve and informs other customers about{" "}
          {contractorName || "this contractor"}
        </p>
      </div>

      {/* Error message - displayed at top if present and form not successful */}
      {error && !success && (
        <div
          role="alert"
          className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg"
        >
          <div className="flex gap-3">
            <svg
              className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4m0 4v.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <div className="flex-1">
              <p className="text-sm font-medium text-red-800">{error}</p>
            </div>
          </div>
        </div>
      )}

      {/* Form */}
      {!success ? (
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Star rating input */}
          <div>
            <label
              htmlFor="star-rating"
              className="block text-sm font-medium text-gray-900 mb-3"
            >
              Your rating <span className="text-red-500">*</span>
            </label>
            <div id="star-rating">
              <StarRatingInput
                rating={rating}
                onRatingChange={setRating}
                disabled={loading}
              />
            </div>
          </div>

          {/* Comment textarea */}
          <div>
            <label
              htmlFor="comment"
              className="block text-sm font-medium text-gray-900 mb-2"
            >
              Tell us about your experience
            </label>
            <textarea
              id="comment"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              disabled={loading}
              placeholder="Share any additional thoughts (optional, max 500 characters)"
              rows={4}
              maxLength={500}
              className={`w-full px-4 py-3 border border-gray-300 rounded-lg shadow-sm text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors ${
                loading ? "bg-gray-50 cursor-not-allowed" : "bg-white"
              }`}
              aria-label="Job experience feedback (optional)"
            />
            <div className="mt-1 flex justify-between">
              <p className="text-xs text-gray-500">
                Optional field to share additional feedback
              </p>
              <p className="text-xs text-gray-500">{comment.length}/500</p>
            </div>
          </div>

          {/* Submit button */}
          <div className="flex gap-3 justify-between">
            <p className="text-xs text-gray-500 self-center">
              {rating === 0 ? (
                <span className="text-red-600 font-medium">
                  Please select a rating to continue
                </span>
              ) : (
                "Ready to submit"
              )}
            </p>

            <button
              type="submit"
              disabled={loading || rating === 0}
              className={`inline-flex items-center gap-2 px-6 py-3 font-medium rounded-lg transition-all ${
                loading || rating === 0
                  ? "bg-gray-300 text-gray-600 cursor-not-allowed"
                  : "bg-blue-600 text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              }`}
              aria-label={loading ? "Submitting rating..." : "Submit Rating"}
            >
              {loading ? (
                <>
                  <LoadingSpinner size="sm" />
                  <span>Submitting...</span>
                </>
              ) : (
                "Submit Rating"
              )}
            </button>
          </div>
        </form>
      ) : (
        /* Success state - show submitted details */
        <div className="space-y-4">
          <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
            <p className="text-sm font-medium text-green-800">
              ✓ Thank you for your rating!
            </p>
          </div>

          <button
            onClick={() => resetForm()}
            className="text-sm text-blue-600 hover:text-blue-800 font-medium"
          >
            Submit another rating
          </button>
        </div>
      )}

      {/* Success message overlay */}
      <RatingSuccessMessage
        isVisible={success}
        onClose={() => resetForm()}
        rating={rating}
        comment={comment || undefined}
      />
    </div>
  );
};
