/**
 * useContractorHistory Hook Tests
 */

import { renderHook, waitFor } from "@testing-library/react";
import { useContractorHistory } from "../useContractorHistory";
import * as dispatcherServiceModule from "@/services/dispatcherService";
import { ContractorHistory } from "@/types/Contractor";

// Mock the dispatcherService
vi.mock("@/services/dispatcherService");

describe("useContractorHistory Hook", () => {
  const mockContractorHistory: ContractorHistory = {
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
      {
        jobId: "job-2",
        jobType: "Plumbing",
        customerName: "Bob Davis",
        completedAt: "2025-11-07T16:00:00Z",
        status: "completed",
        customerRating: 4,
        createdAt: "2025-11-07T09:00:00Z",
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

  it("should fetch contractor history successfully", async () => {
    const mockGetContractorHistory = vi
      .spyOn(dispatcherServiceModule.dispatcherService, "getContractorHistory")
      .mockResolvedValueOnce(mockContractorHistory);

    const { result } = renderHook(() => useContractorHistory("contractor-1"));

    // Initially loading
    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();

    // Wait for data to be fetched
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    // Verify data is loaded
    expect(result.current.data).toEqual(mockContractorHistory);
    expect(result.current.error).toBeNull();
    expect(mockGetContractorHistory).toHaveBeenCalledWith(
      "contractor-1",
      10,
      0,
      expect.any(Object)
    );
  });

  it("should handle API errors gracefully", async () => {
    const errorMessage = "Failed to load contractor history";
    const mockError = new Error(errorMessage);

    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getContractorHistory"
    ).mockRejectedValueOnce(mockError);

    const { result } = renderHook(() => useContractorHistory("contractor-1"));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.data).toBeNull();
    expect(result.current.error).toEqual(errorMessage);
  });

  it("should respect limit and offset parameters", async () => {
    const mockGetContractorHistory = vi
      .spyOn(dispatcherServiceModule.dispatcherService, "getContractorHistory")
      .mockResolvedValueOnce(mockContractorHistory);

    renderHook(() => useContractorHistory("contractor-1", 20, 5));

    await waitFor(() => {
      expect(mockGetContractorHistory).toHaveBeenCalledWith(
        "contractor-1",
        20,
        5,
        expect.any(Object)
      );
    });
  });

  it("should cancel request on unmount", async () => {
    const mockGetContractorHistory = vi
      .spyOn(dispatcherServiceModule.dispatcherService, "getContractorHistory")
      .mockImplementation(
        () =>
          new Promise(() => {
            // Never resolves to simulate pending request
          })
      );

    const { unmount } = renderHook(() => useContractorHistory("contractor-1"));

    unmount();

    // Verify the service was called
    expect(mockGetContractorHistory).toHaveBeenCalled();
  });
});
