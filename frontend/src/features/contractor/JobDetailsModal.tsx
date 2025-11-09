/**
 * JobDetailsModal Component
 * Displays full details of a job assignment in a modal
 * Placeholder for Story 5.2 acceptance/decline workflow
 */

import React from "react";
import { X, MapPin, Clock, User, DollarSign } from "lucide-react";
import { Assignment } from "@/types/Assignment";
import { format } from "date-fns";

interface JobDetailsModalProps {
  assignment: Assignment & {
    jobType?: string;
    location?: string;
    scheduledTime?: string;
    customerName?: string;
    description?: string;
    estimatedDuration?: number;
    estimatedPay?: number;
  };
  isOpen: boolean;
  onClose: () => void;
}

export const JobDetailsModal: React.FC<JobDetailsModalProps> = ({
  assignment,
  isOpen,
  onClose,
}) => {
  if (!isOpen) return null;

  const scheduledDate = assignment.scheduledTime
    ? new Date(assignment.scheduledTime)
    : null;

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40 transition-opacity"
        onClick={onClose}
        role="presentation"
      />

      {/* Modal */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-lg shadow-xl max-w-lg w-full max-h-96 overflow-y-auto">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200 sticky top-0 bg-white">
            <h2 className="text-lg font-semibold text-gray-900">Job Details</h2>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition"
              aria-label="Close"
            >
              <X size={20} />
            </button>
          </div>

          {/* Content */}
          <div className="p-6 space-y-6">
            {/* Job Type and Status */}
            <div>
              <div className="flex items-center gap-2">
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-indigo-100 text-indigo-800">
                  {assignment.jobType || "General Job"}
                </span>
                <span className="inline-flex items-center px-2 py-1 rounded text-xs font-semibold bg-yellow-100 text-yellow-800">
                  {assignment.status}
                </span>
              </div>
            </div>

            {/* Location */}
            <div>
              <div className="flex items-start gap-3">
                <MapPin size={20} className="text-gray-400 mt-1 flex-shrink-0" />
                <div>
                  <p className="text-sm font-medium text-gray-600">Location</p>
                  <p className="text-base text-gray-900">
                    {assignment.location || "Location TBD"}
                  </p>
                </div>
              </div>
            </div>

            {/* Scheduled Time */}
            {scheduledDate && (
              <div>
                <div className="flex items-start gap-3">
                  <Clock size={20} className="text-gray-400 mt-1 flex-shrink-0" />
                  <div>
                    <p className="text-sm font-medium text-gray-600">
                      Scheduled Time
                    </p>
                    <p className="text-base text-gray-900">
                      {format(scheduledDate, "EEEE, MMMM d, yyyy 'at' h:mm a")}
                    </p>
                    {assignment.estimatedDuration && (
                      <p className="text-sm text-gray-500 mt-1">
                        ~{assignment.estimatedDuration} hours
                      </p>
                    )}
                  </div>
                </div>
              </div>
            )}

            {/* Customer Name */}
            <div>
              <div className="flex items-start gap-3">
                <User size={20} className="text-gray-400 mt-1 flex-shrink-0" />
                <div>
                  <p className="text-sm font-medium text-gray-600">Customer</p>
                  <p className="text-base text-gray-900">
                    {assignment.customerName || "Customer"}
                  </p>
                </div>
              </div>
            </div>

            {/* Estimated Pay */}
            {assignment.estimatedPay && (
              <div>
                <div className="flex items-start gap-3">
                  <DollarSign size={20} className="text-gray-400 mt-1 flex-shrink-0" />
                  <div>
                    <p className="text-sm font-medium text-gray-600">
                      Estimated Pay
                    </p>
                    <p className="text-base text-gray-900">
                      ${assignment.estimatedPay.toFixed(2)}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Description */}
            {assignment.description && (
              <div>
                <p className="text-sm font-medium text-gray-600 mb-2">
                  Description
                </p>
                <p className="text-sm text-gray-700 bg-gray-50 p-3 rounded-lg">
                  {assignment.description}
                </p>
              </div>
            )}
          </div>

          {/* Actions (Placeholder for Story 5.2) */}
          <div className="p-6 border-t border-gray-200 bg-gray-50 flex gap-3">
            <button
              onClick={onClose}
              className="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition"
            >
              Close
            </button>
            {assignment.status === "Pending" && (
              <>
                <button className="flex-1 px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition">
                  Decline
                </button>
                <button className="flex-1 px-4 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-lg transition">
                  Accept
                </button>
              </>
            )}
          </div>
        </div>
      </div>
    </>
  );
};



