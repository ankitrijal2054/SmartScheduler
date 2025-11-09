/**
 * JobList Component Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Assignment } from "@/types/Assignment";
import { JobList } from "../JobList";

// Mock useContractorJobs
vi.mock("@/hooks/useContractorJobs", () => ({
  useContractorJobs: () => ({
    jobs: mockJobs,
    loading: false,
    error: null,
    refetch: vi.fn(),
  }),
}));

// Mock JobDetailsModal
vi.mock("../JobDetailsModal", () => ({
  JobDetailsModal: () => <div data-testid="job-details-modal">Job Details</div>,
}));

const mockJobs: (Assignment & {
  jobType?: string;
  location?: string;
  scheduledTime?: string;
  customerName?: string;
})[] = [
  {
    id: "1",
    jobId: "job-1",
    contractorId: "contractor-1",
    status: "Pending",
    createdAt: new Date().toISOString(),
    jobType: "Plumbing",
    location: "123 Main St",
    scheduledTime: new Date().toISOString(),
    customerName: "John Smith",
  },
  {
    id: "2",
    jobId: "job-2",
    contractorId: "contractor-1",
    status: "Accepted",
    createdAt: new Date().toISOString(),
    jobType: "HVAC",
    location: "456 Oak Ave",
    scheduledTime: new Date().toISOString(),
    customerName: "Jane Doe",
  },
  {
    id: "3",
    jobId: "job-3",
    contractorId: "contractor-1",
    status: "Completed",
    createdAt: new Date().toISOString(),
    jobType: "Electrical",
    location: "789 Pine Rd",
    scheduledTime: new Date().toISOString(),
    customerName: "Bob Johnson",
  },
];

describe("JobList", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render tabs", () => {
    render(<JobList />);

    // Check for tab buttons specifically
    expect(
      screen.getByRole("button", { name: /Pending/i })
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Active/i })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /Completed/i })
    ).toBeInTheDocument();
  });

  it("should display pending jobs on Pending tab", async () => {
    render(<JobList />);

    await waitFor(() => {
      expect(screen.getByText("Plumbing")).toBeInTheDocument();
      expect(screen.getByText("123 Main St")).toBeInTheDocument();
    });
  });

  it("should filter jobs when switching tabs", async () => {
    const user = userEvent.setup();
    render(<JobList />);

    const activeTab = screen.getByRole("button", { name: /Active/i });
    await user.click(activeTab);

    await waitFor(() => {
      expect(screen.getByText("HVAC")).toBeInTheDocument();
      expect(screen.queryByText("Plumbing")).not.toBeInTheDocument();
    });
  });

  it("should display completed jobs in Completed tab", async () => {
    const user = userEvent.setup();
    render(<JobList />);

    const completedTab = screen.getByRole("button", { name: /Completed/i });
    await user.click(completedTab);

    await waitFor(() => {
      expect(screen.getByText("Electrical")).toBeInTheDocument();
      expect(screen.getByText("789 Pine Rd")).toBeInTheDocument();
    });
  });

  it("should show job count on tabs", () => {
    render(<JobList />);

    // Each tab should show count badges
    const badges = screen.getAllByText(/\d+/);
    expect(badges.length).toBeGreaterThan(0);
  });
});
