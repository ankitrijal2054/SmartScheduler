/**
 * RecentReviewsList Component
 * Displays recent customer reviews as cards
 * Used in Story 5.4 - Contractor Rating & Earnings History
 */

import React from "react";
import { CustomerReview } from "@/types/ContractorProfile";
import { Star, MessageCircle } from "lucide-react";
import { formatDistanceToNow } from "date-fns";

interface RecentReviewsListProps {
  reviews: CustomerReview[];
}

/**
 * Component to display recent customer reviews
 * Shows rating, customer name, job type, comment, and date
 */
export const RecentReviewsList: React.FC<RecentReviewsListProps> = ({
  reviews,
}) => {
  const renderStars = (rating: number) => {
    return (
      <div className="flex gap-1">
        {Array.from({ length: 5 }).map((_, i) => (
          <Star
            key={i}
            size={16}
            className={
              i < rating ? "fill-yellow-400 text-yellow-400" : "text-gray-300"
            }
          />
        ))}
      </div>
    );
  };

  if (reviews.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 border border-gray-200 text-center">
        <MessageCircle size={48} className="mx-auto text-gray-400 mb-4" />
        <h3 className="text-gray-900 font-semibold mb-2">No reviews yet</h3>
        <p className="text-gray-600">
          Complete more jobs to start receiving reviews from customers
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-gray-900">Recent Reviews</h3>

      <div className="grid gap-4">
        {reviews.map((review) => (
          <div
            key={review.id}
            className="bg-white rounded-lg shadow-sm p-6 border border-gray-200 hover:shadow-md transition-shadow"
          >
            {/* Header with Rating and Name */}
            <div className="flex items-start justify-between mb-3">
              <div>
                <p className="font-semibold text-gray-900">
                  {review.customerName}
                </p>
                <p className="text-sm text-gray-600">{review.jobType}</p>
              </div>
              <div className="text-right">
                {renderStars(review.rating)}
                <p className="text-sm font-bold text-gray-900 mt-1">
                  {review.rating} stars
                </p>
              </div>
            </div>

            {/* Comment */}
            {review.comment && (
              <p className="text-gray-700 mb-4 leading-relaxed">
                "{review.comment}"
              </p>
            )}

            {/* Date */}
            <p className="text-sm text-gray-500">
              {formatDistanceToNow(new Date(review.createdAt), {
                addSuffix: true,
              })}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
};
