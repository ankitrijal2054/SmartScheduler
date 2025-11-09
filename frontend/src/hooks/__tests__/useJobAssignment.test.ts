/**
 * useJobAssignment Hook Unit Tests
 * Tests for the job assignment state management hook
 */

import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, act, waitFor } from "@testing-library/react";
import { useJobAssignment } from "../useJobAssignment";
import { dispatcherService } from "@/services/dispatcherService";
import { AssignmentResponse } from "@/types/Assignment";

// Mock the dispatcher service
vi.mock("@/services/dispatcherService", () => ({
  dispatcherService: {
    assignJob: vi.fn(),
  },
}));

// Mock axios for cancel token
vi.mock("axios", () => ({
  default: {
    isCancel: vi.fn((error) => error?.message === "Request cancelled"),
    CancelToken: {
      source: () => ({
        token: { key: "mock-token" },
        cancel: vi.fn(),
      }),
    },
  },
}));

describe("useJobAssignment", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.clearAllTimers();
  });

  it("initializes with correct state", () => {
    const { result } = renderHook(() => useJobAssignment());

    expect(result.current.isAssigning).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.successMessage).toBeNull();
    expect(result.current.assignmentData).toBeNull();
  });

  it("calls API with correct parameters", async () => {
    const mockResponse: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    vi.mocked(dispatcherService.assignJob).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobAssignment());

    await act(async () => {
      await result.current.assignJob("job_001", "cont_001");
    });

    expect(dispatcherService.assignJob).toHaveBeenCalledWith(
      "job_001",
      { contractorId: "cont_001" },
      expect.any(Object)
    );
  });

  it("sets isAssigning to true during API call", async () => {
    const mockResponse: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    vi.mocked(dispatcherService.assignJob).mockImplementationOnce(
      () =>
        new Promise((resolve) => {
          setTimeout(() => resolve(mockResponse), 100);
        })
    );

    const { result } = renderHook(() => useJobAssignment());

    act(() => {
      result.current.assignJob("job_001", "cont_001");
    });

    // Should be assigning immediately
    expect(result.current.isAssigning).toBe(true);

    await waitFor(() => {
      expect(result.current.isAssigning).toBe(false);
    });
  });

  it("handles successful assignment", async () => {
    const mockResponse: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    vi.mocked(dispatcherService.assignJob).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobAssignment());

    await act(async () => {
      await result.current.assignJob("job_001", "cont_001");
    });

    expect(result.current.isAssigning).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.assignmentData).toEqual(mockResponse);
    expect(result.current.successMessage).toBeTruthy();
  });

  it("handles API error", async () => {
    const errorMessage = "Contractor no longer available";
    vi.mocked(dispatcherService.assignJob).mockRejectedValueOnce(
      new Error(errorMessage)
    );

    const { result } = renderHook(() => useJobAssignment());

    await act(async () => {
      await result.current.assignJob("job_001", "cont_001");
    });

    expect(result.current.isAssigning).toBe(false);
    expect(result.current.error).toBe(errorMessage);
    expect(result.current.assignmentData).toBeNull();
  });

  it("resets state to initial values", async () => {
    const mockResponse: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    vi.mocked(dispatcherService.assignJob).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobAssignment());

    await act(async () => {
      await result.current.assignJob("job_001", "cont_001");
    });

    expect(result.current.assignmentData).toBeTruthy();

    act(() => {
      result.current.reset();
    });

    expect(result.current.isAssigning).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.successMessage).toBeNull();
    expect(result.current.assignmentData).toBeNull();
  });

  it("supports retry capability", async () => {
    const mockResponse: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    // First call fails
    vi.mocked(dispatcherService.assignJob)
      .mockRejectedValueOnce(new Error("Network error"))
      .mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobAssignment());

    // First assignment attempt - fails
    await act(async () => {
      await result.current.assignJob("job_001", "cont_001");
    });

    expect(result.current.error).toBeTruthy();

    // Retry should succeed
    await act(async () => {
      await result.current.retry();
    });

    expect(result.current.error).toBeNull();
    expect(result.current.assignmentData).toEqual(mockResponse);
    expect(dispatcherService.assignJob).toHaveBeenCalledTimes(2);
  });

  it("stores retry parameters correctly", async () => {
    const mockResponse: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    vi.mocked(dispatcherService.assignJob).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobAssignment());

    const jobId = "job_xyz";
    const contractorId = "cont_xyz";

    await act(async () => {
      await result.current.assignJob(jobId, contractorId);
    });

    expect(dispatcherService.assignJob).toHaveBeenCalledWith(
      jobId,
      { contractorId },
      expect.any(Object)
    );
  });

  it("does not error on cancel token cleanup", () => {
    const { unmount } = renderHook(() => useJobAssignment());

    // Should not throw on unmount
    expect(() => {
      unmount();
    }).not.toThrow();
  });

  it("handles multiple sequential assignments", async () => {
    const mockResponse1: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    const mockResponse2: AssignmentResponse = {
      assignmentId: "assign_002",
      jobId: "job_002",
      contractorId: "cont_002",
      status: "Pending",
      createdAt: "2025-11-10T11:00:00Z",
    };

    vi.mocked(dispatcherService.assignJob)
      .mockResolvedValueOnce(mockResponse1)
      .mockResolvedValueOnce(mockResponse2);

    const { result } = renderHook(() => useJobAssignment());

    // First assignment
    await act(async () => {
      await result.current.assignJob("job_001", "cont_001");
    });

    expect(result.current.assignmentData?.assignmentId).toBe("assign_001");

    // Second assignment
    await act(async () => {
      await result.current.assignJob("job_002", "cont_002");
    });

    expect(result.current.assignmentData?.assignmentId).toBe("assign_002");
    expect(dispatcherService.assignJob).toHaveBeenCalledTimes(2);
  });

  it("clears error on successful retry", async () => {
    const errorMessage = "First attempt failed";
    const mockResponse: AssignmentResponse = {
      assignmentId: "assign_001",
      jobId: "job_001",
      contractorId: "cont_001",
      status: "Pending",
      createdAt: "2025-11-10T10:00:00Z",
    };

    vi.mocked(dispatcherService.assignJob)
      .mockRejectedValueOnce(new Error(errorMessage))
      .mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobAssignment());

    // First attempt - fails
    await act(async () => {
      await result.current.assignJob("job_001", "cont_001");
    });

    expect(result.current.error).toBe(errorMessage);
    expect(result.current.assignmentData).toBeNull();

    // Retry - succeeds
    await act(async () => {
      await result.current.retry();
    });

    expect(result.current.error).toBeNull();
    expect(result.current.assignmentData).toBeTruthy();
  });
});
