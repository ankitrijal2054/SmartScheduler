/**
 * useJobReassignment Hook Tests
 * Tests for custom hook managing job reassignment state and logic
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act, waitFor } from "@testing-library/react";
import { useJobReassignment } from "../useJobReassignment";
import { dispatcherService } from "@/services/dispatcherService";
import { ReassignmentResponse } from "@/types/Reassignment";

// Mock the dispatcher service
vi.mock("@/services/dispatcherService");

const mockReassignmentResponse: ReassignmentResponse = {
  newAssignmentId: "assign_new_123",
  oldAssignmentId: "assign_old_123",
  jobId: "job_123",
  previousContractorId: "cont_001",
  newContractorId: "cont_002",
  jobStatus: "Assigned",
  createdAt: "2025-11-10T15:00:00Z",
};

describe("useJobReassignment Hook", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should initialize with correct state", () => {
    const { result } = renderHook(() => useJobReassignment());

    expect(result.current.isReassigning).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.successMessage).toBeNull();
    expect(result.current.reassignmentData).toBeNull();
  });

  it("should call API with correct parameters when reassignJob is called", async () => {
    const mockReassign = vi.fn().mockResolvedValue(mockReassignmentResponse);
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    expect(mockReassign).toHaveBeenCalledWith(
      "job_123",
      expect.objectContaining({
        newContractorId: "cont_002",
      }),
      expect.anything()
    );
  });

  it("should include optional reason parameter when provided", async () => {
    const mockReassign = vi.fn().mockResolvedValue(mockReassignmentResponse);
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    await act(async () => {
      await result.current.reassignJob(
        "job_123",
        "cont_002",
        "Original contractor cancelled"
      );
    });

    expect(mockReassign).toHaveBeenCalledWith(
      "job_123",
      expect.objectContaining({
        newContractorId: "cont_002",
        reason: "Original contractor cancelled",
      }),
      expect.anything()
    );
  });

  it("should set isReassigning to true during API call", async () => {
    const mockReassign = vi.fn(
      () =>
        new Promise((resolve) =>
          setTimeout(() => resolve(mockReassignmentResponse), 100)
        )
    );
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    act(() => {
      result.current.reassignJob("job_123", "cont_002");
    });

    await waitFor(() => {
      expect(result.current.isReassigning).toBe(true);
    });
  });

  it("should handle successful reassignment", async () => {
    const mockReassign = vi.fn().mockResolvedValue(mockReassignmentResponse);
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    expect(result.current.isReassigning).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.successMessage).toBe("Job reassigned successfully");
    expect(result.current.reassignmentData).toEqual(mockReassignmentResponse);
  });

  it("should handle API error", async () => {
    const errorMessage = "Contractor no longer available";
    const mockReassign = vi.fn().mockRejectedValue(new Error(errorMessage));
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    expect(result.current.isReassigning).toBe(false);
    expect(result.current.error).toBe(errorMessage);
    expect(result.current.successMessage).toBeNull();
    expect(result.current.reassignmentData).toBeNull();
  });

  it("should handle 409 CONFLICT error (contractor unavailable)", async () => {
    const errorMessage = "Contractor unavailable; please try again";
    const mockReassign = vi.fn().mockRejectedValue(new Error(errorMessage));
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    expect(result.current.error).toBe(errorMessage);
  });

  it("should handle 404 NOT FOUND error (job not found)", async () => {
    const errorMessage = "Job not found";
    const mockReassign = vi.fn().mockRejectedValue(new Error(errorMessage));
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    expect(result.current.error).toBe(errorMessage);
  });

  it("should reset state when reset is called", async () => {
    const mockReassign = vi.fn().mockResolvedValue(mockReassignmentResponse);
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    act(() => {
      result.current.reset();
    });

    expect(result.current.isReassigning).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.successMessage).toBeNull();
    expect(result.current.reassignmentData).toBeNull();
  });

  it("should support retry functionality", async () => {
    const mockReassign = vi.fn();
    mockReassign
      .mockRejectedValueOnce(new Error("Temporary error"))
      .mockResolvedValueOnce(mockReassignmentResponse);
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    // First attempt fails
    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    expect(result.current.error).toBe("Temporary error");

    // Retry succeeds
    await act(async () => {
      await result.current.retry();
    });

    expect(result.current.error).toBeNull();
    expect(result.current.successMessage).toBe("Job reassigned successfully");
  });

  it("should maintain retry parameters across calls", async () => {
    const mockReassign = vi.fn();
    mockReassign
      .mockRejectedValueOnce(new Error("Error"))
      .mockResolvedValueOnce(mockReassignmentResponse);
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    // First call fails
    await act(async () => {
      await result.current.reassignJob(
        "job_123",
        "cont_002",
        "Original contractor cancelled"
      );
    });

    // Retry should use same parameters
    await act(async () => {
      await result.current.retry();
    });

    // Verify the second call also included the reason
    const secondCall = vi.mocked(dispatcherService.reassignJob).mock.calls[1];
    expect(secondCall[1]).toEqual(
      expect.objectContaining({
        newContractorId: "cont_002",
        reason: "Original contractor cancelled",
      })
    );
  });

  it("should cleanup on unmount", () => {
    const { unmount } = renderHook(() => useJobReassignment());

    // Unmount should not throw errors
    expect(() => unmount()).not.toThrow();
  });

  it("should handle multiple sequential reassignment calls", async () => {
    const mockReassign = vi.fn().mockResolvedValue(mockReassignmentResponse);
    vi.mocked(dispatcherService.reassignJob).mockImplementation(mockReassign);

    const { result } = renderHook(() => useJobReassignment());

    // First reassignment
    await act(async () => {
      await result.current.reassignJob("job_123", "cont_002");
    });

    expect(result.current.successMessage).toBe("Job reassigned successfully");

    // Reset for second attempt
    act(() => {
      result.current.reset();
    });

    // Second reassignment
    await act(async () => {
      await result.current.reassignJob("job_456", "cont_003");
    });

    expect(result.current.successMessage).toBe("Job reassigned successfully");
    expect(mockReassign).toHaveBeenCalledTimes(2);
  });
});
