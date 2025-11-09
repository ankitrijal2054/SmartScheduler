/**
 * ContractorProfileModal Component
 * Modal displaying contractor profile, stats, and job history
 */

import React, { useEffect, useRef } from "react";
import { useContractorHistory } from "@/hooks/useContractorHistory";
import { ContractorStatsCard } from "@/components/shared/ContractorStatsCard";
import { JobHistoryTable } from "@/components/shared/JobHistoryTable";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

interface ContractorProfileModalProps {
  contractorId: string;
  isOpen: boolean;
  onClose: () => void;
}

/**
 * Rating display component
 */
const RatingDisplay: React.FC<{ rating: number | null }> = ({ rating }) => {
  if (rating === null) return <span className="text-gray-500">Not rated</span>;

  const filledStars = Math.round(rating);
  const emptyStars = 5 - filledStars;

  return (
    <div className="flex items-center gap-2">
      <div className="flex gap-0.5">
        {Array(filledStars)
          .fill(0)
          .map((_, i) => (
            <span key={`filled-${i}`} className="text-lg text-yellow-400">
              ★
            </span>
          ))}
        {Array(emptyStars)
          .fill(0)
          .map((_, i) => (
            <span key={`empty-${i}`} className="text-lg text-gray-300">
              ★
            </span>
          ))}
      </div>
      <span className="text-sm text-gray-600">({rating.toFixed(1)})</span>
    </div>
  );
};

export const ContractorProfileModal: React.FC<ContractorProfileModalProps> = ({
  contractorId,
  isOpen,
  onClose,
}) => {
  const { data, loading, error } = useContractorHistory(contractorId);
  const modalRef = useRef<HTMLDivElement>(null);

  // Handle click outside modal to close
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        modalRef.current &&
        !modalRef.current.contains(event.target as Node)
      ) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
      document.body.style.overflow = "hidden";
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.body.style.overflow = "unset";
    };
  }, [isOpen, onClose]);

  // Handle Escape key
  useEffect(() => {
    const handleEscapeKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener("keydown", handleEscapeKey);
    }

    return () => {
      document.removeEventListener("keydown", handleEscapeKey);
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4"
      role="presentation"
    >
      <div
        ref={modalRef}
        className="bg-white rounded-lg shadow-lg max-w-4xl w-full max-h-[90vh] overflow-y-auto"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
      >
        {/* Header */}
        <div className="sticky top-0 bg-white border-b border-gray-200 p-6 flex items-start justify-between">
          <div className="flex-1">
            <div className="flex flex-col lg:flex-row lg:items-start gap-4">
              <div>
                <h2
                  id="modal-title"
                  className="text-2xl font-bold text-gray-900"
                >
                  {loading
                    ? "Loading..."
                    : data?.contractor.name || "Contractor"}
                </h2>
                {!loading && data && (
                  <div className="mt-2 space-y-2">
                    <p className="text-gray-600">
                      📍 {data.contractor.location}
                    </p>
                    <p className="text-gray-600">
                      🔧 {data.contractor.tradeType}
                    </p>
                    <p className="text-gray-600">
                      ☎️ {data.contractor.phoneNumber}
                    </p>
                  </div>
                )}
              </div>
              {!loading && data && (
                <div className="flex-shrink-0">
                  <RatingDisplay rating={data.stats.averageRating} />
                </div>
              )}
            </div>
          </div>
          <button
            onClick={onClose}
            className="flex-shrink-0 p-2 text-gray-400 hover:text-gray-600 transition"
            aria-label="Close modal"
          >
            <svg
              className="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-8">
          {loading && (
            <div className="flex justify-center py-12">
              <LoadingSpinner />
            </div>
          )}

          {error && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
              <h3 className="text-red-900 font-semibold">
                Error Loading Profile
              </h3>
              <p className="text-red-700 text-sm mt-1">{error}</p>
              <button
                onClick={() => window.location.reload()}
                className="mt-3 px-3 py-1 bg-red-100 text-red-800 rounded hover:bg-red-200 transition text-sm font-medium"
              >
                Retry
              </button>
            </div>
          )}

          {data && (
            <>
              {/* Stats Section */}
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Performance Stats
                </h3>
                <ContractorStatsCard
                  stats={data.stats}
                  warnings={data.warnings}
                />
              </div>

              {/* Job History Section */}
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Job History (Last 10)
                </h3>
                <JobHistoryTable jobs={data.jobHistory} />
              </div>
            </>
          )}
        </div>

        {/* Footer */}
        <div className="sticky bottom-0 bg-gray-50 border-t border-gray-200 p-6 flex justify-end gap-3">
          <button
            onClick={onClose}
            className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-100 transition"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
