/**
 * ContractorStatsCard Component Tests
 */

import { render, screen } from "@testing-library/react";
import { ContractorStatsCard } from "../ContractorStatsCard";
import { ContractorStats } from "@/types/Contractor";

describe("ContractorStatsCard Component", () => {
  const mockStats: ContractorStats = {
    totalJobsAssigned: 50,
    totalJobsCompleted: 45,
    acceptanceRate: 90.0,
    averageRating: 4.5,
    totalReviews: 12,
  };

  it("should render all stats correctly", () => {
    render(<ContractorStatsCard stats={mockStats} />);

    expect(screen.getByText("Total Jobs Assigned")).toBeInTheDocument();
    expect(screen.getByText("50")).toBeInTheDocument();
    expect(screen.getByText("Jobs Completed")).toBeInTheDocument();
    expect(screen.getByText("45")).toBeInTheDocument();
    expect(screen.getByText("Acceptance Rate")).toBeInTheDocument();
    expect(screen.getByText("90.0%")).toBeInTheDocument();
    expect(screen.getByText("Reviews")).toBeInTheDocument();
    expect(screen.getByText("12")).toBeInTheDocument();
  });

  it("should render average rating with stars", () => {
    render(<ContractorStatsCard stats={mockStats} />);

    const averageRatingSection = screen
      .getByText("Average Rating")
      .closest("div");
    expect(averageRatingSection).toBeInTheDocument();

    // Check for star display (5 stars total, 4-5 filled for 4.5 rating)
    const stars = averageRatingSection?.querySelectorAll("span");
    expect(stars).toBeTruthy();
  });

  it("should handle null average rating", () => {
    const statsWithNoRating = { ...mockStats, averageRating: null };

    render(<ContractorStatsCard stats={statsWithNoRating} />);

    expect(screen.getByText("Not rated")).toBeInTheDocument();
  });

  it("should display low rating warning", () => {
    const lowRatingStats = { ...mockStats, averageRating: 3.2 };

    render(
      <ContractorStatsCard
        stats={lowRatingStats}
        warnings={{ lowRating: true, highCancellationRate: false }}
      />
    );

    expect(screen.getByText(/Low Rating/)).toBeInTheDocument();
  });

  it("should display high cancellation rate warning", () => {
    render(
      <ContractorStatsCard
        stats={mockStats}
        warnings={{ lowRating: false, highCancellationRate: true }}
      />
    );

    expect(screen.getByText(/High Cancellation Rate/)).toBeInTheDocument();
  });

  it("should display both warnings when both are true", () => {
    render(
      <ContractorStatsCard
        stats={mockStats}
        warnings={{ lowRating: true, highCancellationRate: true }}
      />
    );

    expect(screen.getByText(/Low Rating/)).toBeInTheDocument();
    expect(screen.getByText(/High Cancellation Rate/)).toBeInTheDocument();
  });

  it("should not display warnings section when warnings are false", () => {
    const { container } = render(
      <ContractorStatsCard
        stats={mockStats}
        warnings={{ lowRating: false, highCancellationRate: false }}
      />
    );

    const alertsHeading = screen.queryByText("Alerts");
    expect(alertsHeading).not.toBeInTheDocument();
  });

  it("should format acceptance rate as percentage", () => {
    const statsWithDifferentRate = { ...mockStats, acceptanceRate: 75.5 };

    render(<ContractorStatsCard stats={statsWithDifferentRate} />);

    expect(screen.getByText("75.5%")).toBeInTheDocument();
  });
});
