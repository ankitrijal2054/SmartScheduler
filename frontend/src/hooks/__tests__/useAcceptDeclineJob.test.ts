/**
 * useAcceptDeclineJob Hook Tests
 * Story 5.2: Job Details Modal & Accept/Decline Workflow
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act, waitFor } from "@testing-library/react";
import { useAcceptDeclineJob } from "../useAcceptDeclineJob";

// Mock contractorService
vi.mock("@/services/contractorService", () => ({
  contractorService: {
    acceptAssignment: vi.fn(),
    declineAssignment: vi.fn(),
  },
}));

import { contractorService } from "@/services/contractorService";

describe("useAcceptDeclineJob Hook", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("acceptJob", () => {
    it("should call acceptAssignment API", async () => {
      const mockAccept = vi.spyOn(contractorService, "acceptAssignment") as any;
      mockAccept.mockResolvedValue({});

      const { result } = renderHook(() => useAcceptDeclineJob());

      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toBeNull();

      await act(async () => {
        await result.current.acceptJob("assign-1");
      });

      expect(mockAccept).toHaveBeenCalledWith("assign-1");
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toBeNull();
    });

    it("should set error on accept failure", async () => {
      const mockAccept = vi.spyOn(contractorService, "acceptAssignment") as any;
      mockAccept.mockRejectedValue(new Error("Accept failed"));

      const { result } = renderHook(() => useAcceptDeclineJob());

      await act(async () => {
        try {
          await result.current.acceptJob("assign-1");
        } catch (err) {
          // Expected to throw
        }
      });

      expect(result.current.error).toBe("Accept failed");
      expect(result.current.isLoading).toBe(false);
    });

    it("should handle loading state during accept", async () => {
      const mockAccept = vi.spyOn(contractorService, "acceptAssignment") as any;
      let resolveAccept: () => void = () => {};
      const acceptPromise = new Promise<void>((resolve) => {
        resolveAccept = resolve;
      });
      mockAccept.mockReturnValue(acceptPromise);

      const { result } = renderHook(() => useAcceptDeclineJob());

      let isLoadingDuringCall = false;

      act(() => {
        result.current.acceptJob("assign-1").then(() => {
          // Check loading state after resolution
        });
      });

      await waitFor(() => {
        if (result.current.isLoading) {
          isLoadingDuringCall = true;
        }
      });

      act(() => {
        resolveAccept();
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(isLoadingDuringCall).toBe(true);
    });
  });

  describe("declineJob", () => {
    it("should call declineAssignment API without reason", async () => {
      const mockDecline = vi.spyOn(
        contractorService,
        "declineAssignment"
      ) as any;
      mockDecline.mockResolvedValue({});

      const { result } = renderHook(() => useAcceptDeclineJob());

      await act(async () => {
        await result.current.declineJob("assign-1");
      });

      expect(mockDecline).toHaveBeenCalledWith("assign-1", undefined);
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toBeNull();
    });

    it("should call declineAssignment API with reason", async () => {
      const mockDecline = vi.spyOn(
        contractorService,
        "declineAssignment"
      ) as any;
      mockDecline.mockResolvedValue({});

      const { result } = renderHook(() => useAcceptDeclineJob());

      await act(async () => {
        await result.current.declineJob("assign-1", "Out of service area");
      });

      expect(mockDecline).toHaveBeenCalledWith(
        "assign-1",
        "Out of service area"
      );
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toBeNull();
    });

    it("should set error on decline failure", async () => {
      const mockDecline = vi.spyOn(
        contractorService,
        "declineAssignment"
      ) as any;
      mockDecline.mockRejectedValue(new Error("Decline failed"));

      const { result } = renderHook(() => useAcceptDeclineJob());

      await act(async () => {
        try {
          await result.current.declineJob("assign-1", "Scheduling conflict");
        } catch (err) {
          // Expected to throw
        }
      });

      expect(result.current.error).toBe("Decline failed");
      expect(result.current.isLoading).toBe(false);
    });
  });

  describe("concurrent calls", () => {
    it("should handle multiple calls independently", async () => {
      const mockAccept = vi.spyOn(contractorService, "acceptAssignment") as any;
      const mockDecline = vi.spyOn(
        contractorService,
        "declineAssignment"
      ) as any;

      mockAccept.mockResolvedValue({});
      mockDecline.mockResolvedValue({});

      const { result } = renderHook(() => useAcceptDeclineJob());

      await act(async () => {
        await result.current.acceptJob("assign-1");
      });

      expect(mockAccept).toHaveBeenCalledWith("assign-1");

      await act(async () => {
        await result.current.declineJob("assign-2", "Conflict");
      });

      expect(mockDecline).toHaveBeenCalledWith("assign-2", "Conflict");
    });
  });
});
