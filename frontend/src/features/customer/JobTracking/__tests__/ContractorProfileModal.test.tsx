/**
 * ContractorProfileModal Component Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ContractorProfileModal } from "../ContractorProfileModal";
import { Contractor } from "@/types/Contractor";
import { ReviewWithCustomer } from "@/types/Customer";

describe("ContractorProfileModal Component", () => {
  const mockContractor: Contractor & { phoneNumber: string } = {
    id: "contractor-123",
    name: "John Smith",
    rating: 4.8,
    reviewCount: 27,
    location: "New York, NY",
    tradeType: "Plumbing",
    isActive: true,
    phoneNumber: "5551234567",
  };

  const mockReviews: ReviewWithCustomer[] = [
    {
      id: "review-1",
      jobId: "job-1",
      contractorId: "contractor-123",
      customerId: "customer-1",
      rating: 5,
      comment: "John was professional and quick",
      customerName: "Jane K.",
      createdAt: "2025-11-09T10:00:00Z",
    },
    {
      id: "review-2",
      jobId: "job-2",
      contractorId: "contractor-123",
      customerId: "customer-2",
      rating: 4,
      comment: "Good work, arrived on time",
      customerName: "Bob M.",
      createdAt: "2025-11-08T10:00:00Z",
    },
  ];

  const defaultProps = {
    isOpen: true,
    onClose: vi.fn(),
    contractor: mockContractor,
    reviews: mockReviews,
    loading: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should not render when isOpen is false", () => {
    render(<ContractorProfileModal {...defaultProps} isOpen={false} />);

    expect(screen.queryByText("Contractor Profile")).not.toBeInTheDocument();
  });

  it("should render modal when isOpen is true", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText("Contractor Profile")).toBeInTheDocument();
    expect(screen.getByText("John Smith")).toBeInTheDocument();
  });

  it("should display contractor name", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText("John Smith")).toBeInTheDocument();
  });

  it("should display contractor rating", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText("4.8/5")).toBeInTheDocument();
    expect(screen.getByText("Excellent")).toBeInTheDocument();
  });

  it("should display reviews", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText("Jane K.")).toBeInTheDocument();
    expect(screen.getByText("Bob M.")).toBeInTheDocument();
    expect(
      screen.getByText("John was professional and quick")
    ).toBeInTheDocument();
  });

  it("should display phone number as clickable link", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    const phoneLink = screen.getByRole("link", { name: /Call/ });
    expect(phoneLink).toBeInTheDocument();
    expect(phoneLink).toHaveAttribute("href", "tel:5551234567");
  });

  it("should close modal when close button is clicked", async () => {
    const onClose = vi.fn();
    render(<ContractorProfileModal {...defaultProps} onClose={onClose} />);

    const closeButton = screen.getByLabelText("Close contractor profile modal");
    await userEvent.click(closeButton);

    expect(onClose).toHaveBeenCalledOnce();
  });

  it("should close modal when Escape key is pressed", () => {
    const onClose = vi.fn();
    render(<ContractorProfileModal {...defaultProps} onClose={onClose} />);

    fireEvent.keyDown(document, { key: "Escape" });

    expect(onClose).toHaveBeenCalledOnce();
  });

  it("should close modal when backdrop is clicked", async () => {
    const onClose = vi.fn();
    render(<ContractorProfileModal {...defaultProps} onClose={onClose} />);

    const backdrop = screen.getByRole("presentation");
    fireEvent.mouseDown(backdrop);

    expect(onClose).toHaveBeenCalledOnce();
  });

  it("should not close modal when content is clicked", async () => {
    const onClose = vi.fn();
    render(<ContractorProfileModal {...defaultProps} onClose={onClose} />);

    const modalContent = screen.getByText("John Smith");
    fireEvent.mouseDown(modalContent);

    expect(onClose).not.toHaveBeenCalled();
  });

  it("should display loading spinner when loading is true", () => {
    render(
      <ContractorProfileModal
        {...defaultProps}
        loading={true}
        contractor={null}
      />
    );

    expect(screen.queryByText("John Smith")).not.toBeInTheDocument();
    // The loading spinner is rendered (we can check for the div or just verify the content changes)
    expect(screen.queryByText("Contact")).not.toBeInTheDocument();
  });

  it("should display empty state when contractor is null", () => {
    render(
      <ContractorProfileModal
        {...defaultProps}
        contractor={null}
        loading={false}
      />
    );

    expect(
      screen.getByText("Unable to load contractor profile. Please try again.")
    ).toBeInTheDocument();
  });

  it("should have proper ARIA attributes", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    const dialog = screen.getByRole("dialog");
    expect(dialog).toHaveAttribute("aria-modal", "true");
    expect(dialog).toHaveAttribute(
      "aria-labelledby",
      "contractor-profile-title"
    );
  });

  it("should have proper modal structure with backdrop", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    const presentation = screen.getByRole("presentation");
    expect(presentation).toHaveClass("fixed");
    expect(presentation).toHaveClass("bg-black/50");
  });

  it("should render contact section with phone number", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText("Contact")).toBeInTheDocument();
    expect(screen.getByText(/Call \(/)).toBeInTheDocument();
  });

  it("should render reviews section", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText("Customer Reviews (2)")).toBeInTheDocument();
  });

  it("should handle contractor without phone number gracefully", () => {
    const contractorNoPhone: Contractor & { phoneNumber?: string } = {
      ...mockContractor,
      phoneNumber: undefined, // Explicitly set to undefined
    };

    render(
      <ContractorProfileModal
        {...defaultProps}
        contractor={contractorNoPhone}
      />
    );

    // Contact section should not render when phoneNumber is undefined
    expect(screen.queryByText(/Call/)).not.toBeInTheDocument();
  });

  it("should handle contractor with no reviews", () => {
    render(<ContractorProfileModal {...defaultProps} reviews={[]} />);

    expect(
      screen.getByText("No reviews yet - be the first to rate this contractor")
    ).toBeInTheDocument();
  });

  it("should handle contractor with no rating", () => {
    const noRatingContractor = { ...mockContractor, rating: null };

    render(
      <ContractorProfileModal
        {...defaultProps}
        contractor={noRatingContractor}
      />
    );

    expect(screen.getByText("No ratings yet")).toBeInTheDocument();
  });

  it("should prevent body scroll when modal is open", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(document.body.style.overflow).toBe("hidden");
  });

  it("should restore body scroll when modal is closed", () => {
    const { unmount } = render(<ContractorProfileModal {...defaultProps} />);

    unmount();

    expect(document.body.style.overflow).toBe("unset");
  });

  it("should close button have focus immediately on modal open", async () => {
    render(<ContractorProfileModal {...defaultProps} />);

    await waitFor(() => {
      const closeButton = screen.getByLabelText(
        "Close contractor profile modal"
      );
      expect(closeButton).toHaveFocus();
    });
  });

  it("should display formatted phone number", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText(/\(555\) 123-4567/)).toBeInTheDocument();
  });

  it("should display review count", () => {
    render(<ContractorProfileModal {...defaultProps} />);

    expect(screen.getByText("Excellent")).toBeInTheDocument();
    expect(screen.getByText(/Based on 27 reviews/)).toBeInTheDocument();
  });
});
