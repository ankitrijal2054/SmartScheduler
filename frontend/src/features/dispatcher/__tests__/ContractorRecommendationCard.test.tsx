/**
 * ContractorRecommendationCard Component Tests
 * Unit tests using Vitest + React Testing Library
 */

import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { ContractorRecommendationCard } from "../ContractorRecommendationCard";
import { RecommendedContractor } from "@/types/Contractor";

const mockContractor: RecommendedContractor = {
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
    {
      startTime: "2025-11-10T17:00:00Z",
      endTime: "2025-11-10T19:00:00Z",
    },
  ],
};

const mockContractorRank2: RecommendedContractor = {
  ...mockContractor,
  contractorId: "cont_002",
  name: "Jane's Plumbing Services",
  rank: 2,
  score: 0.87,
  avgRating: 4.6,
  reviewCount: 28,
  distance: 4.1,
  travelTime: 18,
};

const mockContractorRank3: RecommendedContractor = {
  ...mockContractor,
  contractorId: "cont_003",
  name: "Quick Plumbing Team",
  rank: 3,
  score: 0.79,
  avgRating: 4.4,
  reviewCount: 15,
  distance: 6.2,
  travelTime: 22,
};

const mockContractorNoRating: RecommendedContractor = {
  ...mockContractor,
  contractorId: "cont_004",
  name: "New Plumbing Startup",
  rank: 4,
  avgRating: null,
  reviewCount: 0,
};

describe("ContractorRecommendationCard", () => {
  it("should render with contractor data", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    expect(screen.getByText("Bob's Plumbing Pro")).toBeInTheDocument();
    expect(screen.getByText("Plumbing")).toBeInTheDocument();
  });

  it("should display rank badge with correct number", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    const rankBadge = screen.getByLabelText("Rank: 1");
    expect(rankBadge).toBeInTheDocument();
    expect(rankBadge).toHaveTextContent("#1");
  });

  it("should display rank 1 badge with gold styling", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    const rankBadge = screen.getByLabelText("Rank: 1");
    expect(rankBadge).toHaveClass("bg-yellow-100", "text-yellow-800");
  });

  it("should display rank 2 badge with silver styling", () => {
    render(<ContractorRecommendationCard contractor={mockContractorRank2} />);

    const rankBadge = screen.getByLabelText("Rank: 2");
    expect(rankBadge).toHaveClass("bg-gray-100", "text-gray-800");
  });

  it("should display rank 3 badge with bronze styling", () => {
    render(<ContractorRecommendationCard contractor={mockContractorRank3} />);

    const rankBadge = screen.getByLabelText("Rank: 3");
    expect(rankBadge).toHaveClass("bg-orange-100", "text-orange-800");
  });

  it("should display star rating with count", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    // Check for rating text
    expect(screen.getByText(/4\.8.*42.*reviews/i)).toBeInTheDocument();
  });

  it("should display 'No ratings yet' when contractor has no rating", () => {
    render(
      <ContractorRecommendationCard contractor={mockContractorNoRating} />
    );

    expect(screen.getByText("No ratings yet")).toBeInTheDocument();
  });

  it("should display distance with map icon", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    expect(screen.getByText("2.5 miles")).toBeInTheDocument();
    // Check for map pin emoji
    const distanceElement = screen.getByText("2.5 miles").parentElement;
    expect(distanceElement).toHaveTextContent("📍");
  });

  it("should display travel time with clock icon", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    expect(screen.getByText("12 min")).toBeInTheDocument();
    // Check for clock emoji
    const timeElement = screen.getByText("12 min").parentElement;
    expect(timeElement).toHaveTextContent("🕐");
  });

  it("should display score as percentage with progress bar", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    const scorePercentage = "92%";
    expect(screen.getByText(scorePercentage)).toBeInTheDocument();

    const progressBar = screen.getByRole("progressbar");
    expect(progressBar).toHaveAttribute("aria-valuenow", "92");
    expect(progressBar).toHaveStyle("width: 92%");
  });

  it("should display availability slot in readable format", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    expect(screen.getByText(/Available:/i)).toBeInTheDocument();
    // Check for time format - date-fns will format as h:mm a, just verify the availability section exists
    const availabilitySection = screen.getByText(/Available:/i).closest("div");
    expect(availabilitySection).toHaveTextContent(/\d{1,2}:\d{2}/);
  });

  it("should display multiple slots count when contractor has more than one", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    expect(screen.getByText(/\+1 more slot/i)).toBeInTheDocument();
  });

  it("should not display '+more slots' when contractor has only one slot", () => {
    const singleSlotContractor = {
      ...mockContractor,
      availableTimeSlots: mockContractor.availableTimeSlots.slice(0, 1),
    };

    render(<ContractorRecommendationCard contractor={singleSlotContractor} />);

    expect(screen.queryByText(/\+.*more slot/i)).not.toBeInTheDocument();
  });

  it("should call onClick handler when card is clicked", () => {
    const onClick = vi.fn();
    render(
      <ContractorRecommendationCard
        contractor={mockContractor}
        onClick={onClick}
      />
    );

    const card = screen.getByRole("article");
    fireEvent.click(card);

    expect(onClick).toHaveBeenCalledWith(mockContractor);
  });

  it("should have accessible aria label on article element", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    const article = screen.getByRole("article");
    expect(article).toHaveAttribute(
      "aria-label",
      expect.stringContaining("Bob's Plumbing Pro")
    );
    expect(article).toHaveAttribute(
      "aria-label",
      expect.stringContaining("4.8")
    );
    expect(article).toHaveAttribute(
      "aria-label",
      expect.stringContaining("2.5 miles")
    );
  });

  it("should have progress bar with accessibility attributes", () => {
    render(<ContractorRecommendationCard contractor={mockContractor} />);

    const progressBar = screen.getByRole("progressbar");
    expect(progressBar).toHaveAttribute("aria-valuemin", "0");
    expect(progressBar).toHaveAttribute("aria-valuemax", "100");
    expect(progressBar).toHaveAttribute("aria-label", "Recommendation score");
  });

  it("should format high score correctly", () => {
    const highScoreContractor = {
      ...mockContractor,
      score: 0.99,
    };

    render(<ContractorRecommendationCard contractor={highScoreContractor} />);

    expect(screen.getByText("99%")).toBeInTheDocument();
  });

  it("should format low score correctly", () => {
    const lowScoreContractor = {
      ...mockContractor,
      score: 0.45,
    };

    render(<ContractorRecommendationCard contractor={lowScoreContractor} />);

    expect(screen.getByText("45%")).toBeInTheDocument();
  });

  it("should display review count singular correctly", () => {
    const singleReviewContractor = {
      ...mockContractor,
      reviewCount: 1,
    };

    render(
      <ContractorRecommendationCard contractor={singleReviewContractor} />
    );

    expect(screen.getByText(/1 review/i)).toBeInTheDocument();
  });

  it("should handle contractor with zero distance", () => {
    const zeroDistanceContractor = {
      ...mockContractor,
      distance: 0,
    };

    render(
      <ContractorRecommendationCard contractor={zeroDistanceContractor} />
    );

    expect(screen.getByText("0.0 miles")).toBeInTheDocument();
  });

  it("should handle large distances", () => {
    const farContractor = {
      ...mockContractor,
      distance: 99.9,
    };

    render(<ContractorRecommendationCard contractor={farContractor} />);

    expect(screen.getByText("99.9 miles")).toBeInTheDocument();
  });
});
