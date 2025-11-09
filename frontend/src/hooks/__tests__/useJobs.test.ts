/**
 * useJobs Hook Tests
 * Vitest + Testing Library React
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";
import { waitFor } from "@testing-library/react";
import { useJobs } from "../useJobs";
import { dispatcherService } from "@/services/dispatcherService";
import { PaginatedJobsResponse } from "@/types/Job";

// Mock the dispatcher service
vi.mock("@/services/dispatcherService");

describe("useJobs Hook", () => {
  const mockResponse: PaginatedJobsResponse = {
    data: [
      {
        id: "job_1",
        customerId: "cust_1",
        customerName: "Jane Smith",
        location: "123 Main St",
        desiredDateTime: "2025-11-10T14:00:00Z",
        jobType: "Plumbing",
        description: "Fix broken pipe",
        status: "Pending",
        currentAssignedContractorId: null,
        createdAt: "2025-11-08T10:30:00Z",
        updatedAt: "2025-11-08T10:30:00Z",
      },
    ],
    pagination: {
      page: 1,
      limit: 20,
      total: 1,
      totalPages: 1,
    },
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should fetch jobs on initial mount", async () => {
    vi.mocked(dispatcherService.getJobs).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobs());

    await waitFor(
      () => {
        expect(result.current.jobs).toHaveLength(1);
        expect(result.current.loading).toBe(false);
      },
      { timeout: 10000 }
    );

    expect(dispatcherService.getJobs).toHaveBeenCalled();
  });

  it("should handle API errors gracefully", async () => {
    const errorMessage = "Network error";
    vi.mocked(dispatcherService.getJobs).mockRejectedValueOnce(
      new Error(errorMessage)
    );

    const { result } = renderHook(() => useJobs());

    await waitFor(
      () => {
        expect(result.current.error).toBe(errorMessage);
        expect(result.current.loading).toBe(false);
      },
      { timeout: 10000 }
    );
  });

  it("should set pagination state correctly", async () => {
    vi.mocked(dispatcherService.getJobs).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobs());

    await waitFor(
      () => {
        expect(result.current.pagination).not.toBeNull();
        expect(result.current.pagination?.page).toBe(1);
        expect(result.current.pagination?.total).toBe(1);
      },
      { timeout: 10000 }
    );
  });

  it("should parse and normalize API response", async () => {
    vi.mocked(dispatcherService.getJobs).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobs());

    await waitFor(
      () => {
        expect(result.current.jobs).toHaveLength(1);
      },
      { timeout: 10000 }
    );

    const job = result.current.jobs[0];
    expect(job.id).toBe("job_1");
    expect(job.customerName).toBe("Jane Smith");
    expect(job.status).toBe("Pending");
  });

  it("should expose pagination controls", async () => {
    vi.mocked(dispatcherService.getJobs).mockResolvedValueOnce(mockResponse);

    const { result } = renderHook(() => useJobs());

    expect(result.current.setPage).toBeDefined();
    expect(result.current.setLimit).toBeDefined();
    expect(result.current.setSort).toBeDefined();
    expect(result.current.fetchJobs).toBeDefined();
    expect(result.current.refreshJobs).toBeDefined();
  });

  it("should return initial loading state as true", () => {
    vi.mocked(dispatcherService.getJobs).mockImplementation(
      () =>
        new Promise((resolve) => setTimeout(() => resolve(mockResponse), 500))
    );

    const { result } = renderHook(() => useJobs());

    // First render should have loading as true
    expect(result.current.loading).toBe(true);
  });
});
