/**
 * useStatusTransition Hook Tests
 * Tests marking jobs as in-progress and completed
 */

import { renderHook, act, waitFor } from "@testing-library/react";
import { useStatusTransition } from "../useStatusTransition";
import { contractorService } from "@/services/contractorService";
import { vi } from "vitest";

// Mock the contractor service
vi.mock("@/services/contractorService", () => ({
  contractorService: {
    markInProgress: vi.fn(),
    markComplete: vi.fn(),
  },
}));

describe("useStatusTransition", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should initialize with correct default state", () => {
    const { result } = renderHook(() => useStatusTransition());

    expect(result.current.isLoading).toBe(false);
    expect(result.current.error).toBe(null);
    expect(typeof result.current.markInProgress).toBe("function");
    expect(typeof result.current.markComplete).toBe("function");
  });

  it("should call markInProgress with assignment ID", async () => {
    const mockAssignment = {
      id: 1,
      status: "InProgress",
      jobId: 10,
      contractorId: 100,
    };

    vi.mocked(contractorService.markInProgress).mockResolvedValueOnce(
      mockAssignment as any
    );

    const { result } = renderHook(() => useStatusTransition());

    await act(async () => {
      const response = await result.current.markInProgress("1");
      expect(response).toEqual(mockAssignment);
    });

    expect(contractorService.markInProgress).toHaveBeenCalledWith("1");
  });

  it("should call markComplete with assignment ID", async () => {
    const mockAssignment = {
      id: 1,
      status: "Completed",
      jobId: 10,
      contractorId: 100,
    };

    vi.mocked(contractorService.markComplete).mockResolvedValueOnce(
      mockAssignment as any
    );

    const { result } = renderHook(() => useStatusTransition());

    await act(async () => {
      const response = await result.current.markComplete("1");
      expect(response).toEqual(mockAssignment);
    });

    expect(contractorService.markComplete).toHaveBeenCalledWith("1");
  });

  it("should set loading state during API call", async () => {
    const mockAssignment = { id: 1, status: "InProgress" };
    let resolveMarkInProgress: any;
    const markInProgressPromise = new Promise((resolve) => {
      resolveMarkInProgress = resolve;
    });

    vi.mocked(contractorService.markInProgress).mockReturnValueOnce(
      markInProgressPromise as any
    );

    const { result } = renderHook(() => useStatusTransition());

    let loadingDuringCall = false;
    act(() => {
      result.current.markInProgress("1").then(() => {
        // After promise resolves, loading should be false
        loadingDuringCall = result.current.isLoading;
      });
    });

    // Initially isLoading should be true
    await waitFor(() => {
      expect(result.current.isLoading).toBe(true);
    });

    // Resolve the promise
    resolveMarkInProgress(mockAssignment);

    // After resolution, isLoading should be false
    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });
  });

  it("should handle API errors for markInProgress", async () => {
    const errorMessage = "Network error";
    vi.mocked(contractorService.markInProgress).mockRejectedValueOnce(
      new Error(errorMessage)
    );

    const { result } = renderHook(() => useStatusTransition());

    await act(async () => {
      try {
        await result.current.markInProgress("1");
      } catch (error) {
        // Error is expected
      }
    });

    expect(result.current.error).toBe(errorMessage);
    expect(result.current.isLoading).toBe(false);
  });

  it("should handle API errors for markComplete", async () => {
    const errorMessage = "Cannot mark incomplete job as complete";
    vi.mocked(contractorService.markComplete).mockRejectedValueOnce(
      new Error(errorMessage)
    );

    const { result } = renderHook(() => useStatusTransition());

    await act(async () => {
      try {
        await result.current.markComplete("1");
      } catch (error) {
        // Error is expected
      }
    });

    expect(result.current.error).toBe(errorMessage);
    expect(result.current.isLoading).toBe(false);
  });

  it("should clear error on successful call after previous error", async () => {
    const mockAssignment = { id: 1, status: "InProgress" };

    // First call fails
    vi.mocked(contractorService.markInProgress).mockRejectedValueOnce(
      new Error("First error")
    );

    const { result } = renderHook(() => useStatusTransition());

    await act(async () => {
      try {
        await result.current.markInProgress("1");
      } catch (error) {
        // Catch first error
      }
    });

    expect(result.current.error).toBe("First error");

    // Second call succeeds
    vi.mocked(contractorService.markInProgress).mockResolvedValueOnce(
      mockAssignment as any
    );

    await act(async () => {
      await result.current.markInProgress("1");
    });

    expect(result.current.error).toBeNull();
  });
});
