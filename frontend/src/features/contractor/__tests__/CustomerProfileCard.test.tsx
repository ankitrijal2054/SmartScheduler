/**
 * CustomerProfileCard Component Tests
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import { describe, it, expect, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { CustomerProfileCard } from "../CustomerProfileCard";
import { JobDetails } from "@/types/JobDetails";

const mockCustomer: JobDetails["customer"] = {
  id: "cust-1",
  name: "John Smith",
  rating: 4.5,
  reviewCount: 12,
  phoneNumber: "303-555-1234",
};

const mockReviews: JobDetails["pastReviews"] = [
  {
    id: "review-1",
    jobId: "job-1",
    jobType: "HVAC",
    rating: 5,
    comment: "Great service!",
    createdAt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
  },
  {
    id: "review-2",
    jobId: "job-2",
    jobType: "Plumbing",
    rating: 4,
    comment: "Professional work",
    createdAt: new Date(Date.now() - 60 * 24 * 60 * 60 * 1000).toISOString(),
  },
];

describe("CustomerProfileCard", () => {
  beforeEach(() => {
    // Setup
  });

  it("should render customer name", () => {
    render(<CustomerProfileCard customer={mockCustomer} pastReviews={[]} />);

    expect(screen.getByText("John Smith")).toBeInTheDocument();
  });

  it("should display customer rating", () => {
    render(<CustomerProfileCard customer={mockCustomer} pastReviews={[]} />);

    expect(screen.getByText(/4.5/)).toBeInTheDocument();
    expect(screen.getByText(/12 reviews/)).toBeInTheDocument();
  });

  it("should display past job history", () => {
    render(
      <CustomerProfileCard customer={mockCustomer} pastReviews={mockReviews} />
    );

    expect(screen.getByText("HVAC")).toBeInTheDocument();
    expect(screen.getByText("Plumbing")).toBeInTheDocument();
    expect(screen.getByText(/Great service/)).toBeInTheDocument();
    expect(screen.getByText(/Professional work/)).toBeInTheDocument();
  });

  it("should show 'No prior history' when reviews are empty", () => {
    render(<CustomerProfileCard customer={mockCustomer} pastReviews={[]} />);

    expect(
      screen.getByText(/No prior history with this customer/i)
    ).toBeInTheDocument();
  });

  it("should display star ratings in reviews", () => {
    render(
      <CustomerProfileCard customer={mockCustomer} pastReviews={mockReviews} />
    );

    // Check for star display (component shows stars visually)
    const reviewElements = screen.getAllByText(/⭐/);
    expect(reviewElements.length).toBeGreaterThan(0);
  });

  it("should limit displayed reviews to 3 and show more count", () => {
    const manyReviews = Array.from({ length: 5 }, (_, i) => ({
      id: `review-${i}`,
      jobId: `job-${i}`,
      jobType: "HVAC",
      rating: 5,
      comment: `Review ${i}`,
      createdAt: new Date().toISOString(),
    }));

    render(
      <CustomerProfileCard customer={mockCustomer} pastReviews={manyReviews} />
    );

    expect(screen.getByText(/\+2 more jobs/)).toBeInTheDocument();
  });

  it("should handle null customer rating", () => {
    const customerNoRating = { ...mockCustomer, rating: null };

    render(
      <CustomerProfileCard customer={customerNoRating} pastReviews={[]} />
    );

    expect(screen.getByText(/No rating yet/)).toBeInTheDocument();
  });

  it("should display 'Job History with This Customer' header", () => {
    render(
      <CustomerProfileCard customer={mockCustomer} pastReviews={mockReviews} />
    );

    expect(
      screen.getByText(/Job History with This Customer/i)
    ).toBeInTheDocument();
  });

  it("should display review comment when available", () => {
    render(
      <CustomerProfileCard customer={mockCustomer} pastReviews={mockReviews} />
    );

    expect(screen.getByText(/Great service/)).toBeInTheDocument();
    expect(screen.getByText(/Professional work/)).toBeInTheDocument();
  });
});
