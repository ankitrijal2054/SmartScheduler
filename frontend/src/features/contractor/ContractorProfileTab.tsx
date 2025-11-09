/**
 * ContractorProfileTab Component
 * Displays contractor's profile with statistics and recent reviews
 * Used in Story 5.4 - Contractor Rating & Earnings History
 */

import React, { useEffect } from "react";
import { useMyProfile } from "@/hooks/useMyProfile";
import { ProfileStatsPanel } from "./ProfileStatsPanel";
import { RecentReviewsList } from "./RecentReviewsList";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

/**
 * Component to display contractor's own profile tab
 * Shows stats panel and recent reviews
 */
export const ContractorProfileTab: React.FC = () => {
  const { profile, loading, error, refetch } = useMyProfile();

  if (loading && !profile) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-6">
        <h3 className="text-red-900 font-semibold mb-2">
          Error Loading Profile
        </h3>
        <p className="text-red-700 mb-4">{error}</p>
        <button
          onClick={refetch}
          className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
        >
          Try Again
        </button>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">Unable to load profile</p>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Stats Panel */}
      <ProfileStatsPanel profile={profile} />

      {/* Recent Reviews Section */}
      <RecentReviewsList reviews={profile.recentReviews} />
    </div>
  );
};
