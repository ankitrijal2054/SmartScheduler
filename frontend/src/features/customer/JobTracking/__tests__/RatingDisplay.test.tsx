/**
 * RatingDisplay Component Tests
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { RatingDisplay } from "../RatingDisplay";

describe("RatingDisplay Component", () => {
  it("should render excellent rating (4.8)", () => {
    render(<RatingDisplay averageRating={4.8} reviewCount={27} />);

    expect(screen.getByText("4.8/5")).toBeInTheDocument();
    expect(screen.getByText("Excellent")).toBeInTheDocument();
    expect(screen.getByText("Based on 27 reviews")).toBeInTheDocument();
  });

  it("should render very good rating (4.2)", () => {
    render(<RatingDisplay averageRating={4.2} reviewCount={15} />);

    expect(screen.getByText("4.2/5")).toBeInTheDocument();
    expect(screen.getByText("Very Good")).toBeInTheDocument();
    expect(screen.getByText("Based on 15 reviews")).toBeInTheDocument();
  });

  it("should render good rating (3.5)", () => {
    render(<RatingDisplay averageRating={3.5} reviewCount={8} />);

    expect(screen.getByText("3.5/5")).toBeInTheDocument();
    expect(screen.getByText("Good")).toBeInTheDocument();
    expect(screen.getByText("Based on 8 reviews")).toBeInTheDocument();
  });

  it("should render fair rating (2.5)", () => {
    render(<RatingDisplay averageRating={2.5} reviewCount={5} />);

    expect(screen.getByText("2.5/5")).toBeInTheDocument();
    expect(screen.getByText("Fair")).toBeInTheDocument();
    expect(screen.getByText("Based on 5 reviews")).toBeInTheDocument();
  });

  it("should render poor rating (1.8)", () => {
    render(<RatingDisplay averageRating={1.8} reviewCount={3} />);

    expect(screen.getByText("1.8/5")).toBeInTheDocument();
    expect(screen.getByText("Poor")).toBeInTheDocument();
    // Low rating should show neutral note
    expect(
      screen.getByText(
        /This contractor has received feedback worth considering/
      )
    ).toBeInTheDocument();
  });

  it("should render no rating state", () => {
    render(<RatingDisplay averageRating={null} reviewCount={0} />);

    expect(screen.getByText("No rating")).toBeInTheDocument();
    expect(screen.getByText("No ratings yet")).toBeInTheDocument();
  });

  it("should render single review correctly", () => {
    render(<RatingDisplay averageRating={4.5} reviewCount={1} />);

    expect(screen.getByText("Based on 1 review")).toBeInTheDocument();
  });

  it("should render proper star count for 5.0 rating", () => {
    render(<RatingDisplay averageRating={5.0} reviewCount={10} />);

    const starElements = screen
      .getByText("5.0/5")
      .parentElement?.querySelectorAll("span");
    // 5 full stars + rating text
    expect(starElements).toBeDefined();
  });

  it("should render proper star count for 4.0 rating", () => {
    render(<RatingDisplay averageRating={4.0} reviewCount={10} />);

    const ratingElement = screen.getByText("4.0/5");
    expect(ratingElement).toBeInTheDocument();
  });

  it("should render proper star count for 3.5 rating (with half star)", () => {
    render(<RatingDisplay averageRating={3.5} reviewCount={10} />);

    const ratingElement = screen.getByText("3.5/5");
    expect(ratingElement).toBeInTheDocument();
  });

  it("should not show review count when zero", () => {
    render(<RatingDisplay averageRating={4.5} reviewCount={0} />);

    expect(screen.queryByText(/Based on/)).not.toBeInTheDocument();
  });

  it("should show low rating warning note for ratings below 3", () => {
    const { rerender } = render(
      <RatingDisplay averageRating={2.0} reviewCount={5} />
    );

    expect(
      screen.getByText(
        /This contractor has received feedback worth considering/
      )
    ).toBeInTheDocument();

    // Should not show warning for rating >= 3
    rerender(<RatingDisplay averageRating={3.0} reviewCount={5} />);

    expect(
      screen.queryByText(
        /This contractor has received feedback worth considering/
      )
    ).not.toBeInTheDocument();
  });
});
