/**
 * DeclineReasonModal Component
 * Shows reason selector when contractor declines a job
 * Allows "Other" option with custom text input
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import React, { useState } from "react";
import { X } from "lucide-react";

interface DeclineReasonModalProps {
  isOpen: boolean;
  isLoading?: boolean;
  onConfirm: (reason?: string) => Promise<void>;
  onCancel: () => void;
}

const DECLINE_REASONS = [
  "Scheduling conflict",
  "Out of service area",
  "Not my specialty",
  "Other",
] as const;

type DeclineReason = (typeof DECLINE_REASONS)[number];

export const DeclineReasonModal: React.FC<DeclineReasonModalProps> = ({
  isOpen,
  isLoading = false,
  onConfirm,
  onCancel,
}) => {
  const [selectedReason, setSelectedReason] = useState<DeclineReason | null>(
    null
  );
  const [customReason, setCustomReason] = useState("");

  if (!isOpen) return null;

  const handleConfirm = async () => {
    const reason =
      selectedReason === "Other" ? customReason : selectedReason || undefined;
    await onConfirm(reason);
    // Reset on success
    setSelectedReason(null);
    setCustomReason("");
  };

  const handleCancel = () => {
    setSelectedReason(null);
    setCustomReason("");
    onCancel();
  };

  const isConfirmDisabled =
    !selectedReason || (selectedReason === "Other" && !customReason.trim());

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={handleCancel}
        role="presentation"
      />

      {/* Modal */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <h2 className="text-lg font-semibold text-gray-900">Decline Job</h2>
            <button
              onClick={handleCancel}
              disabled={isLoading}
              className="text-gray-400 hover:text-gray-600 transition disabled:opacity-50"
              aria-label="Close"
            >
              <X size={20} />
            </button>
          </div>

          {/* Content */}
          <div className="p-6 space-y-4">
            <p className="text-sm text-gray-600">
              Why are you declining this job? (Optional)
            </p>

            {/* Reason Options */}
            <div className="space-y-2">
              {DECLINE_REASONS.map((reason) => (
                <label
                  key={reason}
                  className="flex items-center gap-3 cursor-pointer"
                >
                  <input
                    type="radio"
                    name="decline-reason"
                    value={reason}
                    checked={selectedReason === reason}
                    onChange={() => setSelectedReason(reason)}
                    disabled={isLoading}
                    className="w-4 h-4 text-indigo-600 focus:ring-2 focus:ring-indigo-500"
                  />
                  <span className="text-sm text-gray-700">{reason}</span>
                </label>
              ))}
            </div>

            {/* Custom Reason Text */}
            {selectedReason === "Other" && (
              <textarea
                value={customReason}
                onChange={(e) => setCustomReason(e.target.value)}
                disabled={isLoading}
                placeholder="Please explain why you're declining..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-sm resize-none h-20"
              />
            )}
          </div>

          {/* Actions */}
          <div className="p-6 border-t border-gray-200 bg-gray-50 flex gap-3">
            <button
              onClick={handleCancel}
              disabled={isLoading}
              className="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              onClick={handleConfirm}
              disabled={isConfirmDisabled || isLoading}
              className="flex-1 px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition disabled:opacity-50"
            >
              {isLoading ? "Declining..." : "Decline"}
            </button>
          </div>
        </div>
      </div>
    </>
  );
};
