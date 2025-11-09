/**
 * useJob Hook Tests
 * Vitest + Testing Library React
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";
import { waitFor } from "@testing-library/react";
import { useJob } from "../useJob";
import { customerService } from "@/services/customerService";
import { JobDetail } from "@/types/Job";

// Mock the customer service
vi.mock("@/services/customerService");

describe("useJob Hook", () => {
  const mockJobDetail: JobDetail = {
    id: "job_123",
    customerId: "cust_1",
    location: "123 Main St, Springfield, IL",
    desiredDateTime: "2025-11-15T10:00:00Z",
    jobType: "Plumbing",
    description: "Fix broken kitchen pipe",
    status: "Assigned",
    currentAssignedContractorId: "contractor_1",
    createdAt: "2025-11-07T15:30:00Z",
    updatedAt: "2025-11-07T16:20:00Z",
    assignment: {
      id: "assign_1",
      contractorId: "contractor_1",
      status: "Accepted",
      assignedAt: "2025-11-07T15:45:00Z",
      acceptedAt: "2025-11-07T16:00:00Z",
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

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should fetch job on initial mount", async () => {
    vi.mocked(customerService.getJobById).mockResolvedValueOnce(mockJobDetail);

    const { result } = renderHook(() => useJob("job_123"));

    await waitFor(
      () => {
        expect(result.current.job).not.toBeNull();
        expect(result.current.loading).toBe(false);
      },
      { timeout: 10000 }
    );

    expect(customerService.getJobById).toHaveBeenCalledWith("job_123");
  });

  it("should handle API errors gracefully", async () => {
    const errorMessage = "Job not found";
    vi.mocked(customerService.getJobById).mockRejectedValueOnce(
      new Error(errorMessage)
    );

    const { result } = renderHook(() => useJob("job_999"));

    await waitFor(
      () => {
        expect(result.current.error).toBe(errorMessage);
        expect(result.current.loading).toBe(false);
      },
      { timeout: 10000 }
    );
  });

  it("should load job details correctly", async () => {
    vi.mocked(customerService.getJobById).mockResolvedValueOnce(mockJobDetail);

    const { result } = renderHook(() => useJob("job_123"));

    await waitFor(
      () => {
        expect(result.current.job).not.toBeNull();
      },
      { timeout: 10000 }
    );

    const job = result.current.job!;
    expect(job.id).toBe("job_123");
    expect(job.jobType).toBe("Plumbing");
    expect(job.status).toBe("Assigned");
    expect(job.location).toBe("123 Main St, Springfield, IL");
  });

  it("should include contractor info when assigned", async () => {
    vi.mocked(customerService.getJobById).mockResolvedValueOnce(mockJobDetail);

    const { result } = renderHook(() => useJob("job_123"));

    await waitFor(
      () => {
        expect(result.current.job?.contractor).not.toBeUndefined();
      },
      { timeout: 10000 }
    );

    expect(result.current.job!.contractor!.name).toBe("John Smith");
    expect(result.current.job!.contractor!.phoneNumber).toBe("555-1234");
    expect(result.current.job!.contractor!.averageRating).toBe(4.8);
  });

  it("should include assignment info when available", async () => {
    vi.mocked(customerService.getJobById).mockResolvedValueOnce(mockJobDetail);

    const { result } = renderHook(() => useJob("job_123"));

    await waitFor(
      () => {
        expect(result.current.job?.assignment).not.toBeUndefined();
      },
      { timeout: 10000 }
    );

    expect(result.current.job!.assignment!.status).toBe("Accepted");
    expect(result.current.job!.assignment!.estimatedArrivalTime).toBe(
      "2025-11-15T10:30:00Z"
    );
  });

  it("should expose refresh function", async () => {
    vi.mocked(customerService.getJobById).mockResolvedValueOnce(mockJobDetail);

    const { result } = renderHook(() => useJob("job_123"));

    expect(result.current.refreshJob).toBeDefined();
    expect(result.current.fetchJob).toBeDefined();
  });

  it("should return initial loading state as true", () => {
    vi.mocked(customerService.getJobById).mockImplementation(
      () =>
        new Promise((resolve) => setTimeout(() => resolve(mockJobDetail), 500))
    );

    const { result } = renderHook(() => useJob("job_123"));

    // First render should have loading as true
    expect(result.current.loading).toBe(true);
  });

  it("should fetch new job when jobId changes", async () => {
    const job1 = { ...mockJobDetail, id: "job_1" };
    const job2 = { ...mockJobDetail, id: "job_2" };

    vi.mocked(customerService.getJobById)
      .mockResolvedValueOnce(job1)
      .mockResolvedValueOnce(job2);

    const { result, rerender } = renderHook(({ jobId }) => useJob(jobId), {
      initialProps: { jobId: "job_1" },
    });

    await waitFor(
      () => {
        expect(result.current.job?.id).toBe("job_1");
      },
      { timeout: 10000 }
    );

    rerender({ jobId: "job_2" });

    await waitFor(
      () => {
        expect(result.current.job?.id).toBe("job_2");
      },
      { timeout: 10000 }
    );

    expect(customerService.getJobById).toHaveBeenCalledWith("job_1");
    expect(customerService.getJobById).toHaveBeenCalledWith("job_2");
  });
});
