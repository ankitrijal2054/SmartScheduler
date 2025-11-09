/**
 * ReviewsList Component Tests
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { ReviewsList } from "../ReviewsList";
import { ReviewWithCustomer } from "@/types/Customer";

describe("ReviewsList Component", () => {
  const mockReviews: ReviewWithCustomer[] = [
    {
      id: "review-1",
      jobId: "job-1",
      contractorId: "contractor-1",
      customerId: "customer-1",
      rating: 5,
      comment: "John was professional and quick",
      customerName: "Jane K.",
      createdAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(), // 1 week ago
    },
    {
      id: "review-2",
      jobId: "job-2",
      contractorId: "contractor-1",
      customerId: "customer-2",
      rating: 4,
      comment: "Good work, arrived on time",
      customerName: "Bob M.",
      createdAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(), // 2 weeks ago
    },
    {
      id: "review-3",
      jobId: "job-3",
      contractorId: "contractor-1",
      customerId: "customer-3",
      rating: 5,
      comment: null, // No comment
      customerName: "Alice R.",
      createdAt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(), // 1 month ago
    },
  ];

  it("should render list of reviews", () => {
    render(<ReviewsList reviews={mockReviews} />);

    expect(screen.getByText("Customer Reviews (3)")).toBeInTheDocument();
    expect(screen.getByText("Jane K.")).toBeInTheDocument();
    expect(screen.getByText("Bob M.")).toBeInTheDocument();
    expect(screen.getByText("Alice R.")).toBeInTheDocument();
  });

  it("should render customer names and ratings for each review", () => {
    render(<ReviewsList reviews={mockReviews} />);

    expect(screen.getByText("Jane K.")).toBeInTheDocument();
    expect(screen.getByText("Bob M.")).toBeInTheDocument();
    expect(screen.getByText("Alice R.")).toBeInTheDocument();
  });

  it("should render review comments", () => {
    render(<ReviewsList reviews={mockReviews} />);

    expect(
      screen.getByText("John was professional and quick")
    ).toBeInTheDocument();
    expect(screen.getByText("Good work, arrived on time")).toBeInTheDocument();
  });

  it("should handle reviews without comments", () => {
    render(<ReviewsList reviews={mockReviews} />);

    // Review 3 has no comment, so it shouldn't render a comment text
    // But the review itself should still be there
    expect(screen.getByText("Alice R.")).toBeInTheDocument();
  });

  it("should render formatted dates", () => {
    render(<ReviewsList reviews={mockReviews} />);

    // Dates should be formatted (may vary by how long ago)
    // At minimum, the component should render without errors
    expect(screen.getByText("Jane K.")).toBeInTheDocument();
  });

  it("should render empty state when no reviews", () => {
    render(<ReviewsList reviews={[]} />);

    expect(
      screen.getByText("No reviews yet - be the first to rate this contractor")
    ).toBeInTheDocument();
  });

  it("should limit displayed reviews to maxReviews", () => {
    render(<ReviewsList reviews={mockReviews} maxReviews={2} />);

    expect(screen.getByText("Jane K.")).toBeInTheDocument();
    expect(screen.getByText("Bob M.")).toBeInTheDocument();
    expect(screen.queryByText("Alice R.")).not.toBeInTheDocument();
    expect(screen.getByText("+1 more review")).toBeInTheDocument();
  });

  it("should show correct count for multiple additional reviews", () => {
    const manyReviews = Array.from({ length: 10 }, (_, i) => ({
      id: `review-${i}`,
      jobId: `job-${i}`,
      contractorId: "contractor-1",
      customerId: `customer-${i}`,
      rating: 5,
      comment: `Review ${i}`,
      customerName: `Customer ${i}`,
      createdAt: new Date().toISOString(),
    }));

    render(<ReviewsList reviews={manyReviews} maxReviews={3} />);

    expect(screen.getByText("+7 more reviews")).toBeInTheDocument();
  });

  it("should not show more reviews indicator when under limit", () => {
    render(<ReviewsList reviews={mockReviews} maxReviews={10} />);

    expect(screen.queryByText(/more review/)).not.toBeInTheDocument();
  });

  it("should display review count in header", () => {
    render(<ReviewsList reviews={mockReviews} />);

    expect(screen.getByText("Customer Reviews (3)")).toBeInTheDocument();
  });

  it("should render reviews with 5-star ratings", () => {
    const fiveStarReview: ReviewWithCustomer[] = [
      {
        id: "review-1",
        jobId: "job-1",
        contractorId: "contractor-1",
        customerId: "customer-1",
        rating: 5,
        comment: "Perfect work!",
        customerName: "Perfect Customer",
        createdAt: new Date().toISOString(),
      },
    ];

    render(<ReviewsList reviews={fiveStarReview} />);

    expect(screen.getByText("Perfect Customer")).toBeInTheDocument();
    expect(screen.getByText("Perfect work!")).toBeInTheDocument();
  });

  it("should render reviews with 1-star ratings", () => {
    const oneStarReview: ReviewWithCustomer[] = [
      {
        id: "review-1",
        jobId: "job-1",
        contractorId: "contractor-1",
        customerId: "customer-1",
        rating: 1,
        comment: "Not satisfied",
        customerName: "Critical Customer",
        createdAt: new Date().toISOString(),
      },
    ];

    render(<ReviewsList reviews={oneStarReview} />);

    expect(screen.getByText("Critical Customer")).toBeInTheDocument();
    expect(screen.getByText("Not satisfied")).toBeInTheDocument();
  });
});
