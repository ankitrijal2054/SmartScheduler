/**
 * JobHistoryTable Component Tests
 */

import { render, screen, fireEvent } from "@testing-library/react";
import { JobHistoryTable } from "../JobHistoryTable";
import { JobHistoryItem } from "@/types/Contractor";

describe("JobHistoryTable Component", () => {
  const mockJobs: JobHistoryItem[] = [
    {
      jobId: "job-1",
      jobType: "Plumbing",
      customerName: "Alice Johnson",
      completedAt: "2025-11-08T14:30:00Z",
      status: "completed",
      customerRating: 5,
      createdAt: "2025-11-08T10:00:00Z",
    },
    {
      jobId: "job-2",
      jobType: "Plumbing",
      customerName: "Bob Davis",
      completedAt: "2025-11-07T16:00:00Z",
      status: "completed",
      customerRating: 4,
      createdAt: "2025-11-07T09:00:00Z",
    },
    {
      jobId: "job-3",
      jobType: "HVAC",
      customerName: "Carol Smith",
      completedAt: "2025-11-06T15:00:00Z",
      status: "cancelled",
      customerRating: null,
      createdAt: "2025-11-06T08:00:00Z",
    },
  ];

  it("should render job history table with all jobs", () => {
    const { container } = render(<JobHistoryTable jobs={mockJobs} />);

    // Check for table headers (both desktop and mobile views are present in DOM)
    const dateHeaders = screen.getAllByText("Date");
    expect(dateHeaders.length).toBeGreaterThan(0);

    const jobTypeHeaders = screen.getAllByText("Job Type");
    expect(jobTypeHeaders.length).toBeGreaterThan(0);

    // Check for desktop table specifically
    const desktopTable = container.querySelector("table");
    expect(desktopTable).toBeInTheDocument();
  });

  it("should render job details in table", () => {
    render(<JobHistoryTable jobs={mockJobs} />);

    // Job details appear in both table and mobile views
    const aliceElements = screen.getAllByText("Alice Johnson");
    expect(aliceElements.length).toBeGreaterThan(0);

    const bobElements = screen.getAllByText("Bob Davis");
    expect(bobElements.length).toBeGreaterThan(0);

    const plumbingElements = screen.getAllByText("Plumbing");
    expect(plumbingElements.length).toBeGreaterThanOrEqual(2);

    const hvacElements = screen.getAllByText("HVAC");
    expect(hvacElements.length).toBeGreaterThan(0);
  });

  it("should render status badges", () => {
    render(<JobHistoryTable jobs={mockJobs} />);

    // Status badges are rendered in both table and mobile card views
    const completedBadges = screen.getAllByText(/Completed/i);
    expect(completedBadges.length).toBeGreaterThan(0);

    const cancelledBadges = screen.getAllByText(/Cancelled/i);
    expect(cancelledBadges.length).toBeGreaterThan(0);
  });

  it("should display 'Not rated' for jobs without customer rating", () => {
    render(<JobHistoryTable jobs={mockJobs} />);

    const notRatedElements = screen.getAllByText("Not rated");
    expect(notRatedElements.length).toBeGreaterThan(0);
  });

  it("should render loading state", () => {
    render(<JobHistoryTable jobs={[]} isLoading={true} />);

    expect(screen.getByText("Loading job history...")).toBeInTheDocument();
  });

  it("should render empty state when no jobs", () => {
    render(<JobHistoryTable jobs={[]} />);

    expect(screen.getByText("No job history available")).toBeInTheDocument();
    expect(
      screen.getByText("This contractor hasn't completed any jobs yet")
    ).toBeInTheDocument();
  });

  it("should paginate jobs (10 per page)", () => {
    const manyJobs = Array.from({ length: 25 }, (_, i) => ({
      jobId: `job-${i}`,
      jobType: "Plumbing",
      customerName: `Customer ${i}`,
      completedAt: "2025-11-08T14:30:00Z",
      status: "completed" as const,
      customerRating: 5,
      createdAt: "2025-11-08T10:00:00Z",
    }));

    render(<JobHistoryTable jobs={manyJobs} />);

    // First page should show first 10 jobs (Customer 0 and 9 should exist)
    const customer0 = screen.getAllByText("Customer 0");
    expect(customer0.length).toBeGreaterThan(0);

    const customer9 = screen.getAllByText("Customer 9");
    expect(customer9.length).toBeGreaterThan(0);

    // Next page button should be visible and enabled
    const nextButtons = screen.getAllByText("Next →");
    expect(nextButtons.length).toBeGreaterThan(0);
    expect(nextButtons[0]).not.toBeDisabled();

    // Pagination info should show page 1 of 3 (may appear multiple times)
    const pageInfo = screen.getAllByText(/Page 1 of 3/);
    expect(pageInfo.length).toBeGreaterThan(0);
  });

  it("should disable previous button on first page", () => {
    // Need enough jobs to trigger pagination (>10 jobs)
    const manyJobs = Array.from({ length: 15 }, (_, i) => ({
      jobId: `job-${i}`,
      jobType: "Plumbing",
      customerName: `Customer ${i}`,
      completedAt: "2025-11-08T14:30:00Z",
      status: "completed" as const,
      customerRating: 5,
      createdAt: "2025-11-08T10:00:00Z",
    }));

    render(<JobHistoryTable jobs={manyJobs} />);

    // Previous button should be disabled on first page
    const prevButtons = screen.getAllByText("← Previous");
    expect(prevButtons.length).toBeGreaterThan(0);
    expect(prevButtons[0]).toBeDisabled();
  });

  it("should render mobile card view", () => {
    const { container } = render(<JobHistoryTable jobs={mockJobs} />);

    // Check for mobile-specific elements (should be hidden on desktop)
    const mobileCards = container.querySelectorAll(".md\\:hidden");
    expect(mobileCards.length).toBeGreaterThan(0);
  });

  it("should call onPageChange callback when pagination changes", () => {
    const onPageChange = vi.fn();
    const manyJobs = Array.from({ length: 25 }, (_, i) => ({
      jobId: `job-${i}`,
      jobType: "Plumbing",
      customerName: `Customer ${i}`,
      completedAt: "2025-11-08T14:30:00Z",
      status: "completed" as const,
      customerRating: 5,
      createdAt: "2025-11-08T10:00:00Z",
    }));

    render(<JobHistoryTable jobs={manyJobs} onPageChange={onPageChange} />);

    const nextButton = screen.getByText("Next →");
    fireEvent.click(nextButton);

    expect(onPageChange).toHaveBeenCalledWith(10); // Second page, offset 10
  });

  it("should format dates correctly", () => {
    render(<JobHistoryTable jobs={mockJobs} />);

    // Dates should be formatted as "Nov 8, 2025" style (no leading zero)
    // May appear multiple times in both table and mobile views
    const dateElements = screen.getAllByText(/Nov \d+, 2025/);
    expect(dateElements.length).toBeGreaterThan(0);
  });
});
