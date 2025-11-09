/**
 * JobList Component Tests
 * Vitest + React Testing Library
 */

import { describe, it, expect, vi } from "vitest";
import { render } from "@testing-library/react";
import { screen } from "@testing-library/react";
import { fireEvent } from "@testing-library/react";
import { JobList } from "../JobList";
import { Job, PaginationMeta } from "@/types/Job";

describe("JobList Component", () => {
  const mockJobs: Job[] = [
    {
      id: "job_1",
      customerId: "cust_1",
      customerName: "Jane Smith",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
      jobType: "Plumbing",
      description: "Fix broken pipe",
      status: "Pending",
      currentAssignedContractorId: null,
      createdAt: "2025-11-08T10:30:00Z",
      updatedAt: "2025-11-08T10:30:00Z",
    },
    {
      id: "job_2",
      customerId: "cust_2",
      customerName: "John Doe",
      location: "456 Oak Ave",
      desiredDateTime: "2025-11-09T09:00:00Z",
      jobType: "HVAC",
      description: "AC maintenance",
      status: "Assigned",
      currentAssignedContractorId: "cont_1",
      assignedContractorName: "Bob's HVAC",
      assignedContractorRating: 4.8,
      createdAt: "2025-11-07T15:45:00Z",
      updatedAt: "2025-11-08T09:15:00Z",
    },
  ];

  const mockPagination: PaginationMeta = {
    page: 1,
    limit: 20,
    total: 2,
    totalPages: 1,
  };

  it("should render job list with correct job information", () => {
    render(
      <JobList
        jobs={mockJobs}
        loading={false}
        error={null}
        pagination={mockPagination}
      />
    );

    expect(screen.getByText("Jane Smith")).toBeInTheDocument();
    expect(screen.getByText("John Doe")).toBeInTheDocument();
    expect(screen.getByText("123 Main St")).toBeInTheDocument();
    expect(screen.getByText("456 Oak Ave")).toBeInTheDocument();
  });

  it("should highlight pending jobs with yellow badge", () => {
    const { container } = render(
      <JobList
        jobs={[mockJobs[0]]}
        loading={false}
        error={null}
        pagination={mockPagination}
      />
    );

    // Check for yellow badge styling
    const badge = container.querySelector(".bg-yellow-100");
    expect(badge).toBeInTheDocument();
  });

  it("should display assigned contractor name for assigned jobs", () => {
    render(
      <JobList
        jobs={[mockJobs[1]]}
        loading={false}
        error={null}
        pagination={mockPagination}
      />
    );

    expect(screen.getByText(/Bob's HVAC/)).toBeInTheDocument();
  });

  it("should show empty state when no jobs provided", () => {
    render(
      <JobList
        jobs={[]}
        loading={false}
        error={null}
        pagination={mockPagination}
      />
    );

    expect(screen.getByText("No jobs at this time")).toBeInTheDocument();
  });

  it("should display loading spinner while loading", () => {
    const { container } = render(
      <JobList jobs={[]} loading={true} error={null} pagination={null} />
    );

    // Check for spinner SVG animation
    expect(container.querySelector("svg")).toBeInTheDocument();
  });

  it("should navigate pagination when next/prev clicked", () => {
    const mockOnPageChange = vi.fn();
    const multiPagePagination: PaginationMeta = {
      page: 1,
      limit: 20,
      total: 40,
      totalPages: 2,
    };

    render(
      <JobList
        jobs={mockJobs}
        loading={false}
        error={null}
        pagination={multiPagePagination}
        onPageChange={mockOnPageChange}
      />
    );

    const nextButton = screen.getByLabelText("Next page");
    fireEvent.click(nextButton);

    expect(mockOnPageChange).toHaveBeenCalledWith(2);
  });

  it("should handle API error and display error message", () => {
    const errorMessage = "Failed to load jobs";
    render(
      <JobList
        jobs={[]}
        loading={false}
        error={errorMessage}
        pagination={null}
      />
    );

    expect(screen.getByText("Error loading jobs")).toBeInTheDocument();
    expect(screen.getByText(errorMessage)).toBeInTheDocument();
  });

  it("should display pagination info correctly", () => {
    const multiPagePagination: PaginationMeta = {
      page: 1,
      limit: 20,
      total: 40,
      totalPages: 2,
    };

    const { container } = render(
      <JobList
        jobs={mockJobs}
        loading={false}
        error={null}
        pagination={multiPagePagination}
      />
    );

    // Check for pagination text (text might be broken across spans)
    expect(container.textContent).toContain("Page");
    expect(screen.getByLabelText("Previous page")).toBeInTheDocument();
    expect(screen.getByLabelText("Next page")).toBeInTheDocument();
  });

  it("should call onJobClick when job card clicked", () => {
    const mockOnJobClick = vi.fn();
    render(
      <JobList
        jobs={[mockJobs[0]]}
        loading={false}
        error={null}
        pagination={mockPagination}
        onJobClick={mockOnJobClick}
      />
    );

    const jobCard = screen.getByRole("article");
    fireEvent.click(jobCard);

    expect(mockOnJobClick).toHaveBeenCalledWith(mockJobs[0]);
  });
});
