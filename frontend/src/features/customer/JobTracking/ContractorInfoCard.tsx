/**
 * ContractorInfoCard Component
 * Displays contractor information including name, rating, phone, and ETA
 * Shows conditionally based on job status
 */

import React from "react";
import { Contractor } from "@/types/Contractor";

interface ContractorInfoCardProps {
  contractor?: Contractor & {
    phoneNumber?: string;
    averageRating?: number;
  };
  jobStatus: string;
  estimatedArrivalTime?: string;
}

/**
 * Format phone number for display
 */
const formatPhoneNumber = (phone: string): string => {
  // Clean the phone number
  const cleaned = phone.replace(/\D/g, "");
  // Format as (XXX) XXX-XXXX if it's 10 digits
  if (cleaned.length === 10) {
    return `(${cleaned.slice(0, 3)}) ${cleaned.slice(3, 6)}-${cleaned.slice(
      6
    )}`;
  }
  return phone;
};

/**
 * Render star rating
 */
const renderStarRating = (
  rating: number | undefined | null
): React.ReactNode => {
  if (rating === undefined || rating === null) return null;

  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 !== 0;

  return (
    <div className="flex items-center gap-2">
      <div className="flex gap-0.5">
        {/* Full stars */}
        {Array.from({ length: fullStars }).map((_, i) => (
          <span key={`full-${i}`} className="text-lg">
            ⭐
          </span>
        ))}

        {/* Half star */}
        {hasHalfStar && <span className="text-lg">⭐</span>}

        {/* Empty stars */}
        {Array.from({ length: 5 - Math.ceil(rating) }).map((_, i) => (
          <span key={`empty-${i}`} className="text-lg opacity-30">
            ⭐
          </span>
        ))}
      </div>
      <span className="text-sm font-semibold text-gray-700">
        {rating.toFixed(1)}/5
      </span>
    </div>
  );
};

/**
 * Format datetime for display
 */
const formatDateTime = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return dateString;
  }
};

export const ContractorInfoCard: React.FC<ContractorInfoCardProps> = ({
  contractor,
  jobStatus,
  estimatedArrivalTime,
}) => {
  const isAssigned = ["Assigned", "InProgress", "Completed"].includes(
    jobStatus
  );

  if (!isAssigned || !contractor) {
    return (
      <div className="w-full bg-white rounded-lg shadow-md p-6">
        <h2 className="text-lg sm:text-xl font-bold text-gray-900 mb-4">
          Contractor Information
        </h2>
        <div className="flex items-center justify-center py-8">
          <p className="text-gray-500 text-center">
            {jobStatus === "Pending"
              ? "Waiting for contractor assignment..."
              : "Contractor details not yet available"}
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full bg-white rounded-lg shadow-md overflow-hidden">
      {/* Header */}
      <div className="border-b px-6 py-4 bg-gradient-to-r from-blue-50 to-indigo-50">
        <h2 className="text-lg sm:text-xl font-bold text-gray-900">
          Assigned Contractor
        </h2>
      </div>

      {/* Content */}
      <div className="px-6 py-6 space-y-6">
        {/* Contractor Name */}
        <div>
          <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
            Name
          </label>
          <p className="text-lg font-semibold text-gray-900">
            {contractor.name}
          </p>
        </div>

        {/* Rating */}
        {contractor.averageRating !== undefined &&
          contractor.averageRating !== null && (
            <div>
              <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
                Rating
              </label>
              <div className="flex items-center justify-between">
                {renderStarRating(contractor.averageRating)}
                {contractor.reviewCount !== undefined &&
                  contractor.reviewCount > 0 && (
                    <span className="text-xs text-gray-500">
                      {contractor.reviewCount} review
                      {contractor.reviewCount !== 1 ? "s" : ""}
                    </span>
                  )}
              </div>
            </div>
          )}

        {/* Phone Number */}
        {contractor.phoneNumber && (
          <div>
            <label className="block text-xs font-semibold text-gray-600 uppercase tracking-wide mb-2">
              Phone Number
            </label>
            <a
              href={`tel:${contractor.phoneNumber}`}
              className="inline-flex items-center gap-2 text-blue-600 hover:text-blue-800 hover:underline font-semibold transition-colors"
            >
              <span>📞</span>
              <span>{formatPhoneNumber(contractor.phoneNumber)}</span>
            </a>
            <p className="text-xs text-gray-500 mt-1">Click to call</p>
          </div>
        )}

        {/* ETA */}
        {estimatedArrivalTime && (
          <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
            <label className="block text-xs font-semibold text-amber-800 uppercase tracking-wide mb-2">
              Estimated Arrival Time
            </label>
            <p className="text-base font-semibold text-amber-900">
              {formatDateTime(estimatedArrivalTime)}
            </p>
            <p className="text-xs text-amber-700 mt-1">
              ⏰ Contractor should arrive around this time
            </p>
          </div>
        )}

        {/* Status Badge */}
        <div className="pt-4 border-t flex items-center justify-between">
          <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">
            Status
          </span>
          <span
            className={`px-3 py-1 text-xs font-semibold rounded-full ${
              jobStatus === "InProgress"
                ? "bg-orange-100 text-orange-800"
                : jobStatus === "Completed"
                ? "bg-green-100 text-green-800"
                : "bg-blue-100 text-blue-800"
            }`}
          >
            {jobStatus === "Pending"
              ? "Awaiting"
              : jobStatus === "InProgress"
              ? "In Progress"
              : jobStatus === "Completed"
              ? "Completed"
              : "Assigned"}
          </span>
        </div>
      </div>
    </div>
  );
};
