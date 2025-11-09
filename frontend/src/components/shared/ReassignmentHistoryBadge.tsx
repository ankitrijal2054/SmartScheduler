/**
 * ReassignmentHistoryBadge Component
 * Optional badge showing how many times a job has been reassigned
 */

import React, { useState } from "react";

interface ReassignmentHistoryBadgeProps {
  reassignmentCount?: number;
  maxCount?: number; // Badge shows color change after this many reassignments
}

/**
 * ReassignmentHistoryBadge Component
 * Displays a visual indicator of how many times a job has been reassigned
 * Helps dispatchers identify complex/problematic jobs
 */
export const ReassignmentHistoryBadge: React.FC<
  ReassignmentHistoryBadgeProps
> = ({ reassignmentCount = 0, maxCount = 2 }) => {
  const [showTooltip, setShowTooltip] = useState(false);

  // Don't show badge if no reassignments
  if (!reassignmentCount || reassignmentCount === 0) {
    return null;
  }

  // Determine badge color based on reassignment count
  const isHighReassignment = reassignmentCount > maxCount;
  const badgeClasses = isHighReassignment
    ? "bg-red-100 text-red-700 border border-red-300"
    : "bg-yellow-100 text-yellow-700 border border-yellow-300";

  return (
    <div
      className="relative inline-block"
      onMouseEnter={() => setShowTooltip(true)}
      onMouseLeave={() => setShowTooltip(false)}
    >
      <span
        className={`inline-flex items-center gap-1 rounded-full px-2 py-1 text-xs font-semibold ${badgeClasses}`}
        title={`This job has been reassigned ${reassignmentCount} time${
          reassignmentCount > 1 ? "s" : ""
        }`}
        role="status"
        aria-label={`Job reassigned ${reassignmentCount} time${
          reassignmentCount > 1 ? "s" : ""
        }`}
      >
        <span>🔄</span>
        <span>{reassignmentCount}x</span>
      </span>

      {/* Tooltip on hover */}
      {showTooltip && (
        <div className="absolute bottom-full left-1/2 mb-2 -translate-x-1/2 transform whitespace-nowrap rounded-md bg-gray-900 px-2 py-1 text-xs text-white">
          {reassignmentCount === 1
            ? "This job has been reassigned 1 time"
            : `This job has been reassigned ${reassignmentCount} times`}
          {isHighReassignment && (
            <div className="mt-1 text-xs text-red-200">
              Consider investigating if there are issues
            </div>
          )}
        </div>
      )}
    </div>
  );
};
