/**
 * ContractorListItem Component Tests
 */

import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi, describe, it, expect, beforeEach } from "vitest";
import { ContractorListItem } from "../ContractorListItem";
import { Contractor } from "@/types/Contractor";

describe("ContractorListItem Component", () => {
  const mockContractor: Contractor = {
    id: "1",
    name: "John's Plumbing",
    rating: 4.5,
    reviewCount: 20,
    location: "123 Main St",
    tradeType: "Plumbing",
    isActive: true,
    inDispatcherList: false,
  };

  const mockContractorWithoutRating: Contractor = {
    ...mockContractor,
    rating: null,
  };

  const mockInactiveContractor: Contractor = {
    ...mockContractor,
    isActive: false,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("My List Mode (Remove)", () => {
    it("should render contractor info in my-list mode", () => {
      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(screen.getByText("John's Plumbing")).toBeInTheDocument();
      expect(screen.getByText("Plumbing • 123 Main St")).toBeInTheDocument();
      expect(screen.getByText("4.5")).toBeInTheDocument();
      expect(screen.getByText("(20 reviews)")).toBeInTheDocument();
    });

    it("should display Remove button in my-list mode", () => {
      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(
        screen.getByRole("button", { name: /Remove/i })
      ).toBeInTheDocument();
    });

    it("should show confirmation buttons when Remove is clicked", async () => {
      const user = userEvent.setup();
      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      const removeButton = screen.getByRole("button", { name: /Remove/i });
      await user.click(removeButton);

      expect(
        screen.getByRole("button", { name: /Confirm/i })
      ).toBeInTheDocument();
      expect(
        screen.getByRole("button", { name: /Cancel/i })
      ).toBeInTheDocument();
    });

    it("should call onRemove when Confirm is clicked", async () => {
      const user = userEvent.setup();
      const onRemove = vi.fn().mockResolvedValue(true);

      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="my-list"
          onRemove={onRemove}
        />
      );

      const removeButton = screen.getByRole("button", { name: /Remove/i });
      await user.click(removeButton);

      const confirmButton = screen.getByRole("button", { name: /Confirm/i });
      await user.click(confirmButton);

      await waitFor(() => {
        expect(onRemove).toHaveBeenCalledWith("1");
      });
    });

    it("should hide confirmation buttons when Cancel is clicked", async () => {
      const user = userEvent.setup();
      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      const removeButton = screen.getByRole("button", { name: /Remove/i });
      await user.click(removeButton);

      const cancelButton = screen.getByRole("button", { name: /Cancel/i });
      await user.click(cancelButton);

      expect(
        screen.queryByRole("button", { name: /Confirm/i })
      ).not.toBeInTheDocument();
      expect(
        screen.getByRole("button", { name: /Remove/i })
      ).toBeInTheDocument();
    });

    it("should display Active badge when contractor is active", () => {
      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(screen.getByText("Active")).toBeInTheDocument();
    });

    it("should display Inactive badge when contractor is inactive", () => {
      render(
        <ContractorListItem
          contractor={mockInactiveContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(screen.getByText("Inactive")).toBeInTheDocument();
    });

    it("should handle null rating correctly", () => {
      const { container } = render(
        <ContractorListItem
          contractor={mockContractorWithoutRating}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(screen.getByText("—")).toBeInTheDocument();
      expect(container.textContent).toContain("0 reviews");
    });
  });

  describe("Available Mode (Add)", () => {
    it("should display Add button in available mode for active contractors", () => {
      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="available"
          onAdd={vi.fn()}
        />
      );

      expect(screen.getByRole("button", { name: /Add/i })).toBeInTheDocument();
    });

    it("should call onAdd when Add button is clicked", async () => {
      const user = userEvent.setup();
      const onAdd = vi.fn().mockResolvedValue(true);

      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="available"
          onAdd={onAdd}
        />
      );

      const addButton = screen.getByRole("button", { name: /Add/i });
      await user.click(addButton);

      await waitFor(() => {
        expect(onAdd).toHaveBeenCalledWith("1");
      });
    });

    it("should display Added badge when contractor is already in list", () => {
      const addedContractor = { ...mockContractor, inDispatcherList: true };

      render(
        <ContractorListItem
          contractor={addedContractor}
          mode="available"
          onAdd={vi.fn()}
        />
      );

      expect(screen.getByText("✓ Added")).toBeInTheDocument();
    });

    it("should disable Add button for inactive contractors", () => {
      render(
        <ContractorListItem
          contractor={mockInactiveContractor}
          mode="available"
          onAdd={vi.fn()}
        />
      );

      const addButton = screen.getByRole("button", { name: /Add/i });
      expect(addButton).toBeDisabled();
    });

    it("should show loading spinner when isLoading is true", () => {
      const { container } = render(
        <ContractorListItem
          contractor={mockContractor}
          mode="available"
          onAdd={vi.fn()}
          isLoading={true}
        />
      );

      const spinner = container.querySelector("svg");
      expect(spinner).toBeInTheDocument();
    });
  });

  describe("Edge Cases", () => {
    it("should display contractor with all fields populated", () => {
      render(
        <ContractorListItem
          contractor={mockContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(screen.getByText("John's Plumbing")).toBeInTheDocument();
      expect(screen.getByText("Plumbing • 123 Main St")).toBeInTheDocument();
      expect(screen.getByText("4.5")).toBeInTheDocument();
      expect(screen.getByText("(20 reviews)")).toBeInTheDocument();
    });

    it("should handle high rating values correctly", () => {
      const highRatedContractor = { ...mockContractor, rating: 5.0 };

      render(
        <ContractorListItem
          contractor={highRatedContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(screen.getByText("5.0")).toBeInTheDocument();
    });

    it("should handle large review counts", () => {
      const highReviewContractor = { ...mockContractor, reviewCount: 999 };

      render(
        <ContractorListItem
          contractor={highReviewContractor}
          mode="my-list"
          onRemove={vi.fn()}
        />
      );

      expect(screen.getByText("(999 reviews)")).toBeInTheDocument();
    });
  });
});
