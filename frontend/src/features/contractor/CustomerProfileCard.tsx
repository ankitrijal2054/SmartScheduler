/**
 * CustomerProfileCard Component
 * Displays customer information and past job history
 * Used in JobDetailsModal to show customer rating and prior jobs
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import React from "react";
import { User, Star, Clock } from "lucide-react";
import { format } from "date-fns";
import { JobDetails } from "@/types/JobDetails";

interface CustomerProfileCardProps {
  customer: JobDetails["customer"];
  pastReviews: JobDetails["pastReviews"];
}

/**
 * Render star rating visually
 */
const StarRating: React.FC<{ rating: number | null }> = ({ rating }) => {
  if (rating === null)
    return <span className="text-gray-500 text-sm">No rating yet</span>;

  const stars = Math.round(rating);
  const decimalPart = rating - Math.floor(rating);

  return (
    <span className="flex items-center gap-2">
      <div className="flex items-center">
        {Array.from({ length: 5 }).map((_, i) => (
          <span key={i} className="text-lg">
            {i < Math.floor(rating)
              ? "⭐"
              : i === Math.floor(rating) && decimalPart > 0
              ? "✨"
              : "☆"}
          </span>
        ))}
      </div>
      <span className="text-sm font-medium text-gray-700">
        {rating.toFixed(1)} ({rating > 0 ? rating.toFixed(1) : "0"})
      </span>
    </span>
  );
};

export const CustomerProfileCard: React.FC<CustomerProfileCardProps> = ({
  customer,
  pastReviews,
}) => {
  return (
    <div className="border border-gray-200 rounded-lg p-5 bg-white space-y-4">
      {/* Customer Header */}
      <div className="flex items-start gap-3">
        <div className="flex-shrink-0 w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
          <User size={20} className="text-blue-600" />
        </div>
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900">
            {customer.name}
          </h3>
          <div className="mt-2">
            <StarRating rating={customer.rating} />
          </div>
          <p className="text-xs text-gray-500 mt-1">
            {customer.reviewCount}{" "}
            {customer.reviewCount === 1 ? "review" : "reviews"}
          </p>
        </div>
      </div>

      {/* Past Jobs Section */}
      <div className="border-t border-gray-100 pt-4">
        <h4 className="flex items-center gap-2 text-sm font-semibold text-gray-900 mb-3">
          <Clock size={16} className="text-gray-500" />
          Job History with This Customer
        </h4>

        {pastReviews.length === 0 ? (
          <p className="text-sm text-gray-600 italic bg-gray-50 p-3 rounded-lg">
            No prior history with this customer
          </p>
        ) : (
          <div className="space-y-2">
            {pastReviews.slice(0, 3).map((review) => (
              <div
                key={review.id}
                className="bg-gray-50 p-3 rounded-lg border border-gray-200"
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">
                      {review.jobType}
                    </p>
                    <p className="text-xs text-gray-600 mt-1">
                      {format(new Date(review.createdAt), "MMM d, yyyy")}
                    </p>
                  </div>
                  <div className="flex-shrink-0">
                    <div className="flex items-center gap-1">
                      {Array.from({ length: 5 }).map((_, i) => (
                        <span key={i} className="text-xs">
                          {i < review.rating ? "⭐" : "☆"}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>
                {review.comment && (
                  <p className="text-xs text-gray-600 mt-2 italic">
                    "{review.comment}"
                  </p>
                )}
              </div>
            ))}

            {pastReviews.length > 3 && (
              <p className="text-xs text-gray-500 text-center pt-2">
                +{pastReviews.length - 3} more{" "}
                {pastReviews.length - 3 === 1 ? "job" : "jobs"}
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
