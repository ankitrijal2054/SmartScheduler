/**
 * ProfileStatsPanel Component Tests
 * Tests for rendering contractor profile statistics
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { ProfileStatsPanel } from "../ProfileStatsPanel";
import { ContractorProfileData } from "@/types/ContractorProfile";

describe("ProfileStatsPanel", () => {
  const mockProfileWithRating: ContractorProfileData = {
    id: 1,
    name: "John Doe",
    averageRating: 4.7,
    reviewCount: 42,
    totalJobsAssigned: 50,
    totalJobsAccepted: 38,
    totalJobsCompleted: 38,
    acceptanceRate: 76,
    totalEarnings: 3400,
    createdAt: "2025-01-01T00:00:00Z",
    recentReviews: [],
  };

  const mockProfileNoRating: ContractorProfileData = {
    ...mockProfileWithRating,
    averageRating: null,
    reviewCount: 0,
  };

  it("renders contractor name", () => {
    render(<ProfileStatsPanel profile={mockProfileWithRating} />);
    expect(screen.getByText("John Doe")).toBeInTheDocument();
  });

  it("displays rating with stars when available", () => {
    render(<ProfileStatsPanel profile={mockProfileWithRating} />);
    expect(screen.getByText("4.7/5")).toBeInTheDocument();
    expect(screen.getByText("42 reviews")).toBeInTheDocument();
  });

  it("shows 'No ratings yet' when averageRating is null", () => {
    render(<ProfileStatsPanel profile={mockProfileNoRating} />);
    expect(screen.getByText("No ratings yet")).toBeInTheDocument();
  });

  it("displays job statistics correctly", () => {
    render(<ProfileStatsPanel profile={mockProfileWithRating} />);

    // Check for total jobs assigned
    expect(screen.getByText("50")).toBeInTheDocument(); // Total jobs
    expect(screen.getByText("Jobs Assigned")).toBeInTheDocument();

    // Check for accepted jobs (should be in the "Accepted" section)
    const acceptedSection = screen.getByText("Accepted").closest("div");
    expect(acceptedSection).toHaveTextContent("38");

    // Check for acceptance rate
    expect(screen.getByText("76%")).toBeInTheDocument();
    expect(screen.getByText("Acceptance Rate")).toBeInTheDocument();
  });

  it("displays earnings when available", () => {
    render(<ProfileStatsPanel profile={mockProfileWithRating} />);
    expect(screen.getByText("$3400.00")).toBeInTheDocument();
  });
});
