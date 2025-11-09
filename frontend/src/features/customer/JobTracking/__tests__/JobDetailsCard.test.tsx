/**
 * JobDetailsCard Component Tests
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { JobDetailsCard } from "../JobDetailsCard";
import { JobDetail } from "@/types/Job";

describe("JobDetailsCard Component", () => {
  const mockJob: JobDetail = {
    id: "job_123",
    customerId: "cust_1",
    location: "123 Main St, Springfield, IL",
    desiredDateTime: "2025-11-15T10:00:00Z",
    jobType: "Plumbing",
    description: "Fix broken kitchen pipe and leaky faucet",
    status: "InProgress",
    currentAssignedContractorId: "contractor_1",
    createdAt: "2025-11-07T15:30:00Z",
    updatedAt: "2025-11-07T16:20:00Z",
    assignment: {
      id: "assign_1",
      contractorId: "contractor_1",
      status: "Accepted",
      assignedAt: "2025-11-07T15:45:00Z",
      acceptedAt: "2025-11-07T16:00:00Z",
      completedAt: null,
      estimatedArrivalTime: "2025-11-15T10:30:00Z",
    },
  };

  it("should render job details card", () => {
    render(<JobDetailsCard job={mockJob} />);

    expect(screen.getByText("Job Details")).toBeInTheDocument();
  });

  it("should display all job fields", () => {
    render(<JobDetailsCard job={mockJob} />);

    expect(screen.getByText(/Plumbing/i)).toBeInTheDocument();
    expect(
      screen.getByText("123 Main St, Springfield, IL")
    ).toBeInTheDocument();
    expect(
      screen.getByText("Fix broken kitchen pipe and leaky faucet")
    ).toBeInTheDocument();
  });

  it("should format dates correctly", () => {
    render(<JobDetailsCard job={mockJob} />);

    // Should display date in human-readable format
    const dateElements = screen.getAllByText(/Nov \d+/i);
    expect(dateElements.length).toBeGreaterThan(0);
  });

  it("should show assignment details when available", () => {
    render(<JobDetailsCard job={mockJob} />);

    expect(screen.getByText("Assignment Details")).toBeInTheDocument();
    expect(screen.getByText("Accepted")).toBeInTheDocument();
  });

  it("should display ETA when available", () => {
    render(<JobDetailsCard job={mockJob} />);

    expect(screen.getByText("Estimated Arrival Time")).toBeInTheDocument();
  });

  it("should display job type with emoji", () => {
    render(<JobDetailsCard job={mockJob} />);

    expect(screen.getByText(/🔧 Plumbing/)).toBeInTheDocument();
  });

  it("should handle missing description", () => {
    const jobWithoutDescription = { ...mockJob, description: "" };

    render(<JobDetailsCard job={jobWithoutDescription} />);

    expect(screen.getByText("No description provided")).toBeInTheDocument();
  });

  it("should display job ID snippet", () => {
    render(<JobDetailsCard job={mockJob} />);

    expect(screen.getByText(/Job ID: job_123.../)).toBeInTheDocument();
  });

  it("should show created and updated timestamps", () => {
    render(<JobDetailsCard job={mockJob} />);

    const timestampTexts = screen.getAllByText(/Created:|Updated:/);
    expect(timestampTexts.length).toBe(2);
  });

  it("should not show completed date if not available", () => {
    render(<JobDetailsCard job={mockJob} />);

    // Completed date should not be in document
    const completedLabels = screen.queryAllByText("Completed Date");
    expect(completedLabels.length).toBe(0);
  });

  it("should show completed date if available", () => {
    const jobCompleted = {
      ...mockJob,
      assignment: {
        ...mockJob.assignment!,
        completedAt: "2025-11-15T14:30:00Z",
      },
    };

    render(<JobDetailsCard job={jobCompleted} />);

    expect(screen.getByText("Completed Date")).toBeInTheDocument();
  });

  it("should show accepted date if available", () => {
    render(<JobDetailsCard job={mockJob} />);

    expect(screen.getByText("Accepted Date")).toBeInTheDocument();
  });

  it("should render without assignment info when not assigned", () => {
    const jobUnassigned = { ...mockJob, assignment: undefined };

    render(<JobDetailsCard job={jobUnassigned} />);

    expect(screen.queryByText("Assignment Details")).not.toBeInTheDocument();
  });

  it("should display correct job type labels for different types", () => {
    const jobTypes = ["Flooring", "HVAC", "Plumbing", "Electrical"];

    jobTypes.forEach((type) => {
      const job = { ...mockJob, jobType: type as any };
      const { unmount } = render(<JobDetailsCard job={job} />);

      expect(screen.getByText(new RegExp(type))).toBeInTheDocument();
      unmount();
    });
  });

  it("should have responsive design classes", () => {
    const { container } = render(<JobDetailsCard job={mockJob} />);

    // Check for responsive classes
    const responsiveElements = container.querySelectorAll("[class*='sm:']");
    expect(responsiveElements.length).toBeGreaterThan(0);
  });

  it("should display location correctly", () => {
    render(<JobDetailsCard job={mockJob} />);

    const locationElement = screen.getByText("123 Main St, Springfield, IL");
    expect(locationElement).toBeInTheDocument();
  });
});
