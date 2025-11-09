/**
 * ContractorListItem Component
 * Displays contractor info with Add/Remove button
 */

import React, { useState } from "react";
import { Contractor } from "@/types/Contractor";
import { LoadingSpinner } from "./LoadingSpinner";

interface ContractorListItemProps {
  contractor: Contractor;
  mode: "my-list" | "available";
  onAdd?: (contractorId: string) => Promise<boolean>;
  onRemove?: (contractorId: string) => Promise<boolean>;
  isLoading?: boolean;
}

export const ContractorListItem: React.FC<ContractorListItemProps> = ({
  contractor,
  mode,
  onAdd,
  onRemove,
  isLoading = false,
}) => {
  const [confirmRemove, setConfirmRemove] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);

  const handleAddClick = async () => {
    if (!onAdd) return;
    setActionLoading(true);
    await onAdd(contractor.id);
    setActionLoading(false);
  };

  const handleRemoveClick = async () => {
    if (!onRemove) return;
    setActionLoading(true);
    const success = await onRemove(contractor.id);
    setActionLoading(false);
    if (success) {
      setConfirmRemove(false);
    }
  };

  const ratingDisplay =
    contractor.rating !== null ? contractor.rating.toFixed(1) : "—";

  return (
    <>
      <div className="flex items-center justify-between border-b border-gray-200 py-4">
        <div className="flex-1">
          {/* Contractor name and trade type */}
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-gray-900">{contractor.name}</h3>
            {contractor.isActive ? (
              <span className="inline-block rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800">
                Active
              </span>
            ) : (
              <span className="inline-block rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
                Inactive
              </span>
            )}
          </div>

          {/* Trade type and location */}
          <p className="mt-1 text-sm text-gray-600">
            {contractor.tradeType} • {contractor.location}
          </p>

          {/* Rating and review count */}
          <div className="mt-1 flex items-center gap-2">
            <div className="flex items-center gap-1">
              <span className="text-sm font-medium text-gray-900">
                {ratingDisplay}
              </span>
              <span className="text-sm text-gray-600">
                ({contractor.reviewCount} reviews)
              </span>
            </div>
          </div>
        </div>

        {/* Action button */}
        <div className="ml-4">
          {isLoading || actionLoading ? (
            <div className="flex justify-center">
              <LoadingSpinner size="sm" />
            </div>
          ) : mode === "my-list" ? (
            // Remove button with confirmation
            <div>
              {confirmRemove ? (
                <div className="flex gap-2">
                  <button
                    onClick={handleRemoveClick}
                    className="rounded-md bg-red-600 px-3 py-1 text-sm font-medium text-white hover:bg-red-700"
                  >
                    Confirm
                  </button>
                  <button
                    onClick={() => setConfirmRemove(false)}
                    className="rounded-md border border-gray-300 bg-white px-3 py-1 text-sm font-medium text-gray-700 hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                </div>
              ) : (
                <button
                  onClick={() => setConfirmRemove(true)}
                  className="rounded-md border border-gray-300 bg-white px-3 py-1 text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  Remove
                </button>
              )}
            </div>
          ) : (
            // Add button
            <button
              onClick={handleAddClick}
              disabled={!contractor.isActive}
              className={`rounded-md px-3 py-1 text-sm font-medium ${
                contractor.inDispatcherList
                  ? "border border-green-300 bg-green-50 text-green-700"
                  : contractor.isActive
                  ? "border border-blue-300 bg-blue-50 text-blue-700 hover:bg-blue-100"
                  : "border border-gray-300 bg-gray-50 text-gray-400"
              }`}
            >
              {contractor.inDispatcherList ? "✓ Added" : "Add"}
            </button>
          )}
        </div>
      </div>
    </>
  );
};
