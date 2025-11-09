/**
 * ReviewsList Component
 * Displays a list of contractor reviews with customer names, ratings, and comments
 * Handles empty state and date formatting
 */

import React from "react";
import { ReviewWithCustomer } from "@/types/Customer";

interface ReviewsListProps {
  reviews: ReviewWithCustomer[];
  maxReviews?: number; // Default: 5
}

/**
 * Format date as relative or absolute string
 * Returns format like "2 weeks ago" or "Nov 2025"
 */
const formatReviewDate = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const diffWeeks = Math.floor(diffDays / 7);
    const diffMonths = Math.floor(diffDays / 30);

    if (diffDays < 7) {
      return diffDays === 0
        ? "Today"
        : `${diffDays} day${diffDays !== 1 ? "s" : ""} ago`;
    }
    if (diffWeeks < 4) {
      return `${diffWeeks} week${diffWeeks !== 1 ? "s" : ""} ago`;
    }
    if (diffMonths < 12) {
      return `${diffMonths} month${diffMonths !== 1 ? "s" : ""} ago`;
    }

    // For older reviews, use absolute format: "Nov 2025"
    return date.toLocaleDateString("en-US", {
      month: "short",
      year: "numeric",
    });
  } catch {
    return dateString;
  }
};

/**
 * Render star rating for a single review
 */
const renderStarRating = (rating: number): React.ReactNode => {
  return (
    <div className="flex gap-0.5">
      {Array.from({ length: rating }).map((_, i) => (
        <span key={`filled-${i}`} className="text-sm">
          ⭐
        </span>
      ))}
      {Array.from({ length: 5 - rating }).map((_, i) => (
        <span key={`empty-${i}`} className="text-sm opacity-20">
          ⭐
        </span>
      ))}
    </div>
  );
};

export const ReviewsList: React.FC<ReviewsListProps> = ({
  reviews,
  maxReviews = 5,
}) => {
  const displayedReviews = reviews.slice(0, maxReviews);

  // Empty state
  if (reviews.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-500 text-sm">
          No reviews yet - be the first to rate this contractor
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h3 className="text-sm font-semibold text-gray-900">
        Customer Reviews ({reviews.length})
      </h3>

      <div className="space-y-4">
        {displayedReviews.map((review) => (
          <div
            key={review.id}
            className="border border-gray-200 rounded-lg p-4 bg-white"
          >
            {/* Header: Customer name and stars */}
            <div className="flex items-start justify-between mb-2">
              <div className="flex flex-col gap-2 flex-1">
                <p className="font-semibold text-sm text-gray-900">
                  {review.customerName}
                </p>
                {renderStarRating(review.rating)}
              </div>
              <p className="text-xs text-gray-500">
                {formatReviewDate(review.createdAt)}
              </p>
            </div>

            {/* Comment */}
            {review.comment && (
              <p className="text-sm text-gray-700 leading-relaxed">
                {review.comment}
              </p>
            )}
          </div>
        ))}
      </div>

      {/* Show indicator if there are more reviews */}
      {reviews.length > maxReviews && (
        <p className="text-xs text-gray-500 text-center pt-2">
          +{reviews.length - maxReviews} more review
          {reviews.length - maxReviews !== 1 ? "s" : ""}
        </p>
      )}
    </div>
  );
};
