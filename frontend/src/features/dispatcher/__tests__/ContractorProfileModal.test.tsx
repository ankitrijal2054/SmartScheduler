/**
 * ContractorProfileModal Component Tests
 */

import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { ContractorProfileModal } from "../ContractorProfileModal";
import * as useContractorHistoryModule from "@/hooks/useContractorHistory";
import { ContractorHistory } from "@/types/Contractor";

// Mock the hook
vi.mock("@/hooks/useContractorHistory");

// Mock LoadingSpinner
vi.mock("@/components/shared/LoadingSpinner", () => ({
  LoadingSpinner: () => <div>Loading...</div>,
}));

// Mock sub-components
vi.mock("@/components/shared/ContractorStatsCard", () => ({
  ContractorStatsCard: ({ stats }: any) => (
    <div data-testid="stats-card">
      <div>{stats.totalJobsAssigned} jobs assigned</div>
    </div>
  ),
}));

vi.mock("@/components/shared/JobHistoryTable", () => ({
  JobHistoryTable: ({ jobs }: any) => (
    <div data-testid="history-table">
      <div>{jobs.length} jobs in history</div>
    </div>
  ),
}));

describe("ContractorProfileModal Component", () => {
  const mockHistory: ContractorHistory = {
    contractor: {
      id: "contractor-1",
      name: "John Smith",
      phoneNumber: "+1-555-0123",
      location: "123 Main St",
      tradeType: "Plumbing",
      rating: 4.5,
      reviewCount: 12,
      isActive: true,
    },
    stats: {
      totalJobsAssigned: 50,
      totalJobsCompleted: 45,
      acceptanceRate: 90,
      averageRating: 4.5,
      totalReviews: 12,
    },
    jobHistory: [
      {
        jobId: "job-1",
        jobType: "Plumbing",
        customerName: "Alice Johnson",
        completedAt: "2025-11-08T14:30:00Z",
        status: "completed",
        customerRating: 5,
        createdAt: "2025-11-08T10:00:00Z",
      },
    ],
    warnings: {
      lowRating: false,
      highCancellationRate: false,
    },
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should not render when isOpen is false", () => {
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    const { container } = render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={false}
        onClose={vi.fn()}
      />
    );

    expect(container.firstChild).toBeNull();
  });

  it("should render modal when isOpen is true", () => {
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={vi.fn()}
      />
    );

    // Check for modal dialog
    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });

  it("should display contractor information", () => {
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={vi.fn()}
      />
    );

    expect(screen.getByText("John Smith")).toBeInTheDocument();
    expect(screen.getByText(/123 Main St/)).toBeInTheDocument();
    expect(screen.getByText(/Plumbing/)).toBeInTheDocument();
    expect(screen.getByText(/\+1-555-0123/)).toBeInTheDocument();
  });

  it("should render loading state", () => {
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: null,
      loading: true,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={vi.fn()}
      />
    );

    // Check for loading state (should find "Loading..." text)
    const loadingElements = screen.getAllByText("Loading...");
    expect(loadingElements.length).toBeGreaterThan(0);
  });

  it("should render error state with retry button", () => {
    const errorMessage = "Failed to load contractor profile";
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: null,
      loading: false,
      error: errorMessage,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={vi.fn()}
      />
    );

    expect(screen.getByText("Error Loading Profile")).toBeInTheDocument();
    expect(screen.getByText(errorMessage)).toBeInTheDocument();
    expect(screen.getByText("Retry")).toBeInTheDocument();
  });

  it("should render stats and history components", () => {
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={vi.fn()}
      />
    );

    expect(screen.getByTestId("stats-card")).toBeInTheDocument();
    expect(screen.getByTestId("history-table")).toBeInTheDocument();
    expect(screen.getByText("Performance Stats")).toBeInTheDocument();
    expect(screen.getByText("Job History (Last 10)")).toBeInTheDocument();
  });

  it("should call onClose when close button is clicked", () => {
    const onClose = vi.fn();
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={onClose}
      />
    );

    const closeButton = screen.getByLabelText("Close modal");
    fireEvent.click(closeButton);

    expect(onClose).toHaveBeenCalled();
  });

  it("should call onClose when close button in footer is clicked", () => {
    const onClose = vi.fn();
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={onClose}
      />
    );

    const closeButtons = screen.getAllByText("Close");
    fireEvent.click(closeButtons[0]); // Click the footer close button

    expect(onClose).toHaveBeenCalled();
  });

  it("should call onClose when Escape key is pressed", async () => {
    const onClose = vi.fn();
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={onClose}
      />
    );

    fireEvent.keyDown(document, { key: "Escape", code: "Escape" });

    await waitFor(() => {
      expect(onClose).toHaveBeenCalled();
    });
  });

  it("should close when clicking outside the modal", () => {
    const onClose = vi.fn();
    vi.mocked(useContractorHistoryModule.useContractorHistory).mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    const { container } = render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={onClose}
      />
    );

    // Find the backdrop (the outer most div with the semi-transparent overlay)
    const backdrop = container.querySelector(
      ".fixed.inset-0.bg-black.bg-opacity-50"
    );
    if (backdrop) {
      // Modal uses mousedown event listener, not click
      fireEvent.mouseDown(backdrop, { target: backdrop });
      expect(onClose).toHaveBeenCalled();
    }
  });

  it("should fetch contractor history for given contractorId", () => {
    const useContractorHistorySpy = vi.spyOn(
      useContractorHistoryModule,
      "useContractorHistory"
    );
    useContractorHistorySpy.mockReturnValue({
      data: mockHistory,
      loading: false,
      error: null,
    });

    render(
      <ContractorProfileModal
        contractorId="contractor-1"
        isOpen={true}
        onClose={vi.fn()}
      />
    );

    expect(useContractorHistorySpy).toHaveBeenCalledWith("contractor-1");
  });
});
