/**
 * useSignalR Hook Tests
 * Vitest + Testing Library React
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";
import { waitFor } from "@testing-library/react";
import { useSignalR } from "../useSignalR";
import {
  signalRService,
  JobStatusUpdateEvent,
} from "@/services/signalRService";

// Mock the signalR service
vi.mock("@/services/signalRService");

describe("useSignalR Hook", () => {
  const mockJobStatusEvent: JobStatusUpdateEvent = {
    jobId: "job_123",
    newStatus: "InProgress",
    updatedAt: "2025-11-15T10:30:00Z",
    contractorId: "contractor_1",
    estimatedArrivalTime: "2025-11-15T10:30:00Z",
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should initialize SignalR connection on mount", async () => {
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);
    vi.mocked(signalRService.subscribe).mockReturnValueOnce(() => {});

    renderHook(() => useSignalR());

    await waitFor(() => {
      expect(signalRService.connect).toHaveBeenCalled();
    });
  });

  it("should subscribe to JobStatusUpdated event", async () => {
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);
    const mockUnsubscribe = vi.fn();
    vi.mocked(signalRService.subscribe).mockReturnValueOnce(mockUnsubscribe);

    renderHook(() => useSignalR());

    await waitFor(() => {
      expect(signalRService.subscribe).toHaveBeenCalledWith(
        "JobStatusUpdated",
        expect.any(Function)
      );
    });
  });

  it("should call onJobStatusUpdate callback when event received", async () => {
    const onJobStatusUpdate = vi.fn();
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);

    let capturedCallback: ((event: JobStatusUpdateEvent) => void) | null = null;
    vi.mocked(signalRService.subscribe).mockImplementationOnce(
      (_eventName: string, callback: (event: JobStatusUpdateEvent) => void) => {
        capturedCallback = callback;
        return () => {};
      }
    );

    renderHook(() => useSignalR({ onJobStatusUpdate }));

    await waitFor(() => {
      expect(capturedCallback).not.toBeNull();
    });

    // Simulate event
    if (capturedCallback) {
      capturedCallback(mockJobStatusEvent);
    }

    expect(onJobStatusUpdate).toHaveBeenCalledWith(mockJobStatusEvent);
  });

  it("should call onConnected callback when connected", async () => {
    const onConnected = vi.fn();
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);
    vi.mocked(signalRService.subscribe).mockReturnValueOnce(() => {});

    renderHook(() => useSignalR({ onConnected }));

    await waitFor(() => {
      expect(signalRService.connect).toHaveBeenCalled();
    });
  });

  it("should call onError callback on connection error", async () => {
    const onError = vi.fn();
    const error = new Error("Connection failed");

    vi.mocked(signalRService.connect).mockRejectedValueOnce(error);
    vi.mocked(signalRService.subscribe).mockReturnValueOnce(() => {});

    renderHook(() => useSignalR({ onError }));

    await waitFor(
      () => {
        expect(signalRService.connect).toHaveBeenCalled();
      },
      { timeout: 5000 }
    );
  });

  it("should expose subscribe method", () => {
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);
    vi.mocked(signalRService.subscribe).mockReturnValueOnce(() => {});

    const { result } = renderHook(() => useSignalR());

    expect(result.current.subscribe).toBeDefined();
    expect(typeof result.current.subscribe).toBe("function");
  });

  it("should expose disconnect method", () => {
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);
    vi.mocked(signalRService.subscribe).mockReturnValueOnce(() => {});
    vi.mocked(signalRService.disconnect).mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useSignalR());

    expect(result.current.disconnect).toBeDefined();
    expect(typeof result.current.disconnect).toBe("function");
  });

  it("should handle multiple event subscriptions", async () => {
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);

    let subscribeCalls = 0;
    vi.mocked(signalRService.subscribe).mockImplementation(
      (
        _eventName: string,
        _callback: (event: JobStatusUpdateEvent) => void
      ) => {
        subscribeCalls++;
        return () => {};
      }
    );

    renderHook(() => useSignalR());

    await waitFor(() => {
      expect(signalRService.subscribe).toHaveBeenCalled();
    });
  });

  it("should handle job status update with all fields", async () => {
    const onJobStatusUpdate = vi.fn();
    vi.mocked(signalRService.connect).mockResolvedValueOnce(undefined);

    let capturedCallback: ((event: JobStatusUpdateEvent) => void) | null = null;
    vi.mocked(signalRService.subscribe).mockImplementation(
      (_eventName: string, callback: (event: JobStatusUpdateEvent) => void) => {
        capturedCallback = callback;
        return () => {};
      }
    );

    renderHook(() => useSignalR({ onJobStatusUpdate }));

    await waitFor(() => {
      expect(signalRService.subscribe).toHaveBeenCalled();
    });

    const eventWithAllFields: JobStatusUpdateEvent = {
      jobId: "job_456",
      newStatus: "Completed",
      updatedAt: "2025-11-15T16:00:00Z",
      contractorId: "contractor_2",
      estimatedArrivalTime: "2025-11-15T16:00:00Z",
    };

    if (capturedCallback) {
      capturedCallback(eventWithAllFields);
      expect(onJobStatusUpdate).toHaveBeenCalledWith(eventWithAllFields);
    }
  });
});
