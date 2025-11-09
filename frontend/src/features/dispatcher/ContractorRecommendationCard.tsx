/**
 * ContractorRecommendationCard Component
 * Displays a single contractor recommendation with rank, rating, distance, and availability
 */

import React from "react";
import { format } from "date-fns";
import { RecommendedContractor } from "@/types/Contractor";

interface ContractorRecommendationCardProps {
  contractor: RecommendedContractor;
  onClick?: (contractor: RecommendedContractor) => void;
  onAssign?: (contractor: RecommendedContractor) => void;
  isAssigning?: boolean;
}

/**
 * Helper to render star rating with visual stars
 */
const StarRating: React.FC<{ rating: number | null; count: number }> = ({
  rating,
  count,
}) => {
  if (rating === null) {
    return <span className="text-sm text-gray-500">No ratings yet</span>;
  }

  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;

  return (
    <div className="flex items-center gap-1">
      <span className="text-sm">
        {Array.from({ length: 5 }).map((_, i) => (
          <span
            key={i}
            className={
              i < fullStars
                ? "text-yellow-400"
                : i === fullStars && hasHalfStar
                ? "text-yellow-400"
                : "text-gray-300"
            }
          >
            ★
          </span>
        ))}
      </span>
      <span className="text-sm font-medium text-gray-700">
        {rating.toFixed(1)} ({count} {count === 1 ? "review" : "reviews"})
      </span>
    </div>
  );
};

/**
 * Helper to get rank badge styling
 */
const getRankBadgeStyle = (rank: number): string => {
  switch (rank) {
    case 1:
      return "bg-yellow-100 text-yellow-800 border-yellow-300"; // Gold
    case 2:
      return "bg-gray-100 text-gray-800 border-gray-300"; // Silver
    default:
      return "bg-orange-100 text-orange-800 border-orange-300"; // Bronze (3-5)
  }
};

/**
 * Helper to format time slot
 */
const formatTimeSlot = (startTime: string, endTime: string): string => {
  try {
    const start = new Date(startTime);
    const end = new Date(endTime);
    return `${format(start, "h:mm a")} - ${format(end, "h:mm a")}`;
  } catch {
    return "Time available";
  }
};

export const ContractorRecommendationCard: React.FC<
  ContractorRecommendationCardProps
> = ({ contractor, onClick, onAssign, isAssigning = false }) => {
  const handleClick = () => {
    onClick?.(contractor);
  };

  const handleAssignClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    onAssign?.(contractor);
  };

  const firstSlot = contractor.availableTimeSlots[0];
  const scorePercentage = Math.round(contractor.score * 100);

  return (
    <div
      onClick={handleClick}
      className={`rounded-lg border border-gray-200 bg-white p-4 transition-all hover:shadow-lg ${
        onClick ? "cursor-pointer hover:border-blue-300" : ""
      }`}
      role="article"
      aria-label={`Contractor ${contractor.rank}: ${contractor.name}, ${
        contractor.avgRating?.toFixed(1) || "No"
      } stars, ${contractor.distance.toFixed(1)} miles away`}
    >
      <div className="space-y-3">
        {/* Rank Badge and Name */}
        <div className="flex items-start justify-between gap-3">
          <div className="flex-1">
            <h3 className="text-lg font-semibold text-gray-900">
              {contractor.name}
            </h3>
            <p className="text-xs text-gray-600">{contractor.tradeType}</p>
          </div>
          <div
            className={`flex h-10 w-10 items-center justify-center rounded-full border-2 font-bold ${getRankBadgeStyle(
              contractor.rank
            )}`}
            aria-label={`Rank: ${contractor.rank}`}
          >
            #{contractor.rank}
          </div>
        </div>

        {/* Rating */}
        <div className="py-2">
          <StarRating
            rating={contractor.avgRating}
            count={contractor.reviewCount}
          />
        </div>

        {/* Distance and Travel Time */}
        <div className="flex gap-4 text-sm">
          <div className="flex items-center gap-1">
            <span className="text-lg">📍</span>
            <span className="text-gray-700">
              {contractor.distance.toFixed(1)} miles
            </span>
          </div>
          <div className="flex items-center gap-1">
            <span className="text-lg">🕐</span>
            <span className="text-gray-700">{contractor.travelTime} min</span>
          </div>
        </div>

        {/* Score (internal use, optional) */}
        <div className="flex items-center gap-2">
          <div className="h-2 w-24 overflow-hidden rounded-full bg-gray-200">
            <div
              className="h-full bg-blue-500"
              style={{ width: `${scorePercentage}%` }}
              role="progressbar"
              aria-valuenow={scorePercentage}
              aria-valuemin={0}
              aria-valuemax={100}
              aria-label="Recommendation score"
            />
          </div>
          <span className="text-xs font-medium text-gray-600">
            {scorePercentage}%
          </span>
        </div>

        {/* Availability Slot */}
        {firstSlot && (
          <div className="rounded-md bg-green-50 p-2 text-sm">
            <p className="font-medium text-green-900">
              Available:{" "}
              {formatTimeSlot(firstSlot.startTime, firstSlot.endTime)}
            </p>
            {contractor.availableTimeSlots.length > 1 && (
              <p className="mt-1 text-xs text-green-700">
                +{contractor.availableTimeSlots.length - 1} more slot
                {contractor.availableTimeSlots.length > 2 ? "s" : ""}
              </p>
            )}
          </div>
        )}

        {/* Assign Button */}
        {onAssign && (
          <button
            onClick={handleAssignClick}
            disabled={isAssigning}
            className="w-full rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center justify-center gap-2"
            type="button"
            aria-label={`Assign ${contractor.name} to job`}
          >
            {isAssigning ? (
              <>
                <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                Assigning...
              </>
            ) : (
              <>
                Assign
                <span>→</span>
              </>
            )}
          </button>
        )}
      </div>
    </div>
  );
};
