/**
 * JobCard Component Tests
 * Tests for individual job card including reassignment functionality
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { JobCard } from "../JobCard";
import { Job } from "@/types/Job";

// Mock child components to focus on JobCard logic
vi.mock("../ReassignmentFlow", () => ({
  ReassignmentFlow: ({ job, onSuccess }: any) =>
    job.status === "Assigned" ? (
      <button data-testid="reassignment-flow" onClick={() => onSuccess?.()}>
        Reassignment Flow
      </button>
    ) : null,
}));

vi.mock("@/components/shared/JobStatusBadge", () => ({
  JobStatusBadge: ({ status, contractorName }: any) => (
    <div data-testid="status-badge">
      {status} - {contractorName}
    </div>
  ),
}));

vi.mock("@/components/shared/ReassignmentHistoryBadge", () => ({
  ReassignmentHistoryBadge: ({ reassignmentCount }: any) =>
    reassignmentCount ? (
      <div data-testid="reassignment-history">
        Reassigned {reassignmentCount}x
      </div>
    ) : null,
}));

describe("JobCard Component", () => {
  const mockPendingJob: Job = {
    id: "job_pending_1",
    customerId: "cust_001",
    customerName: "John Doe",
    location: "123 Main St",
    desiredDateTime: "2025-12-01T10:00:00Z",
    jobType: "Plumbing",
    description: "Fix leaky faucet",
    status: "Pending",
    currentAssignedContractorId: null,
    createdAt: "2025-11-01T10:00:00Z",
    updatedAt: "2025-11-10T10:00:00Z",
  };

  const mockAssignedJob: Job = {
    id: "job_assigned_1",
    customerId: "cust_002",
    customerName: "Jane Smith",
    location: "456 Oak Ave",
    desiredDateTime: "2025-12-02T14:00:00Z",
    jobType: "HVAC",
    description: "AC maintenance",
    status: "Assigned",
    currentAssignedContractorId: "cont_001",
    assignedContractorName: "Bob's HVAC",
    assignedContractorRating: 4.8,
    createdAt: "2025-11-02T10:00:00Z",
    updatedAt: "2025-11-10T10:00:00Z",
  };

  const mockReassignedJob: Job = {
    ...mockAssignedJob,
    reassignmentCount: 2,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render job card with basic information", () => {
    render(<JobCard job={mockPendingJob} />);

    expect(screen.getByText("job_pend...")).toBeInTheDocument();
    expect(screen.getByText("John Doe")).toBeInTheDocument();
    expect(screen.getByText("123 Main St")).toBeInTheDocument();
    expect(screen.getByText("Plumbing")).toBeInTheDocument();
    expect(screen.getByText("Fix leaky faucet")).toBeInTheDocument();
  });

  it("should show 'Get Recommendations' button for pending jobs", () => {
    render(<JobCard job={mockPendingJob} />);

    const getRecButton = screen.getByText(/Get Recommendations/i);
    expect(getRecButton).toBeInTheDocument();
    expect(getRecButton).toHaveClass("bg-blue-50", "text-blue-700");
  });

  it("should not show 'Get Recommendations' button for assigned jobs", () => {
    render(<JobCard job={mockAssignedJob} />);

    const getRecButton = screen.queryByRole("button", {
      name: /Get Recommendations/i,
    });
    expect(getRecButton).not.toBeInTheDocument();
  });

  it("should show ReassignmentFlow component for assigned jobs", () => {
    render(<JobCard job={mockAssignedJob} />);

    const reassignmentFlow = screen.getByTestId("reassignment-flow");
    expect(reassignmentFlow).toBeInTheDocument();
  });

  it("should not show ReassignmentFlow component for pending jobs", () => {
    render(<JobCard job={mockPendingJob} />);

    const reassignmentFlow = screen.queryByTestId("reassignment-flow");
    expect(reassignmentFlow).not.toBeInTheDocument();
  });

  it("should call onGetRecommendations callback when Get Recommendations button clicked", () => {
    const mockOnGetRecommendations = vi.fn();

    render(
      <JobCard
        job={mockPendingJob}
        onGetRecommendations={mockOnGetRecommendations}
      />
    );

    const getRecButton = screen.getByText(/Get Recommendations/i);
    fireEvent.click(getRecButton);

    expect(mockOnGetRecommendations).toHaveBeenCalledWith(mockPendingJob);
  });

  it("should call onClick callback when job card clicked", () => {
    const mockOnClick = vi.fn();

    render(<JobCard job={mockPendingJob} onClick={mockOnClick} />);

    const jobCard = screen.getByRole("article");
    fireEvent.click(jobCard);

    expect(mockOnClick).toHaveBeenCalledWith(mockPendingJob);
  });

  it("should not call onClick when action button is clicked (event stops propagation)", () => {
    const mockOnClick = vi.fn();
    const mockOnGetRecommendations = vi.fn();

    render(
      <JobCard
        job={mockPendingJob}
        onClick={mockOnClick}
        onGetRecommendations={mockOnGetRecommendations}
      />
    );

    const getRecButton = screen.getByText(/Get Recommendations/i);
    fireEvent.click(getRecButton);

    expect(mockOnGetRecommendations).toHaveBeenCalled();
    expect(mockOnClick).not.toHaveBeenCalled();
  });

  it("should display contractor name for assigned jobs", () => {
    render(<JobCard job={mockAssignedJob} />);

    expect(screen.getByText(/Bob's HVAC/)).toBeInTheDocument();
  });

  it("should pass onReassignmentSuccess callback to ReassignmentFlow", () => {
    const mockOnReassignmentSuccess = vi.fn();

    render(
      <JobCard
        job={mockAssignedJob}
        onReassignmentSuccess={mockOnReassignmentSuccess}
      />
    );

    const reassignmentFlow = screen.getByTestId("reassignment-flow");
    fireEvent.click(reassignmentFlow);

    expect(mockOnReassignmentSuccess).toHaveBeenCalled();
  });

  it("should display reassignment history badge when reassignmentCount > 0", () => {
    render(<JobCard job={mockReassignedJob} />);

    const historyBadge = screen.getByTestId("reassignment-history");
    expect(historyBadge).toBeInTheDocument();
    expect(historyBadge).toHaveTextContent("Reassigned 2x");
  });

  it("should not display reassignment history badge when reassignmentCount is 0", () => {
    render(<JobCard job={mockAssignedJob} />);

    const historyBadge = screen.queryByTestId("reassignment-history");
    expect(historyBadge).not.toBeInTheDocument();
  });

  it("should have proper ARIA labels for accessibility", () => {
    render(<JobCard job={mockPendingJob} />);

    const jobCard = screen.getByRole("article");
    expect(jobCard).toHaveAttribute("aria-label");

    const getRecButton = screen.getByText(/Get Recommendations/i);
    expect(getRecButton).toHaveAttribute("aria-label");
  });

  it("should format date and time correctly", () => {
    render(<JobCard job={mockPendingJob} />);

    // Job has desiredDateTime: "2025-12-01T10:00:00Z"
    // Should display date formatted by date-fns (MMM dd, yyyy HH:mm format)
    // Check for the presence of date-formatted text
    expect(screen.getByText(/Dec 01/)).toBeInTheDocument();
  });

  it("should render status badge with correct information", () => {
    render(<JobCard job={mockAssignedJob} />);

    const statusBadge = screen.getByTestId("status-badge");
    expect(statusBadge).toHaveTextContent("Assigned");
    expect(statusBadge).toHaveTextContent("Bob's HVAC");
  });

  it("should have responsive grid layout", () => {
    const { container } = render(<JobCard job={mockPendingJob} />);

    const gridContainer = container.querySelector(".grid");
    expect(gridContainer).toHaveClass("grid-cols-1", "md:grid-cols-5");
  });

  it("should handle missing optional fields gracefully", () => {
    const jobWithMissingFields: Job = {
      ...mockPendingJob,
      customerName: undefined,
      assignedContractorName: undefined,
    };

    render(<JobCard job={jobWithMissingFields} />);

    expect(screen.getByText("Unknown")).toBeInTheDocument(); // fallback for missing customer name
  });

  it("should truncate long job descriptions", () => {
    const jobWithLongDescription: Job = {
      ...mockPendingJob,
      description:
        "This is a very long description that should be truncated to show only the first few lines to keep the job card compact and readable",
    };

    const { container } = render(<JobCard job={jobWithLongDescription} />);

    // Check for line-clamp class
    const descriptionElement = container.querySelector(".line-clamp-2");
    expect(descriptionElement).toBeInTheDocument();
  });

  it("should render correctly for all job statuses", () => {
    const statuses = [
      "Pending",
      "Assigned",
      "InProgress",
      "Completed",
    ] as const;

    statuses.forEach((status) => {
      const job: Job = {
        ...mockPendingJob,
        status,
        currentAssignedContractorId: status !== "Pending" ? "cont_001" : null,
      };

      const { unmount } = render(<JobCard job={job} />);
      expect(screen.getByText(/job_pend/)).toBeInTheDocument();
      unmount();
    });
  });

  it("should show ReassignmentFlow only for Assigned status, not other statuses", () => {
    const { rerender } = render(<JobCard job={mockPendingJob} />);

    expect(screen.queryByTestId("reassignment-flow")).not.toBeInTheDocument();

    rerender(<JobCard job={mockAssignedJob} />);
    expect(screen.getByTestId("reassignment-flow")).toBeInTheDocument();

    const completedJob: Job = {
      ...mockAssignedJob,
      status: "Completed",
    };
    rerender(<JobCard job={completedJob} />);
    expect(screen.queryByTestId("reassignment-flow")).not.toBeInTheDocument();
  });
});
