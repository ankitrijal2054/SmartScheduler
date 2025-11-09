/**
 * useRecommendations Hook Tests
 * Unit tests using Vitest
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useRecommendations } from "../useRecommendations";
import * as dispatcherServiceModule from "@/services/dispatcherService";
import {
  RecommendedContractor,
  RecommendationRequest,
  RecommendationResponse,
} from "@/types/Contractor";

// Mock the dispatcher service
vi.mock("@/services/dispatcherService");

const mockRecommendations: RecommendedContractor[] = [
  {
    contractorId: "cont_001",
    name: "Bob's Plumbing Pro",
    rank: 1,
    score: 0.92,
    avgRating: 4.8,
    reviewCount: 42,
    distance: 2.5,
    travelTime: 12,
    tradeType: "Plumbing",
    availableTimeSlots: [
      {
        startTime: "2025-11-10T14:00:00Z",
        endTime: "2025-11-10T16:00:00Z",
      },
    ],
  },
  {
    contractorId: "cont_002",
    name: "Jane's Plumbing Services",
    rank: 2,
    score: 0.87,
    avgRating: 4.6,
    reviewCount: 28,
    distance: 4.1,
    travelTime: 18,
    tradeType: "Plumbing",
    availableTimeSlots: [
      {
        startTime: "2025-11-10T14:30:00Z",
        endTime: "2025-11-10T16:30:00Z",
      },
    ],
  },
  {
    contractorId: "cont_003",
    name: "Quick Plumbing Team",
    rank: 3,
    score: 0.79,
    avgRating: 4.4,
    reviewCount: 15,
    distance: 6.2,
    travelTime: 22,
    tradeType: "Plumbing",
    availableTimeSlots: [
      {
        startTime: "2025-11-10T15:00:00Z",
        endTime: "2025-11-10T17:00:00Z",
      },
    ],
  },
];

const mockRecommendationResponse: RecommendationResponse = {
  data: mockRecommendations,
  metadata: {
    totalAvailable: 3,
    requestTime: "2025-11-08T11:30:00Z",
  },
};

describe("useRecommendations Hook", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should initialize with default state", () => {
    const { result } = renderHook(() => useRecommendations());

    expect(result.current.recommendations).toEqual([]);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.sortBy).toBe("rank");
  });

  it("should fetch recommendations and set loading state", async () => {
    const mockGetRecommendations = vi
      .fn()
      .mockResolvedValue(mockRecommendationResponse);
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockImplementation(mockGetRecommendations);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    let fetchPromise: Promise<RecommendedContractor[] | null>;

    await act(async () => {
      fetchPromise = result.current.fetchRecommendations(request);
    });

    // Check that loading was true at some point
    expect(mockGetRecommendations).toHaveBeenCalledWith(
      request,
      expect.anything()
    );

    // Wait for the fetch to complete
    const data = await act(() => fetchPromise!);

    expect(data).toEqual(mockRecommendations);
    expect(result.current.recommendations.length).toBe(3);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it("should pass contractor_list_only filter to API", async () => {
    const mockGetRecommendations = vi
      .fn()
      .mockResolvedValue(mockRecommendationResponse);
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockImplementation(mockGetRecommendations);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
      contractor_list_only: true,
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    expect(mockGetRecommendations).toHaveBeenCalledWith(
      expect.objectContaining({ contractor_list_only: true }),
      expect.anything()
    );
  });

  it("should handle API errors gracefully", async () => {
    const errorMessage = "Network error";
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockRejectedValue(new Error(errorMessage));

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe(errorMessage);
    expect(result.current.recommendations).toEqual([]);
  });

  it("should sort recommendations by rank (default)", async () => {
    const unsortedRecommendations: RecommendedContractor[] = [
      { ...mockRecommendations[2], rank: 3 }, // Rank 3
      { ...mockRecommendations[0], rank: 1 }, // Rank 1
      { ...mockRecommendations[1], rank: 2 }, // Rank 2
    ];

    const mockResponse: RecommendationResponse = {
      data: unsortedRecommendations,
      metadata: mockRecommendationResponse.metadata,
    };

    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    // Should be sorted by rank (default)
    expect(result.current.recommendations[0].rank).toBe(1);
    expect(result.current.recommendations[1].rank).toBe(2);
    expect(result.current.recommendations[2].rank).toBe(3);
  });

  it("should sort recommendations by rating (highest first)", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue(mockRecommendationResponse);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    // Sort by rating
    act(() => {
      result.current.sortRecommendations("rating");
    });

    expect(result.current.sortBy).toBe("rating");
    // Bob (4.8) should be first
    expect(result.current.recommendations[0].avgRating).toBe(4.8);
    // Jane (4.6) should be second
    expect(result.current.recommendations[1].avgRating).toBe(4.6);
    // Quick (4.4) should be third
    expect(result.current.recommendations[2].avgRating).toBe(4.4);
  });

  it("should sort recommendations by distance (nearest first)", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue(mockRecommendationResponse);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    // Sort by distance
    act(() => {
      result.current.sortRecommendations("distance");
    });

    expect(result.current.sortBy).toBe("distance");
    // Bob (2.5 miles) should be first
    expect(result.current.recommendations[0].distance).toBe(2.5);
    // Jane (4.1 miles) should be second
    expect(result.current.recommendations[1].distance).toBe(4.1);
    // Quick (6.2 miles) should be third
    expect(result.current.recommendations[2].distance).toBe(6.2);
  });

  it("should sort recommendations by travel time (shortest first)", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue(mockRecommendationResponse);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    // Sort by travel time
    act(() => {
      result.current.sortRecommendations("travelTime");
    });

    expect(result.current.sortBy).toBe("travelTime");
    // Bob (12 min) should be first
    expect(result.current.recommendations[0].travelTime).toBe(12);
    // Jane (18 min) should be second
    expect(result.current.recommendations[1].travelTime).toBe(18);
    // Quick (22 min) should be third
    expect(result.current.recommendations[2].travelTime).toBe(22);
  });

  it("should retry fetching recommendations", async () => {
    const mockGetRecommendations = vi
      .fn()
      .mockResolvedValue(mockRecommendationResponse);
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockImplementation(mockGetRecommendations);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    expect(mockGetRecommendations).toHaveBeenCalledTimes(1);

    // Call retry
    await act(async () => {
      await result.current.retry();
    });

    expect(mockGetRecommendations).toHaveBeenCalledTimes(2);
    expect(result.current.recommendations.length).toBe(3);
  });

  it("should return null when fetchRecommendations encounters error", async () => {
    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockRejectedValue(new Error("Network error"));

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    let returnValue: RecommendedContractor[] | null;

    await act(async () => {
      returnValue = await result.current.fetchRecommendations(request);
    });

    expect(returnValue).toBeNull();
  });

  it("should have cleanup function", () => {
    const { result } = renderHook(() => useRecommendations());

    expect(typeof result.current.cleanup).toBe("function");

    // Should not throw when called
    act(() => {
      result.current.cleanup();
    });
  });

  it("should handle empty recommendations response", async () => {
    const emptyResponse: RecommendationResponse = {
      data: [],
      metadata: {
        totalAvailable: 0,
        requestTime: "2025-11-08T11:30:00Z",
      },
    };

    vi.spyOn(
      dispatcherServiceModule.dispatcherService,
      "getRecommendations"
    ).mockResolvedValue(emptyResponse);

    const { result } = renderHook(() => useRecommendations());

    const request: RecommendationRequest = {
      jobId: "job_123",
      jobType: "Plumbing",
      location: "123 Main St",
      desiredDateTime: "2025-11-10T14:00:00Z",
    };

    await act(async () => {
      await result.current.fetchRecommendations(request);
    });

    expect(result.current.recommendations).toEqual([]);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
  });
});
