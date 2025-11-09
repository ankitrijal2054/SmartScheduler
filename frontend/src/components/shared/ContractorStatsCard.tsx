/**
 * ContractorStatsCard Component
 * Displays contractor performance metrics and warning badges
 */

import React from "react";
import { ContractorStats, ContractorWarnings } from "@/types/Contractor";

interface ContractorStatsCardProps {
  stats: ContractorStats;
  warnings?: ContractorWarnings;
}

/**
 * Renders star rating visually (1-5 stars)
 */
const StarRating: React.FC<{ rating: number | null; size?: "sm" | "md" }> = ({
  rating,
  size = "md",
}) => {
  if (rating === null) return <span className="text-gray-400">Not rated</span>;

  const filledStars = Math.round(rating);
  const emptyStars = 5 - filledStars;
  const sizeClass = size === "sm" ? "text-sm" : "text-lg";

  return (
    <div className={`flex items-center gap-1 ${sizeClass}`}>
      {Array(filledStars)
        .fill(0)
        .map((_, i) => (
          <span key={`filled-${i}`} className="text-yellow-400">
            ★
          </span>
        ))}
      {Array(emptyStars)
        .fill(0)
        .map((_, i) => (
          <span key={`empty-${i}`} className="text-gray-300">
            ★
          </span>
        ))}
      <span className="ml-2 text-gray-600">({rating.toFixed(1)})</span>
    </div>
  );
};

/**
 * Warning Badge Component
 */
const WarningBadge: React.FC<{
  label: string;
  type: "danger" | "warning";
}> = ({ label, type }) => {
  const typeStyles =
    type === "danger"
      ? "bg-red-100 text-red-800 border-red-300"
      : "bg-yellow-100 text-yellow-800 border-yellow-300";

  return (
    <div
      className={`px-3 py-1 rounded-full text-sm font-medium border ${typeStyles}`}
    >
      {label}
    </div>
  );
};

/**
 * Stat Item Component
 */
const StatItem: React.FC<{ label: string; value: string | number }> = ({
  label,
  value,
}) => (
  <div className="flex flex-col items-start gap-1">
    <span className="text-xs font-semibold text-gray-600 uppercase">
      {label}
    </span>
    <span className="text-2xl font-bold text-gray-900">{value}</span>
  </div>
);

export const ContractorStatsCard: React.FC<ContractorStatsCardProps> = ({
  stats,
  warnings,
}) => {
  return (
    <div className="w-full bg-white rounded-lg border border-gray-200 p-6 space-y-6">
      {/* Main Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatItem label="Total Jobs Assigned" value={stats.totalJobsAssigned} />
        <StatItem label="Jobs Completed" value={stats.totalJobsCompleted} />
        <StatItem
          label="Acceptance Rate"
          value={`${stats.acceptanceRate.toFixed(1)}%`}
        />
        <StatItem label="Reviews" value={stats.totalReviews} />
      </div>

      {/* Average Rating */}
      <div className="border-t border-gray-200 pt-4">
        <h4 className="text-xs font-semibold text-gray-600 uppercase mb-3">
          Average Rating
        </h4>
        <StarRating rating={stats.averageRating} />
      </div>

      {/* Warnings */}
      {warnings && (warnings.lowRating || warnings.highCancellationRate) && (
        <div className="border-t border-gray-200 pt-4 space-y-3">
          <h4 className="text-xs font-semibold text-gray-600 uppercase">
            Alerts
          </h4>
          <div className="flex flex-wrap gap-3">
            {warnings.lowRating && (
              <WarningBadge label="⚠️ Low Rating" type="danger" />
            )}
            {warnings.highCancellationRate && (
              <WarningBadge label="⚠️ High Cancellation Rate" type="warning" />
            )}
          </div>
        </div>
      )}
    </div>
  );
};
