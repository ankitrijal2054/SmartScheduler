/**
 * RatingSuccessMessage Component
 * Displays success confirmation after rating submission
 * Auto-dismisses after 5 seconds or on manual close
 */

import React, { useEffect, useRef } from "react";

interface RatingSuccessMessageProps {
  isVisible: boolean;
  onClose: () => void;
  rating?: number;
  comment?: string | null;
}

export const RatingSuccessMessage: React.FC<RatingSuccessMessageProps> = ({
  isVisible,
  onClose,
  rating,
  comment,
}) => {
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const messageRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isVisible) {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
        timeoutRef.current = null;
      }
      return;
    }

    // Focus the message container for accessibility
    setTimeout(() => {
      messageRef.current?.focus();
    }, 0);

    // Auto-dismiss after 5 seconds
    timeoutRef.current = setTimeout(() => {
      onClose();
    }, 5000);

    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
        timeoutRef.current = null;
      }
    };
  }, [isVisible, onClose]);

  if (!isVisible) {
    return null;
  }

  return (
    <div
      ref={messageRef}
      tabIndex={-1}
      role="status"
      aria-live="polite"
      aria-atomic="true"
      className="fixed inset-0 z-50 flex items-end justify-center pointer-events-none pb-4 px-4 sm:pb-6 sm:px-6 lg:pb-8 lg:px-8"
    >
      {/* Backdrop overlay */}
      <div
        className="absolute inset-0 bg-black/20 pointer-events-auto"
        onClick={onClose}
        data-testid="rating-success-backdrop"
      />

      {/* Success message card */}
      <div className="pointer-events-auto relative bg-white rounded-lg shadow-2xl max-w-md w-full overflow-hidden">
        {/* Background accent */}
        <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-green-500 to-emerald-500" />

        <div className="p-6 sm:p-8">
          {/* Success icon */}
          <div className="flex items-start gap-4">
            <div className="flex-shrink-0">
              <div className="flex items-center justify-center h-12 w-12 rounded-full bg-green-100">
                <svg
                  className="h-6 w-6 text-green-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M5 13l4 4L19 7"
                  />
                </svg>
              </div>
            </div>

            {/* Message content */}
            <div className="flex-1 min-w-0">
              <h3 className="text-lg font-semibold text-gray-900 mb-1">
                Thank you for your feedback!
              </h3>

              <p className="text-sm text-gray-600 mb-4">
                Your rating has been submitted successfully.
              </p>

              {/* Display submitted details */}
              {(rating || comment) && (
                <div className="bg-gray-50 rounded-lg p-3 mb-4 space-y-2">
                  {rating && (
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-gray-600">
                        Your Rating:
                      </span>
                      <div className="flex gap-0.5">
                        {[...Array(5)].map((_, i) => (
                          <span
                            key={i}
                            className={`text-lg ${
                              i < rating ? "text-yellow-400" : "text-gray-300"
                            }`}
                          >
                            ★
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                  {comment && (
                    <div>
                      <p className="text-sm text-gray-600 mb-1">
                        Your Comment:
                      </p>
                      <p className="text-sm text-gray-800 italic line-clamp-2">
                        "{comment}"
                      </p>
                    </div>
                  )}
                </div>
              )}

              {/* Additional message */}
              <p className="text-xs text-gray-500">
                This rating helps other customers make informed decisions.
              </p>
            </div>

            {/* Close button */}
            <button
              onClick={onClose}
              className="flex-shrink-0 text-gray-400 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 rounded transition-colors"
              aria-label="Close success message"
            >
              <svg
                className="h-6 w-6"
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
        </div>

        {/* Auto-dismiss indicator */}
        <div className="h-1 bg-gray-100 relative overflow-hidden">
          <div
            className="h-full bg-gradient-to-r from-blue-500 to-blue-600 animate-pulse"
            style={{
              animation: "shrink 5s linear forwards",
            }}
          />
        </div>
      </div>

      {/* CSS for shrink animation */}
      <style>{`
        @keyframes shrink {
          from {
            width: 100%;
          }
          to {
            width: 0%;
          }
        }
      `}</style>
    </div>
  );
};
