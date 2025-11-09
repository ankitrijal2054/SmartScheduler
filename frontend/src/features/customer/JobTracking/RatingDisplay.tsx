/**
 * RatingDisplay Component
 * Displays contractor rating with stars and qualitative label
 * Handles both rated and unrated contractors with neutral tone for low ratings
 */

import React from "react";

interface RatingDisplayProps {
  averageRating: number | null;
  reviewCount: number;
}

/**
 * Get rating label based on score
 * Rating ranges: Excellent (4.5-5), Very Good (4-4.5), Good (3-4), Fair (2-3), Poor (<2)
 */
const getRatingLabel = (rating: number | null): string => {
  if (rating === null) return "No ratings yet";
  if (rating >= 4.5) return "Excellent";
  if (rating >= 4) return "Very Good";
  if (rating >= 3) return "Good";
  if (rating >= 2) return "Fair";
  return "Poor";
};

/**
 * Get color class for rating label
 */
const getRatingColor = (rating: number | null): string => {
  if (rating === null) return "text-gray-500";
  if (rating >= 4.5) return "text-green-700";
  if (rating >= 4) return "text-green-600";
  if (rating >= 3) return "text-yellow-600";
  if (rating >= 2) return "text-orange-600";
  return "text-red-600";
};

/**
 * Get background color for rating badge
 */
const getRatingBgColor = (rating: number | null): string => {
  if (rating === null) return "bg-gray-100";
  if (rating >= 4.5) return "bg-green-50";
  if (rating >= 4) return "bg-green-50";
  if (rating >= 3) return "bg-yellow-50";
  if (rating >= 2) return "bg-orange-50";
  return "bg-red-50";
};

export const RatingDisplay: React.FC<RatingDisplayProps> = ({
  averageRating,
  reviewCount,
}) => {
  const ratingLabel = getRatingLabel(averageRating);
  const ratingColor = getRatingColor(averageRating);
  const ratingBgColor = getRatingBgColor(averageRating);

  // Calculate number of full and partial stars
  const fullStars = averageRating ? Math.floor(averageRating) : 0;
  const hasHalfStar = averageRating ? averageRating % 1 !== 0 : false;
  const emptyStars = 5 - (averageRating ? Math.ceil(averageRating) : 0);

  return (
    <div className={`rounded-lg p-4 ${ratingBgColor} border border-gray-200`}>
      {/* Stars and rating score */}
      <div className="flex items-center gap-3 mb-3">
        {/* Star visualization */}
        <div className="flex gap-0.5">
          {/* Full stars */}
          {Array.from({ length: fullStars }).map((_, i) => (
            <span key={`full-${i}`} className="text-lg">
              ⭐
            </span>
          ))}

          {/* Half star */}
          {hasHalfStar && (
            <span key="half" className="text-lg">
              ⭐
            </span>
          )}

          {/* Empty stars */}
          {Array.from({ length: emptyStars }).map((_, i) => (
            <span key={`empty-${i}`} className="text-lg opacity-30">
              ⭐
            </span>
          ))}
        </div>

        {/* Rating score */}
        {averageRating !== null ? (
          <span className="text-lg font-bold text-gray-900">
            {averageRating.toFixed(1)}/5
          </span>
        ) : (
          <span className="text-lg font-bold text-gray-500">No rating</span>
        )}
      </div>

      {/* Rating label and review count */}
      <div className="flex items-center justify-between">
        <p className={`font-semibold text-sm ${ratingColor}`}>{ratingLabel}</p>
        {reviewCount > 0 && (
          <p className="text-xs text-gray-600">
            Based on {reviewCount} review{reviewCount !== 1 ? "s" : ""}
          </p>
        )}
      </div>

      {/* Note for low ratings (neutral tone) */}
      {averageRating !== null && averageRating < 3 && (
        <p className="text-xs text-gray-600 mt-2">
          This contractor has received feedback worth considering before
          assignment.
        </p>
      )}
    </div>
  );
};
