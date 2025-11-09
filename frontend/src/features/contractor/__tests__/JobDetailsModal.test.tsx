/**
 * JobDetailsModal Component Tests
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import {
  render,
  screen,
  fireEvent,
  waitFor,
  within,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { JobDetailsModal } from "../JobDetailsModal";
import { JobDetails } from "@/types/JobDetails";

// Mock hooks
vi.mock("@/hooks/useJobDetails", () => ({
  useJobDetails: vi.fn(),
}));

vi.mock("@/hooks/useAcceptDeclineJob", () => ({
  useAcceptDeclineJob: vi.fn(),
}));

// Mock components
vi.mock("@/components/shared/JobInfoSection", () => ({
  JobInfoSection: ({ jobDetails }: any) => (
    <div data-testid="job-info-section">{jobDetails.jobType}</div>
  ),
}));

vi.mock("@/features/contractor/CustomerProfileCard", () => ({
  CustomerProfileCard: ({ customer }: any) => (
    <div data-testid="customer-profile">{customer.name}</div>
  ),
}));

vi.mock("@/features/contractor/DeclineReasonModal", () => ({
  DeclineReasonModal: ({ isOpen, onConfirm, onCancel }: any) =>
    isOpen ? (
      <div data-testid="decline-modal">
        <button onClick={() => onConfirm("Scheduling conflict")}>
          Confirm Decline
        </button>
        <button onClick={onCancel}>Cancel Decline</button>
      </div>
    ) : null,
}));

vi.mock("@/components/shared/LoadingSpinner", () => ({
  LoadingSpinner: ({ size }: any) => (
    <div data-testid="loading-spinner">{size}</div>
  ),
}));

vi.mock("@/components/shared/Toast", () => ({
  Toast: ({ message, onClose }: any) => (
    <div data-testid="toast">{message}</div>
  ),
}));

import { useJobDetails } from "@/hooks/useJobDetails";
import { useAcceptDeclineJob } from "@/hooks/useAcceptDeclineJob";

const mockJobDetails: JobDetails = {
  assignmentId: "assign-1",
  status: "Pending",
  assignedAt: new Date().toISOString(),
  jobId: "job-1",
  jobType: "HVAC",
  location: "123 Main St, Denver, CO",
  desiredDateTime: new Date().toISOString(),
  description: "AC not cooling properly",
  estimatedDuration: 120,
  estimatedPay: 150,
  customer: {
    id: "cust-1",
    name: "John Smith",
    rating: 4.5,
    reviewCount: 12,
  },
  pastReviews: [],
};

describe("JobDetailsModal", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (useJobDetails as any).mockReturnValue({
      jobDetails: null,
      loading: false,
      error: null,
      refetch: () => {},
    });
    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: false,
      error: null,
      acceptJob: () => {},
      declineJob: () => {},
    });
  });

  it("should not render when isOpen is false", () => {
    const { container } = render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={false}
        onClose={() => {}}
      />
    );

    expect(container.innerHTML).toBe("");
  });

  it("should render loading spinner while fetching", () => {
    (useJobDetails as any).mockReturnValue({
      jobDetails: null,
      loading: true,
      error: null,
      refetch: () => {},
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    expect(screen.getByTestId("loading-spinner")).toBeInTheDocument();
  });

  it("should render error state with retry button", () => {
    const mockRefetch = vi.fn();
    (useJobDetails as any).mockReturnValue({
      jobDetails: null,
      loading: false,
      error: "Failed to load job details",
      refetch: mockRefetch,
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    expect(screen.getByText("Failed to load job details")).toBeInTheDocument();
    const retryButton = screen.getByRole("button", { name: /retry/i });
    expect(retryButton).toBeInTheDocument();
  });

  it("should render job details when data is loaded", () => {
    (useJobDetails as any).mockReturnValue({
      jobDetails: mockJobDetails,
      loading: false,
      error: null,
      refetch: () => {},
    });

    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: false,
      error: null,
      acceptJob: () => {},
      declineJob: () => {},
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    expect(screen.getByText("Job Details")).toBeInTheDocument();
    expect(screen.getByTestId("job-info-section")).toBeInTheDocument();
    expect(screen.getByTestId("customer-profile")).toBeInTheDocument();
  });

  it("should show Accept and Decline buttons when status is Pending", () => {
    (useJobDetails as any).mockReturnValue({
      jobDetails: mockJobDetails,
      loading: false,
      error: null,
      refetch: () => {},
    });

    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: false,
      error: null,
      acceptJob: () => {},
      declineJob: () => {},
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    expect(screen.getByRole("button", { name: /accept/i })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /decline/i })
    ).toBeInTheDocument();
  });

  it("should call acceptJob when Accept button is clicked", async () => {
    const mockAcceptJob = vi.fn().mockResolvedValue({});
    (useJobDetails as any).mockReturnValue({
      jobDetails: mockJobDetails,
      loading: false,
      error: null,
      refetch: () => {},
    });

    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: false,
      error: null,
      acceptJob: mockAcceptJob,
      declineJob: () => {},
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    const acceptButton = screen.getByRole("button", { name: /accept/i });
    fireEvent.click(acceptButton);

    await waitFor(() => {
      expect(mockAcceptJob).toHaveBeenCalledWith("assign-1");
    });
  });

  it("should show decline modal when Decline button is clicked", async () => {
    (useJobDetails as any).mockReturnValue({
      jobDetails: mockJobDetails,
      loading: false,
      error: null,
      refetch: () => {},
    });

    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: false,
      error: null,
      acceptJob: () => {},
      declineJob: () => {},
    });

    const user = userEvent.setup();
    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    const declineButton = screen.getByRole("button", { name: /decline/i });
    await user.click(declineButton);

    expect(screen.getByTestId("decline-modal")).toBeInTheDocument();
  });

  it("should close modal when Close button is clicked", async () => {
    const mockOnClose = vi.fn();
    (useJobDetails as any).mockReturnValue({
      jobDetails: mockJobDetails,
      loading: false,
      error: null,
      refetch: () => {},
    });

    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: false,
      error: null,
      acceptJob: () => {},
      declineJob: () => {},
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={mockOnClose}
      />
    );

    const closeButton = screen.getByRole("button", {
      name: /close without action/i,
    });
    fireEvent.click(closeButton);

    expect(mockOnClose).toHaveBeenCalled();
  });

  it("should show status label when not Pending", () => {
    const acceptedJobDetails = {
      ...mockJobDetails,
      status: "Accepted" as const,
    };

    (useJobDetails as any).mockReturnValue({
      jobDetails: acceptedJobDetails,
      loading: false,
      error: null,
      refetch: () => {},
    });

    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: false,
      error: null,
      acceptJob: () => {},
      declineJob: () => {},
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    expect(screen.getByText(/Status:/i)).toBeInTheDocument();
    expect(screen.getByText(/Accepted/i)).toBeInTheDocument();
  });

  it("should disable buttons during loading", () => {
    (useJobDetails as any).mockReturnValue({
      jobDetails: mockJobDetails,
      loading: false,
      error: null,
      refetch: () => {},
    });

    (useAcceptDeclineJob as any).mockReturnValue({
      isLoading: true,
      error: null,
      acceptJob: () => {},
      declineJob: () => {},
    });

    render(
      <JobDetailsModal
        assignmentId="assign-1"
        isOpen={true}
        onClose={() => {}}
      />
    );

    const acceptButton = screen.getByRole("button", { name: /accept/i });
    const declineButton = screen.getByRole("button", { name: /decline/i });

    expect(acceptButton).toBeDisabled();
    expect(declineButton).toBeDisabled();
  });
});
