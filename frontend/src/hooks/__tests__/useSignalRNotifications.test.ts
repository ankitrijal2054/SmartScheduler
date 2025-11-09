/**
 * useSignalRNotifications Hook Tests
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { useSignalRNotifications } from "../useSignalRNotifications";

// Mock useAuthContext
vi.mock("@/hooks/useAuthContext", () => ({
  useAuthContext: () => ({
    user: { id: "contractor-1", email: "test@test.com", role: "Contractor" },
    token: "test-token",
    isAuthenticated: true,
    isLoading: false,
  }),
}));

// Mock useNotifications
vi.mock("@/hooks/useNotifications", () => ({
  useNotifications: () => ({
    addNotification: vi.fn(),
    dismissNotification: vi.fn(),
    clearAll: vi.fn(),
    notifications: [],
  }),
}));

// Mock signalRService
vi.mock("@/services/signalRService", () => ({
  signalRService: {
    connect: vi.fn().mockResolvedValue(undefined),
    joinGroup: vi.fn().mockResolvedValue(undefined),
    on: vi.fn(),
    off: vi.fn(),
  },
}));

import { signalRService } from "@/services/signalRService";

describe("useSignalRNotifications Hook", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should connect to SignalR on mount", async () => {
    const { result } = renderHook(() => useSignalRNotifications());

    await waitFor(() => {
      expect(signalRService.connect).toHaveBeenCalled();
    });
  });

  it("should join contractor group", async () => {
    const { result } = renderHook(() => useSignalRNotifications());

    await waitFor(() => {
      expect(signalRService.joinGroup).toHaveBeenCalledWith("contractor-contractor-1");
    });
  });

  it("should register message handlers", async () => {
    const { result } = renderHook(() => useSignalRNotifications());

    await waitFor(() => {
      expect(signalRService.on).toHaveBeenCalledWith(
        "NewJobAssigned",
        expect.any(Function)
      );
    });
  });

  it("should set isConnected to true on successful connection", async () => {
    const { result } = renderHook(() => useSignalRNotifications());

    await waitFor(() => {
      expect(result.current.isConnected).toBe(true);
    });
  });

  it("should handle connection errors", async () => {
    const error = new Error("Connection failed");
    vi.mocked(signalRService.connect).mockRejectedValueOnce(error);

    const { result } = renderHook(() => useSignalRNotifications());

    await waitFor(() => {
      expect(result.current.error).toBe("Connection failed");
    });
  });

  it("should expose manualReconnect function", async () => {
    const { result } = renderHook(() => useSignalRNotifications());

    expect(typeof result.current.manualReconnect).toBe("function");
  });
});



