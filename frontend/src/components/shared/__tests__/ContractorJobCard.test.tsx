/**
 * ContractorJobCard Component Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ContractorJobCard } from "../ContractorJobCard";
import { Assignment } from "@/types/Assignment";

// Mock JobDetailsModal
vi.mock("@/features/contractor/JobDetailsModal", () => ({
  JobDetailsModal: ({ isOpen }: any) =>
    isOpen ? <div data-testid="job-details-modal">Job Details</div> : null,
}));

const mockAssignment: Assignment & {
  jobType?: string;
  location?: string;
  scheduledTime?: string;
  customerName?: string;
} = {
  id: "1",
  jobId: "job-1",
  contractorId: "contractor-1",
  status: "Pending",
  createdAt: new Date().toISOString(),
  jobType: "Plumbing",
  location: "123 Main St",
  scheduledTime: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
  customerName: "John Smith",
};

describe("ContractorJobCard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render job information", () => {
    render(<ContractorJobCard assignment={mockAssignment} />);

    expect(screen.getByText("Plumbing")).toBeInTheDocument();
    expect(screen.getByText("123 Main St")).toBeInTheDocument();
    expect(screen.getByText("John Smith")).toBeInTheDocument();
  });

  it("should display pending status badge", () => {
    render(<ContractorJobCard assignment={mockAssignment} />);

    expect(screen.getByText("Pending")).toBeInTheDocument();
  });

  it("should display scheduled time", () => {
    render(<ContractorJobCard assignment={mockAssignment} />);

    // The date should be formatted
    expect(screen.getByText(/\d+/)).toBeInTheDocument();
  });

  it("should open job details modal when clicked", async () => {
    const user = userEvent.setup();
    render(<ContractorJobCard assignment={mockAssignment} />);

    const button = screen.getByRole("button", { name: /View Details/i });
    await user.click(button);

    expect(screen.getByTestId("job-details-modal")).toBeInTheDocument();
  });

  it("should highlight pending jobs", () => {
    const { container } = render(
      <ContractorJobCard assignment={mockAssignment} />
    );

    const cardDiv = container.querySelector(".bg-yellow-50");
    expect(cardDiv).toBeInTheDocument();
  });

  it("should not highlight non-pending jobs", () => {
    const acceptedAssignment = { ...mockAssignment, status: "Accepted" as const };
    const { container } = render(
      <ContractorJobCard assignment={acceptedAssignment} />
    );

    const cardDiv = container.querySelector(".bg-white");
    expect(cardDiv).toBeInTheDocument();
  });
});



