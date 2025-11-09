/**
 * ReassignmentFlow Component Tests
 * Tests for job reassignment workflow
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { ReassignmentFlow } from "../ReassignmentFlow";
import { Job } from "@/types/Job";
import { useJobReassignment } from "@/hooks/useJobReassignment";
import { useToast } from "@/components/shared/Toast";

// Mock dependencies
vi.mock("@/hooks/useJobReassignment");
vi.mock("@/components/shared/Toast");
vi.mock("../RecommendationsModal", () => ({
  RecommendationsModal: () => (
    <div data-testid="recommendations-modal">Modal</div>
  ),
}));

// Test data
const mockJob: Job = {
  id: "job_123",
  customerId: "cust_001",
  customerName: "John Doe",
  location: "123 Main St",
  desiredDateTime: "2025-12-01T10:00:00Z",
  jobType: "Plumbing",
  description: "Fix leaky faucet",
  status: "Assigned",
  currentAssignedContractorId: "cont_001",
  assignedContractorName: "Original Contractor",
  assignedContractorRating: 4.5,
  createdAt: "2025-11-01T10:00:00Z",
  updatedAt: "2025-11-10T10:00:00Z",
};

const mockPendingJob: Job = {
  ...mockJob,
  status: "Pending",
  currentAssignedContractorId: null,
  assignedContractorName: undefined,
};

describe("ReassignmentFlow Component", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    // Setup default mocks
    vi.mocked(useJobReassignment).mockReturnValue({
      isReassigning: false,
      error: null,
      successMessage: null,
      reassignmentData: null,
      reassignJob: vi.fn(),
      reset: vi.fn(),
      retry: vi.fn(),
    });

    vi.mocked(useToast).mockReturnValue({
      toasts: [],
      removeToast: vi.fn(),
      success: vi.fn(),
      error: vi.fn(),
    });
  });

  it("should render Reassign button when job status is Assigned", () => {
    const { container } = render(<ReassignmentFlow job={mockJob} />);

    const button = container.querySelector('button[aria-label*="Reassign"]');
    expect(button).toBeInTheDocument();
    expect(button).toHaveTextContent("Reassign");
  });

  it("should not render Reassign button when job status is Pending", () => {
    const { container } = render(<ReassignmentFlow job={mockPendingJob} />);

    const button = container.querySelector('button[aria-label*="Reassign"]');
    expect(button).not.toBeInTheDocument();
  });

  it("should render RecommendationsModal component for assigned jobs", () => {
    render(<ReassignmentFlow job={mockJob} />);

    expect(screen.getByTestId("recommendations-modal")).toBeInTheDocument();
  });

  it("should have proper styling on Reassign button", () => {
    const { container } = render(<ReassignmentFlow job={mockJob} />);

    const button = container.querySelector('button[aria-label*="Reassign"]');
    expect(button).toHaveClass("bg-amber-50", "text-amber-700");
  });

  it("should have proper ARIA label", () => {
    const { container } = render(<ReassignmentFlow job={mockJob} />);

    const button = container.querySelector('button[aria-label*="Reassign"]');
    expect(button).toHaveAttribute(
      "aria-label",
      "Reassign this job to a different contractor"
    );
  });

  it("should disable button while reassigning", () => {
    vi.mocked(useJobReassignment).mockReturnValue({
      isReassigning: true,
      error: null,
      successMessage: null,
      reassignmentData: null,
      reassignJob: vi.fn(),
      reset: vi.fn(),
      retry: vi.fn(),
    });

    const { container } = render(<ReassignmentFlow job={mockJob} />);

    const button = container.querySelector('button[aria-label*="Reassign"]');
    expect(button).toBeDisabled();
    expect(button).toHaveTextContent("Reassigning...");
  });
});
