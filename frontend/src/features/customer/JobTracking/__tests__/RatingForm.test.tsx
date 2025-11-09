/**
 * RatingForm Component Tests
 * Tests for form rendering, submission, validation, and integration
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { RatingForm } from "../RatingForm";
import { customerService } from "@/services/customerService";
import { Review } from "@/types/Customer";
import * as useRatingModule from "@/hooks/useRating";

// Mock dependencies
vi.mock("@/services/customerService");
vi.mock("@/components/shared/LoadingSpinner", () => ({
  LoadingSpinner: ({ size }: { size?: string }) => (
    <div data-testid="loading-spinner" data-size={size}>
      Loading...
    </div>
  ),
}));

describe("RatingForm Component", () => {
  const mockOnRatingSubmitted = vi.fn();
  const defaultProps = {
    jobId: "job-123",
    contractorId: "contractor-456",
    jobStatus: "Completed" as const,
    onRatingSubmitted: mockOnRatingSubmitted,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render when jobStatus is Completed", () => {
      render(<RatingForm {...defaultProps} />);

      expect(screen.getByText("Rate this job")).toBeInTheDocument();
    });

    it("should not render when jobStatus is not Completed", () => {
      render(<RatingForm {...defaultProps} jobStatus="Pending" />);

      expect(screen.queryByText("Rate this job")).not.toBeInTheDocument();
    });

    it("should not render for other job statuses", () => {
      const statuses = ["Pending", "Assigned", "InProgress", "Cancelled"];

      statuses.forEach((status) => {
        const { unmount } = render(
          <RatingForm {...defaultProps} jobStatus={status as any} />
        );

        expect(screen.queryByText("Rate this job")).not.toBeInTheDocument();
        unmount();
      });
    });

    it("should render form elements", () => {
      render(<RatingForm {...defaultProps} />);

      // Section header
      expect(screen.getByText("Rate this job")).toBeInTheDocument();

      // Rating input label
      expect(screen.getByText(/Your rating/i)).toBeInTheDocument();

      // Comment textarea
      expect(
        screen.getByPlaceholderText(/Share any additional thoughts/i)
      ).toBeInTheDocument();

      // Submit button
      expect(
        screen.getByRole("button", { name: /Submit Rating/i })
      ).toBeInTheDocument();
    });

    it("should display contractor context in description", () => {
      render(<RatingForm {...defaultProps} />);

      expect(
        screen.getByText(/informs other customers about contractor-456/)
      ).toBeInTheDocument();
    });
  });

  describe("Star Rating Input", () => {
    it("should render star rating component", () => {
      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      expect(starButtons).toHaveLength(5);
    });

    it("should initialize with no rating selected", () => {
      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      starButtons.forEach((star) => {
        expect(star).toHaveAttribute("aria-checked", "false");
      });
    });

    it("should allow selecting a rating", async () => {
      const user = userEvent.setup();
      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[3]); // Click 4th star

      // The rating should be reflected
      expect(screen.getByText("Very Good")).toBeInTheDocument();
    });
  });

  describe("Comment Textarea", () => {
    it("should render comment textarea", () => {
      render(<RatingForm {...defaultProps} />);

      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      expect(textarea).toBeInTheDocument();
    });

    it("should allow typing in comment", async () => {
      const user = userEvent.setup();
      render(<RatingForm {...defaultProps} />);

      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      await user.type(textarea, "Great service!");

      expect(textarea).toHaveValue("Great service!");
    });

    it("should display character count", async () => {
      const user = userEvent.setup();
      render(<RatingForm {...defaultProps} />);

      expect(screen.getByText("0/500")).toBeInTheDocument();

      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      await user.type(textarea, "Good job");

      expect(screen.getByText("8/500")).toBeInTheDocument();
    });

    it("should enforce max length of 500 characters", () => {
      render(<RatingForm {...defaultProps} />);

      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      expect(textarea).toHaveAttribute("maxLength", "500");
    });
  });

  describe("Submit Button", () => {
    it("should render submit button", () => {
      render(<RatingForm {...defaultProps} />);

      expect(
        screen.getByRole("button", { name: /Submit Rating/i })
      ).toBeInTheDocument();
    });

    it("should be disabled when no rating is selected", () => {
      render(<RatingForm {...defaultProps} />);

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      expect(submitButton).toBeDisabled();
    });

    it("should show warning text when no rating selected", () => {
      render(<RatingForm {...defaultProps} />);

      expect(
        screen.getByText(/Please select a rating to continue/)
      ).toBeInTheDocument();
    });

    it("should be enabled after selecting a rating", async () => {
      const user = userEvent.setup();
      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[2]); // Select 3 stars

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      expect(submitButton).not.toBeDisabled();
    });

    it("should show loading state during submission", async () => {
      const user = userEvent.setup();

      const mockReview: Review = {
        id: "review-123",
        jobId: "job-123",
        contractorId: "contractor-456",
        customerId: "customer-123",
        rating: 5,
        comment: null,
        createdAt: "2025-11-15T14:30:00Z",
      };

      vi.mocked(customerService.submitRating).mockImplementation(
        () =>
          new Promise((resolve) => setTimeout(() => resolve(mockReview), 1000))
      );

      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[4]); // Select 5 stars

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      // Should show loading spinner
      expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();
      expect(screen.getByText("Submitting...")).toBeInTheDocument();

      // Should be disabled during submission
      expect(submitButton).toBeDisabled();
    });
  });

  describe("Form Submission", () => {
    it("should call submitRating on form submit", async () => {
      const user = userEvent.setup();

      const mockReview: Review = {
        id: "review-123",
        jobId: "job-123",
        contractorId: "contractor-456",
        customerId: "customer-123",
        rating: 4,
        comment: "Good work",
        createdAt: "2025-11-15T14:30:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      render(<RatingForm {...defaultProps} />);

      // Select rating
      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[3]); // 4 stars

      // Enter comment
      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      await user.type(textarea, "Good work");

      // Submit
      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      // Should call service
      await waitFor(() => {
        expect(customerService.submitRating).toHaveBeenCalledWith("job-123", {
          rating: 4,
          comment: "Good work",
        });
      });
    });

    it("should submit with empty comment", async () => {
      const user = userEvent.setup();

      const mockReview: Review = {
        id: "review-124",
        jobId: "job-123",
        contractorId: "contractor-456",
        customerId: "customer-123",
        rating: 5,
        comment: null,
        createdAt: "2025-11-15T14:35:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      render(<RatingForm {...defaultProps} />);

      // Select rating only, no comment
      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[4]); // 5 stars

      // Submit
      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      // Should call service with null comment
      await waitFor(() => {
        expect(customerService.submitRating).toHaveBeenCalledWith("job-123", {
          rating: 5,
          comment: null,
        });
      });
    });

    it("should call onRatingSubmitted callback on success", async () => {
      const user = userEvent.setup();

      const mockReview: Review = {
        id: "review-125",
        jobId: "job-123",
        contractorId: "contractor-456",
        customerId: "customer-123",
        rating: 3,
        comment: null,
        createdAt: "2025-11-15T14:40:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[2]); // 3 stars

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockOnRatingSubmitted).toHaveBeenCalled();
      });
    });

    it("should prevent submission with invalid rating", async () => {
      const user = userEvent.setup();
      render(<RatingForm {...defaultProps} />);

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });

      // Try to click submit without selecting a rating
      await user.click(submitButton);

      // Should not call service
      expect(customerService.submitRating).not.toHaveBeenCalled();
    });
  });

  describe("Error Handling", () => {
    it("should display error message on submission failure", async () => {
      const user = userEvent.setup();

      const error = new Error("You have already rated this job.");
      vi.mocked(customerService.submitRating).mockRejectedValue(error);

      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[2]); // 3 stars

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      // Should display error message
      await waitFor(() => {
        expect(
          screen.getByText("You have already rated this job.")
        ).toBeInTheDocument();
      });

      // Should not call callback
      expect(mockOnRatingSubmitted).not.toHaveBeenCalled();
    });

    it("should display user-friendly error for 404", async () => {
      const user = userEvent.setup();

      const error = new Error("Job not found or not in completed status.");
      vi.mocked(customerService.submitRating).mockRejectedValue(error);

      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[0]); // 1 star

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      await waitFor(() => {
        expect(
          screen.getByText(/Job not found or not in completed status/)
        ).toBeInTheDocument();
      });
    });

    it("should display error in alert role", async () => {
      const user = userEvent.setup();

      const error = new Error("Server error.");
      vi.mocked(customerService.submitRating).mockRejectedValue(error);

      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[4]); // 5 stars

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByRole("alert")).toBeInTheDocument();
      });
    });

    it("should clear error when new rating is selected", async () => {
      const user = userEvent.setup();

      const error = new Error("Test error");
      vi.mocked(customerService.submitRating)
        .mockRejectedValueOnce(error)
        .mockResolvedValueOnce({
          id: "review-126",
          jobId: "job-123",
          contractorId: "contractor-456",
          customerId: "customer-123",
          rating: 4,
          comment: null,
          createdAt: "2025-11-15T14:45:00Z",
        });

      render(<RatingForm {...defaultProps} />);

      // First submission fails
      let starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[0]); // 1 star

      let submitButton = screen.getByRole("button", { name: /Submit Rating/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText("Test error")).toBeInTheDocument();
      });

      // Select new rating
      starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[3]); // 4 stars

      // Error should be cleared
      await waitFor(() => {
        expect(screen.queryByText("Test error")).not.toBeInTheDocument();
      });
    });
  });

  describe("Success State", () => {
    it("should display success message after submission", async () => {
      const user = userEvent.setup();

      const mockReview: Review = {
        id: "review-127",
        jobId: "job-123",
        contractorId: "contractor-456",
        customerId: "customer-123",
        rating: 5,
        comment: "Excellent!",
        createdAt: "2025-11-15T14:50:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[4]); // 5 stars

      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      await user.type(textarea, "Excellent!");

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      await waitFor(() => {
        expect(
          screen.getByText(/Thank you for your rating/)
        ).toBeInTheDocument();
      });
    });

    it("should show option to submit another rating", async () => {
      const user = userEvent.setup();

      const mockReview: Review = {
        id: "review-128",
        jobId: "job-123",
        contractorId: "contractor-456",
        customerId: "customer-123",
        rating: 4,
        comment: null,
        createdAt: "2025-11-15T14:55:00Z",
      };

      vi.mocked(customerService.submitRating).mockResolvedValue(mockReview);

      render(<RatingForm {...defaultProps} />);

      const starButtons = screen.getAllByRole("radio");
      await user.click(starButtons[3]); // 4 stars

      const submitButton = screen.getByRole("button", {
        name: /Submit Rating/i,
      });
      await user.click(submitButton);

      await waitFor(() => {
        expect(
          screen.getByRole("button", { name: /Submit another rating/i })
        ).toBeInTheDocument();
      });
    });
  });

  describe("Accessibility", () => {
    it("should have proper form structure", () => {
      render(<RatingForm {...defaultProps} />);

      const form = screen
        .getByRole("button", { name: /Submit Rating/i })
        .closest("form");
      expect(form).toBeInTheDocument();
    });

    it("should have required indicator on rating field", () => {
      render(<RatingForm {...defaultProps} />);

      expect(
        screen.getByText(/Your rating/).parentElement?.textContent
      ).toContain("*");
    });

    it("should have proper textarea attributes", () => {
      render(<RatingForm {...defaultProps} />);

      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      expect(textarea).toHaveAttribute("maxLength", "500");
      expect(textarea).toHaveAttribute("rows", "4");
    });

    it("should have aria-label on textarea", () => {
      render(<RatingForm {...defaultProps} />);

      const textarea = screen.getByPlaceholderText(
        /Share any additional thoughts/i
      );
      expect(textarea).toHaveAttribute("aria-label");
    });
  });
});
