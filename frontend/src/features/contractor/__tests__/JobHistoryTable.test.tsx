/**
 * JobHistoryTable Component Tests
 * Tests for rendering job history with pagination
 */

import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { JobHistoryTable } from "../JobHistoryTable";
import { JobHistoryItem } from "@/types/ContractorProfile";

describe("JobHistoryTable", () => {
  const mockJobs: JobHistoryItem[] = [
    {
      id: 1,
      jobId: 1,
      jobType: "Plumbing",
      location: "123 Main St",
      scheduledDateTime: "2025-01-15T10:00:00Z",
      status: "Completed",
      customerName: "Jane Smith",
      customerRating: 5,
      customerReviewText: "Great work!",
      acceptedAt: "2025-01-14T09:00:00Z",
      completedAt: "2025-01-15T14:00:00Z",
    },
    {
      id: 2,
      jobId: 2,
      jobType: "HVAC",
      location: "456 Oak Ave",
      scheduledDateTime: "2025-01-10T14:00:00Z",
      status: "Completed",
      customerName: "John Johnson",
      customerRating: 4,
      customerReviewText: "Good service",
      acceptedAt: "2025-01-09T10:00:00Z",
      completedAt: "2025-01-10T18:00:00Z",
    },
  ];

  it("renders job history table with jobs", () => {
    render(
      <JobHistoryTable
        jobs={mockJobs}
        totalCount={2}
        currentPage={1}
        pageSize={20}
        onPageChange={() => {}}
      />
    );

    expect(screen.getByText("Plumbing")).toBeInTheDocument();
    expect(screen.getByText("HVAC")).toBeInTheDocument();
    expect(screen.getByText("Jane Smith")).toBeInTheDocument();
  });

  it("displays empty state when no jobs", () => {
    render(
      <JobHistoryTable
        jobs={[]}
        totalCount={0}
        currentPage={1}
        pageSize={20}
        onPageChange={() => {}}
      />
    );

    expect(screen.getByText(/No jobs found/)).toBeInTheDocument();
  });

  it("shows customer ratings in table", () => {
    render(
      <JobHistoryTable
        jobs={mockJobs}
        totalCount={2}
        currentPage={1}
        pageSize={20}
        onPageChange={() => {}}
      />
    );

    expect(screen.getByText("5/5")).toBeInTheDocument();
    expect(screen.getByText("4/5")).toBeInTheDocument();
  });

  it("handles pagination button clicks", () => {
    const mockOnPageChange = vi.fn();

    render(
      <JobHistoryTable
        jobs={mockJobs}
        totalCount={40}
        currentPage={1}
        pageSize={20}
        onPageChange={mockOnPageChange}
      />
    );

    const nextButton = screen.getByRole("button", { name: /Next/i });
    fireEvent.click(nextButton);

    expect(mockOnPageChange).toHaveBeenCalledWith(2);
  });

  it("displays correct pagination info", () => {
    render(
      <JobHistoryTable
        jobs={mockJobs}
        totalCount={50}
        currentPage={2}
        pageSize={20}
        onPageChange={() => {}}
      />
    );

    expect(screen.getByText(/Page 2 of 3/)).toBeInTheDocument();
  });
});
