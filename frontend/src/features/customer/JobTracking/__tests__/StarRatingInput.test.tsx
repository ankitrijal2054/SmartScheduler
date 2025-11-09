/**
 * StarRatingInput Component Tests
 * Tests for star selection, keyboard navigation, accessibility, and hover behavior
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StarRatingInput } from "../StarRatingInput";

describe("StarRatingInput Component", () => {
  const mockOnRatingChange = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render 5 star buttons", () => {
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      expect(stars).toHaveLength(5);
    });

    it("should render all stars as empty (gray) initially", () => {
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars.forEach((star) => {
        const starIcon = star.querySelector("span");
        expect(starIcon).toHaveClass("text-gray-300");
      });
    });

    it("should render filled stars for rating", () => {
      render(
        <StarRatingInput rating={3} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      // First 3 should be filled
      for (let i = 0; i < 3; i++) {
        const starIcon = stars[i].querySelector("span");
        expect(starIcon).toHaveClass("text-yellow-400");
      }

      // Last 2 should be empty
      for (let i = 3; i < 5; i++) {
        const starIcon = stars[i].querySelector("span");
        expect(starIcon).toHaveClass("text-gray-300");
      }
    });

    it("should render rating label when rating is selected", () => {
      render(
        <StarRatingInput rating={4} onRatingChange={mockOnRatingChange} />
      );

      expect(screen.getByText("Very Good")).toBeInTheDocument();
    });

    it("should display correct labels for each rating", () => {
      const ratings = [
        { rating: 1, label: "Poor" },
        { rating: 2, label: "Fair" },
        { rating: 3, label: "Good" },
        { rating: 4, label: "Very Good" },
        { rating: 5, label: "Excellent" },
      ];

      ratings.forEach(({ rating, label }) => {
        const { unmount } = render(
          <StarRatingInput rating={rating} onRatingChange={mockOnRatingChange} />
        );

        expect(screen.getByText(label)).toBeInTheDocument();
        unmount();
      });
    });

    it("should render clear button when rating > 0", () => {
      const { rerender } = render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      expect(screen.queryByRole("button", { name: /clear/i })).not.toBeInTheDocument();

      rerender(
        <StarRatingInput rating={3} onRatingChange={mockOnRatingChange} />
      );

      expect(screen.getByRole("button", { name: /clear/i })).toBeInTheDocument();
    });

    it("should not render clear button when rating is 0", () => {
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      expect(screen.queryByRole("button", { name: /clear/i })).not.toBeInTheDocument();
    });

    it("should have correct accessibility group role", () => {
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const radioGroup = screen.getByRole("radiogroup");
      expect(radioGroup).toHaveAttribute("aria-label", "Job rating");
      expect(radioGroup).toHaveAttribute("aria-required", "true");
    });
  });

  describe("Click Interactions", () => {
    it("should call onRatingChange when star is clicked", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      await user.click(stars[2]); // Click 3rd star

      expect(mockOnRatingChange).toHaveBeenCalledWith(3);
      expect(mockOnRatingChange).toHaveBeenCalledTimes(1);
    });

    it("should call onRatingChange with correct value for each star", async () => {
      const user = userEvent.setup();
      const { rerender } = render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      for (let i = 0; i < 5; i++) {
        await user.click(stars[i]);
        expect(mockOnRatingChange).toHaveBeenCalledWith(i + 1);
      }

      expect(mockOnRatingChange).toHaveBeenCalledTimes(5);
    });

    it("should update rating when already selected and clicked again", async () => {
      const user = userEvent.setup();
      const { rerender } = render(
        <StarRatingInput rating={2} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      await user.click(stars[4]); // Click 5th star

      expect(mockOnRatingChange).toHaveBeenCalledWith(5);
    });

    it("should call onRatingChange(0) when clear button is clicked", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={4} onRatingChange={mockOnRatingChange} />
      );

      const clearButton = screen.getByRole("button", { name: /clear/i });
      await user.click(clearButton);

      expect(mockOnRatingChange).toHaveBeenCalledWith(0);
    });
  });

  describe("Hover Behavior", () => {
    it("should show rating preview on hover", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={2} onRatingChange={mockOnRatingChange} />
      );

      expect(screen.getByText("Fair")).toBeInTheDocument();

      const stars = screen.getAllByRole("radio");
      await user.hover(stars[4]); // Hover over 5th star

      // Should show "Excellent" preview
      expect(screen.getByText("Excellent")).toBeInTheDocument();
    });

    it("should restore original rating after hover ends", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={2} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      await user.hover(stars[4]);
      expect(screen.getByText("Excellent")).toBeInTheDocument();

      await user.unhover(stars[4]);
      expect(screen.getByText("Fair")).toBeInTheDocument();
    });

    it("should show filled stars on hover", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={2} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      // Initially only 2 stars are filled
      let filledStars = stars.filter((star) =>
        star.querySelector("span")?.classList.contains("text-yellow-400")
      );
      expect(filledStars).toHaveLength(2);

      // Hover over 4th star
      await user.hover(stars[3]);

      // Now 4 stars should appear filled (preview)
      filledStars = stars.filter((star) =>
        star.querySelector("span")?.classList.contains("text-yellow-400")
      );
      expect(filledStars).toHaveLength(4);
    });
  });

  describe("Keyboard Navigation", () => {
    it("should increase rating with ArrowRight key", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={2} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars[1].focus(); // Focus on 2nd star

      await user.keyboard("{ArrowRight}");

      expect(mockOnRatingChange).toHaveBeenCalledWith(3);
    });

    it("should decrease rating with ArrowLeft key", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={3} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars[2].focus(); // Focus on 3rd star

      await user.keyboard("{ArrowLeft}");

      expect(mockOnRatingChange).toHaveBeenCalledWith(2);
    });

    it("should not go below 1 with ArrowLeft", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={1} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars[0].focus(); // Focus on 1st star

      await user.keyboard("{ArrowLeft}");

      expect(mockOnRatingChange).not.toHaveBeenCalled();
    });

    it("should not go above 5 with ArrowRight", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={5} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars[4].focus(); // Focus on 5th star

      await user.keyboard("{ArrowRight}");

      expect(mockOnRatingChange).not.toHaveBeenCalled();
    });

    it("should clear rating with Delete key", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={3} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars[2].focus();

      await user.keyboard("{Delete}");

      expect(mockOnRatingChange).toHaveBeenCalledWith(0);
    });

    it("should clear rating with Backspace key", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={3} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars[2].focus();

      await user.keyboard("{Backspace}");

      expect(mockOnRatingChange).toHaveBeenCalledWith(0);
    });

    it("should have correct tabIndex for current rating", () => {
      render(
        <StarRatingInput rating={3} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      // 3rd star (index 2) should be focusable
      expect(stars[2]).toHaveAttribute("tabIndex", "0");

      // Others should not be focusable via Tab
      [0, 1, 3, 4].forEach((i) => {
        expect(stars[i]).toHaveAttribute("tabIndex", "-1");
      });
    });
  });

  describe("Disabled State", () => {
    it("should not respond to clicks when disabled", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput
          rating={0}
          onRatingChange={mockOnRatingChange}
          disabled={true}
        />
      );

      const stars = screen.getAllByRole("radio");
      await user.click(stars[2]);

      expect(mockOnRatingChange).not.toHaveBeenCalled();
    });

    it("should not respond to keyboard input when disabled", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput
          rating={2}
          onRatingChange={mockOnRatingChange}
          disabled={true}
        />
      );

      const stars = screen.getAllByRole("radio");
      stars[1].focus();

      await user.keyboard("{ArrowRight}");

      expect(mockOnRatingChange).not.toHaveBeenCalled();
    });

    it("should apply disabled styling", () => {
      render(
        <StarRatingInput
          rating={0}
          onRatingChange={mockOnRatingChange}
          disabled={true}
        />
      );

      const stars = screen.getAllByRole("radio");
      stars.forEach((star) => {
        expect(star).toHaveAttribute("disabled");
        expect(star).toHaveClass("cursor-not-allowed");
      });
    });

    it("should disable clear button when disabled", () => {
      render(
        <StarRatingInput
          rating={3}
          onRatingChange={mockOnRatingChange}
          disabled={true}
        />
      );

      const clearButton = screen.getByRole("button", { name: /clear/i });
      expect(clearButton).toHaveAttribute("disabled");
    });
  });

  describe("ARIA Attributes", () => {
    it("should have correct aria-label for each star", () => {
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      stars.forEach((star, index) => {
        expect(star).toHaveAttribute("aria-label", `Rate ${index + 1} stars`);
      });
    });

    it("should have aria-checked attribute", () => {
      render(
        <StarRatingInput rating={2} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      stars.forEach((star, index) => {
        if (index + 1 === 2) {
          expect(star).toHaveAttribute("aria-checked", "true");
        } else {
          expect(star).toHaveAttribute("aria-checked", "false");
        }
      });
    });

    it("should have required attribute", () => {
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const radioGroup = screen.getByRole("radiogroup");
      expect(radioGroup).toHaveAttribute("aria-required", "true");
    });
  });

  describe("Edge Cases", () => {
    it("should handle rapid clicks", async () => {
      const user = userEvent.setup();
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");

      await user.click(stars[0]);
      await user.click(stars[2]);
      await user.click(stars[4]);

      expect(mockOnRatingChange).toHaveBeenCalledTimes(3);
      expect(mockOnRatingChange).toHaveBeenNthCalledWith(1, 1);
      expect(mockOnRatingChange).toHaveBeenNthCalledWith(2, 3);
      expect(mockOnRatingChange).toHaveBeenNthCalledWith(3, 5);
    });

    it("should handle rating of 0 (unselected)", () => {
      render(
        <StarRatingInput rating={0} onRatingChange={mockOnRatingChange} />
      );

      const stars = screen.getAllByRole("radio");
      stars.forEach((star) => {
        expect(star).toHaveAttribute("aria-checked", "false");
      });

      expect(screen.queryByText(/Poor|Fair|Good|Very Good|Excellent/)).not.toBeInTheDocument();
    });

    it("should handle all valid rating values 1-5", () => {
      for (let i = 1; i <= 5; i++) {
        const { unmount } = render(
          <StarRatingInput
            rating={i}
            onRatingChange={mockOnRatingChange}
          />
        );

        const stars = screen.getAllByRole("radio");
        const filledStars = stars.filter((star) =>
          star.querySelector("span")?.classList.contains("text-yellow-400")
        );

        expect(filledStars).toHaveLength(i);
        unmount();
      }
    });
  });
});


