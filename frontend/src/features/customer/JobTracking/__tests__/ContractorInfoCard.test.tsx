/**
 * ContractorInfoCard Component Tests
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { ContractorInfoCard } from "../ContractorInfoCard";
import { Contractor } from "@/types/Contractor";

describe("ContractorInfoCard Component", () => {
  const mockContractor: Contractor & {
    phoneNumber?: string;
    averageRating?: number;
  } = {
    id: "contractor_1",
    name: "John Smith",
    phoneNumber: "555-123-4567",
    averageRating: 4.8,
    reviewCount: 24,
    rating: 4.8,
    location: "Springfield, IL",
    tradeType: "Plumbing",
    isActive: true,
  };

  it("should render contractor info when job is assigned", () => {
    render(
      <ContractorInfoCard
        contractor={mockContractor}
        jobStatus="Assigned"
        estimatedArrivalTime="2025-11-15T10:30:00Z"
      />
    );

    expect(screen.getByText("Assigned Contractor")).toBeInTheDocument();
    expect(screen.getByText("John Smith")).toBeInTheDocument();
  });

  it("should show not assigned message when job is pending", () => {
    render(<ContractorInfoCard contractor={undefined} jobStatus="Pending" />);

    expect(
      screen.getByText("Waiting for contractor assignment...")
    ).toBeInTheDocument();
  });

  it("should display contractor name correctly", () => {
    render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    expect(screen.getByText("John Smith")).toBeInTheDocument();
  });

  it("should display rating as stars", () => {
    render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    expect(screen.getByText("4.8/5")).toBeInTheDocument();
  });

  it("should display review count", () => {
    render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    expect(screen.getByText(/24 reviews/)).toBeInTheDocument();
  });

  it("should make phone number clickable with tel link", () => {
    render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    const phoneLink = screen.getByRole("link");
    expect(phoneLink).toHaveAttribute("href", "tel:555-123-4567");
  });

  it("should format phone number correctly", () => {
    render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    expect(screen.getByText("(555) 123-4567")).toBeInTheDocument();
  });

  it("should show ETA when available", () => {
    render(
      <ContractorInfoCard
        contractor={mockContractor}
        jobStatus="InProgress"
        estimatedArrivalTime="2025-11-15T10:30:00Z"
      />
    );

    expect(screen.getByText("Estimated Arrival Time")).toBeInTheDocument();
  });

  it("should not show ETA when not available", () => {
    render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    expect(
      screen.queryByText("Estimated Arrival Time")
    ).not.toBeInTheDocument();
  });

  it("should show different status badges for different job statuses", () => {
    const { rerender } = render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    expect(screen.getByText("Assigned")).toBeInTheDocument();

    rerender(
      <ContractorInfoCard contractor={mockContractor} jobStatus="InProgress" />
    );

    expect(screen.getByText("In Progress")).toBeInTheDocument();

    rerender(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Completed" />
    );

    expect(screen.getByText("Completed")).toBeInTheDocument();
  });

  it("should hide contractor info for pending status", () => {
    render(<ContractorInfoCard contractor={undefined} jobStatus="Pending" />);

    expect(screen.queryByText("John Smith")).not.toBeInTheDocument();
  });

  it("should handle contractor with no rating gracefully", () => {
    const contractorNoRating = { ...mockContractor, averageRating: undefined };

    render(
      <ContractorInfoCard
        contractor={contractorNoRating}
        jobStatus="Assigned"
      />
    );

    // Should not show rating section
    expect(screen.queryByText(/\d+\.\d\/5/)).not.toBeInTheDocument();
  });

  it("should handle contractor with no phone number", () => {
    const contractorNoPhone = { ...mockContractor, phoneNumber: undefined };

    render(
      <ContractorInfoCard contractor={contractorNoPhone} jobStatus="Assigned" />
    );

    expect(screen.queryByRole("link")).not.toBeInTheDocument();
  });

  it("should display contractor info for all assigned statuses", () => {
    ["Assigned", "InProgress", "Completed"].forEach((status) => {
      const { unmount } = render(
        <ContractorInfoCard contractor={mockContractor} jobStatus={status} />
      );

      expect(screen.getByText("Assigned Contractor")).toBeInTheDocument();
      expect(screen.getByText("John Smith")).toBeInTheDocument();
      unmount();
    });
  });

  it("should have responsive text sizing", () => {
    const { container } = render(
      <ContractorInfoCard contractor={mockContractor} jobStatus="Assigned" />
    );

    // Check for responsive classes
    const responsiveElements = container.querySelectorAll("[class*='sm:']");
    expect(responsiveElements.length).toBeGreaterThan(0);
  });

  it("should render ETA in highlighted box", () => {
    const { container } = render(
      <ContractorInfoCard
        contractor={mockContractor}
        jobStatus="InProgress"
        estimatedArrivalTime="2025-11-15T10:30:00Z"
      />
    );

    const etaBox = container.querySelector(".bg-amber-50");
    expect(etaBox).toBeInTheDocument();
  });

  it("should show zero rating correctly", () => {
    const contractorZeroRating = {
      ...mockContractor,
      averageRating: 0,
      reviewCount: 5,
    };

    render(
      <ContractorInfoCard
        contractor={contractorZeroRating}
        jobStatus="Assigned"
      />
    );

    expect(screen.getByText("0.0/5")).toBeInTheDocument();
  });

  it("should format datetime in ETA correctly", () => {
    render(
      <ContractorInfoCard
        contractor={mockContractor}
        jobStatus="InProgress"
        estimatedArrivalTime="2025-11-15T10:30:00Z"
      />
    );

    // Should display formatted date/time
    const timeElements = screen.getAllByText(/Nov \d+/i);
    expect(timeElements.length).toBeGreaterThan(0);
  });
});
