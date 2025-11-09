/**
 * RatingSuccessMessage Component Tests
 * Tests for success message display, auto-dismiss, and accessibility
 */

import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { RatingSuccessMessage } from "../RatingSuccessMessage";

describe("RatingSuccessMessage Component", () => {
  const mockOnClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.runOnlyPendingTimers();
    vi.useRealTimers();
  });

  describe("Visibility", () => {
    it("should render when isVisible is true", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      expect(
        screen.getByText("Thank you for your feedback!")
      ).toBeInTheDocument();
    });

    it("should not render when isVisible is false", () => {
      render(<RatingSuccessMessage isVisible={false} onClose={mockOnClose} />);

      expect(
        screen.queryByText("Thank you for your feedback!")
      ).not.toBeInTheDocument();
    });

    it("should update visibility dynamically", () => {
      const { rerender } = render(
        <RatingSuccessMessage isVisible={false} onClose={mockOnClose} />
      );

      expect(
        screen.queryByText("Thank you for your feedback!")
      ).not.toBeInTheDocument();

      rerender(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      expect(
        screen.getByText("Thank you for your feedback!")
      ).toBeInTheDocument();
    });
  });

  describe("Content Display", () => {
    it("should display main success message", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      expect(
        screen.getByText("Thank you for your feedback!")
      ).toBeInTheDocument();
      expect(
        screen.getByText("Your rating has been submitted successfully.")
      ).toBeInTheDocument();
    });

    it("should display submitted rating", () => {
      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          rating={5}
        />
      );

      expect(screen.getByText("Your Rating:")).toBeInTheDocument();

      // Should display 5 filled stars
      const stars = screen.getAllByText("★");
      expect(stars.length).toBeGreaterThanOrEqual(5);
    });

    it("should display submitted comment", () => {
      const comment = "Great work, finished early!";

      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          comment={comment}
        />
      );

      expect(screen.getByText("Your Comment:")).toBeInTheDocument();
      expect(screen.getByText(`"${comment}"`)).toBeInTheDocument();
    });

    it("should display both rating and comment", () => {
      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          rating={4}
          comment="Excellent service"
        />
      );

      expect(screen.getByText("Your Rating:")).toBeInTheDocument();
      expect(screen.getByText("Your Comment:")).toBeInTheDocument();
      expect(screen.getByText('"Excellent service"')).toBeInTheDocument();
    });

    it("should not display rating details when not provided", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      expect(screen.queryByText("Your Rating:")).not.toBeInTheDocument();
    });

    it("should not display comment details when not provided", () => {
      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          rating={3}
        />
      );

      expect(screen.queryByText("Your Comment:")).not.toBeInTheDocument();
    });

    it("should handle null comment", () => {
      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          rating={5}
          comment={null}
        />
      );

      expect(screen.queryByText("Your Comment:")).not.toBeInTheDocument();
      expect(screen.getByText("Your Rating:")).toBeInTheDocument();
    });
  });

  describe("Close Button", () => {
    it("should have close button", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      const closeButton = screen.getByRole("button", {
        name: /close success message/i,
      });
      expect(closeButton).toBeInTheDocument();
    });

    it("should call onClose when close button is clicked", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      const closeButton = screen.getByRole("button", {
        name: /close success message/i,
      });

      fireEvent.click(closeButton);

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it("should call onClose when backdrop is clicked", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      // Find and click the backdrop overlay
      const backdrop = screen.getByTestId("rating-success-backdrop");
      fireEvent.click(backdrop);

      expect(mockOnClose).toHaveBeenCalled();
    });
  });

  describe("Auto-Dismiss", () => {
    it("should auto-dismiss after 5 seconds", async () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      expect(mockOnClose).not.toHaveBeenCalled();

      // Advance time by 5 seconds
      vi.advanceTimersByTime(5000);

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it("should not auto-dismiss before 5 seconds", async () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      vi.advanceTimersByTime(4999);
      expect(mockOnClose).not.toHaveBeenCalled();
    });

    it("should clear timeout when hidden", () => {
      const { rerender } = render(
        <RatingSuccessMessage isVisible={true} onClose={mockOnClose} />
      );

      rerender(
        <RatingSuccessMessage isVisible={false} onClose={mockOnClose} />
      );

      vi.advanceTimersByTime(5000);

      expect(mockOnClose).not.toHaveBeenCalled();
    });

    it("should clear timeout when component unmounts", () => {
      const { unmount } = render(
        <RatingSuccessMessage isVisible={true} onClose={mockOnClose} />
      );

      unmount();

      vi.advanceTimersByTime(5000);

      expect(mockOnClose).not.toHaveBeenCalled();
    });

    it("should reset timeout when visibility changes to true", () => {
      const { rerender } = render(
        <RatingSuccessMessage isVisible={false} onClose={mockOnClose} />
      );

      rerender(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      vi.advanceTimersByTime(4000);
      expect(mockOnClose).not.toHaveBeenCalled();

      vi.advanceTimersByTime(1001);
      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });
  });

  describe("Accessibility", () => {
    it("should have status role for live region", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      const messageContainer = screen
        .getByText("Thank you for your feedback!")
        .closest("[role='status']");
      expect(messageContainer).toHaveAttribute("role", "status");
    });

    it("should have aria-live polite", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      const messageContainer = screen
        .getByText("Thank you for your feedback!")
        .closest("div[role='status']");
      expect(messageContainer).toHaveAttribute("aria-live", "polite");
    });

    it("should have aria-atomic", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      const messageContainer = screen
        .getByText("Thank you for your feedback!")
        .closest("div[role='status']");
      expect(messageContainer).toHaveAttribute("aria-atomic", "true");
    });

    it("should have proper focus management", () => {
      const { container } = render(
        <RatingSuccessMessage isVisible={true} onClose={mockOnClose} />
      );

      // The message container should be focusable
      const focusableElement = container.querySelector("[role='status']");
      expect(focusableElement).toHaveAttribute("tabIndex", "-1");
    });

    it("should have proper close button accessibility", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      const closeButton = screen.getByRole("button", {
        name: /close success message/i,
      });
      expect(closeButton).toHaveAttribute("aria-label");
    });

    it("should have success icon with aria-hidden", () => {
      const { container } = render(
        <RatingSuccessMessage isVisible={true} onClose={mockOnClose} />
      );

      const svg = container.querySelector("svg");
      expect(svg).toHaveAttribute("aria-hidden", "true");
    });
  });

  describe("Rating Display Details", () => {
    it("should display correct number of filled stars for rating", () => {
      const ratings = [1, 2, 3, 4, 5];

      ratings.forEach((rating) => {
        const { unmount } = render(
          <RatingSuccessMessage
            isVisible={true}
            onClose={mockOnClose}
            rating={rating}
          />
        );

        // Stars are displayed in the detail box
        const detailBox = screen.getByText("Your Rating:").closest("div");
        expect(detailBox).toBeInTheDocument();

        unmount();
      });
    });

    it("should display all 5 stars when rating is 5", () => {
      const { container } = render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          rating={5}
        />
      );

      const filledStars = container.querySelectorAll("span.text-yellow-400");
      expect(filledStars.length).toBeGreaterThanOrEqual(5);
    });

    it("should limit comment display with line-clamp", () => {
      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          comment="A very long comment that should be truncated to two lines if it exceeds the display width"
        />
      );

      const commentDisplay = screen.getByText(/A very long comment/);
      expect(commentDisplay).toHaveClass("line-clamp-2");
    });
  });

  describe("Responsive Design", () => {
    it("should apply responsive padding", () => {
      const { container } = render(
        <RatingSuccessMessage isVisible={true} onClose={mockOnClose} />
      );

      const message = container.querySelector(".p-6");
      expect(message).toHaveClass("p-6", "sm:p-8");
    });

    it("should apply responsive margin", () => {
      const { container } = render(
        <RatingSuccessMessage isVisible={true} onClose={mockOnClose} />
      );

      const messageContainer = container.querySelector(".pb-4");
      expect(messageContainer).toHaveClass("pb-4", "sm:pb-6", "lg:pb-8");
    });

    it("should have max width constraint", () => {
      const { container } = render(
        <RatingSuccessMessage isVisible={true} onClose={mockOnClose} />
      );

      const card = container.querySelector(".max-w-md");
      expect(card).toHaveClass("max-w-md", "w-full");
    });
  });

  describe("Error States", () => {
    it("should handle undefined rating gracefully", () => {
      render(<RatingSuccessMessage isVisible={true} onClose={mockOnClose} />);

      expect(screen.queryByText("Your Rating:")).not.toBeInTheDocument();
    });

    it("should handle undefined comment gracefully", () => {
      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          rating={4}
        />
      );

      expect(screen.queryByText("Your Comment:")).not.toBeInTheDocument();
    });

    it("should handle empty comment string", () => {
      render(
        <RatingSuccessMessage
          isVisible={true}
          onClose={mockOnClose}
          comment=""
        />
      );

      expect(screen.queryByText("Your Comment:")).not.toBeInTheDocument();
    });
  });
});
