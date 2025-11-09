/**
 * ProfileStatsPanel Component
 * Displays contractor statistics: name, rating, job counts, acceptance rate
 * Used in Story 5.4 - Contractor Rating & Earnings History
 */

import React from "react";
import { ContractorProfileData } from "@/types/ContractorProfile";
import { Star } from "lucide-react";

interface ProfileStatsPanelProps {
  profile: ContractorProfileData;
}

/**
 * Component to display contractor profile statistics
 * Shows: Name, Average Rating (with stars), Job counts, Acceptance rate
 */
export const ProfileStatsPanel: React.FC<ProfileStatsPanelProps> = ({
  profile,
}) => {
  const renderStars = (rating: number | null) => {
    if (rating === null) return null;

    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 !== 0;

    return (
      <div className="flex items-center gap-1">
        {Array.from({ length: 5 }).map((_, i) => (
          <Star
            key={i}
            size={18}
            className={
              i < fullStars
                ? "fill-yellow-400 text-yellow-400"
                : i === fullStars && hasHalfStar
                ? "text-yellow-400"
                : "text-gray-300"
            }
          />
        ))}
      </div>
    );
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-8 border border-gray-200">
      {/* Profile Header */}
      <div className="mb-8">
        <h1 className="text-4xl font-bold text-gray-900">{profile.name}</h1>
        <p className="text-gray-600 mt-2">Your professional profile</p>
      </div>

      {/* Rating Section */}
      <div className="mb-8 pb-8 border-b border-gray-200">
        <div className="flex items-center gap-4">
          <div>
            <h3 className="text-gray-700 font-semibold mb-2">Your Rating</h3>
            {profile.averageRating !== null ? (
              <div className="flex items-center gap-3">
                {renderStars(profile.averageRating)}
                <span className="text-2xl font-bold text-gray-900">
                  {profile.averageRating.toFixed(1)}/5
                </span>
              </div>
            ) : (
              <p className="text-gray-500 italic">No ratings yet</p>
            )}
            <p className="text-sm text-gray-600 mt-2">
              {profile.reviewCount}{" "}
              {profile.reviewCount === 1 ? "review" : "reviews"}
            </p>
          </div>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {/* Total Jobs Assigned */}
        <div className="bg-gray-50 rounded-lg p-4">
          <p className="text-gray-600 text-sm font-medium">Jobs Assigned</p>
          <p className="text-3xl font-bold text-gray-900 mt-2">
            {profile.totalJobsAssigned}
          </p>
        </div>

        {/* Jobs Accepted */}
        <div className="bg-gray-50 rounded-lg p-4">
          <p className="text-gray-600 text-sm font-medium">Accepted</p>
          <p className="text-3xl font-bold text-indigo-600 mt-2">
            {profile.totalJobsAccepted}
          </p>
        </div>

        {/* Jobs Completed */}
        <div className="bg-gray-50 rounded-lg p-4">
          <p className="text-gray-600 text-sm font-medium">Completed</p>
          <p className="text-3xl font-bold text-green-600 mt-2">
            {profile.totalJobsCompleted}
          </p>
        </div>

        {/* Acceptance Rate */}
        <div className="bg-gray-50 rounded-lg p-4">
          <p className="text-gray-600 text-sm font-medium">Acceptance Rate</p>
          <p className="text-3xl font-bold text-teal-600 mt-2">
            {profile.acceptanceRate.toFixed(0)}%
          </p>
        </div>
      </div>

      {/* Optional: Earnings (shown if available) */}
      {profile.totalEarnings !== null &&
        profile.totalEarnings !== undefined && (
          <div className="mt-8 pt-8 border-t border-gray-200 bg-gradient-to-r from-indigo-50 to-teal-50 rounded-lg p-6">
            <p className="text-gray-700 font-semibold">Total Earnings</p>
            <p className="text-4xl font-bold text-indigo-600 mt-2">
              ${profile.totalEarnings.toFixed(2)}
            </p>
          </div>
        )}
    </div>
  );
};
