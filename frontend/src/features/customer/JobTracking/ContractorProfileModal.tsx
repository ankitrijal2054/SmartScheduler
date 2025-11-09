/**
 * ContractorProfileModal Component
 * Modal displaying contractor profile with rating, reviews, and contact information
 * Accessible modal with keyboard navigation and focus management
 */

import React, { useEffect, useRef, useCallback } from "react";
import { Contractor } from "@/types/Contractor";
import { ReviewWithCustomer } from "@/types/Customer";
import { RatingDisplay } from "./RatingDisplay";
import { ReviewsList } from "./ReviewsList";

interface ContractorProfileModalProps {
  isOpen: boolean;
  onClose: () => void;
  contractor: Contractor | null;
  reviews: ReviewWithCustomer[];
  loading?: boolean;
}

/**
 * Format phone number for display
 */
const formatPhoneNumber = (phone: string): string => {
  const cleaned = phone.replace(/\D/g, "");
  if (cleaned.length === 10) {
    return `(${cleaned.slice(0, 3)}) ${cleaned.slice(3, 6)}-${cleaned.slice(
      6
    )}`;
  }
  return phone;
};

export const ContractorProfileModal: React.FC<ContractorProfileModalProps> = ({
  isOpen,
  onClose,
  contractor,
  reviews,
  loading = false,
}) => {
  const modalRef = useRef<HTMLDivElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  const previousActiveElementRef = useRef<Element | null>(null);

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
      // Store the previously focused element
      previousActiveElementRef.current = document.activeElement;

      document.addEventListener("mousedown", handleClickOutside);
      document.body.style.overflow = "hidden";

      // Focus close button for accessibility
      if (closeButtonRef.current) {
        setTimeout(() => closeButtonRef.current?.focus(), 0);
      }
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.body.style.overflow = "unset";
    };
  }, [isOpen, onClose]);

  // Handle Escape key
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && isOpen) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener("keydown", handleKeyDown);
    }

    return () => {
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen, onClose]);

  // Handle close and restore focus
  const handleClose = useCallback(() => {
    onClose();
    // Restore focus to the element that triggered the modal
    if (previousActiveElementRef.current instanceof HTMLElement) {
      previousActiveElementRef.current.focus();
    }
  }, [onClose]);

  if (!isOpen) return null;

  return (
    <>
      {/* Modal backdrop */}
      <div
        className="fixed inset-0 bg-black/50 z-40 transition-opacity"
        aria-modal="true"
        role="presentation"
      />

      {/* Modal container */}
      <div
        className="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="dialog"
        aria-labelledby="contractor-profile-title"
        aria-modal="true"
      >
        {/* Modal content */}
        <div
          ref={modalRef}
          className="bg-white rounded-lg shadow-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto"
        >
          {/* Modal header */}
          <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between z-10">
            <h2
              id="contractor-profile-title"
              className="text-xl font-bold text-gray-900"
            >
              Contractor Profile
            </h2>
            <button
              ref={closeButtonRef}
              onClick={handleClose}
              className="text-gray-500 hover:text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded p-1"
              aria-label="Close contractor profile modal"
              type="button"
            >
              <span className="text-2xl">✕</span>
            </button>
          </div>

          {/* Modal body */}
          <div className="px-6 py-6 space-y-6">
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
              </div>
            ) : contractor ? (
              <>
                {/* Contractor name and info section */}
                <div className="space-y-4">
                  <h3 className="text-2xl font-bold text-gray-900">
                    {contractor.name}
                  </h3>

                  {/* Placeholder for contractor photo */}
                  <div className="w-24 h-24 bg-gray-200 rounded-lg flex items-center justify-center">
                    <span className="text-4xl">👤</span>
                  </div>
                </div>

                {/* Rating section */}
                <div>
                  <h4 className="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-3">
                    Rating
                  </h4>
                  <RatingDisplay
                    averageRating={contractor.rating}
                    reviewCount={contractor.reviewCount}
                  />
                </div>

                {/* Contact section */}
                {contractor &&
                  "phoneNumber" in contractor &&
                  contractor.phoneNumber &&
                  typeof contractor.phoneNumber === "string" && (
                    <div>
                      <h4 className="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-3">
                        Contact
                      </h4>
                      <a
                        href={`tel:${contractor.phoneNumber}`}
                        className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                      >
                        📞 Call {formatPhoneNumber(contractor.phoneNumber)}
                      </a>
                      <p className="text-xs text-gray-500 mt-2">
                        Click to call this contractor
                      </p>
                    </div>
                  )}

                {/* Reviews section */}
                <div className="border-t pt-6">
                  <ReviewsList reviews={reviews} maxReviews={5} />
                </div>
              </>
            ) : (
              <div className="text-center py-12">
                <p className="text-gray-500">
                  Unable to load contractor profile. Please try again.
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
};
