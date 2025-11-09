/**
 * useContractorList Hook Tests
 * Tests for contractor list management hook
 */

import { renderHook, act, waitFor } from "@testing-library/react";
import axios from "axios";
import { vi, describe, it, expect, beforeEach, afterEach } from "vitest";
import { useContractorList } from "../useContractorList";
import { dispatcherService } from "@/services/dispatcherService";
import { Contractor } from "@/types/Contractor";

// Mock dispatcherService
vi.mock("@/services/dispatcherService", () => ({
  dispatcherService: {
    getContractorList: vi.fn(),
    getAvailableContractors: vi.fn(),
    addContractorToList: vi.fn(),
    removeContractorFromList: vi.fn(),
  },
}));

// Mock axios
vi.mock("axios", () => ({
  default: {
    CancelToken: {
      source: () => ({
        token: {},
        cancel: vi.fn(),
      }),
    },
    isCancel: vi.fn((e) => e?.message === "Request cancelled"),
  },
}));

describe("useContractorList Hook", () => {
  const mockContractors: Contractor[] = [
    {
      id: "1",
      name: "John's Plumbing",
      rating: 4.5,
      reviewCount: 20,
      location: "123 Main St",
      tradeType: "Plumbing",
      isActive: true,
      inDispatcherList: true,
    },
    {
      id: "2",
      name: "Electric Pro",
      rating: 4.8,
      reviewCount: 35,
      location: "456 Oak Ave",
      tradeType: "Electrical",
      isActive: true,
      inDispatcherList: false,
    },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it("should initialize with default state", () => {
    const { result } = renderHook(() => useContractorList());

    expect(result.current.myList).toEqual([]);
    expect(result.current.allContractors).toEqual([]);
    expect(result.current.totalContractors).toBe(0);
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBe(null);
    expect(result.current.contractorListOnly).toBe(false);
  });

  it("should fetch my contractor list", async () => {
    vi.mocked(dispatcherService.getContractorList).mockResolvedValue([
      mockContractors[0],
    ]);

    const { result } = renderHook(() => useContractorList());

    await act(async () => {
      await result.current.fetchMyList();
    });

    await waitFor(() => {
      expect(result.current.myList).toEqual([mockContractors[0]]);
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe(null);
    });
  });

  it("should fetch available contractors with pagination", async () => {
    vi.mocked(dispatcherService.getAvailableContractors).mockResolvedValue({
      contractors: mockContractors,
      total: 100,
      hasMore: true,
    });

    const { result } = renderHook(() => useContractorList());

    await act(async () => {
      await result.current.fetchAvailableContractors(50, 0);
    });

    await waitFor(() => {
      expect(result.current.allContractors).toEqual(mockContractors);
      expect(result.current.totalContractors).toBe(100);
      expect(result.current.loading).toBe(false);
    });
  });

  it("should search contractors by name", async () => {
    vi.mocked(dispatcherService.getAvailableContractors).mockResolvedValue({
      contractors: [mockContractors[0]],
      total: 1,
      hasMore: false,
    });

    const { result } = renderHook(() => useContractorList());

    await act(async () => {
      await result.current.fetchAvailableContractors(50, 0, "John's Plumbing");
    });

    await waitFor(() => {
      expect(result.current.allContractors).toEqual([mockContractors[0]]);
      expect(dispatcherService.getAvailableContractors).toHaveBeenCalledWith(
        50,
        0,
        "John's Plumbing",
        expect.any(Object)
      );
    });
  });

  it("should add contractor to list", async () => {
    const updatedList = [...mockContractors];
    vi.mocked(dispatcherService.addContractorToList).mockResolvedValue(
      updatedList
    );

    const { result } = renderHook(() => useContractorList());

    let success = false;
    await act(async () => {
      success = await result.current.addContractor("2");
    });

    await waitFor(() => {
      expect(success).toBe(true);
      expect(result.current.myList).toEqual(updatedList);
      expect(result.current.loading).toBe(false);
    });
  });

  it("should remove contractor from list", async () => {
    const updatedList = [mockContractors[1]];
    vi.mocked(dispatcherService.removeContractorFromList).mockResolvedValue(
      updatedList
    );

    const { result } = renderHook(() => useContractorList());

    let success = false;
    await act(async () => {
      success = await result.current.removeContractor("1");
    });

    await waitFor(() => {
      expect(success).toBe(true);
      expect(result.current.myList).toEqual(updatedList);
      expect(result.current.loading).toBe(false);
    });
  });

  it("should handle error when adding contractor fails", async () => {
    const error = new Error("Network error");
    vi.mocked(dispatcherService.addContractorToList).mockRejectedValue(error);

    const { result } = renderHook(() => useContractorList());

    let success = false;
    await act(async () => {
      success = await result.current.addContractor("2");
    });

    await waitFor(() => {
      expect(success).toBe(false);
      expect(result.current.error).toBe("Network error");
      expect(result.current.loading).toBe(false);
    });
  });

  it("should handle error when fetching fails", async () => {
    const error = new Error("Failed to fetch contractor list");
    vi.mocked(dispatcherService.getContractorList).mockRejectedValue(error);

    const { result } = renderHook(() => useContractorList());

    await act(async () => {
      await result.current.fetchMyList();
    });

    await waitFor(() => {
      expect(result.current.error).toBe("Failed to fetch contractor list");
      expect(result.current.loading).toBe(false);
    });
  });

  it("should toggle contractor_list_only filter", () => {
    const { result } = renderHook(() => useContractorList());

    expect(result.current.contractorListOnly).toBe(false);

    act(() => {
      result.current.toggleFilter();
    });

    expect(result.current.contractorListOnly).toBe(true);

    act(() => {
      result.current.toggleFilter();
    });

    expect(result.current.contractorListOnly).toBe(false);
  });

  it("should handle multiple sequential requests without cancellation conflicts", async () => {
    vi.mocked(dispatcherService.getContractorList).mockResolvedValue([
      mockContractors[0],
    ]);

    const { result } = renderHook(() => useContractorList());

    // Make two sequential calls
    await act(async () => {
      await result.current.fetchMyList();
    });

    expect(result.current.myList).toEqual([mockContractors[0]]);

    // Second call should cancel the first and proceed
    await act(async () => {
      await result.current.fetchMyList();
    });

    expect(result.current.myList).toEqual([mockContractors[0]]);
  });

  it("should be idempotent when adding same contractor twice", async () => {
    vi.mocked(dispatcherService.addContractorToList).mockResolvedValue(
      mockContractors
    );

    const { result } = renderHook(() => useContractorList());

    let success1 = false;
    let success2 = false;

    await act(async () => {
      success1 = await result.current.addContractor("1");
    });

    await act(async () => {
      success2 = await result.current.addContractor("1");
    });

    expect(success1).toBe(true);
    expect(success2).toBe(true);
    expect(result.current.myList).toEqual(mockContractors);
  });

  it("should maintain loading state during fetch operations", async () => {
    vi.mocked(dispatcherService.getContractorList).mockResolvedValue([
      mockContractors[0],
    ]);

    const { result } = renderHook(() => useContractorList());

    act(() => {
      result.current.fetchMyList();
    });

    // Check loading is true during async operation
    expect(result.current.loading).toBe(true);

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
    });
  });

  it("should cleanup on unmount", () => {
    const { unmount, result } = renderHook(() => useContractorList());

    // Verify cleanup function exists
    expect(result.current.cleanup).toBeDefined();
    expect(typeof result.current.cleanup).toBe("function");

    // Should not throw when unmounting
    expect(() => unmount()).not.toThrow();
  });
});
