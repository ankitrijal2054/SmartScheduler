/**
 * RecommendationsModal Component Tests
 * Unit tests using Vitest + React Testing Library
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  render,
  screen,
  fireEvent,
  waitFor,
  within,
} from "@testing-library/react";
import { RecommendationsModal } from "../RecommendationsModal";
import * as dispatcherServiceModule from "@/services/dispatcherService";
import { RecommendedContractor } from "@/types/Contractor";

// Mock the dispatcher service
vi.mock("@/services/dispatcherService");

const mockRecommendations: RecommendedContractor[] = [
  {
    contractorId: "cont_001",
    name: "Bob's Plumbing Pro",
    rank: 1,
    score: 0.92,
    avgRating: 4.8,
    reviewCount: 42,
    distance: 2.5,
    travelTime: 12,
    tradeType: "Plumbing",
    availableTimeSlots: [
      {
        startTime: "2025-11-10T14:00:00Z",
        endTime: "2025-11-10T16:00:00Z",
      },
    ],
  },
  {
    contractorId: "cont_002",
    name: "Jane's Plumbing Services",
    rank: 2,
    score: 0.87,
    avgRating: 4.6,
    reviewCount: 28,
    distance: 4.1,
    travelTime: 18,
    tradeType: "Plumbing",
    availableTimeSlots: [
      {
        startTime: "2025-11-10T14:30:00Z",
        endTime: "2025-11-10T16:30:00Z",
      },
    ],
  },
  {
    contractorId: "cont_003",
    name: "Quick Plumbing Team",
    rank: 3,
    score: 0.79,
    avgRating: 4.4,
    reviewCount: 15,
    distance: 6.2,
    travelTime: 22,
    tradeType: "Plumbing",
    availableTimeSlots: [
      {
        startTime: "2025-11-10T15:00:00Z",
        endTime: "2025-11-10T17:00:00Z",
      },
    ],
  },
];

const defaultProps = {
  isOpen: true,
  jobId: "job_123",
  jobType: "Plumbing",
  location: "123 Main St, Springfield, IL",
  desiredDateTime: "2025-11-10T14:00:00Z",
  contractorListOnly: false,
  onClose: vi.fn(),
};

describe("RecommendationsModal", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should not render when isOpen is false", () => {
    render(<RecommendationsModal {...defaultProps} isOpen={false} />);
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("should render modal when isOpen is true", () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: [],
      metadata: { totalAvailable: 0, requestTime: "2025-11-08T11:30:00Z" },
    });

    render(<RecommendationsModal {...defaultProps} />);
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(screen.getByText("Contractor Recommendations")).toBeInTheDocument();
  });

  it("should display loading spinner while fetching recommendations", async () => {
    let resolveRecommendations: ((value: any) => void) | null = null;
    const mockGetRecommendations = vi.fn(
      () =>
        new Promise((resolve) => {
          resolveRecommendations = resolve;
        })
    );
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockImplementation(mockGetRecommendations);

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(
      () => {
        expect(
          screen.getByText(/Fetching contractor recommendations/i)
        ).toBeInTheDocument();
      },
      { timeout: 500 }
    );

    // Resolve the promise to clean up
    if (resolveRecommendations) {
      resolveRecommendations({
        data: mockRecommendations,
        metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
      });
    }
  });

  it("should call dispatcherService.getRecommendations with correct parameters", async () => {
    const mockGetRecommendations = vi.fn().mockResolvedValue({
      data: mockRecommendations,
      metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
    });
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockImplementation(mockGetRecommendations);

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(() => {
      expect(mockGetRecommendations).toHaveBeenCalledWith(
        expect.objectContaining({
          jobId: "job_123",
          jobType: "Plumbing",
          location: "123 Main St, Springfield, IL",
          desiredDateTime: "2025-11-10T14:00:00Z",
          contractor_list_only: false,
        }),
        expect.anything()
      );
    });
  });

  it("should display all recommendations after loading", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: mockRecommendations,
      metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
    });

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText("Bob's Plumbing Pro")).toBeInTheDocument();
      expect(screen.getByText("Jane's Plumbing Services")).toBeInTheDocument();
      expect(screen.getByText("Quick Plumbing Team")).toBeInTheDocument();
    });
  });

  it("should display sorting dropdown when recommendations are loaded", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: mockRecommendations,
      metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
    });

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(() => {
      const sortSelect = screen.getByRole("combobox", {
        name: /Sort recommendations/i,
      });
      expect(sortSelect).toBeInTheDocument();
      expect(sortSelect).toHaveValue("rank");
    });
  });

  it("should handle sorting by rating", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: mockRecommendations,
      metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
    });

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText("Bob's Plumbing Pro")).toBeInTheDocument();
    });

    const sortSelect = screen.getByRole("combobox", {
      name: /Sort recommendations/i,
    });
    fireEvent.change(sortSelect, { target: { value: "rating" } });

    // After sorting by rating (highest first), Bob (4.8) should still be first
    expect(sortSelect).toHaveValue("rating");
  });

  it("should display empty state when no recommendations are returned", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: [],
      metadata: { totalAvailable: 0, requestTime: "2025-11-08T11:30:00Z" },
    });

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText("No Available Contractors")).toBeInTheDocument();
      expect(
        screen.getByText(/No contractors are available for this time slot/i)
      ).toBeInTheDocument();
    });
  });

  it("should display error message when API fails", async () => {
    const errorMessage = "Failed to fetch recommendations";
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockRejectedValue(new Error(errorMessage));

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(() => {
      expect(
        screen.getByText(/Failed to Load Recommendations/i)
      ).toBeInTheDocument();
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });
  });

  it("should show retry button on error", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockRejectedValue(new Error("Network error"));

    render(<RecommendationsModal {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText(/Try Again/i)).toBeInTheDocument();
    });
  });

  it("should close modal on background click", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: mockRecommendations,
      metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
    });

    const onClose = vi.fn();
    render(<RecommendationsModal {...defaultProps} onClose={onClose} />);

    await waitFor(() => {
      expect(screen.getByText("Bob's Plumbing Pro")).toBeInTheDocument();
    });

    // Click the backdrop
    const backdrop = screen.getByRole("presentation");
    fireEvent.click(backdrop);

    expect(onClose).toHaveBeenCalled();
  });

  it("should close modal on close button click", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: mockRecommendations,
      metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
    });

    const onClose = vi.fn();
    render(<RecommendationsModal {...defaultProps} onClose={onClose} />);

    await waitFor(() => {
      expect(screen.getByText("Bob's Plumbing Pro")).toBeInTheDocument();
    });

    const closeButton = screen.getByLabelText("Close recommendations modal");
    fireEvent.click(closeButton);

    expect(onClose).toHaveBeenCalled();
  });

  it("should close modal on Escape key press", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: mockRecommendations,
      metadata: { totalAvailable: 3, requestTime: "2025-11-08T11:30:00Z" },
    });

    const onClose = vi.fn();
    render(<RecommendationsModal {...defaultProps} onClose={onClose} />);

    await waitFor(() => {
      expect(screen.getByText("Bob's Plumbing Pro")).toBeInTheDocument();
    });

    fireEvent.keyDown(document, { key: "Escape" });

    expect(onClose).toHaveBeenCalled();
  });

  it("should have accessible dialog attributes", () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue({
      data: [],
      metadata: { totalAvailable: 0, requestTime: "2025-11-08T11:30:00Z" },
    });

    render(<RecommendationsModal {...defaultProps} />);

    const dialog = screen.getByRole("dialog");
    expect(dialog).toHaveAttribute("aria-modal", "true");
    expect(dialog).toHaveAttribute("aria-labelledby", "recommendations-title");
    expect(dialog).toHaveAttribute("aria-describedby", "recommendations-desc");
  });
});
