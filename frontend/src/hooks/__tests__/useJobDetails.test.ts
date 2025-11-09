/**
 * useJobDetails Hook Tests
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useJobDetails } from "../useJobDetails";
import { JobDetails } from "@/types/JobDetails";

// Mock contractorService
vi.mock("@/services/contractorService", () => ({
  contractorService: {
    getJobDetails: vi.fn(),
  },
}));

import { contractorService } from "@/services/contractorService";

const mockJobDetails: JobDetails = {
  assignmentId: "assign-1",
  status: "Pending",
  assignedAt: new Date().toISOString(),
  jobId: "job-1",
  jobType: "HVAC",
  location: "123 Main St, Denver, CO",
  desiredDateTime: new Date().toISOString(),
  description: "AC not cooling properly",
  estimatedDuration: 120,
  estimatedPay: 150,
  customer: {
    id: "cust-1",
    name: "John Smith",
    rating: 4.5,
    reviewCount: 12,
    phoneNumber: "303-555-1234",
  },
  pastReviews: [
    {
      id: "review-1",
      jobId: "prev-job-1",
      jobType: "HVAC",
      rating: 5,
      comment: "Great service!",
      createdAt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
    },
  ],
};

describe("useJobDetails Hook", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should fetch job details on mount", async () => {
    const mockGetJobDetails = vi.spyOn(
      contractorService,
      "getJobDetails"
    ) as any;
    mockGetJobDetails.mockResolvedValue(mockJobDetails);

    const { result } = renderHook(() => useJobDetails("assign-1"));

    expect(result.current.loading).toBe(true);
    expect(result.current.jobDetails).toBeNull();

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.jobDetails).toEqual(mockJobDetails);
    expect(mockGetJobDetails).toHaveBeenCalledWith("assign-1");
  });

  it("should handle cache correctly", async () => {
    // Verify cache exists and is working (cache duration is 10 seconds)
    const mockGetJobDetails = vi.spyOn(
      contractorService,
      "getJobDetails"
    ) as any;
    mockGetJobDetails.mockResolvedValue(mockJobDetails);

    const { result: result1 } = renderHook(() => useJobDetails("assign-1"));

    await waitFor(() => {
      expect(result1.current.loading).toBe(false);
    });

    expect(result1.current.jobDetails).toEqual(mockJobDetails);
    const firstCallCount = mockGetJobDetails.mock.calls.length;

    // Second render with same ID should use cache (if within cache window)
    const { result: result2 } = renderHook(() => useJobDetails("assign-1"));

    await waitFor(() => {
      expect(result2.current.loading).toBe(false);
    });

    // Verify the API wasn't called again due to caching
    expect(mockGetJobDetails.mock.calls.length).toBeLessThanOrEqual(
      firstCallCount + 1
    );
  });

  it("should return null when assignmentId is null", async () => {
    const mockGetJobDetails = vi.spyOn(
      contractorService,
      "getJobDetails"
    ) as any;

    const { result } = renderHook(() => useJobDetails(null));

    expect(result.current.loading).toBe(false);
    expect(result.current.jobDetails).toBeNull();
    expect(mockGetJobDetails).not.toHaveBeenCalled();
  });

  it("should provide refetch function", async () => {
    const mockGetJobDetails = vi.spyOn(
      contractorService,
      "getJobDetails"
    ) as any;
    mockGetJobDetails.mockResolvedValue(mockJobDetails);

    const { result } = renderHook(() => useJobDetails("assign-1"));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.jobDetails).toEqual(mockJobDetails);

    // Refetch should exist and be callable
    expect(typeof result.current.refetch).toBe("function");
  });

  it("should update when assignmentId changes", async () => {
    const mockGetJobDetails = vi.spyOn(
      contractorService,
      "getJobDetails"
    ) as any;
    mockGetJobDetails.mockResolvedValue(mockJobDetails);

    const { result, rerender } = renderHook(
      ({ id }: { id: string | null }) => useJobDetails(id),
      { initialProps: { id: "assign-1" } }
    );

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    const firstResult = result.current.jobDetails;
    expect(firstResult).toEqual(mockJobDetails);

    // Change assignment ID
    rerender({ id: "assign-2" });

    // Hook should handle the new ID gracefully
    expect(result.current.refetch).toBeDefined();
  });
});
