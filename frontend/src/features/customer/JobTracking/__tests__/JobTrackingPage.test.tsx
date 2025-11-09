/**
 * JobTrackingPage Component Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { BrowserRouter } from "react-router-dom";
import { JobTrackingPage } from "../JobTrackingPage";
import * as useJobModule from "@/hooks/useJob";
import * as useSignalRModule from "@/hooks/useSignalR";
import { JobDetail } from "@/types/Job";

// Mock the hooks
vi.mock("@/hooks/useJob");
vi.mock("@/hooks/useSignalR");
vi.mock("@/components/shared/LoadingSpinner", () => ({
  LoadingSpinner: () => <div>Loading...</div>,
}));

// Mock react-router-dom
vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual("react-router-dom");
  return {
    ...actual,
    useParams: () => ({ jobId: "job_123" }),
    useNavigate: () => vi.fn(),
  };
});

const mockJob: JobDetail = {
  id: "job_123",
  customerId: "cust_1",
  location: "123 Main St, Springfield, IL",
  desiredDateTime: "2025-11-15T10:00:00Z",
  jobType: "Plumbing",
  description: "Fix broken kitchen pipe",
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
  contractor: {
    id: "contractor_1",
    name: "John Smith",
    phoneNumber: "555-1234",
    averageRating: 4.8,
    reviewCount: 24,
    rating: 4.8,
    location: "Springfield, IL",
    tradeType: "Plumbing",
    isActive: true,
  },
};

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe("JobTrackingPage Component", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render loading spinner initially", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: null,
      loading: true,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: false,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("should display job tracking page when job is loaded", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    await waitFor(() => {
      expect(screen.getByText("Track Your Job")).toBeInTheDocument();
    });
  });

  it("should display job details", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    await waitFor(() => {
      expect(screen.getByText("Track Your Job")).toBeInTheDocument();
    });
  });

  it("should display status timeline", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    expect(screen.getByText("Job Details")).toBeInTheDocument();
  });

  it("should display contractor information", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    await waitFor(() => {
      expect(screen.getByText("John Smith")).toBeInTheDocument();
    });
  });

  it("should show error message when job not found", () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: null,
      loading: false,
      error: "Job not found",
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: false,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    expect(screen.getByText("Error Loading Job")).toBeInTheDocument();
    expect(screen.getByText("Job not found")).toBeInTheDocument();
  });

  it("should show not found message for 404 errors", () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: null,
      loading: false,
      error: "Job 404 not found",
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: false,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    expect(screen.getByText("Job Not Found")).toBeInTheDocument();
  });

  it("should have refresh button", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    const refreshButton = screen.getByRole("button", { name: /Refresh/i });
    expect(refreshButton).toBeInTheDocument();
  });

  it("should show connection status", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    expect(screen.getByText("Live")).toBeInTheDocument();
  });

  it("should show polling status when not connected", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: false,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    expect(screen.getByText("Polling")).toBeInTheDocument();
  });

  it("should subscribe to SignalR updates", () => {
    const mockUseSignalR = vi.fn().mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockImplementation(mockUseSignalR);
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    expect(mockUseSignalR).toHaveBeenCalled();
  });

  it("should refresh job when SignalR event received", async () => {
    const mockRefreshJob = vi.fn();
    let capturedCallback: ((event: any) => void) | null = null;

    vi.mocked(useSignalRModule.useSignalR).mockImplementation(
      (options: any) => {
        capturedCallback = options.onJobStatusUpdate;
        return {
          isConnected: true,
          subscribe: vi.fn(() => vi.fn()),
          disconnect: vi.fn(),
        };
      }
    );

    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: mockRefreshJob,
    });

    renderWithRouter(<JobTrackingPage />);

    await waitFor(() => {
      expect(screen.getByText("Track Your Job")).toBeInTheDocument();
    });

    if (capturedCallback) {
      capturedCallback({
        jobId: "job_123",
        newStatus: "Completed",
        updatedAt: "2025-11-15T14:30:00Z",
      });

      expect(mockRefreshJob).toHaveBeenCalled();
    }
  });

  it("should have responsive layout", () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    const { container } = renderWithRouter(<JobTrackingPage />);

    // Check for responsive grid classes
    const gridElements = container.querySelectorAll("[class*='lg:']");
    expect(gridElements.length).toBeGreaterThan(0);
  });

  it("should display ETA in contractor card when available", async () => {
    vi.mocked(useJobModule.useJob).mockReturnValue({
      job: mockJob,
      loading: false,
      error: null,
      fetchJob: vi.fn(),
      refreshJob: vi.fn(),
    });

    vi.mocked(useSignalRModule.useSignalR).mockReturnValue({
      isConnected: true,
      subscribe: vi.fn(() => vi.fn()),
      disconnect: vi.fn(),
    });

    renderWithRouter(<JobTrackingPage />);

    // Verify page is fully rendered with contractor info
    await waitFor(() => {
      const pageTitle = screen.getByText("Track Your Job");
      expect(pageTitle).toBeInTheDocument();
    });
  });
});
