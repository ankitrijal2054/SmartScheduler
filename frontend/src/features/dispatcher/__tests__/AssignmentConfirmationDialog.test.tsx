/**
 * AssignmentConfirmationDialog Unit Tests
 * Tests for the assignment confirmation dialog component
 */

import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { AssignmentConfirmationDialog } from "../AssignmentConfirmationDialog";
import { RecommendedContractor } from "@/types/Contractor";
import { Job } from "@/types/Job";

// Mock contractors
const mockContractor: RecommendedContractor = {
  contractorId: "cont_001",
  name: "John Smith",
  rank: 1,
  score: 0.95,
  avgRating: 4.8,
  reviewCount: 25,
  distance: 2.5,
  travelTime: 15,
  tradeType: "Plumbing",
  availableTimeSlots: [
    {
      startTime: "2025-11-10T10:00:00Z",
      endTime: "2025-11-10T12:00:00Z",
    },
  ],
};

// Mock job
const mockJob: Job = {
  id: "job_001",
  customerId: "cust_001",
  customerName: "Jane Doe",
  location: "123 Main St, Austin, TX",
  desiredDateTime: "2025-11-10T10:30:00Z",
  jobType: "Plumbing",
  description: "Fix kitchen sink",
  status: "Pending",
  currentAssignedContractorId: null,
  createdAt: "2025-11-10T08:00:00Z",
  updatedAt: "2025-11-10T08:00:00Z",
};

describe("AssignmentConfirmationDialog", () => {
  it("renders when isOpen is true", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(
      screen.getByRole("alertdialog", { name: /confirm assignment/i })
    ).toBeInTheDocument();
  });

  it("does not render when isOpen is false", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    const { container } = render(
      <AssignmentConfirmationDialog
        isOpen={false}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(
      container.querySelector('[role="alertdialog"]')
    ).not.toBeInTheDocument();
  });

  it("displays contractor and job details", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    // Check contractor details (using more specific patterns to avoid duplicates)
    expect(screen.getByText(/4\.8 \(25 reviews\)/)).toBeInTheDocument();
    expect(screen.getByText(/2\.5 miles away/)).toBeInTheDocument();
    expect(screen.getByText(/15 min travel time/)).toBeInTheDocument();

    // Check job details
    expect(screen.getByText(/Plumbing/)).toBeInTheDocument();
    expect(screen.getByText(/123 Main St, Austin, TX/)).toBeInTheDocument();
  });

  it("calls onCancel when cancel button is clicked", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    const cancelButton = screen.getByRole("button", { name: /cancel/i });
    fireEvent.click(cancelButton);

    expect(mockOnCancel).toHaveBeenCalledTimes(1);
  });

  it("calls onConfirm when confirm button is clicked", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    const confirmButton = screen.getByRole("button", { name: /confirm/i });
    fireEvent.click(confirmButton);

    expect(mockOnConfirm).toHaveBeenCalledTimes(1);
  });

  it("shows loading spinner while assigning", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={true}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(screen.getByText(/assigning\.\.\./i)).toBeInTheDocument();
  });

  it("disables buttons while assigning", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={true}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    const buttons = screen.getAllByRole("button");
    expect(buttons.length).toBeGreaterThan(0);

    // All buttons should be disabled while assigning
    buttons.forEach((button) => {
      expect(button).toBeDisabled();
    });
  });

  it("displays error message when error is present", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();
    const errorMessage = "Contractor no longer available";

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error={errorMessage}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(screen.getByText(errorMessage)).toBeInTheDocument();
  });

  it("shows retry button when error is present", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error="Assignment failed"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(screen.getByRole("button", { name: /retry/i })).toBeInTheDocument();
  });

  it("calls onRetry when retry button is clicked on error", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error="Assignment failed"
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    const retryButton = screen.getByRole("button", { name: /retry/i });
    fireEvent.click(retryButton);

    expect(mockOnRetry).toHaveBeenCalledTimes(1);
  });

  it("closes dialog on Escape key when not assigning", async () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    fireEvent.keyDown(document, { key: "Escape" });

    await waitFor(() => {
      expect(mockOnCancel).toHaveBeenCalled();
    });
  });

  it("does not close dialog on Escape key when assigning", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={mockJob}
        isAssigning={true}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    fireEvent.keyDown(document, { key: "Escape" });

    expect(mockOnCancel).not.toHaveBeenCalled();
  });

  it("handles null contractor gracefully", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    const { container } = render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={null}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(
      container.querySelector('[role="alertdialog"]')
    ).not.toBeInTheDocument();
  });

  it("handles null job gracefully", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();

    const { container } = render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={mockContractor}
        job={null}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(
      container.querySelector('[role="alertdialog"]')
    ).not.toBeInTheDocument();
  });

  it("displays contractor with no ratings correctly", () => {
    const mockOnConfirm = vi.fn();
    const mockOnCancel = vi.fn();
    const mockOnRetry = vi.fn();
    const contractorNoRating = {
      ...mockContractor,
      avgRating: null,
      reviewCount: 0,
    };

    render(
      <AssignmentConfirmationDialog
        isOpen={true}
        contractor={contractorNoRating}
        job={mockJob}
        isAssigning={false}
        error={null}
        onConfirm={mockOnConfirm}
        onCancel={mockOnCancel}
        onRetry={mockOnRetry}
      />
    );

    expect(screen.getByText(/no ratings yet/i)).toBeInTheDocument();
  });
});
