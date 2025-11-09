/**
 * useContractorProfile Hook Tests
 * Tests for contractor profile data fetching and state management
 */

import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useContractorProfile } from "../useContractorProfile";
import { customerService } from "@/services/customerService";
import {
  ContractorProfileResponse,
  ReviewWithCustomer,
} from "@/types/Customer";

vi.mock("@/services/customerService");

describe("useContractorProfile Hook", () => {
  const mockContractorId = "contractor-123";

  const mockReviews: ReviewWithCustomer[] = [
    {
      id: "review-1",
      jobId: "job-1",
      contractorId: mockContractorId,
      customerId: "customer-1",
      rating: 5,
      comment: "John was professional and quick",
      customerName: "Jane K.",
      createdAt: "2025-11-09T10:00:00Z",
    },
    {
      id: "review-2",
      jobId: "job-2",
      contractorId: mockContractorId,
      customerId: "customer-2",
      rating: 4,
      comment: "Good work, arrived on time",
      customerName: "Bob M.",
      createdAt: "2025-11-08T10:00:00Z",
    },
  ];

  const mockProfileResponse: ContractorProfileResponse = {
    contractor: {
      id: mockContractorId,
      name: "John Smith",
      phoneNumber: "(555) 123-4567",
      averageRating: 4.8,
      reviewCount: 2,
      totalJobsCompleted: 15,
      isActive: true,
    },
    reviews: mockReviews,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it("should fetch contractor profile successfully", async () => {
    vi.mocked(customerService.getContractorProfile).mockResolvedValueOnce(
      mockProfileResponse
    );

    const { result } = renderHook(() => useContractorProfile(mockContractorId));

    expect(result.current.loading).toBe(true);
    expect(result.current.contractor).toBeNull();
    expect(result.current.reviews).toEqual([]);

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.contractor).toBeDefined();
    expect(result.current.contractor?.name).toBe("John Smith");
    expect(result.current.reviews).toHaveLength(2);
    expect(result.current.error).toBeNull();
    expect(customerService.getContractorProfile).toHaveBeenCalledWith(
      mockContractorId
    );
  });

  it("should handle loading state", async () => {
    vi.mocked(customerService.getContractorProfile).mockImplementationOnce(
      () =>
        new Promise((resolve) =>
          setTimeout(() => resolve(mockProfileResponse), 100)
        )
    );

    const { result } = renderHook(() => useContractorProfile(mockContractorId));

    expect(result.current.loading).toBe(true);

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
  });

  it("should handle API error", async () => {
    const errorMessage = "Failed to fetch contractor profile";
    vi.mocked(customerService.getContractorProfile).mockRejectedValueOnce(
      new Error(errorMessage)
    );

    const { result } = renderHook(() => useContractorProfile(mockContractorId));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.error).toBe(errorMessage);
    expect(result.current.contractor).toBeNull();
    expect(result.current.reviews).toEqual([]);
  });

  it("should handle null contractorId", () => {
    const { result } = renderHook(() => useContractorProfile(null));

    expect(result.current.loading).toBe(false);
    expect(result.current.contractor).toBeNull();
    expect(result.current.reviews).toEqual([]);
    expect(result.current.error).toBeNull();
  });

  it("should refetch contractor profile on demand", async () => {
    vi.mocked(customerService.getContractorProfile).mockResolvedValueOnce(
      mockProfileResponse
    );

    const { result } = renderHook(() => useContractorProfile(mockContractorId));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(customerService.getContractorProfile).toHaveBeenCalledTimes(1);
    expect(result.current.reviews).toHaveLength(2);

    // Setup mock for refetch with additional review
    const additionalReview: ReviewWithCustomer = {
      id: "review-3",
      jobId: "job-3",
      contractorId: mockContractorId,
      customerId: "customer-3",
      rating: 5,
      comment: "Excellent work!",
      customerName: "Charlie D.",
      createdAt: "2025-11-07T10:00:00Z",
    };

    vi.mocked(customerService.getContractorProfile).mockResolvedValueOnce({
      ...mockProfileResponse,
      reviews: [...mockReviews, additionalReview],
    });

    // Trigger refetch
    await result.current.refetch();

    // Verify refetch was called
    expect(customerService.getContractorProfile).toHaveBeenCalledTimes(2);

    // Verify new data is loaded
    await waitFor(() => {
      expect(result.current.reviews).toHaveLength(3);
    });
  });

  it("should handle contractor with no reviews", async () => {
    const noReviewsResponse: ContractorProfileResponse = {
      contractor: mockProfileResponse.contractor,
      reviews: [],
    };

    vi.mocked(customerService.getContractorProfile).mockResolvedValueOnce(
      noReviewsResponse
    );

    const { result } = renderHook(() => useContractorProfile(mockContractorId));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.reviews).toEqual([]);
    expect(result.current.contractor?.reviewCount).toBe(2);
  });

  it("should handle contractor with null rating", async () => {
    const noRatingResponse: ContractorProfileResponse = {
      contractor: {
        ...mockProfileResponse.contractor,
        averageRating: null,
      },
      reviews: [],
    };

    vi.mocked(customerService.getContractorProfile).mockResolvedValueOnce(
      noRatingResponse
    );

    const { result } = renderHook(() => useContractorProfile(mockContractorId));

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });

    expect(result.current.contractor?.rating).toBeNull();
  });

  it("should clear data when contractorId changes to null", async () => {
    vi.mocked(customerService.getContractorProfile).mockResolvedValueOnce(
      mockProfileResponse
    );

    const { result, rerender } = renderHook(
      ({ id }: { id: string | null }) => useContractorProfile(id),
      { initialProps: { id: mockContractorId } }
    );

    await waitFor(() => {
      expect(result.current.contractor).toBeDefined();
    });

    rerender({ id: null });

    await waitFor(() => {
      expect(result.current.contractor).toBeNull();
      expect(result.current.reviews).toEqual([]);
    });
  });
});
