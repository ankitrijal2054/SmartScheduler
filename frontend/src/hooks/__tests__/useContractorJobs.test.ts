/**
 * useContractorJobs Hook Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import {
  useContractorJobs,
  clearContractorJobsCache,
} from "../useContractorJobs";

// Mock contractorService
vi.mock("@/services/contractorService", () => ({
  contractorService: {
    getAssignments: vi.fn(),
  },
}));

import { contractorService } from "@/services/contractorService";

const mockAssignments = [
  {
    id: "1",
    jobId: "job-1",
    contractorId: "contractor-1",
    status: "Pending" as const,
    createdAt: new Date().toISOString(),
    jobType: "Plumbing",
    location: "123 Main St",
    scheduledTime: new Date().toISOString(),
    customerName: "John Smith",
  },
];

describe("useContractorJobs Hook", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    clearContractorJobsCache();
  });

  it("should fetch assignments on mount", async () => {
    const mockGetAssignments = vi
      .spyOn(contractorService, "getAssignments")
      .mockResolvedValue(mockAssignments);

    const { result } = renderHook(() => useContractorJobs());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(mockGetAssignments).toHaveBeenCalled();
    expect(result.current.jobs).toEqual(mockAssignments);
  });

  it("should cache results for 30 seconds", async () => {
    const mockGetAssignments = vi
      .spyOn(contractorService, "getAssignments")
      .mockResolvedValue(mockAssignments);

    const { result: result1 } = renderHook(() => useContractorJobs());

    await waitFor(() => {
      expect(result1.current.loading).toBe(false);
    });

    const { result: result2 } = renderHook(() => useContractorJobs());

    await waitFor(() => {
      expect(result2.current.loading).toBe(false);
    });

    // Should only be called once due to caching
    expect(mockGetAssignments).toHaveBeenCalledTimes(1);
  });

  it("should handle errors gracefully", async () => {
    const error = new Error("API Error");
    vi.spyOn(contractorService, "getAssignments").mockRejectedValue(error);

    const { result } = renderHook(() => useContractorJobs());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.error).toBe("API Error");
    expect(result.current.jobs).toBeNull();
  });

  it("should refetch data when refetch is called", async () => {
    const mockGetAssignments = vi
      .spyOn(contractorService, "getAssignments")
      .mockResolvedValue(mockAssignments);

    const { result } = renderHook(() => useContractorJobs());

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(mockGetAssignments).toHaveBeenCalledTimes(1);

    result.current.refetch();

    await waitFor(
      () => {
        expect(mockGetAssignments).toHaveBeenCalledTimes(2);
      },
      { timeout: 3000 }
    );
  });

  it("should filter by status when provided", async () => {
    const mockGetAssignments = vi
      .spyOn(contractorService, "getAssignments")
      .mockResolvedValue(mockAssignments);

    const { result } = renderHook(() => useContractorJobs("Pending"));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(mockGetAssignments).toHaveBeenCalledWith("Pending");
  });
});
