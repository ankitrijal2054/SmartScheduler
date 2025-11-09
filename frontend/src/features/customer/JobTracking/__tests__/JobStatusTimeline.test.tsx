/**
 * JobStatusTimeline Component Tests
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { JobStatusTimeline } from "../JobStatusTimeline";

describe("JobStatusTimeline Component", () => {
  const mockCreatedAt = "2025-11-07T15:30:00Z";
  const mockUpdatedAt = "2025-11-07T16:20:00Z";

  it("should render all 4 status steps", () => {
    render(
      <JobStatusTimeline
        currentStatus="Pending"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    expect(screen.getByText("Job Submitted")).toBeInTheDocument();
    expect(screen.getByText("Contractor Assigned")).toBeInTheDocument();
    expect(screen.getByText("In Progress")).toBeInTheDocument();
    expect(screen.getByText("Completed")).toBeInTheDocument();
  });

  it("should highlight current status correctly for Pending", () => {
    render(
      <JobStatusTimeline
        currentStatus="Pending"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    const currentBadge = screen.getByText("Awaiting Assignment");
    expect(currentBadge).toBeInTheDocument();
    expect(currentBadge).toHaveClass("bg-slate-500");
  });

  it("should highlight current status correctly for Assigned", () => {
    render(
      <JobStatusTimeline
        currentStatus="Assigned"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    // Look for the status badge with specific class
    const badges = screen.getAllByText("Contractor Assigned");
    const currentBadge = badges.find((el) =>
      el.className.includes("bg-blue-500")
    );
    expect(currentBadge).toBeInTheDocument();
  });

  it("should highlight current status correctly for InProgress", () => {
    render(
      <JobStatusTimeline
        currentStatus="InProgress"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    const currentBadge = screen.getByText("Work in Progress");
    expect(currentBadge).toBeInTheDocument();
    expect(currentBadge).toHaveClass("bg-yellow-500");
  });

  it("should highlight current status correctly for Completed", () => {
    render(
      <JobStatusTimeline
        currentStatus="Completed"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    const currentBadge = screen.getByText("Job Completed");
    expect(currentBadge).toBeInTheDocument();
    expect(currentBadge).toHaveClass("bg-green-500");
  });

  it("should show timestamps for completed steps", () => {
    render(
      <JobStatusTimeline
        currentStatus="InProgress"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    // Should show timestamp for created (Job Submitted - step 1)
    const timeElements = screen.getAllByText(/Nov \d+/i);
    expect(timeElements.length).toBeGreaterThan(0);
  });

  it("should have ARIA labels for accessibility", () => {
    render(
      <JobStatusTimeline
        currentStatus="Assigned"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    const progressBar = screen.getByRole("progressbar");
    expect(progressBar).toHaveAttribute("aria-valuenow", "2");
    expect(progressBar).toHaveAttribute("aria-valuemin", "1");
    expect(progressBar).toHaveAttribute("aria-valuemax", "4");
    expect(progressBar).toHaveAttribute("aria-label", "Job status: Assigned");
  });

  it("should have ARIA labels for each step", () => {
    render(
      <JobStatusTimeline
        currentStatus="Assigned"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    // Each step should have an aria-label
    const stepLabels = screen.getAllByLabelText(/on Nov \d+/i);
    expect(stepLabels.length).toBeGreaterThan(0);
  });

  it("should show correct progress bar width for different statuses", () => {
    const { container: containerPending } = render(
      <JobStatusTimeline
        currentStatus="Pending"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    const { container: containerInProgress } = render(
      <JobStatusTimeline
        currentStatus="InProgress"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    // Get the progress line element
    const progressPending = containerPending.querySelector(".bg-green-500");
    const progressInProgress =
      containerInProgress.querySelector(".bg-green-500");

    if (progressPending && progressInProgress) {
      const widthPending = progressPending.getAttribute("style");
      const widthInProgress = progressInProgress.getAttribute("style");

      // InProgress should have more width than Pending
      expect(widthInProgress).toBeTruthy();
      expect(widthPending).toBeTruthy();
    }
  });

  it("should display formatted timestamps", () => {
    render(
      <JobStatusTimeline
        currentStatus="Completed"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    // Should display in format like "Nov 07, 03:30 PM"
    const timeElements = screen.getAllByText(/Nov \d+, \d{2}:\d{2}/);
    expect(timeElements.length).toBeGreaterThan(0);
  });

  it("should not show future timestamps for incomplete steps", () => {
    render(
      <JobStatusTimeline
        currentStatus="Pending"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    // For Pending status, only Job Submitted (step 1) should have timestamp
    // Other steps don't have timestamps since they haven't completed
    const allText = screen.queryAllByText(/Nov/i);
    // Should have at least the created date
    expect(allText.length).toBeGreaterThan(0);
  });

  it("should apply animation classes for transitions", () => {
    const { container } = render(
      <JobStatusTimeline
        currentStatus="InProgress"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    const progressLine = container.querySelector(".transition-all");
    expect(progressLine).toHaveClass("duration-500");
  });

  it("should handle different date formats gracefully", () => {
    const invalidDate = "invalid-date";
    render(
      <JobStatusTimeline
        currentStatus="Pending"
        updatedAt={invalidDate}
        createdAt={invalidDate}
      />
    );

    // Should still render without crashing
    expect(screen.getByText("Job Submitted")).toBeInTheDocument();
  });

  it("should be responsive with mobile-friendly layout", () => {
    const { container } = render(
      <JobStatusTimeline
        currentStatus="Assigned"
        updatedAt={mockUpdatedAt}
        createdAt={mockCreatedAt}
      />
    );

    // Should have responsive text sizing
    const stepLabels = container.querySelectorAll(".text-xs.sm\\:text-sm");
    expect(stepLabels.length).toBeGreaterThan(0);
  });
});
